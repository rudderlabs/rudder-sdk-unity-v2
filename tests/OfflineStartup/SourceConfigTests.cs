using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RudderStack;
using RudderStack.Model;
using RudderStack.Unity;

namespace OfflineStartupTests
{
    [TestFixture]
    public class SourceConfigTests
    {
        [TestCase(200, "{\"source\":{\"enabled\":true}}", RSSourceConfigStatus.Enabled)]
        [TestCase(200, "{\"source\":{\"enabled\":false}}", RSSourceConfigStatus.Disabled)]
        [TestCase(200, "{}", RSSourceConfigStatus.InvalidResponse)]
        [TestCase(200, "null", RSSourceConfigStatus.InvalidResponse)]
        [TestCase(200, "{\"source\":{\"enabled\":\"true\"}}", RSSourceConfigStatus.InvalidResponse)]
        [TestCase(200, "broken json", RSSourceConfigStatus.InvalidResponse)]
        [TestCase(400, "Invalid write key", RSSourceConfigStatus.InvalidWriteKey)]
        [TestCase(401, "", RSSourceConfigStatus.InvalidWriteKey)]
        [TestCase(403, "", RSSourceConfigStatus.InvalidWriteKey)]
        [TestCase(404, "", RSSourceConfigStatus.InvalidResponse)]
        [TestCase(408, "", RSSourceConfigStatus.RetryableFailure)]
        [TestCase(429, "", RSSourceConfigStatus.RetryableFailure)]
        [TestCase(500, "", RSSourceConfigStatus.RetryableFailure)]
        [TestCase(503, "", RSSourceConfigStatus.RetryableFailure)]
        public async Task ClassifiesConfigurationWithoutEnablingOnFailure(int code, string body, object expected)
        {
            using var handler = new StubHttpHandler((_, __) => Task.FromResult(Response(code, body)));
            using var fetcher = Fetcher(handler);
            Assert.That(await fetcher.FetchAsync(CancellationToken.None), Is.EqualTo(expected));
        }

        [Test]
        public async Task TransportFailureAndTimeoutAreRetryable()
        {
            using var transport = new StubHttpHandler((_, __) => throw new HttpRequestException("DNS unavailable"));
            using var fetcher = Fetcher(transport);
            Assert.That(await fetcher.FetchAsync(CancellationToken.None), Is.EqualTo(RSSourceConfigStatus.RetryableFailure));

            using var timeout = new StubHttpHandler(async (_, token) =>
            {
                await Task.Delay(Timeout.Infinite, token);
                return Response(200, "");
            });
            var config = new RSConfig();
            config.Inner.SetTimeout(TimeSpan.FromMilliseconds(50));
            using var timedFetcher = new RSSourceConfigFetcher(config, "test", new HttpClient(timeout));
            Assert.That(await timedFetcher.FetchAsync(CancellationToken.None), Is.EqualTo(RSSourceConfigStatus.RetryableFailure));
        }

        [Test]
        public void ExplicitCancellationIsNotRetried()
        {
            using var handler = new StubHttpHandler(async (_, token) =>
            {
                await Task.Delay(Timeout.Infinite, token);
                return Response(200, "");
            });
            using var fetcher = Fetcher(handler);
            using var cancel = new CancellationTokenSource();
            cancel.Cancel();
            Assert.That(async () => await fetcher.FetchAsync(cancel.Token), Throws.InstanceOf<OperationCanceledException>());
        }

        [Test]
        public async Task RetriesWithCappedBackoffAndStopsFetchingAfterRecovery()
        {
            var now = TimeSpan.Zero;
            var healthy = false;
            using var http = new StubHttpHandler((_, __) => Task.FromResult(healthy
                ? Response(200, "{\"source\":{\"enabled\":true}}") : Response(503, "")));
            var inner = new RecordingRequestHandler();
            using var recovering = new RSRecoveringRequestHandler(inner, Fetcher(http), () => now);
            var batch = new Batch();
            var failures = 0;
            recovering.BatchCompleted += (_, result) => { if (result == BatchResult.Fail) failures++; };
            foreach (var seconds in new[] { 1, 2, 4, 8, 16, 32, 60, 60 })
            {
                var before = http.Calls;
                await recovering.MakeRequest(batch);
                Assert.That(http.Calls, Is.EqualTo(before));
                now = now.Add(TimeSpan.FromSeconds(seconds));
                await recovering.MakeRequest(batch);
                Assert.That(http.Calls, Is.EqualTo(before + 1));
                Assert.That(inner.Batches, Is.Empty);
            }
            healthy = true;
            now = now.Add(TimeSpan.FromSeconds(60));
            await recovering.MakeRequest(batch);
            var callsAfterRecovery = http.Calls;
            await recovering.MakeRequest(batch);
            Assert.That(inner.Batches, Has.Count.EqualTo(2));
            Assert.That(http.Calls, Is.EqualTo(callsAfterRecovery));
            Assert.That(failures, Is.EqualTo(16));
        }

        [TestCase(200, "{\"source\":{\"enabled\":false}}")]
        [TestCase(401, "")]
        [TestCase(403, "")]
        [TestCase(200, "not json")]
        public async Task RejectedRecoveryNeverSendsOrKeepsRetrying(int code, string body)
        {
            var now = TimeSpan.Zero;
            using var http = new StubHttpHandler((_, __) => Task.FromResult(Response(code, body)));
            var inner = new RecordingRequestHandler();
            using var recovering = new RSRecoveringRequestHandler(inner, Fetcher(http), () => now);
            now = now.Add(TimeSpan.FromSeconds(1));
            await recovering.MakeRequest(new Batch());
            now = now.Add(TimeSpan.FromHours(1));
            await recovering.MakeRequest(new Batch());
            Assert.That(http.Calls, Is.EqualTo(1));
            Assert.That(inner.Batches, Is.Empty);
        }

        [Test]
        public async Task DisposalCancelsPendingRecoveryAndPreventsDelivery()
        {
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var http = new StubHttpHandler(async (_, token) =>
            {
                started.SetResult(true);
                await Task.Delay(Timeout.Infinite, token);
                return Response(200, "{\"source\":{\"enabled\":true}}");
            });
            var now = TimeSpan.Zero;
            var inner = new RecordingRequestHandler();
            using var recovering = new RSRecoveringRequestHandler(inner, Fetcher(http), () => now);
            now = now.Add(TimeSpan.FromSeconds(1));
            var pending = recovering.MakeRequest(new Batch());
            await started.Task.WaitAsync(TimeSpan.FromSeconds(2));
            recovering.Dispose();
            await pending.WaitAsync(TimeSpan.FromSeconds(2));
            await recovering.MakeRequest(new Batch());
            Assert.That(inner.Batches, Is.Empty);
        }

        private static RSSourceConfigFetcher Fetcher(HttpMessageHandler handler) =>
            new RSSourceConfigFetcher(new RSConfig(), "test", new HttpClient(handler));

        internal static HttpResponseMessage Response(int code, string body) =>
            new HttpResponseMessage((HttpStatusCode)code) { Content = new StringContent(body) };
    }

    internal sealed class StubHttpHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _send;
        internal int Calls;
        internal StubHttpHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) => _send = send;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Calls++;
            return _send(request, token);
        }
    }

    internal sealed class RecordingRequestHandler : IRSRequestHandler
    {
        public event Action<Batch, BatchResult> BatchCompleted;
        internal readonly List<Batch> Batches = new List<Batch>();
        public void Init(RudderClient client, HttpClient httpClient) { }
        public Task MakeRequest(Batch batch)
        {
            Batches.Add(batch);
            BatchCompleted?.Invoke(batch, BatchResult.Success);
            return Task.CompletedTask;
        }
    }
}
