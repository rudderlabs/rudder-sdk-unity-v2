using System;

namespace RudderStack.Unity
{
    public class RsConfig : RudderConfig
    {
        private bool autoCollectAdvertId;

        public RsConfig(
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
            this.autoCollectAdvertId = autoCollectAdvertId;
        }

        public RsConfig SetAutoCollectAdvertId(bool newSendStatus)
        {
            autoCollectAdvertId = newSendStatus;
            return this;
        }

        public bool GetAutoCollectAdvertId() => autoCollectAdvertId;
    }
}