﻿using DJWatermelon.AudioService.Lavalink.Models;
using DJWatermelon.AudioService.Lavalink.Models.REST;
using DJWatermelon.AudioService.Lavalink.Models.WebSocket;
using DJWatermelon.AudioService.Lavalink.Models.WebSocket.EventPayloads;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway;
using Remora.Rest.Core;
using Remora.Rest.Extensions;
using Remora.Rest.Json;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DJWatermelon.AudioService.Lavalink;

internal sealed class LavalinkPlayersManager : IPlayersManager, IAsyncDisposable
{
    private const string VersionPrefix = "/v4";

    private readonly ILogger<LavalinkPlayersManager> _logger;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly LavalinkOptions _options;
    private readonly VoiceStatesService _voiceStates;

    private readonly IDictionary<Snowflake, IPlayer> _playersCache =
        new ConcurrentDictionary<Snowflake, IPlayer>();

    private readonly TaskCompletionSource<bool> _readyTaskCompletionSource;
    private bool _disposed;

    private readonly ILavalinkAPI _lavalinkAPI;

    private readonly IUser _bot;

    private readonly ReadOnlyDictionary<Snowflake, IPlayer> _readonlyPlayersCache;

    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new(LavalinkModelsSourceGenerationContext.Default.Options)
        {
            Converters =
            {
                new SnowflakeConverter(Constants.DiscordEpoch)
            }
        };

    public LavalinkPlayersManager(
        ILogger<LavalinkPlayersManager> logger,
        IHostEnvironment hostEnvironment,
        IOptions<LavalinkOptions> options,
        VoiceStatesService voiceStates,
        IDiscordRestUserAPI userAPI)
    {
        _logger = logger;
        _hostEnvironment = hostEnvironment;
        _options = options.Value;
        _voiceStates = voiceStates;

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
        _readonlyPlayersCache = new(_playersCache);
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

        using CancellationTokenSource linkedCancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            await LifecycleAsync(linkedCancellationTokenSource.Token).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _readyTaskCompletionSource.TrySetException(exception);
            throw;
        }
    }

    public bool TryGetPlayer(Snowflake id, [NotNullWhen(true)] out IPlayer? player)
    {
        return _playersCache.TryGetValue(id, out player);
    }

    public IEnumerable<IPlayer> GetPlayers()
    {
        return _readonlyPlayersCache.Values;
    }

    #region WebSocket

    private async ValueTask ProcessPayloadAsync(
        IWebSocketPayload payload, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogReceivedPayload();

        if (string.IsNullOrEmpty(SessionId))
        {
            _logger.LogPayloadReceivedBeforeReady();
            return;
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

        if (payload is EventPayload eventPayload)
        {
            await ProcessEventAsync(eventPayload, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (payload is PlayerUpdatePayload playerUpdatePayload)
        {
            if (!TryGetPlayer(playerUpdatePayload.GuildID, out IPlayer? player))
            {
                _logger.LogPayloadForInexistentPlayer(playerUpdatePayload.GuildID.ToString());
                return;
            }

            LavalinkTrackHandle? curentTrack = player.CurrentTrack as LavalinkTrackHandle?;
            if (curentTrack != null)
            {
                player.CurrentTrack = curentTrack.Value with
                {
                    Info = curentTrack.Value.Info with
                    {
                        CurrentPosition = playerUpdatePayload.State.Position
                    }
                };
            }
        }
    }

    private async ValueTask ProcessEventAsync(
        EventPayload payload, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryGetPlayer(payload.GuildID, out IPlayer? player))
        {
            _logger.LogPayloadForInexistentPlayer(payload.GuildID.ToString());
            return;
        }

        ValueTask eventProcessTask = payload switch
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
                ProcessWebSocketClosedEventAsync(closedEvent, cancellationToken),

            _ => throw new InvalidOperationException(),
        };

        await eventProcessTask.ConfigureAwait(false);
    }

    private async ValueTask ProcessTrackStartEventAsync(
        IPlayer player,
        TrackStartEventPayload trackStartEvent,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogTrackStartEvent(trackStartEvent.ToString());

        if (player.SendMessageAsync != null)
        {
            await player.SendMessageAsync(new IEmbed[]
            {
                new Embed(
                    Type: EmbedType.Link,
                    Title: "Playing track", 
                    Url: trackStartEvent.Track.Info.Uri?.ToString() ?? string.Empty,
                    Timestamp: DateTimeOffset.Now,
                    Colour: Color.Green)
            });
        }
    }

    private async ValueTask ProcessTrackEndEventAsync(
        IPlayer player,
        TrackEndEventPayload trackEndEvent,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogTrackEndEvent(trackEndEvent.ToString());

        if (trackEndEvent.Reason.MayStartNext())
        {
            await player.PlayQueued();
        }
    }

    private async ValueTask ProcessTrackExceptionEventAsync(
        IPlayer player,
        TrackExceptionEventPayload trackExceptionEvent,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogTrackExceptionEvent(trackExceptionEvent.ToString());

        if (player.SendMessageAsync != null)
        {
            await player.SendMessageAsync(new IEmbed[]
            {
                new Embed(
                    Type: EmbedType.Article,
                    Title: "An exception occurred",
                    Description:
                        "Unfortunately, there was an error playing " +
                        $"the track `{trackExceptionEvent.Track.Title}`. " +
                        $"Moving on to the next track in the queue.",
                    Timestamp: DateTimeOffset.UtcNow,
                    Colour: Color.Green)
                });
        }

        await player.PlayQueued();
    }

    private async ValueTask ProcessTrackStuckEventAsync(
        IPlayer player,
        TrackStuckEventPayload trackStuckEvent,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogTrackStuckEvent(trackStuckEvent.ToString());

        if (trackStuckEvent.ExceededThreshold > 3000)
        {
            if (player.SendMessageAsync != null)
            {
                await player.SendMessageAsync(new IEmbed[]
                {
                    new Embed(
                        Type: EmbedType.Article,
                        Title: "Track stuck",
                        Description:
                            $"Unhappily, track '{trackStuckEvent.Track.Title}' stuck, so " +
                            "starting the next track in the queue.",
                        Timestamp: DateTimeOffset.UtcNow,
                        Colour: Color.Green)
                    });
            }

            await player.PlayQueued();
        }
    }

    private ValueTask ProcessWebSocketClosedEventAsync(
        WebSocketClosedEventPayload webSocketClosedEvent,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        WebSocketCloseStatus closeCode = (WebSocketCloseStatus)webSocketClosedEvent.Code;

        throw new Exception($"The WebSocket connection was " +
            $"closed with the following code: <{closeCode}>.");
    }

    private async Task LifecycleAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

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

        _readyTaskCompletionSource.SetResult(true);
        _logger.LogWebSocketConnectionEstablished();

        // Payload processing.
        Memory<byte> buffer = GC.AllocateUninitializedArray<byte>(4 * 1024);
        while (!cancellationToken.IsCancellationRequested)
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

            IWebSocketPayload? payload = JsonSerializer.Deserialize<IWebSocketPayload>(
                utf8Json: buffer[..receiveResult.Count].Span,
                options: _jsonSerializerOptions);

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

        if (_playersCache.ContainsKey(guildID))
        {
            throw new Exception("The player already exists for a given guild.");
        }

        _logger.LogCreatingPlayer(guildID.Value);

        (bool isVoiceServerSet, VoiceServer? voiceServer) =
            await _voiceStates.RetrieveVoiceServerAsync(guildID, cancellationToken);

        if (!isVoiceServerSet)
        {
            throw new Exception("Not able to retrieve a voice server for the given guild ID.");
        }

        // Expliptical check for Endpoint presence.
        if (string.IsNullOrEmpty(voiceServer!.Endpoint))
        {
            throw new Exception("Not able to proceed because of " +
                "unavailability of the guild's voice server. ");
        }

        (bool isVoiceSessionSet, VoiceSession? voiceSession) =
            await _voiceStates.RetrieveVoiceSessionAsync(voiceChannelID, cancellationToken);

        if (!isVoiceSessionSet)
        {
            throw new Exception("Can not retrieve a voice " +
                "session for the given channel ID.");
        }

        _ = await _lavalinkAPI.UpateOrCreatePlayerAsync(
            sessionID: SessionId,
            guildID: guildID.Value,
            playerUpdate: new PlayerUpdateModel(
                Volume: 50,
                VoiceState: new VoiceStateModel(
                    Token: voiceServer!.Token,
                    Endpoint: voiceServer!.Endpoint,
                    SessionId: voiceSession!.SessionId)));

        LavalinkPlayer player = new(guildID, this);
        _playersCache.Add(guildID, player);

        return player;
    }

    public async Task DestroyPlayerAsync(
        Snowflake guildID, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(SessionId);

        if (_playersCache.ContainsKey(guildID))
        {
            _playersCache.Remove(guildID);
            await _lavalinkAPI.DestroyPlayerAsync(SessionId, guildID.Value);
        }
        else
        {
            throw new Exception("No player was found with the " +
                "corresponding guild ID to destroy.");
        }
    }

    public async Task<IEnumerable<ITrackHandle>> SearchForTrackAsync(
        string prompt,
        CancellationToken cancellationToken)
    {
        TrackLoadResultModel result = await _lavalinkAPI.LoadTracksAsync(prompt);
        return result.ResultType == LoadResultTypes.Error
            ? throw new Exception($"Lavalink returned a search error.\n\n{result.Data}")
            : result.ResultType switch
            {
                LoadResultTypes.Track =>
                    new ITrackHandle[]
                    {
                        JsonSerializer.Deserialize<LavalinkTrackHandle>(result.Data)
                    },

                LoadResultTypes.Playlist =>
                    JsonSerializer.Deserialize<PlaylistResultDataModel>(result.Data)!.Tracks
                        .Select(p => (ITrackHandle)p),

                LoadResultTypes.Search =>
                    JsonSerializer.Deserialize<IEnumerable<LavalinkTrackHandle>>(result.Data)!
                        .Select(p => (ITrackHandle)p),

                LoadResultTypes.Empty => Array.Empty<ITrackHandle>(),

                _ => throw new Exception("Invalid value for LoadResultTypes enum.")
            };
    }

    #endregion

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_disposed)
        {
            return ValueTask.CompletedTask;
        }

        _disposed = true;

        _readyTaskCompletionSource.TrySetCanceled();

        _logger.LogLavalinkDisposed();

        return ValueTask.CompletedTask;
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
}