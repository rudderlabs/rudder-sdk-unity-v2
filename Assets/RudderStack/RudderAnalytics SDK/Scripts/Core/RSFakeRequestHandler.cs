#if NET35
#else
using System.Net.Http;
using System.Net.Http.Headers;
#endif
using System;
using System.Threading.Tasks;
using RudderStack.Model;

namespace RudderStack.Unity
{

    public class RSFakeRequestHandler : IRSRequestHandler
    {
        public event Action<Batch, BatchResult> BatchCompleted;

        private RudderClient _client;

        public async Task MakeRequest(Batch batch)
        {
            foreach (var action in batch.batch)
            {
                _client.Statistics.IncrementSucceeded();
                _client.RaiseSuccess(action);
            }

            BatchCompleted?.Invoke(batch, BatchResult.Success);
        }
        
#if NET35
        public void Init(RudderClient client, IHttpClient httpClient) => _client = client;
#else
        public void Init(RudderClient client, HttpClient httpClient) => _client = client;
#endif

    }
}