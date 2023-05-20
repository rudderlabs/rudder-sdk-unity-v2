using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RudderStack.Flush;
using UnityEngine;

namespace RudderStack.Unity
{
    public class RSAnalytics
    {
        private static RSClient _client;
        public static  string   VERSION = "2.0.0.beta.1";

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

            if (string.IsNullOrEmpty(writeKey) || config is null || string.IsNullOrEmpty(config.Inner.GetHost()))
                throw new InvalidOperationException("Please supply a valid writeKey and config to initialize.");

            var sourceConfigTask = FetchConfig(config, writeKey);

            yield return new WaitUntil(() => sourceConfigTask.IsCompleted);

            var sourceConfig = sourceConfigTask.Result;

            if (sourceConfig.HasValue == false)
            {
                Debug.LogError("Wrong write key");
                
                IAsyncFlushHandler flushHandler = new RSOfflineFlushHandler(config);
                RudderAnalytics.Initialize(writeKey, config.Inner, flushHandler);
                Client = new RSClient(RudderAnalytics.Client, config);
            }
            else
            {
                if (sourceConfig.Value.source.enabled == false)
                    throw new InvalidOperationException("The RudderStack source is disabled!");

                IRSRequestHandler requestHandler;
                if (config.Inner.Send)
                {
                    if (config.Inner.GetMaxRetryTime().HasValue)
                        requestHandler = new RSRequestHandler(config.Inner.GetTimeout(),
                            new RSBacko(Convert.ToInt32(config.Inner.GetMaxRetryTime().Value.TotalSeconds) * 1000));
                    else
                        requestHandler = new RSRequestHandler(config.Inner.GetTimeout());
                }
                else
                {
                    requestHandler = new RSFakeRequestHandler();
                }

                IBatchFactory batchFactory = new SimpleBatchFactory(writeKey);

                IAsyncFlushHandler flushHandler = config.Inner.GetAsync()
                    ? new RSFlushHandler(batchFactory, requestHandler, config)
                    : new BlockingFlushHandler(batchFactory, requestHandler);

                RudderAnalytics.Initialize(writeKey, config.Inner, flushHandler);
                Client = new RSClient(RudderAnalytics.Client, config);
                requestHandler.Init(RudderAnalytics.Client, new HttpClient());
            }
        }

        public static void Initialize(RSClient client)
        {
            RudderAnalytics.Initialize(client.Inner);
            Client = new RSClient(RudderAnalytics.Client, client.Config);
        }

        public static void Dispose()
        {
            RudderAnalytics.Dispose();
            Client.Dispose();
        }

        [ItemCanBeNull]
        private static async Task<RSSourceConfig?> FetchConfig(RSConfig config, string writeKey)
        {
            if (string.IsNullOrEmpty(config.GetControlPlaneUrl()))
                throw new ArgumentException($"Invalid URL {config}");

            var uri = $"{config.GetControlPlaneUrl()}/sourceConfig?p=unity&v={VERSION}&w={writeKey}";
            
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", BasicAuthHeader(writeKey, string.Empty));

            var responseStr = "";

            var response = await client.GetAsync(uri).ConfigureAwait(false);
            
            if (response != null && response.Content != null)
                responseStr = await response.Content.ReadAsStringAsync();

            if (System.Text.RegularExpressions.Regex.IsMatch(responseStr, ".*Invalid write key.*"))
                return null;
            if (response != null && response.StatusCode == HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<RSSourceConfig>(responseStr);

            throw new System.Exception($"RudderStack: Error when trying to fetch config. {responseStr}");
        }
        
        private static string BasicAuthHeader(string user, string pass)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}"));
        }
    }
}