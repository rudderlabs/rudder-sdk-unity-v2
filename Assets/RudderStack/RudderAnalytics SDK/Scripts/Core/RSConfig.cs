using System;

namespace RudderStack.Unity
{
    public class RSConfig
    {
        private string       _controlPlaneUrl;
        private bool         _autoCollectAdvertId;
        private bool         _recordScreenViews;
        private int          _dbThresholdCount     = 1000;
        private Logger.Level _logLevel             = Logger.Level.INFO;
        private bool         _trackLifeCycleEvents = true;

        public RudderConfig Inner { get; private set; }


        public RSConfig
        (
            string    dataPlaneUrl        = "https://hosted.rudderlabs.com",
            string    proxy               = null,
            TimeSpan? timeout             = null,
            int       maxQueueSize        = 30,
            int       flushAt             = 20,
            bool      async               = true,
            int       threads             = 1,
            double    flushInterval       = 10.0,
            bool      gzip                = true,
            bool      send                = true,
            string    userAgent           = null,
            TimeSpan? maxRetryTime        = null,
            bool      autoCollectAdvertId = false
        )
        {
            Inner = new RudderConfig(
                dataPlaneUrl,
                proxy,
                timeout,
                maxQueueSize,
                flushAt,
                async,
                threads,
                flushInterval,
                gzip,
                send,
                userAgent,
                maxRetryTime);
            _autoCollectAdvertId = autoCollectAdvertId;
        }

        public RSConfig(RudderConfig config)
        {
            Inner = config;
        }

        public RSConfig SetDbThresholdCount(int count)
        {
            _dbThresholdCount = count;
            return this;
        }

        public int GetDbThresholdCount() => _dbThresholdCount;
        
        public RSConfig SetControlPlaneUrl(string url)
        {
            _controlPlaneUrl = url;
            return this;
        }

        public string GetControlPlaneUrl() => _controlPlaneUrl;

        public RSConfig SetLogLevel(Logger.Level level)
        {
            _logLevel = level;
            return this;
        }

        public Logger.Level GetLogLevel() => _logLevel;

        public RSConfig SetTrackLifeCycleEvents(bool track)
        {
            _trackLifeCycleEvents = track;
            return this;
        }

        public bool GetTrackLifeCycleEvents() => _trackLifeCycleEvents;

        public RSConfig SetRecordScreenViews(bool record)
        {
            _recordScreenViews = record;
            return this;
        }

        public bool GetRecordScreenViews() => _recordScreenViews;

        public RSConfig SetAutoCollectAdvertId(bool collect)
        {
            _autoCollectAdvertId = collect;
            return this;
        }

        public bool GetAutoCollectAdvertId() => _autoCollectAdvertId;

        /// <summary>
        /// Set the API host server address, instead of default server "https://hosted.rudderlabs.com"
        /// </summary>
        /// <param name="host">Host server url</param>
        /// <returns></returns>
        public RSConfig SetHost(string host)
        {
            Inner.SetHost(host);
            return this;
        }
        public string GetHost() => Inner.GetHost();

        /// <summary>
        /// Set the proxy server Uri
        /// </summary>
        /// <param name="proxy">Proxy server Uri</param>
        /// <returns></returns>
        public RSConfig SetProxy(string proxy)
        {
            Inner.SetProxy(proxy);
            return this;
        }
        public string GetProxy() => Inner.GetProxy();

        /// <summary>
        /// Sets the maximum amount of timeout on the HTTP request flushes to the server.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public RSConfig SetTimeout(TimeSpan timeout)
        {
            Inner.SetTimeout(timeout);
            return this;
        }
        public TimeSpan GetTimeout() => Inner.GetTimeout();

        /// <summary>
        /// Sets the maximum amount of retry time for request to flush to the server when Timeout or error occurs.
        /// </summary>
        /// <param name="maxRetryTime"></param>
        /// <returns></returns>
        public RSConfig SetMaxRetryTime(TimeSpan maxRetryTime)
        {
            Inner.SetMaxRetryTime(maxRetryTime);
            return this;
        }
        public TimeSpan? GetMaxRetryTime() => Inner.GetMaxRetryTime();

        /// <summary>
        /// Sets the maximum amount of items that can be in the queue before no more are accepted.
        /// </summary>
        /// <param name="maxQueueSize"></param>
        /// <returns></returns>
        public RSConfig SetFlushQueueSize(int maxQueueSize)
        {
            Inner.SetMaxQueueSize(maxQueueSize);
            return this;
        }

        public int GetFlushQueueSize() => Inner.GetMaxQueueSize();

        /// <summary>
        /// Sets the maximum amount of messages to send per batch
        /// </summary>
        /// <param name="flushAt"></param>
        /// <returns></returns>
        public RSConfig SetFlushAt(int flushAt)
        {
            Inner.SetFlushAt(flushAt);
            return this;
        }
        public int GetFlushAt() => Inner.GetFlushAt();

        /// <summary>
        /// Count of concurrent internal threads to post data from queue
        /// </summary>
        /// <param name="threads"></param>
        /// <returns></returns>
        public RSConfig SetThreads(int threads)
        {
            Inner.SetThreads(threads);
            return this;
        }
        public int GetThreads() => Inner.GetThreads();

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
        public RSConfig SetAsync(bool async)
        {
            Inner.SetAsync(async);
            return this;
        }
        public bool GetAsync() => Inner.GetAsync();

        /// <summary>
        /// Sets the API request header uses GZip option.
        /// Enable this when the network is the bottleneck for your application (typically in client side applications).
        /// If useGZip is set, it compresses request content with GZip algorithm
        /// </summary>
        /// <param name="gzip">True to compress request header, false for no compression</param>
        /// <returns></returns>
        public RSConfig SetGzip(bool gzip)
        {
            Inner.SetGzip(gzip);
            return this;
        }
        public bool GetGzip() => Inner.GetGzip();

        public RSConfig SetUserAgent(string userAgent)
        {
            Inner.SetUserAgent(userAgent);
            return this;
        }
        public string GetUserAgent() => Inner.GetUserAgent();

        /// <summary>
        /// Don’t send data to RudderStack
        /// </summary>
        /// <param name="send"></param>
        /// <returns></returns>
        public RSConfig SetSend(bool send)
        {
            Inner.SetSend(send);
            return this;
        }
        public bool GetSend() => Inner.GetSend();

        /// <summary>
        /// Set the interval in seconds at which the client should flush events. 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public RSConfig SetSleepCount(double interval)
        {
            Inner.SetFlushInterval(interval);
            return this;
        }

        public double GetSleepCount() => Inner.GetFlushInterval();
    }
}