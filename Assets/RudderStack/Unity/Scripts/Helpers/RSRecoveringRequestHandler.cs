using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RudderStack.Model;

namespace RudderStack.Unity
{
    // The existing persistent flush queue owns all events throughout recovery. No batch is
    // passed to the data plane until the control plane has explicitly enabled the source.
    internal sealed class RSRecoveringRequestHandler : IRSRequestHandler, IDisposable
    {
        private readonly IRSRequestHandler _inner;
        private readonly RSSourceConfigFetcher _fetcher;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private readonly CancellationToken _token;
        private readonly Func<TimeSpan> _elapsed;
        private TimeSpan _nextAttempt;
        private int _retrySeconds = 1;
        private RSSourceConfigStatus _status = RSSourceConfigStatus.RetryableFailure;
        private volatile bool _disposed;

        public event Action<Batch, BatchResult> BatchCompleted;

        internal bool CanMakeRequest => !_disposed && (_status == RSSourceConfigStatus.Enabled ||
            _status == RSSourceConfigStatus.RetryableFailure && _elapsed() >= _nextAttempt);

        internal RSRecoveringRequestHandler(IRSRequestHandler inner, RSSourceConfigFetcher fetcher,
            Func<TimeSpan> elapsed = null)
        {
            _inner = inner;
            _fetcher = fetcher;
            _token = _cancellation.Token;
            var clock = Stopwatch.StartNew();
            _elapsed = elapsed ?? (() => clock.Elapsed);
            ScheduleRetry();
            _inner.BatchCompleted += OnBatchCompleted;
        }

        public void Init(RudderClient client, HttpClient httpClient) => _inner.Init(client, httpClient);

        public async Task MakeRequest(Batch batch)
        {
            if (_disposed) return;

            if (_status == RSSourceConfigStatus.RetryableFailure && _elapsed() >= _nextAttempt)
            {
                try
                {
                    _status = await _fetcher.FetchAsync(_token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (_token.IsCancellationRequested)
                {
                    return;
                }

                if (_disposed) return;
                if (_status == RSSourceConfigStatus.RetryableFailure)
                    ScheduleRetry();
                else
                {
                    _fetcher.Dispose();
                    if (_status != RSSourceConfigStatus.Enabled)
                        Logger.Error("Source configuration rejected (" + _status + "). Events remain queued; delivery is disabled.");
                }
            }

            if (_status == RSSourceConfigStatus.Enabled)
                await _inner.MakeRequest(batch).ConfigureAwait(false);
            else
                BatchCompleted?.Invoke(batch, BatchResult.Fail);
        }

        private void ScheduleRetry()
        {
            _nextAttempt = _elapsed().Add(TimeSpan.FromSeconds(_retrySeconds));
            _retrySeconds = Math.Min(_retrySeconds * 2, 60);
        }

        private void OnBatchCompleted(Batch batch, BatchResult result) => BatchCompleted?.Invoke(batch, result);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _cancellation.Cancel();
            _cancellation.Dispose();
            _inner.BatchCompleted -= OnBatchCompleted;
            _fetcher.Dispose();
            (_inner as IDisposable)?.Dispose();
        }
    }
}
