using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NodaTime;
using RudderStack.Model;
using RudderStack.Stats;
using UnityEngine;

namespace RudderStack.Unity
{
    public class RSClient
    {
        private const string AnonIdKey = "rudderstack-anon-id";
        private const string UserIdKey = "rudderstack-user-id";
        private const string TraitsKey = "rudderstack-user-traits";
        
        private static string _advertisingId;
        private static string _deviceToken;
        
        private        string _userId;
        private static string _anonymousId;
        
        private IDictionary<string, object> _userTraits;

        public string UserId
        {
            get => _userId;
            private set
            {
                _userId = value;
                PlayerPrefs.SetString(UserIdKey, value);
            }
        }

        public IDictionary<string, object> UserTraits
        {
            get => _userTraits;
            private set
            {
                _userTraits = value;
                PlayerPrefs.SetString(TraitsKey, JsonConvert.SerializeObject(value));
            }
        }
        
        public static string AnonymousId
        {
            get => _anonymousId;
            private set
            {
                _anonymousId = value;
                PlayerPrefs.SetString(AnonIdKey, value);
            }
        }

        public RudderClient Inner { get; }

        public Statistics Statistics
        {
            get => Inner.Statistics;
            set => Inner.Statistics = value;
        }

        public event FailedHandler Failed
        {
            add => Inner.Failed += value;
            remove => Inner.Failed -= value;
        }

        public event SucceededHandler Succeeded
        {
            add => Inner.Succeeded += value;
            remove => Inner.Succeeded -= value;
        }

        public RSClient(RudderClient innerClient, RSConfig config)
        {
            Inner      = innerClient;
            UserId     = PlayerPrefs.GetString(UserIdKey);
            UserTraits = JsonConvert.DeserializeObject<IDictionary<string, object>>(PlayerPrefs.GetString(TraitsKey));
            Config     = config;

            if (PlayerPrefs.HasKey(AnonIdKey))
                _anonymousId = PlayerPrefs.GetString(AnonIdKey);
            else
                AnonymousId = Guid.NewGuid().ToString();
        }
        
        public string WriteKey
        {
            get => Inner.WriteKey;
        }

        public RSConfig Config
        {
            get;
            private set;
        }

        public void Identify(string userId) =>
            Identify(userId, null, new RSOptions());
        
        public void Identify(string userId, IDictionary<string, object> traits) =>
            Identify(userId, traits, new RSOptions());

        public void Identify(string userId, IDictionary<string, object> traits, RSOptions options)
        {
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(AnonymousId))
            {
                Logger.Error("Please supply a valid userId to Identify.");
                return;
            }

            UserId     = userId;
            UserTraits = traits;
            
            SetAdditionalValues(options);
            Inner.Identify(userId, traits, options.Inner);
        }


        public void Group(string groupId) =>
            Group(groupId, null, new RSOptions());
        
        public void Group(string groupId, RSOptions options) =>
            Group(groupId, null, options);

        public void Group(string groupId, IDictionary<string, object> traits) =>
            Group(groupId, traits, new RSOptions());

        public void Group(string groupId, IDictionary<string, object> traits, RSOptions options)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                Logger.Error("Please supply a valid groupId to call #Group.");
                return;
            }
            if (string.IsNullOrEmpty(UserId) && string.IsNullOrEmpty(AnonymousId))
            {
                Logger.Error("Please supply a valid userId or anonymousId to call #Group.");
                return;
            }
            SetAdditionalValues(options);
            Inner.Group(UserId, groupId, traits, options.Inner);
        }

        public void Track(string eventName) =>
            Track(eventName, null, new RSOptions());

        public void Track(string eventName, IDictionary<string, object> properties) =>
            Track(eventName, properties, new RSOptions());

        public void Track(string eventName, RSOptions options) =>
            Track(eventName, null, new RSOptions());

        public void Track(string eventName, IDictionary<string, object> properties, RSOptions options)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Logger.Error("Please supply a valid event to call #Track.");
                return;
            }
            if (string.IsNullOrEmpty(UserId) && string.IsNullOrEmpty(AnonymousId))
            {
                Logger.Error("Please supply a valid userId or anonymousId to call #Track.");
                return;
            }

            SetAdditionalValues(options);
            Inner.Track(UserId, eventName, properties, options.Inner);
        }

        public void Alias(string newId) =>
            Alias(newId, new RSOptions());

        public void Alias(string newId, RSOptions options)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                Logger.Error("The previous 'userId' is not valid.");
                return;
            }
            if (string.IsNullOrEmpty(newId))
            {
                Logger.Error("Please supply a valid 'userId' to Alias.");
                return;
            }

            SetAdditionalValues(options);
            
            Inner.Alias(UserId, newId, options.Inner);
            UserId = newId;
        }

        public void Page(string name) =>
            Page(name, null, null, new RSOptions());

        public void Page(string name, RSOptions options) =>
            Page(name, null, null, options);

        public void Page(string name, string category) =>
            Page(name, category, null, new RSOptions());

        public void Page(string name, IDictionary<string, object> properties) =>
            Page(name, null, properties, new RSOptions());

        public void Page(string name, IDictionary<string, object> properties, RSOptions options) =>
            Page(name, null, properties, options);

        public void Page(string name, string category, IDictionary<string, object> properties, RSOptions options)
        {
            if (string.IsNullOrEmpty(name))
            {
                Logger.Error("Please supply a valid name to call #Page.");
                return;
            }
            if (string.IsNullOrEmpty(UserId) && string.IsNullOrEmpty(AnonymousId))
            {
                Logger.Error("Please supply a valid userId or anonymousId to call #Page.");
                return;
            }

            SetAdditionalValues(options);
            Inner.Page(UserId, name, category, properties, options.Inner);
        }

        public void Screen(string name) =>
            Screen(name, null, null, new RSOptions());

        public void Screen(string name, RSOptions options) =>
            Screen(name, null, null, options);

        public void Screen(string name, string category) =>
            Screen(name, category, null, new RSOptions());

        public void Screen(string name, IDictionary<string, object> properties) =>
            Screen(name, null, properties, new RSOptions());

        public void Screen(string name, IDictionary<string, object> properties, RSOptions options) =>
            Screen(name, null, properties, options);

        public void Screen(string name, string category, IDictionary<string, object> properties, RSOptions options)
        {
            if (string.IsNullOrEmpty(name))
            {
                Logger.Error("Please supply a valid name to call #Screen.");
                return;
            }

            if (string.IsNullOrEmpty(UserId) && string.IsNullOrEmpty(AnonymousId))
            {
                Logger.Error("Please supply a valid userId or anonymousId to call #Page.");
                return;
            }
            SetAdditionalValues(options);
            Inner.Screen(UserId, name, category, properties, options.Inner);
        }

        private void SetAdditionalValues(RSOptions options)
        {
            var osName = SystemInfo.operatingSystem;
            
            var device = new Dict()
            {
                { "name", SystemInfo.deviceName },
                { "model", SystemInfo.deviceModel },
                { "type", osName.Substring(0, osName.IndexOf(' ')) },
                { "id", SystemInfo.deviceUniqueIdentifier },
                { "adTrackingEnabled", Config.GetAutoCollectAdvertId() },
            };

            if (!string.IsNullOrEmpty(_deviceToken)) 
                device["token"] = _deviceToken;
                
            if (!string.IsNullOrEmpty(_advertisingId)) 
                device["advertisingId"] = _advertisingId;
            
            options.Context["device"] = device;
            
            options.Context["screen"] = new Dict
            {
                { "density", UnityEngine.Screen.dpi },
                { "width", UnityEngine.Screen.width },
                { "height", UnityEngine.Screen.height },
            };
            
            options.Context["app"] = new Dict
            {
                { "name", Application.productName },
                //{ "build", UnityEngine.Screen.width },
                //{ "namespace", UnityEngine.Screen.height },
                { "version", Application.version },
            };
                
            options.Context["os"] = new Dict
            {
                { "name", osName },
            };
                
            options.Context["library"] = new Dict
            {
                { "name", "rudder-unity-library" },
                { "version", RSAnalytics.VERSION },
            };

            options.Context["timezone"] = DateTimeZoneProviders.Tzdb.GetSystemDefault().Id;

            if (UserTraits != null)
            {
                options.Context.Add("traits", UserTraits);
            }
            options.SetAnonymousId(AnonymousId);
        }

        public void Flush() => Inner.Flush();

        public Task FlushAsync() => Inner.FlushAsync();

        public void Dispose() => Inner.Dispose();

        /// <summary>
        /// Set the AdvertisingId yourself. If set, SDK will not capture idfa automatically
        /// <b>Call this method before initializing the RudderClient</b>
        /// </summary>
        /// <param name="advertisingId">IDFA for the device</param>
        public static void PutAdvertisingId(string advertisingId)
        {
            _advertisingId = advertisingId;
        }
        
        /// <summary>
        /// Set the push token for the device to be passed to the downstream destinations
        /// </summary>
        /// <param name="deviceToken">Push Token from FCM</param>
        public static void PutDeviceToken(string deviceToken)
        {
            _deviceToken = deviceToken;
        }

        public static void PutAnonymousId(string newId)
        {
            AnonymousId = newId;
        }

        public void Reset()
        {
            //Flush();
            FlushAsync().GetAwaiter().OnCompleted(() =>
            {
                //AnonymousId = Guid.NewGuid().ToString();
            
                PlayerPrefs.DeleteKey(UserIdKey);
                PlayerPrefs.DeleteKey(TraitsKey);
                _userTraits = null;
                _userId     = null;
            });
        }
    }
}