using Discord.WebSocket;
using DJWatermelon.AudioService.Lavalink.Models;
using DJWatermelon.AudioService.Lavalink.Models.REST;
using DJWatermelon.AudioService.Lavalink.Models.WebSocket;
using DJWatermelon.AudioService.Lavalink.Models.WebSocket.EventPayloads;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DJWatermelon.AudioService.Lavalink;

internal sealed class LavalinkPlayersManager : IPlayersManager, IAsyncDisposable
{
    private const string VersionPrefix = "/v4";
    private const bool UseIdsInsteadOfNames = false;

    // A possible null endpoint in time between moving
    // from a failed voice server to the newly allocated one.
    private sealed record VoiceServer(string Token, string? Endpoint);
    private sealed record VoiceSession(string SessionId, bool IsConnected);

    private readonly ILogger<LavalinkPlayersManager> _logger;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly LavalinkOptions _options;
    private readonly DiscordSocketClient _discordSocketClient;

    private readonly IDictionary<ulong, IPlayer> _playersCache =
        new ConcurrentDictionary<ulong, IPlayer>();

    // List of guild's voice servers. Every guild has its voice server. 
    private readonly IDictionary<ulong, VoiceServer> _voiceServers =
        new ConcurrentDictionary<ulong, VoiceServer>();
    private readonly IDictionary<ulong, string> _voiceSessions = 
        new ConcurrentDictionary<ulong, string>();
    private readonly SemaphoreSlim _voiceSemaphore = new(1);

    private TaskCompletionSource<bool> _readyTaskCompletionSource;
    private bool _disposed;

    private CancellationToken _shutdownCancellationToken;

    private readonly ILavalinkAPI _lavalinkAPI;

    public LavalinkPlayersManager(
        ILogger<LavalinkPlayersManager> logger,
        IHostEnvironment hostEnvironment,
        IOptions<LavalinkOptions> options,
        DiscordSocketClient discordSocketClient)
    {
        _logger = logger;
        _hostEnvironment = hostEnvironment;
        _options = options.Value;
        _discordSocketClient = discordSocketClient;

        _readyTaskCompletionSource = new TaskCompletionSource<bool>(
            creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);

        _lavalinkAPI = RestService.For<ILavalinkAPI>(
            new HttpClient()
            {
                BaseAddress = new UriBuilder(
                    scheme: Uri.UriSchemeHttp,
                    host: _options.RemoteHostName,
                    port: _options.Port.GetValueOrDefault(),
                    pathValue: VersionPrefix).Uri,

                DefaultRequestHeaders =
                {
                    Authorization = AuthenticationHeaderValue.Parse(_options.Authorization),
                }
            },
            new RefitSettings()
            {
                ContentSerializer = new SystemTextJsonContentSerializer(
                    LavalinkModelsSourceGenerationContext.Default.Options)
            });
        
        _discordSocketClient.VoiceServerUpdated += OnVoiceServerUpdated;
        _discordSocketClient.UserVoiceStateUpdated += OnVoiceStateUpdated;
    }

    public bool IsReady => _readyTaskCompletionSource.Task.IsCompletedSuccessfully;

    public string? SessionId { get; private set; }

    public async Task InitAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        
        if (_readyTaskCompletionSource.Task.Status == TaskStatus.Running)
        {
            throw new InvalidOperationException("The node was already started.");
        }

        _shutdownCancellationToken = cancellationToken;
        using CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            await ReceiveInternalAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _readyTaskCompletionSource.TrySetException(exception);
            throw;
        }
        finally
        {
            _readyTaskCompletionSource.TrySetCanceled(CancellationToken.None);
        }
    }

    private async Task OnVoiceServerUpdated(SocketVoiceServer voiceServer)
    {
        await _voiceSemaphore.WaitAsync();

        // Expliptical checks whatever log level is enabled to
        // ensure we don't resolve the guild name insolently.
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogVoiceServerUpdate(
                FormatObject(voiceServer));
        }
        
        // Null endpoint means that the current voice server failed.
        // Removing from the cache, also stop and destroy all currently active players.
        if (string.IsNullOrEmpty(voiceServer.Endpoint))
        {
            _voiceServers.Remove(voiceServer.Guild.Id);
            if (_playersCache.Remove(voiceServer.Guild.Id, out IPlayer? player))
            {
                player.Dispose();
            }
        }
        else
        {
            // Cache guild's voice server.
            if (_voiceServers.TryGetValue(voiceServer.Guild.Id, out VoiceServer? cachedVoiceServer))
            {
                _voiceServers[voiceServer.Guild.Id] = cachedVoiceServer with
                {
                    Token = voiceServer.Token,
                    Endpoint = voiceServer.Endpoint
                };
            }
            else
            {
                _voiceServers.Add(
                    voiceServer.Guild.Id, 
                    new VoiceServer(voiceServer.Token, voiceServer.Endpoint));
            }
        }

        _voiceSemaphore.Release();
    }

    private async Task OnVoiceStateUpdated(
        SocketUser user,
        SocketVoiceState stateBefore,
        SocketVoiceState state)
    {
        await _voiceSemaphore.WaitAsync();

        // Expliptical checks whatever log level is enabled to
        // ensure we don't resolve the guild name insolently.
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogVoiceStateUpdated(
                UseIdsInsteadOfNames ? user.Id.ToString() : user.Username,
                FormatObject(stateBefore),
                FormatObject(state));
        }

        // Check if the user that changed the state is us.
        if (user.Id != _discordSocketClient.CurrentUser.Id)
        {
            return;
        }

        // User disconected.
        if (state.VoiceChannel is null)
        {
            _voiceSessions.Remove(stateBefore.VoiceChannel.Guild.Id);
        }
        else
        {
            if (_voiceSessions.TryGetValue(state.VoiceChannel.Guild.Id, out string? id))
            {
                _voiceSessions[state.VoiceChannel.Guild.Id] = state.VoiceSessionId;
            }
            else
            {
                _voiceSessions.Add(state.VoiceChannel.Guild.Id, state.VoiceSessionId);
            }
        }

        _voiceSemaphore.Release();
    }

    public bool TryGetPlayer(ulong id, [NotNullWhen(true)] out IPlayer? player)
    {
        return _playersCache.TryGetValue(id, out player);
    }

    public IEnumerable<IPlayer> GetPlayers()
    {
        return new ReadOnlyDictionary<ulong, IPlayer>(_playersCache).Values;
    }

    #region WebSocket

    private async ValueTask ProcessPayloadAsync(
        IPayload payload,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(payload);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogReceivedPayload();
        }

        if (payload is ReadyPayload readyPayload)
        {
            if (!string.IsNullOrEmpty(SessionId))
            {
                _logger.LogMultipleReadyPayloadsReceived();
            }

            SessionId = readyPayload.SessionId;

            _logger.LogLavalinkReady();
        }

        if (string.IsNullOrEmpty(SessionId))
        {
            _logger.LogPayloadReceivedBeforeReady();
            return;
        }

        if (payload is EventPayload eventPayload)
        {
            await ProcessEventAsync(eventPayload, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (payload is PlayerUpdatePayload playerUpdatePayload)
        {
            if (TryGetPlayer(playerUpdatePayload.GuildId, out IPlayer? _))
            {
                _logger.LogPayloadForInexistentPlayer(playerUpdatePayload.GuildId);
                return;
            }

            // TODO.
        }
    }

    private async ValueTask ProcessEventAsync(
        EventPayload payload,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(payload);

        if (!TryGetPlayer(payload.GuildId, out IPlayer? player))
        {
            _logger.LogPayloadForInexistentPlayer(payload.GuildId);
            return;
        }

        ValueTask task = payload switch
        {
            TrackEndEventPayload trackEvent =>
                ProcessTrackEndEventAsync(player, trackEvent, cancellationToken),

            TrackStartEventPayload trackEvent =>
                ProcessTrackStartEventAsync(player, trackEvent, cancellationToken),

            TrackStuckEventPayload trackEvent =>
                ProcessTrackStuckEventAsync(player, trackEvent, cancellationToken),

            TrackExceptionEventPayload trackEvent =>
                ProcessTrackExceptionEventAsync(player, trackEvent, cancellationToken),

            WebSocketClosedEventPayload closedEvent =>
                ProcessWebSocketClosedEventAsync(player, closedEvent, cancellationToken),

            _ => throw new InvalidOperationException(),
        };

        await task.ConfigureAwait(false);
    }

    private async ValueTask ProcessTrackEndEventAsync(
        IPlayer player,
        TrackEndEventPayload trackEndEvent,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(trackEndEvent);

        ITrackHandle track = trackEndEvent.Track;

        // TODO.
    }

    private async ValueTask ProcessTrackExceptionEventAsync(
        IPlayer player,
        TrackExceptionEventPayload trackExceptionEvent,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(trackExceptionEvent);

        ITrackHandle track = trackExceptionEvent.Track;

        // TODO.
    }

    private async ValueTask ProcessTrackStartEventAsync(
        IPlayer player,
        TrackStartEventPayload trackStartEvent,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(trackStartEvent);

        ITrackHandle track = trackStartEvent.Track;

        // TODO.
    }

    private async ValueTask ProcessTrackStuckEventAsync(
        IPlayer player,
        TrackStuckEventPayload trackStuckEvent,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(trackStuckEvent);

        ITrackHandle track = trackStuckEvent.Track;

        // TODO.
    }

    private async ValueTask ProcessWebSocketClosedEventAsync(
        IPlayer player,
        WebSocketClosedEventPayload webSocketClosedEvent,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(webSocketClosedEvent);

        WebSocketCloseStatus closeCode = (WebSocketCloseStatus)webSocketClosedEvent.Code;

        // TODO.
    }

    private async Task ReceiveInternalAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        if (_readyTaskCompletionSource.Task.IsCompleted)
        {
            // Initiate reconnect.
            _readyTaskCompletionSource = new TaskCompletionSource<bool>(
                creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
        }

        // Init WebSocket.
        using ClientWebSocket webSocket = new();

        webSocket.Options.SetRequestHeader("Authorization", _options.Authorization);
        webSocket.Options.SetRequestHeader("User-Id", _discordSocketClient.CurrentUser.Id.ToString());
        webSocket.Options.SetRequestHeader("Client-Name", "DJWatermelonBot");

        using HttpClientHandler httpMessageHandler = new();
        using HttpMessageInvoker httpMessageInvoker = new(
            httpMessageHandler,
            disposeHandler: true);

        try
        {
            // So far, use the default WS scheme, not secured.
            UriBuilder wsUri = new(
                Uri.UriSchemeWs,
                _options.RemoteHostName,
                _options.Port.GetValueOrDefault(),
                "/v4/websocket");

            await webSocket
                .ConnectAsync(wsUri.Uri, httpMessageInvoker, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (_hostEnvironment.IsDevelopment())
            {
                Debugger.Break();
            }
            throw new InvalidOperationException(
                "Something went wrong while trying to establish a " +
                "connection with a remote host.", ex);
        }

        _logger.LogWebSocketConnectionEstablished();

        _readyTaskCompletionSource.SetResult(true);

        // Payload processing.
        Memory<byte> buffer = GC.AllocateUninitializedArray<byte>(4 * 1024);
        while (!_shutdownCancellationToken.IsCancellationRequested)
        {
            ValueWebSocketReceiveResult receiveResult = await webSocket
                .ReceiveAsync(buffer, cancellationToken)
                .ConfigureAwait(false);

            if (!receiveResult.EndOfMessage)
            {
                _logger.LogBufferOutOfRange();
                continue;
            }

            if (receiveResult.MessageType is not WebSocketMessageType.Text)
            {
                if (receiveResult.MessageType is WebSocketMessageType.Close)
                {
                    if (_hostEnvironment.IsDevelopment())
                    {
                        Debugger.Break();
                    }
                    _logger.LogRemoteHostClosedConnection();
                    throw new InvalidOperationException(
                        "Websocket Close Connection message received.");
                }

                _logger.LogBadPayloadReceived();
                continue;
            }

            if (_hostEnvironment.IsDevelopment() && _logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogPayloadText(
                    text: Encoding.UTF8.GetString(buffer[..receiveResult.Count].Span));
            }

            IPayload? payload = JsonSerializer.Deserialize(
                buffer[..receiveResult.Count].Span,
                LavalinkModelsSourceGenerationContext.Default.IPayload);

            if (payload == null)
            {
                _logger.LogBadPayloadReceived();
                continue;
            }

            await ProcessPayloadAsync(payload, cancellationToken).ConfigureAwait(false);
        }
    }

    #endregion

    #region REST

    public async Task<IPlayer> CreatePlayerAsync(
        ulong guildId, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(SessionId);

        await _voiceSemaphore.WaitAsync(cancellationToken);

        _logger.LogCreatingPlayer(guildId);

        if (_playersCache.ContainsKey(guildId))
        {
            _voiceSemaphore.Release();
            throw new Exception("The player already exists for a given guild.");
        }

        if (!_voiceServers.TryGetValue(guildId, out VoiceServer? voiceServer))
        {
            _voiceSemaphore.Release();
            throw new Exception("Before trying to create a player, a " +
                "connection to the voice chat should be established.");
        }

        if (string.IsNullOrEmpty(voiceServer.Endpoint))
        {
            _voiceSemaphore.Release();
            throw new Exception("The voice server endpoint is currently null. " +
                "The voice server was probably flattered, and a new one is now being allocated.");
        }

        if (!_voiceSessions.TryGetValue(guildId, out string? voiceSessionId))
        {
            _voiceSemaphore.Release();
            throw new Exception("The session ID is null - probably, " +
                "the bot left the voice channel.");
        }

        PlayerModel result = await _lavalinkAPI.UpateOrCreatePlayerAsync(
            SessionId, guildId,
            new PlayerUpdateModel(
                TrackUpate: null,
                VoiceState: new VoiceStateModel(
                    Token: voiceServer.Token,
                    Endpoint: voiceServer.Endpoint,
                    SessionId: voiceSessionId)));

        LavalinkPlayer player = new(guildId, SessionId, _lavalinkAPI);
        _playersCache.Add(guildId, player);

        _voiceSemaphore.Release();
        return player;
    }

    public async Task DestroyPlayerAsync(
        ulong guilId,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(SessionId);

        if (_playersCache.ContainsKey(guilId))
        {
            _playersCache.Remove(guilId);
            await _lavalinkAPI.DestroyPlayerAsync(SessionId, guilId);
        }
    }

    public async Task<IEnumerable<ITrackHandle>> SearchForTrackAsync(
        string prompt,
        CancellationToken cancellationToken)
    {
        TrackLoadResultModel result = await _lavalinkAPI.LoadTracksAsync(prompt);
        if (result.ResultType == LoadResultTypes.Error)
        {
            throw new Exception();
        }

        return result.ResultType switch
            {
                LoadResultTypes.Track => 
                    new Collection<ITrackHandle>()
                    {
                        JsonSerializer.Deserialize<LavalinkTrackHandle>(result.Data)
                    },

                LoadResultTypes.Playlist =>
                    JsonSerializer.Deserialize<PlaylistModel>(result.Data)!.Tracks
                        .Select(p => (ITrackHandle)p),

                LoadResultTypes.Search => 
                    JsonSerializer.Deserialize<IEnumerable<LavalinkTrackHandle>>(result.Data)!
                        .Select(p => (ITrackHandle)p),

                LoadResultTypes.Empty => new Collection<ITrackHandle>(),

                _ => throw new Exception("Invalid value for LoadResultTypes enum.")
            };
    }

    #endregion

    private static string FormatObject(object objectToFormat)
        => JsonSerializer.Serialize(
            objectToFormat, 
            new JsonSerializerOptions() 
            { 
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _discordSocketClient.VoiceServerUpdated -= OnVoiceServerUpdated;
        _discordSocketClient.UserVoiceStateUpdated -= OnVoiceStateUpdated;

        _readyTaskCompletionSource.TrySetCanceled();

        _logger.LogLavalinkDisposed();
    }

    void IDisposable.Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _discordSocketClient.VoiceServerUpdated -= OnVoiceServerUpdated;
        _discordSocketClient.UserVoiceStateUpdated -= OnVoiceStateUpdated;

        _readyTaskCompletionSource.TrySetCanceled();

        _logger.LogLavalinkDisposed();
    }
}
