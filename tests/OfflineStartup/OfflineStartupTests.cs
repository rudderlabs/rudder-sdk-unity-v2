using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RudderStack.Unity;
using RudderStack.Unity.Utility;
using UnityEngine;

namespace OfflineStartupTests
{
    [TestFixture, NonParallelizable]
    public class InitializationTests
    {
        private string _directory;

        [SetUp]
        public void SetUp()
        {
            RSAnalytics.Dispose();
            PlayerPrefs.Reset();
            _directory = Path.Combine(Path.GetTempPath(), "rudder-unity-tests-" + Guid.NewGuid());
            UnityEngine.Device.Application.persistentDataPath = _directory;
        }

        [TearDown]
        public void TearDown()
        {
            RSAnalytics.Dispose();
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task OfflineStartupPreservesEventsAndRecoversWithoutReplacingClient(bool connectionRefused)
        {
            using var server = new LoopbackServer();
            if (!connectionRefused)
            {
                server.ConfigStatus = 503;
                server.Start();
            }
            var config = Config(server);
            await RunRoutine(RSAnalytics.InitializeRoutine("test-key", config));
            var client = RSAnalytics.Client;
            Assert.That(client, Is.Not.Null);
            var succeeded = new ConcurrentQueue<string>();
            client.Succeeded += action => succeeded.Enqueue(action.MessageId);
            client.Track("queued offline");
            client.Track("also queued offline");
            var storedIds = StoredActions().Select(action => action.MessageId).ToArray();
            Assert.That(storedIds, Has.Length.EqualTo(2));
            Assert.That(server.Events, Is.Empty);

            server.ConfigStatus = 200;
            if (connectionRefused) server.Start();
            await WaitFor(() => succeeded.Count == 2);
            Assert.That(RSAnalytics.Client, Is.SameAs(client));
            Assert.That(server.Events.Select(e => (string)e["messageId"]), Is.EquivalentTo(storedIds));
            Assert.That(StoredActions(), Is.Empty);

            client.Track("after recovery");
            client.Flush();
            await WaitFor(() => succeeded.Count == 3);
            Assert.That(server.Events.Select(e => (string)e["event"]), Is.EquivalentTo(new[]
                { "queued offline", "also queued offline", "after recovery" }));
        }

        [Test]
        public async Task StoredOfflineEventsSurviveDisposalAndNextStartup()
        {
            using var server = new LoopbackServer { ConfigStatus = 503 };
            server.Start();
            await RunRoutine(RSAnalytics.InitializeRoutine("test-key", Config(server)));
            RSAnalytics.Client.Track("retained across restart");
            var id = StoredActions().Single().MessageId;
            RSAnalytics.Dispose();
            Assert.That(StoredActions().Single().MessageId, Is.EqualTo(id));

            server.ConfigStatus = 200;
            await RunRoutine(RSAnalytics.InitializeRoutine("test-key", Config(server)));
            await WaitFor(() => server.Events.Count == 1 && StoredActions().Count == 0);
            Assert.That((string)server.Events.Single()["messageId"], Is.EqualTo(id));
        }

        [Test]
        public async Task ConcurrentInitializationAndRepeatedFlushesCreateOnlyOneDeliveryPipeline()
        {
            using var server = new LoopbackServer();
            var releaseConfig = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            server.BeforeConfigResponse = () => releaseConfig.Task;
            server.Start();
            var config = Config(server);
            var first = RunRoutine(RSAnalytics.InitializeRoutine("test-key", config));
            await WaitFor(() => server.ConfigRequests == 1);
            var second = RunRoutine(RSAnalytics.InitializeRoutine("test-key", config));
            releaseConfig.SetResult(true);
            await Task.WhenAll(first, second);
            Assert.That(server.ConfigRequests, Is.EqualTo(1));

            var successes = 0;
            RSAnalytics.Client.Succeeded += _ => Interlocked.Increment(ref successes);
            RSAnalytics.Client.Track("one event");
            Parallel.For(0, 100, _ => RSAnalytics.Client.Flush());
            await WaitFor(() => Volatile.Read(ref successes) == 1);
            await RunRoutine(RSAnalytics.InitializeRoutine("test-key", config));
            Assert.That(server.ConfigRequests, Is.EqualTo(1));
            Assert.That(server.Events, Has.Count.EqualTo(1));
        }

        [TestCase(401, "")]
        [TestCase(200, "{\"source\":{\"enabled\":false}}")]
        [TestCase(200, "malformed")]
        public async Task RecoveryRejectionPreservesQueueWithoutSending(int status, string body)
        {
            using var server = new LoopbackServer { ConfigStatus = 503 };
            server.Start();
            await RunRoutine(RSAnalytics.InitializeRoutine("test-key", Config(server)));
            RSAnalytics.Client.Track("must not send");
            server.ConfigBody = body;
            server.ConfigStatus = status;
            await WaitFor(() => server.ConfigRequests >= 2);
            await Task.Delay(100);
            Assert.That(server.Events, Is.Empty);
            Assert.That(StoredActions(), Has.Count.EqualTo(1));
        }

        [Test]
        public async Task DisposeDuringConfigurationDoesNotPublishClient()
        {
            using var server = new LoopbackServer();
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            server.BeforeConfigResponse = () => release.Task;
            server.Start();
            var pending = RunRoutine(RSAnalytics.InitializeRoutine("test-key", Config(server)));
            await WaitFor(() => server.ConfigRequests == 1);
            RSAnalytics.Dispose();
            release.SetResult(true);
            await pending;
            Assert.That(RSAnalytics.Client, Is.Null);
            Assert.That(server.Events, Is.Empty);
        }

        [Test]
        public async Task InitializeEntryPointSchedulesSameSafeCoroutine()
        {
            using var server = new LoopbackServer { ConfigStatus = 503 };
            server.Start();
            RSAnalytics.Initialize("test-key", Config(server));
            await RunRoutine(RSLifecycleEvents.Instance.ScheduledRoutine);
            Assert.That(RSAnalytics.Client, Is.Not.Null);
        }

        [TestCase("{\"source\":{\"enabled\":false}}")]
        [TestCase("malformed")]
        public void InitialConfigurationRejectionRemainsAnError(string body)
        {
            using var server = new LoopbackServer { ConfigBody = body };
            server.Start();
            Assert.That(async () => await RunRoutine(RSAnalytics.InitializeRoutine("test-key", Config(server))),
                Throws.TypeOf<InvalidOperationException>());
            Assert.That(RSAnalytics.Client, Is.Null);
            Assert.That(server.Events, Is.Empty);
        }

        [Test]
        public async Task InvalidWriteKeyRetainsExistingOfflineBehavior()
        {
            using var server = new LoopbackServer { ConfigStatus = 401 };
            server.Start();
            await RunRoutine(RSAnalytics.InitializeRoutine("invalid-test-key", Config(server)));
            RSAnalytics.Client.Track("not authorized");
            RSAnalytics.Client.Flush();
            Assert.That(StoredActions(), Has.Count.EqualTo(1));
            Assert.That(server.ConfigRequests, Is.EqualTo(1));
            Assert.That(server.Events, Is.Empty);
        }

        [Test]
        public async Task OfflineQueueRespectsStorageLimit()
        {
            using var server = new LoopbackServer { ConfigStatus = 503 };
            server.Start();
            await RunRoutine(RSAnalytics.InitializeRoutine("test-key", Config(server).SetDbThresholdCount(2)));
            RSAnalytics.Client.Track("first");
            RSAnalytics.Client.Track("second");
            RSAnalytics.Client.Track("over limit");
            Assert.That(StoredActions(), Has.Count.EqualTo(2));
            Assert.That(server.Events, Is.Empty);
        }

        [Test]
        public void FlushAndDisposeCanBeRepeatedWhileWorkerStops()
        {
            for (var i = 0; i < 25; i++)
            {
                var handler = new RSFlushHandler(new RudderStack.Flush.SimpleBatchFactory("test"),
                    new RecordingRequestHandler(), new RSConfig());
                handler.Start();
                handler.Flush();
                handler.Dispose();
                handler.Flush();
                handler.Dispose();
            }
        }

        [Test]
        public async Task FlushSignalsDuringBackoffDoNotRewritePersistence()
        {
            var now = TimeSpan.Zero;
            using var http = new StubHttpHandler((_, __) => Task.FromResult(SourceConfigTests.Response(503, "")));
            var config = new RSConfig();
            var recovering = new RSRecoveringRequestHandler(new RecordingRequestHandler(),
                new RSSourceConfigFetcher(config, "test", new HttpClient(http)), () => now);
            using var handler = new RSFlushHandler(new RudderStack.Flush.SimpleBatchFactory("test"), recovering, config);
            var client = new RudderStack.RudderClient("test", config.Inner, handler);
            client.Track("user", "pending");
            var path = Path.Combine(_directory, "RudderStack", "rs_persistence");
            var modified = File.GetLastWriteTimeUtc(path);
            handler.Start();
            for (var i = 0; i < 20; i++)
            {
                handler.Flush();
                await Task.Delay(5);
            }
            Assert.That(http.Calls, Is.Zero);
            Assert.That(File.GetLastWriteTimeUtc(path), Is.EqualTo(modified));
            Assert.That(StoredActions(), Has.Count.EqualTo(1));
        }

        private static RSConfig Config(LoopbackServer server) =>
            new RSConfig(dataPlaneUrl: server.Url, controlPlaneUrl: server.Url, sleepCount: 1,
                gzip: false, trackLifeCycleEvents: false);

        private static System.Collections.Generic.List<RudderStack.Model.BaseAction> StoredActions() =>
            new RSStorageManager(PlayerPrefs.GetString("04j4bf5dkd8")).LoadFromFile();

        private static async Task RunRoutine(IEnumerator routine)
        {
            try
            {
                while (routine.MoveNext())
                {
                    Assert.That(routine.Current, Is.TypeOf<WaitUntil>());
                    await WaitFor(((WaitUntil)routine.Current).Predicate);
                }
            }
            finally
            {
                (routine as IDisposable)?.Dispose();
            }
        }

        internal static async Task WaitFor(Func<bool> predicate)
        {
            var watch = Stopwatch.StartNew();
            while (!predicate())
            {
                Assert.That(watch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(8)), "Timed out waiting for SDK behavior");
                await Task.Delay(10);
            }
        }
    }

    internal sealed class LoopbackServer : IDisposable
    {
        private readonly HttpListener _listener = new HttpListener();
        private Task _loop;
        private int _configRequests;
        internal int ConfigStatus = 200;
        internal string ConfigBody = "{\"source\":{\"enabled\":true}}";
        internal Func<Task> BeforeConfigResponse;
        internal readonly ConcurrentQueue<JToken> Events = new ConcurrentQueue<JToken>();
        internal int ConfigRequests => Volatile.Read(ref _configRequests);
        internal string Url { get; }

        internal LoopbackServer()
        {
            var reservation = new TcpListener(IPAddress.Loopback, 0);
            reservation.Start();
            Url = "http://127.0.0.1:" + ((IPEndPoint)reservation.LocalEndpoint).Port;
            reservation.Stop();
            _listener.Prefixes.Add(Url + "/");
        }

        internal void Start()
        {
            _listener.Start();
            _loop = Run();
        }

        private async Task Run()
        {
            try
            {
                while (_listener.IsListening)
                {
                    var context = await _listener.GetContextAsync();
                    string body;
                    if (context.Request.Url.AbsolutePath == "/sourceConfig")
                    {
                        Interlocked.Increment(ref _configRequests);
                        if (BeforeConfigResponse != null) await BeforeConfigResponse();
                        context.Response.StatusCode = ConfigStatus;
                        body = ConfigBody;
                    }
                    else
                    {
                        using var reader = new StreamReader(context.Request.InputStream);
                        var batch = JObject.Parse(await reader.ReadToEndAsync());
                        foreach (var action in batch["batch"]) Events.Enqueue(action);
                        body = "{}";
                    }
                    var bytes = Encoding.UTF8.GetBytes(body);
                    context.Response.ContentLength64 = bytes.Length;
                    await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                    context.Response.Close();
                }
            }
            catch (HttpListenerException) when (!_listener.IsListening) { }
            catch (ObjectDisposedException) { }
        }

        public void Dispose()
        {
            _listener.Close();
            _loop?.GetAwaiter().GetResult();
        }
    }
}
