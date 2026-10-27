using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RudderStack.Unity
{
    internal enum RSSourceConfigStatus
    {
        Enabled,
        Disabled,
        InvalidWriteKey,
        RetryableFailure,
        InvalidResponse
    }

    internal sealed class RSSourceConfigFetcher : IDisposable
    {
        private readonly HttpClient _client;
        private readonly Uri _uri;

        internal RSSourceConfigFetcher(RSConfig config, string writeKey)
            : this(config, writeKey, new HttpClient()) { }

        internal RSSourceConfigFetcher(RSConfig config, string writeKey, HttpClient client)
        {
            _client = client;
            _client.Timeout = config.Inner.GetTimeout();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(writeKey + ":")));
            _uri = new Uri(config.GetControlPlaneUrl().TrimEnd('/') + "/sourceConfig?p=unity&v=" +
                Uri.EscapeDataString(RSAnalytics.VERSION) + "&w=" + Uri.EscapeDataString(writeKey));
        }

        internal async Task<RSSourceConfigStatus> FetchAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var response = await _client.GetAsync(_uri, cancellationToken).ConfigureAwait(false);
                var status = (int)response.StatusCode;
                if (status == 408 || status == 429 || status >= 500 && status <= 599)
                    return RSSourceConfigStatus.RetryableFailure;

                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                    return RSSourceConfigStatus.InvalidWriteKey;

                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (body.IndexOf("Invalid write key", StringComparison.OrdinalIgnoreCase) >= 0)
                    return RSSourceConfigStatus.InvalidWriteKey;

                if (response.StatusCode != HttpStatusCode.OK)
                    return RSSourceConfigStatus.InvalidResponse;

                var enabled = (JObject.Parse(body)["source"] as JObject)?["enabled"];
                if (enabled == null || enabled.Type != JTokenType.Boolean)
                    return RSSourceConfigStatus.InvalidResponse;

                return enabled.Value<bool>() ? RSSourceConfigStatus.Enabled : RSSourceConfigStatus.Disabled;
            }
            catch (HttpRequestException)
            {
                return RSSourceConfigStatus.RetryableFailure;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // HttpClient timeouts are cancellation exceptions; explicit disposal is not a retry.
                return RSSourceConfigStatus.RetryableFailure;
            }
            catch (JsonException)
            {
                return RSSourceConfigStatus.InvalidResponse;
            }
        }

        public void Dispose() => _client.Dispose();
    }
}
