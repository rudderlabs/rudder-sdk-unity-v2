using System;

namespace RudderStack.Unity
{
    public class RSConfig : RudderConfig
    {
        private bool _autoCollectAdvertId;
        private bool _recordScreenViews;
        private bool _trackLifeCycleEvents = true;

        public RSConfig
        (
            string    dataPlaneUrl        = "https://hosted.rudderlabs.com",
            string    proxy               = null,
            TimeSpan? timeout             = null,
            int       maxQueueSize        = 10000,
            int       flushAt             = 20,
            bool      async               = true,
            int       threads             = 1,
            double    flushInterval       = 10.0,
            bool      gzip                = true,
            bool      send                = true,
            string    userAgent           = null,
            TimeSpan? maxRetryTime        = null,
            bool      autoCollectAdvertId = false
        ) : base(dataPlaneUrl, proxy, timeout, maxQueueSize, flushAt, async, threads, flushInterval, gzip, send,
            userAgent, maxRetryTime)
        {
            _autoCollectAdvertId = autoCollectAdvertId;
        }

        public RSConfig SetTrackLifeCycleEvents(bool track)
        {
            _trackLifeCycleEvents = track;
            return this;
        }

        public bool GetTrackLifeCycleEvents()
        {
            return _trackLifeCycleEvents;
        }
        
        public RSConfig SetRecordScreenViews(bool record)
        {
            _recordScreenViews = record;
            return this;
        }

        public bool GetRecordScreenViews()
        {
            return _recordScreenViews;
        }

        public RSConfig SetAutoCollectAdvertId(bool newSendStatus)
        {
            _autoCollectAdvertId = newSendStatus;
            return this;
        }

        public bool GetAutoCollectAdvertId()
        {
            return _autoCollectAdvertId;
        }

        /// <summary>
        /// Set the API host server address, instead of default server "https://hosted.rudderlabs.com"
        /// </summary>
        /// <param name="host">Host server url</param>
        /// <returns></returns>
        public new RSConfig SetHost(string host)
        {
            this.DataPlaneUrl = host;
            return this;
        }

        /// <summary>
        /// Set the proxy server Uri
        /// </summary>
        /// <param name="proxy">Proxy server Uri</param>
        /// <returns></returns>
        public new RSConfig SetProxy(string proxy)
        {
            this.Proxy = proxy;
            return this;
        }

        /// <summary>
        /// Sets the maximum amount of timeout on the HTTP request flushes to the server.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public new RSConfig SetTimeout(TimeSpan timeout)
        {
            this.Timeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the maximum amount of retry time for request to flush to the server when Timeout or error occurs.
        /// </summary>
        /// <param name="maxRetryTime"></param>
        /// <returns></returns>
        public new RSConfig SetMaxRetryTime(TimeSpan maxRetryTime)
        {
            this.MaxRetryTime = maxRetryTime;
            return this;
        }

        /// <summary>
        /// Sets the maximum amount of items that can be in the queue before no more are accepted.
        /// </summary>
        /// <param name="maxQueueSize"></param>
        /// <returns></returns>
        public new RSConfig SetMaxQueueSize(int maxQueueSize)
        {
            this.MaxQueueSize = maxQueueSize;
            return this;
        }

        /// <summary>
        /// Sets the maximum amount of messages to send per batch
        /// </summary>
        /// <param name="flushAt"></param>
        /// <returns></returns>
        public new RSConfig SetFlushAt(int flushAt)
        {
            this.FlushAt = flushAt;
            return this;
        }

        /// <summary>
        /// Count of concurrent internal threads to post data from queue
        /// </summary>
        /// <param name="threads"></param>
        /// <returns></returns>
        public new RSConfig SetThreads(int threads)
        {
            Threads = threads;
            return this;
        }

        /// <summary>
        /// Sets whether the flushing to the server is synchronous or asynchronous.
        ///
        /// True is the default and will allow your calls to Analytics.Client.Identify(...), Track(...), etc
        /// to return immediately and to be queued to be flushed on a different thread.
        ///
        /// False is convenient for testing but should not be used in production. False will cause the
        /// HTTP requests to happen immediately.
        ///
        /// </summary>
        /// <param name="async">True for async flushing, false for blocking flushing</param>
        /// <returns></returns>
        public new RSConfig SetAsync(bool async)
        {
            this.Async = async;
            return this;
        }

        /// <summary>
        /// Sets the API request header uses GZip option.
        /// Enable this when the network is the bottleneck for your application (typically in client side applications).
        /// If useGZip is set, it compresses request content with GZip algorithm
        /// </summary>
        /// <param name="gzip">True to compress request header, false for no compression</param>
        /// <returns></returns>
        public new RSConfig SetGzip(bool gzip)
        {
            this.Gzip = gzip;
            return this;
        }

        public new RSConfig SetUserAgent(string userAgent)
        {
            this.UserAgent = userAgent;
            return this;
        }

        /// <summary>
        /// Don’t send data to RudderStack
        /// </summary>
        /// <param name="send"></param>
        /// <returns></returns>
        public new RSConfig SetSend(bool send)
        {
            this.Send = send;
            return this;
        }

        /// <summary>
        /// Set the interval in seconds at which the client should flush events. 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public new RSConfig SetFlushInterval(double interval)
        {
            base.SetFlushInterval(interval);
            return this;
        }
    }
}