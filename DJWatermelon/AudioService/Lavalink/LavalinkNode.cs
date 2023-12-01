using DJWatermelon.AudioService.Lavalink.EventPayloads;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DJWatermelon.AudioService.Lavalink;

internal sealed class LavalinkNode : IAsyncDisposable
{
    private readonly ILogger<LavalinkNode> _logger;
    private readonly PlayersManager<LavalinkPlayer> _playersManager;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IConfiguration _configuration;
    private readonly LavalinkOptions _options;

    private readonly CancellationToken _shutdownCancellationToken;

    private readonly Stopwatch _readyStopwatch;

    private TaskCompletionSource<string> _readyTaskCompletionSource;
    private Task? _executeTask;
    private bool _disposed;

    public LavalinkNode(
        ILogger<LavalinkNode> logger,
        PlayersManager<LavalinkPlayer> playersManager,
        IHostEnvironment hostEnvironment,
        IConfiguration configuration,
        IOptions<LavalinkOptions> options,
        CancellationToken shutdownCancellationToken)
    {
        _logger = logger;
        _playersManager = playersManager;
        _hostEnvironment = hostEnvironment;
        _configuration = configuration;
        _options = options.Value;
        _shutdownCancellationToken = shutdownCancellationToken;

        _readyTaskCompletionSource = new TaskCompletionSource<string>(
            creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);

        _readyStopwatch = new Stopwatch();
    }

    public bool IsReady => _readyTaskCompletionSource.Task.IsCompletedSuccessfully;

    public string? SessionId { get; private set; }

    private static string SerializePayload(IPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        JsonWriterOptions jsonWriterOptions = new()
        {
            Indented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        ArrayBufferWriter<byte> arrayBufferWriter = new();
        Utf8JsonWriter utf8JsonWriter = new(arrayBufferWriter, jsonWriterOptions);

        JsonSerializer.Serialize(utf8JsonWriter, payload);

        return Encoding.UTF8.GetString(arrayBufferWriter.WrittenSpan);
    }

    #region Payload's processing

    private async ValueTask ProcessPayloadAsync(IPayload payload, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(payload);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogReceivedPayload(SerializePayload(payload));
        }

        if (payload is ReadyPayload readyPayload)
        {
            if (!_readyTaskCompletionSource.TrySetResult(readyPayload.SessionId))
            {
                _logger.LogMultipleReadyPayloadsReceived();
            }

            SessionId = readyPayload.SessionId;

            _logger.LogLavalinkReady();
        }

        if (SessionId is null)
        {
            _logger.LogPayloadReceivedBeforeReady();
            return;
        }

        if (payload is IEventPayload eventPayload)
        {
            await ProcessEventAsync(eventPayload, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (payload is PlayerUpdatePayload playerUpdatePayload)
        {
            if (_playersManager.TryGet(playerUpdatePayload.GuildId, out LavalinkPlayer? player))
            {
                _logger.LogPayloadForInexistentPlayer(playerUpdatePayload.GuildId);
                return;
            }
        }
    }

    private async ValueTask ProcessEventAsync(IEventPayload payload, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(payload);

        if (!_playersManager.TryGet(payload.GuildId, out LavalinkPlayer? player))
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

            _ => ValueTask.CompletedTask,
        };

        await task.ConfigureAwait(false);
    }

    private async ValueTask ProcessTrackEndEventAsync(
        LavalinkPlayer player,
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
        LavalinkPlayer player, 
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
        LavalinkPlayer player, 
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
        LavalinkPlayer player, 
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
        LavalinkPlayer player, 
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

    #endregion

    public async ValueTask RunAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        if (_executeTask is not null)
        {
            throw new InvalidOperationException("The node was already started.");
        }

        using CancellationTokenSource cancellationTokenSource = 
            CancellationTokenSource.CreateLinkedTokenSource(
                token1: cancellationToken,
                token2: _shutdownCancellationToken);

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

    private async Task ReceiveInternalAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        string webSocketUri = _options.WebSocketUri;

        _readyStopwatch.Restart();

        if (_readyTaskCompletionSource.Task.IsCompleted)
        {
            // Initiate reconnect.
            _readyTaskCompletionSource = new TaskCompletionSource<string>(
                creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
        }

        // Init Web Socket.
        using ClientWebSocket webSocket = new();

        webSocket.Options.SetRequestHeader("Authorization", _options.Authorization);
        webSocket.Options.SetRequestHeader("User-Id", _options.UserId);
        webSocket.Options.SetRequestHeader("Client-Name", "DJWatermelonBot");

        HttpClientHandler httpMessageHandler = new();
        HttpMessageInvoker httpMessageInvoker = new(httpMessageHandler, disposeHandler: true);

        if (Uri.TryCreate(webSocketUri, UriKind.Absolute, out Uri? wsUri))
        {
            await webSocket
                .ConnectAsync(wsUri, httpMessageInvoker, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            throw new InvalidOperationException("Bad URI passed.");
        }

        _logger.LogWebSocketConnecttionEstablished();

        // Payload processing.
        Memory<byte> buffer = GC.AllocateUninitializedArray<byte>(4 * 1024);
        while (!_shutdownCancellationToken.IsCancellationRequested)
        {
            ValueWebSocketReceiveResult receiveResult = await webSocket
                .ReceiveAsync(buffer, cancellationToken)
                .ConfigureAwait(false);

            if (!receiveResult.EndOfMessage)
            {
                _logger.LogBadPayloadReceived();
                continue;
            }

            if (receiveResult.MessageType is not WebSocketMessageType.Text)
            {
                if (receiveResult.MessageType is WebSocketMessageType.Close)
                {
                    _logger.LogRemoteHostClosedConnection();
                    return;
                }

                _logger.LogBadPayloadReceived();
                continue;
            }

            IPayload? payload = JsonSerializer.Deserialize<IPayload>(buffer[..receiveResult.Count].Span);
            if (payload == null)
            {
                _logger.LogBadPayloadReceived();
                continue;
            }

            await ProcessPayloadAsync(payload, cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _readyTaskCompletionSource.TrySetCanceled();

        if (_executeTask is not null)
        {
            try
            {
                await _executeTask.ConfigureAwait(false);
            }
            finally
            {
                _logger.LogLavalinkDisposed();
            }
        }
    }
}
