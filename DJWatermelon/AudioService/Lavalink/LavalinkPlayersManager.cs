using DJWatermelon.AudioService.Lavalink.Models;
using DJWatermelon.AudioService.Lavalink.Models.REST;
using DJWatermelon.AudioService.Lavalink.Models.WebSocket;
using DJWatermelon.AudioService.Lavalink.Models.WebSocket.EventPayloads;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Gateway.Events;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

internal sealed class LavalinkPlayersManager : 
    IPlayersManager, IAsyncDisposable,
    IResponder<VoiceStateUpdate>, IResponder<VoiceServerUpdate>
{
    private const string VersionPrefix = "/v4";

    // A possible null endpoint in time between moving
    // from a failed voice server to the newly allocated one.
    private sealed record VoiceServer(string Token, string? Endpoint);
    private sealed record VoiceSession(string SessionId, bool IsConnected);

    private readonly ILogger<LavalinkPlayersManager> _logger;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly LavalinkOptions _options;
    private readonly DiscordGatewayClient _discordClient;

    private readonly IDictionary<Snowflake, IPlayer> _playersCache =
        new ConcurrentDictionary<Snowflake, IPlayer>();

    // List of guild's voice servers. Every guild has its voice server. 
    private readonly IDictionary<Snowflake, VoiceServer> _voiceServers =
        new ConcurrentDictionary<Snowflake, VoiceServer>();
    // Voice chat ID serves as a key for each session ID.
    private readonly IDictionary<Snowflake, string> _voiceSessions = 
        new ConcurrentDictionary<Snowflake, string>();
    private readonly SemaphoreSlim _voiceSemaphore = new(1);

    private TaskCompletionSource<bool> _readyTaskCompletionSource;
    private bool _disposed;

    private CancellationToken _shutdownCancellationToken;

    private readonly ILavalinkAPI _lavalinkAPI;

    private readonly IUser _bot;

    public LavalinkPlayersManager(
        ILogger<LavalinkPlayersManager> logger,
        IHostEnvironment hostEnvironment,
        IOptions<LavalinkOptions> options,
        DiscordGatewayClient discordClient,
        IDiscordRestUserAPI userAPI)
    {
        _logger = logger;
        _hostEnvironment = hostEnvironment;
        _options = options.Value;
        _discordClient = discordClient;

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

        _bot = userAPI.GetCurrentUserAsync().Result.Entity;
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

    public bool TryGetPlayer(Snowflake id, [NotNullWhen(true)] out IPlayer? player)
    {
        return _playersCache.TryGetValue(id, out player);
    }

    public IEnumerable<IPlayer> GetPlayers()
    {
        return new ReadOnlyDictionary<Snowflake, IPlayer>(_playersCache).Values;
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
            if (TryGetPlayer(playerUpdatePayload.GuildID, out IPlayer? _))
            {
                _logger.LogPayloadForInexistentPlayer(playerUpdatePayload.GuildID.Value);
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

        if (!TryGetPlayer(payload.GuildID, out IPlayer? player))
        {
            _logger.LogPayloadForInexistentPlayer(payload.GuildID.Value);
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
        webSocket.Options.SetRequestHeader("User-Id", _bot.ID.ToString());
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
                VersionPrefix + "/websocket");

            await webSocket
                .ConnectAsync(wsUri.Uri, httpMessageInvoker, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
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
        Snowflake guildID, 
        Snowflake voiceChannelID, 
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(SessionId);

        await _voiceSemaphore.WaitAsync(cancellationToken);

        _logger.LogCreatingPlayer(guildID.Value);

        if (_playersCache.ContainsKey(guildID))
        {
            _voiceSemaphore.Release();
            throw new Exception("The player already exists for a given guild.");
        }

        if (!_voiceServers.TryGetValue(guildID, out VoiceServer? voiceServer))
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

        if (!_voiceSessions.TryGetValue(voiceChannelID, out string? voiceSessionID))
        {
            _voiceSemaphore.Release();
            throw new Exception("The session ID is null - probably, " +
                "the bot left the voice channel.");
        }

        _ = await _lavalinkAPI.UpateOrCreatePlayerAsync(
            sessionID: SessionId,
            guildID: guildID.Value,
            playerUpdate: new PlayerUpdateModel(
                TrackUpate: null,
                VoiceState: new VoiceStateModel(
                    Token: voiceServer.Token,
                    Endpoint: voiceServer.Endpoint,
                    SessionId: voiceSessionID)));

        LavalinkPlayer player = new();
        _playersCache.Add(guildID, player);

        _voiceSemaphore.Release();
        return player;
    }

    public async Task DestroyPlayerAsync(
        Snowflake guilID,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(SessionId);

        if (_playersCache.ContainsKey(guilID))
        {
            _playersCache.Remove(guilID);
            await _lavalinkAPI.DestroyPlayerAsync(SessionId, guilID.Value);
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

        _readyTaskCompletionSource.TrySetCanceled();

        _logger.LogLavalinkDisposed();
    }

    async Task<Result> IResponder<VoiceStateUpdate>.RespondAsync(
        VoiceStateUpdate voiceStateUpdate, 
        CancellationToken cancellationToken)
    {
        // Check if the user that changed the state is us.
        if (_bot.ID != voiceStateUpdate.UserID)
        {
            return Result.Success;
        }

        await _voiceSemaphore.WaitAsync(cancellationToken);
        
        // Expliptical checks whatever log level is enabled to
        // ensure we don't resolve the guild name insolently.
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogVoiceStateUpdated(
                voiceStateUpdate.UserID.ToString(),
                FormatObject(voiceStateUpdate));
        }

        // User disconected.
        if (voiceStateUpdate.ChannelID == null)
        {
            _voiceSessions.Remove(voiceStateUpdate.GuildID.Value);
        }
        // A user connected or its state updated.
        else
        {
            if (_voiceSessions.TryGetValue(voiceStateUpdate.ChannelID.Value, out _))
            {
                _voiceSessions[voiceStateUpdate.ChannelID.Value] = voiceStateUpdate.SessionID;
            }
            else
            {
                _voiceSessions.Add(voiceStateUpdate.ChannelID.Value, voiceStateUpdate.SessionID);
            }
        }

        _voiceSemaphore.Release();

        return Result.Success;
    }

    async Task<Result> IResponder<VoiceServerUpdate>.RespondAsync(
        VoiceServerUpdate serverUpate, CancellationToken cancellationToken)
    {
        await _voiceSemaphore.WaitAsync(cancellationToken);

        // Expliptical checks whatever log level is enabled to
        // ensure we don't resolve the guild name insolently. 
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogVoiceServerUpdate(
                FormatObject(serverUpate));
        }

        // Null endpoint means that the current voice server failed.
        // Removing from the cache, also stop and destroy all currently active players.
        if (string.IsNullOrEmpty(serverUpate.Endpoint))
        {
            _voiceServers.Remove(serverUpate.GuildID);
            if (_playersCache.Remove(serverUpate.GuildID, out IPlayer? player))
            {
                player.Dispose();
            }
        }
        else
        {
            // Cache guild's voice server.
            if (_voiceServers.TryGetValue(serverUpate.GuildID, out VoiceServer? cachedVoiceServer))
            {
                _voiceServers[serverUpate.GuildID] = cachedVoiceServer with
                {
                    Token = serverUpate.Token,
                    Endpoint = serverUpate.Endpoint
                };
            }
            else
            {
                _voiceServers.Add(
                    serverUpate.GuildID,
                    new VoiceServer(
                        Token: serverUpate.Token, 
                        Endpoint: serverUpate.Endpoint));
            }
        }

        _voiceSemaphore.Release();

        return Result.Success;
    }
}
