using System;

namespace RudderStack.Unity
{
    public class RSConfig
    {
        private string       _controlPlaneUrl       = "https://api.rudderlabs.com";
        private bool         _recordScreenViews     = false;
        private int          _dbThresholdCount      = 10000;
        private Logger.Level _logLevel              = Logger.Level.INFO;
        private bool         _trackLifeCycleEvents  = true;

        internal RudderConfig Inner { get; private set; }


        public RSConfig
        (
            string          dataPlaneUrl            = "https://hosted.rudderlabs.com",
            string          controlPlaneUrl         = "https://api.rudderlabs.com",
            int             flushQueueSize          = 30,
            double          sleepCount              = 10.0,
            bool            gzip                    = true,
            int             dbThresholdCount        = 10000,
            bool            recordScreenViews       = false,
            Logger.Level    logLevel                = Logger.Level.INFO,
            bool            trackLifeCycleEvents    = true
        )
        {
            Inner = new RudderConfig(
                dataPlaneUrl,
                null,
                null,
                flushQueueSize,
                20,
                true,
                1,
                sleepCount,
                gzip,
                true,
                null,
                null);
            _controlPlaneUrl = controlPlaneUrl;
            _recordScreenViews = recordScreenViews;
            _dbThresholdCount = dbThresholdCount;
            _logLevel = logLevel;
            _trackLifeCycleEvents = trackLifeCycleEvents;
        }

        public RSConfig SetDbThresholdCount(int count)
        {
            _dbThresholdCount = count;
            return this;
        }

        internal int GetDbThresholdCount() => _dbThresholdCount;

        public RSConfig SetDataPlaneUrl(string url)
        {
            Inner.SetHost(url);
            return this;
        }

        internal string GetDataPlaneUrl() => Inner.GetHost();

        public RSConfig SetControlPlaneUrl(string url)
        {
            _controlPlaneUrl = url;
            return this;
        }

        internal string GetControlPlaneUrl() => _controlPlaneUrl;

        public RSConfig SetLogLevel(Logger.Level level)
        {
            _logLevel = level;
            return this;
        }

        internal Logger.Level GetLogLevel() => _logLevel;

        public RSConfig SetTrackLifeCycleEvents(bool track)
        {
            _trackLifeCycleEvents = track;
            return this;
        }

        internal bool GetTrackLifeCycleEvents() => _trackLifeCycleEvents;

        public RSConfig SetRecordScreenViews(bool record)
        {
            _recordScreenViews = record;
            return this;
        }

        internal bool GetRecordScreenViews() => _recordScreenViews;


        /// <summary>
        /// Sets the maximum amount of items that can be in the queue before no more are accepted.
        /// </summary>
        /// <param name="flushQueueSize"></param>
        /// <returns></returns>
        public RSConfig SetFlushQueueSize(int flushQueueSize)
        {
            Inner.SetMaxQueueSize(flushQueueSize);
            return this;
        }

        internal int GetFlushQueueSize() => Inner.GetMaxQueueSize();

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

        internal bool GetGzip() => Inner.GetGzip();

        /// <summary>
        /// Set the interval in seconds at which the client should flush events. 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public RSConfig SetSleepCount(double count)
        {
            Inner.SetFlushInterval(count);
            return this;
        }

        internal double GetSleepCount() => Inner.GetFlushInterval();
    }
}