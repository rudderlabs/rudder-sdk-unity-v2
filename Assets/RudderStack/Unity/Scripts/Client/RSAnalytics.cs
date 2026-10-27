using System;
using System.Collections;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using RudderStack.Flush;
using UnityEngine;

namespace RudderStack.Unity
{
    public class RSAnalytics
    {
        private static RSClient _client;
        private static CancellationTokenSource _initialization;
        public static string VERSION;

        static RSAnalytics()
        {
            var package = JsonConvert.DeserializeObject<RSPackage>(Resources.Load("package").ToString());
            if (package != null)
                VERSION = package.Version;
            else
                Debug.LogError("Cannot read package.json in Resources. Please, reimport the project to restore this file");
        }

        public static RSClient Client
        {
            get => _client;
            private set => _client ??= value;
        }

        public static void Initialize(string writeKey, RSConfig config)
        {
            RSLifecycleEvents.Instance.StartCoroutine(InitializeRoutine(writeKey, config));
        }

        public static IEnumerator InitializeRoutine(string writeKey, RSConfig config)
        {
            if (_client != null)
                yield break;

            while (_initialization != null)
            {
                var pending = _initialization;
                var pendingToken = pending.Token;
                yield return new WaitUntil(() => !ReferenceEquals(_initialization, pending));
                if (pendingToken.IsCancellationRequested || _client != null)
                    yield break;
            }

            if (string.IsNullOrEmpty(writeKey) || config is null || string.IsNullOrEmpty(config.Inner.GetHost()))
                throw new InvalidOperationException("Please supply a valid writeKey and config to initialize.");
            if (!Uri.TryCreate(config.GetControlPlaneUrl(), UriKind.Absolute, out var controlPlaneUri) ||
                (controlPlaneUri.Scheme != Uri.UriSchemeHttp && controlPlaneUri.Scheme != Uri.UriSchemeHttps))
                throw new ArgumentException("Please supply a valid HTTP(S) control plane URL.");

            var cancellation = new CancellationTokenSource();
            _initialization = cancellation;
            RSSourceConfigFetcher fetcher = null;
            try
            {
                fetcher = new RSSourceConfigFetcher(config, writeKey);
                var sourceConfigTask = fetcher.FetchAsync(cancellation.Token);
                yield return new WaitUntil(() => sourceConfigTask.IsCompleted || cancellation.IsCancellationRequested);
                if (cancellation.IsCancellationRequested || _client != null)
                    yield break;

                var status = sourceConfigTask.GetAwaiter().GetResult();
                if (status == RSSourceConfigStatus.InvalidWriteKey)
                {
                    Debug.LogError("Wrong write key");
                    RudderAnalytics.Initialize(writeKey, config.Inner, new RSOfflineFlushHandler(config));
                    Client = new RSClient(RudderAnalytics.Client, config);
                    yield break;
                }
                if (status == RSSourceConfigStatus.Disabled)
                    throw new InvalidOperationException("The RudderStack source is disabled!");
                if (status == RSSourceConfigStatus.InvalidResponse)
                    throw new InvalidOperationException("RudderStack: Invalid source configuration response.");

                IRSRequestHandler requestHandler = !config.Inner.Send
                    ? (IRSRequestHandler)new RSFakeRequestHandler()
                    : config.Inner.GetMaxRetryTime().HasValue
                        ? new RSRequestHandler(config.Inner.GetTimeout(),
                            new RSBacko(Convert.ToInt32(config.Inner.GetMaxRetryTime().Value.TotalSeconds) * 1000))
                        : new RSRequestHandler(config.Inner.GetTimeout());

                var recovering = status == RSSourceConfigStatus.RetryableFailure;
                if (recovering)
                {
                    Debug.LogWarning("RudderStack source configuration is temporarily unavailable. Events will be queued while configuration is retried.");
                    requestHandler = new RSRecoveringRequestHandler(requestHandler, fetcher);
                    fetcher = null; // The recovering handler owns the fetcher until enabled, rejected or disposed.
                }

                IBatchFactory batchFactory = new SimpleBatchFactory(writeKey);
                // Pending configuration must use persistence, including for a blocking inner configuration.
                IAsyncFlushHandler flushHandler = config.Inner.GetAsync() || recovering
                    ? (IAsyncFlushHandler)new RSFlushHandler(batchFactory, requestHandler, config)
                    : new BlockingFlushHandler(batchFactory, requestHandler);

                RudderAnalytics.Initialize(writeKey, config.Inner, flushHandler);
                requestHandler.Init(RudderAnalytics.Client, new HttpClient());
                Client = new RSClient(RudderAnalytics.Client, config);
                (flushHandler as RSFlushHandler)?.Start();
            }
            finally
            {
                fetcher?.Dispose();
                if (ReferenceEquals(_initialization, cancellation))
                    _initialization = null;
                cancellation.Dispose();
            }
        }

        public static void Initialize(RSClient client)
        {
            RudderAnalytics.Initialize(client.Inner);
            Client = new RSClient(RudderAnalytics.Client, client.Config);
        }

        public static void Dispose()
        {
            _initialization?.Cancel();
            _initialization = null;
            RudderAnalytics.Dispose();
            _client = null;
        }
    }
}
