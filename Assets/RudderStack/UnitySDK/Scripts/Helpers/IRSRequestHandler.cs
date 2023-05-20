#if NET35
#else
using System.Net.Http;
#endif
using System;
using RudderStack.Model;
using RudderStack.Request;

namespace RudderStack.Unity
{
    public enum BatchResult
    {
        Success,
        Fail,
        WrongKey,
    }
    
    public interface IRSRequestHandler : IRequestHandler
    {
        event Action<Batch, BatchResult> BatchCompleted;
        
#if NET35
        public void Init(RudderClient client, IHttpClient httpClient);
#else
        public void Init(RudderClient client, HttpClient httpClient);
#endif
    }
}