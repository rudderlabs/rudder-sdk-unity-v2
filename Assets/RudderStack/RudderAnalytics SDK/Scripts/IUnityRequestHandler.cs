#if NET35
#else
using System.Net.Http;
using System.Net.Http.Headers;
#endif
using System;
using RudderStack.Model;
using RudderStack.Request;

namespace RudderStack.Unity
{
    public interface IUnityRequestHandler : IRequestHandler
    {
        event Action<Batch, bool> BatchCompleted;
        
#if NET35
        public void Init(RudderClient client, IHttpClient httpClient);
#else
        public void Init(RudderClient client, HttpClient httpClient);
#endif
    }
}