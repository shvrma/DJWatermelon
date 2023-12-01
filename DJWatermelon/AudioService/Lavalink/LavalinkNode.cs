using System;
using System.Buffers;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DJWatermelon.AudioService.Lavalink.EventPayloads;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DJWatermelon.AudioService.Lavalink;

internal sealed class LavalinkNode : IAsyncDisposable
{
    private readonly ILogger<LavalinkNode> _logger;
    private readonly PlayersManager<LavalinkPlayer> _playersManager;

    private readonly CancellationTokenSource _shutdownCancellationTokenSource;
    private readonly CancellationToken _shutdownCancellationToken;
    private readonly Stopwatch _readyStopwatch;

    private TaskCompletionSource<string> _readyTaskCompletionSource;
    private Task? _executeTask;
    private bool _disposed;

    public LavalinkNode(
        ILogger<LavalinkNode> logger,
        PlayersManager<LavalinkPlayer> playersManager)
    {
        _logger = logger;
        _playersManager = playersManager;

        _readyTaskCompletionSource = new TaskCompletionSource<string>(
            creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);

        _readyStopwatch = new Stopwatch();

        _shutdownCancellationTokenSource = new CancellationTokenSource();
        _shutdownCancellationToken = _shutdownCancellationTokenSource.Token;
    }

    public bool IsReady => _readyTaskCompletionSource.Task.IsCompletedSuccessfully;

    public string? SessionId { get; private set; }

    public async ValueTask WaitForReadyAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        await _readyTaskCompletionSource.Task
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);
    }

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

    #region Event processing
    private async ValueTask ProcessEventAsync(IEventPayload payload, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(payload);

        if (_playersManager.TryGet(payload.GuildId, out var player))
        {
            _logger.LogEventPayloadForInexistentPlayer(payload.GuildId);
            return;
        }

        var task = payload switch
        {
            TrackEndEventPayload trackEvent => ProcessTrackEndEventAsync(player, trackEvent, cancellationToken),
            TrackStartEventPayload trackEvent => ProcessTrackStartEventAsync(player, trackEvent, cancellationToken),
            TrackStuckEventPayload trackEvent => ProcessTrackStuckEventAsync(player, trackEvent, cancellationToken),
            TrackExceptionEventPayload trackEvent => ProcessTrackExceptionEventAsync(player, trackEvent, cancellationToken),
            WebSocketClosedEventPayload closedEvent => ProcessWebSocketClosedEventAsync(player, closedEvent, cancellationToken),
            _ => ValueTask.CompletedTask,
        };

        await task.ConfigureAwait(false);
    }

    private async ValueTask ProcessPayloadAsync(IPayload payload, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(payload);

        _logger.LogReceivedPayload(SerializePayload(payload));

        if (payload is ReadyPayload readyPayload)
        {
            if (!_readyTaskCompletionSource.TrySetResult(readyPayload.SessionId))
            {
                // _logger.MultipleReadyPayloadsReceived(Label);
            }

            SessionId = readyPayload.SessionId;

            // Enable resuming, if wanted
            if (_options.ResumptionOptions.IsEnabled && !readyPayload.SessionResumed)
            {
                var sessionUpdateProperties = new SessionUpdateProperties
                {
                    IsSessionResumptionEnabled = true,
                    Timeout = _options.ResumptionOptions.Timeout.Value,
                };

                await _apiClient
                    .UpdateSessionAsync(readyPayload.SessionId, sessionUpdateProperties, cancellationToken)
                    .ConfigureAwait(false);
            }

            _logger.Ready(Label, SessionId);
        }

        if (SessionId is null)
        {
            _logger.PayloadReceivedBeforeReadyPayload(Label);
            return;
        }

        if (payload is IEventPayload eventPayload)
        {
            await ProcessEventAsync(eventPayload, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (payload is PlayerUpdatePayload playerUpdatePayload)
        {
            var player = await _serviceContext.PlayerManager
                .GetPlayerAsync(playerUpdatePayload.GuildId, cancellationToken)
                .ConfigureAwait(false);

            if (player is null)
            {
                _logger.ReceivedPlayerUpdatePayloadForNonRegisteredPlayer(Label, playerUpdatePayload.GuildId);
                return;
            }

            if (player is ILavalinkPlayerListener playerListener)
            {
                var state = playerUpdatePayload.State;

                await playerListener
                    .NotifyPlayerUpdateAsync(state.AbsoluteTimestamp, state.Position, state.IsConnected, state.Latency, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        if (payload is StatisticsPayload statisticsPayload)
        {
            await ProcessStatisticsPayloadAsync(statisticsPayload, cancellationToken).ConfigureAwait(false);
        }
    }

    private async ValueTask ProcessStatisticsPayloadAsync(StatisticsPayload statisticsPayload, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(statisticsPayload);

        // For now ignore statistic payloads.
    }

    private async ValueTask ProcessTrackEndEventAsync(ILavalinkPlayer player, TrackEndEventPayload trackEndEvent, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(trackEndEvent);

        var track = CreateTrack(trackEndEvent.Track);

        if (player is ILavalinkPlayerListener playerListener)
        {
            await playerListener
                .NotifyTrackEndedAsync(track, trackEndEvent.Reason, cancellationToken)
                .ConfigureAwait(false);
        }

        var eventArgs = new TrackEndedEventArgs(
            player: player,
            track: track,
            reason: trackEndEvent.Reason);

        await _serviceContext.NodeListener
            .OnTrackEndedAsync(eventArgs, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask ProcessTrackExceptionEventAsync(ILavalinkPlayer player, TrackExceptionEventPayload trackExceptionEvent, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(trackExceptionEvent);

        var track = CreateTrack(trackExceptionEvent.Track);

        var exception = new TrackException(
            Severity: trackExceptionEvent.Exception.Severity,
            Message: trackExceptionEvent.Exception.Message,
            Cause: trackExceptionEvent.Exception.Cause);

        if (player is ILavalinkPlayerListener playerListener)
        {
            await playerListener
                .NotifyTrackExceptionAsync(track, exception, cancellationToken)
                .ConfigureAwait(false);
        }

        var eventArgs = new TrackExceptionEventArgs(
            player: player,
            track: track,
            exception: exception);

        await _serviceContext.NodeListener
            .OnTrackExceptionAsync(eventArgs, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask ProcessTrackStartEventAsync(ILavalinkPlayer player, TrackStartEventPayload trackStartEvent, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(trackStartEvent);

        var track = CreateTrack(trackStartEvent.Track);

        if (player is ILavalinkPlayerListener playerListener)
        {
            await playerListener
                .NotifyTrackStartedAsync(track, cancellationToken)
                .ConfigureAwait(false);
        }

        var eventArgs = new TrackStartedEventArgs(
            player: player,
            track: track);

        await _serviceContext.NodeListener
            .OnTrackStartedAsync(eventArgs, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask ProcessTrackStuckEventAsync(ILavalinkPlayer player, TrackStuckEventPayload trackStuckEvent, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(trackStuckEvent);

        var track = CreateTrack(trackStuckEvent.Track);

        if (player is ILavalinkPlayerListener playerListener)
        {
            await playerListener
                .NotifyTrackStuckAsync(track, trackStuckEvent.ExceededThreshold, cancellationToken)
                .ConfigureAwait(false);
        }

        var eventArgs = new TrackStuckEventArgs(
            player: player,
            track: track,
            threshold: trackStuckEvent.ExceededThreshold);

        await _serviceContext.NodeListener
            .OnTrackStuckAsync(eventArgs, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask ProcessWebSocketClosedEventAsync(ILavalinkPlayer player, WebSocketClosedEventPayload webSocketClosedEvent, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(webSocketClosedEvent);

        var closeCode = (WebSocketCloseStatus)webSocketClosedEvent.Code;

        if (player is ILavalinkPlayerListener playerListener)
        {
            await playerListener
                .NotifyWebSocketClosedAsync(closeCode, webSocketClosedEvent.Reason, webSocketClosedEvent.WasByRemote, cancellationToken)
                .ConfigureAwait(false);
        }

        var eventArgs = new WebSocketClosedEventArgs(
            player: player,
            closeCode: closeCode,
            reason: webSocketClosedEvent.Reason,
            byRemote: webSocketClosedEvent.WasByRemote);

        await _serviceContext.NodeListener
            .OnWebSocketClosedAsync(eventArgs, cancellationToken)
            .ConfigureAwait(false);
    }

    #endregion

    public async ValueTask RunAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        cancellationToken.ThrowIfCancellationRequested();

        if (_executeTask is not null)
        {
            throw new InvalidOperationException("The node was already started.");
        }

        using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            token1: cancellationToken,
            token2: _shutdownCancellationToken);

        var linkedCancellationToken = cancellationTokenSource.Token;

        try
        {
            _executeTask = ReceiveInternalAsync(linkedCancellationToken);
            await _executeTask.ConfigureAwait(false);
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

    private async Task ReceiveInternalAsync(ClientInformation clientInformation, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        var webSocketUri = _options.WebSocketUri ?? _apiEndpoints.WebSocket;

        _readyStopwatch.Restart();

        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_shutdownCancellationToken);
        using var _ = new CancellationTokenDisposable(cancellationTokenSource);

        while (!_shutdownCancellationToken.IsCancellationRequested)
        {
            if (_readyTaskCompletionSource.Task.IsCompleted)
            {
                // Initiate reconnect
                _readyTaskCompletionSource = new TaskCompletionSource<string>(
                    creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
            }

            using var socket = _serviceContext.LavalinkSocketFactory.Create(Options.Create(socketOptions));
            using var socketCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            using var ___ = new CancellationTokenDisposable(socketCancellationSource);

            if (socket is null)
            {
                break;
            }

            _ = socket.RunAsync(socketCancellationSource.Token).AsTask();

            await ReceiveInternalAsync(socket, cancellationToken).ConfigureAwait(false);
        }
    }

    private async ValueTask ReceiveInternalAsync(ILavalinkSocket socket, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IPayload? payload;
            try
            {
                payload = await socket
                    .ReceiveAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.ExceptionOccurredDuringCommunication(Label, exception);
                continue;
            }

            if (payload is null)
            {
                break;
            }

            try
            {
                await ProcessPayloadAsync(payload, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.ExceptionOccurredWhileProcessingPayload(Label, payload, exception);
            }

            foreach (var (_, integration) in _serviceContext.IntegrationManager)
            {
                try
                {
                    await integration
                        .ProcessPayloadAsync(payload, cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    _logger.ExceptionOccurredWhileExecutingIntegrationHandler(Label, exception);
                }
            }
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _readyTaskCompletionSource.TrySetCanceled();
        _shutdownCancellationTokenSource.Cancel();
        _shutdownCancellationTokenSource.Dispose();

        if (_executeTask is not null)
        {
            try
            {
                await _executeTask.ConfigureAwait(false);
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
