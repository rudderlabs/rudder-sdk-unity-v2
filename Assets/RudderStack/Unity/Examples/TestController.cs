using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using RudderStack.Model;
using RudderStack.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Logger = RudderStack.Logger;
using System;
using System.Xml.Linq;

namespace Examples.RudderStack.Unity
{

    public class TestController : MonoBehaviour
    {
        public class RSEnv
        {
            public string WRITE_KEY;
            public string PROD_DATA_PLANE_URL;
            public string DEV_DATA_PLANE_URL;
            public string DEV_CONTROL_PLANE_URL;
            public string LOCAL_DATA_PLANE_URL;
        }

        private int count = 0;

        private void Start()
        {
            Initialize();
        }

        public void PutAnonymousId() => RSClient.PutAnonymousId("anonymous_id_3");

        public void PutAdvertisingId() => RSClient.PutAdvertisingId("ios_advertisement_id");

        public void PutDeviceToken() => RSClient.PutDeviceToken("ios_device_token");

        public void Initialize()
        {
            //Copy SAMPLE_ENV.json and rename it to RUDDER_ENV.json under `Assets/RudderStack/Unity/Resources/` folder. Fill the required details.
            RSEnv rsEnv = JsonConvert.DeserializeObject<RSEnv>(Resources.Load("RUDDER_ENV").ToString());

            RSAnalytics.Initialize(rsEnv.WRITE_KEY,
                new RSConfig()
                .SetDataPlaneUrl(rsEnv.PROD_DATA_PLANE_URL)
                //.SetControlPlaneUrl(rsEnv.DEV_CONTROL_PLANE_URL)
                .SetLogLevel(Logger.Level.DEBUG)
                //.SetSleepCount(5)
                //.SetDbThresholdCount(1)
                //.SetGzip(false)
                //.SetTrackLifeCycleEvents(true)
                //.SetRecordScreenViews(false)
            );

        }

        public void IdentifyUser()
        {
            RSOptions rudderOptions = new RSOptions();
            rudderOptions.SetIntegration("Firebase", true);

            //RSAnalytics.Client.Identify("ios_unity_user_id");
            RSAnalytics.Client.Identify("ios_unity_user_id", new Dictionary<string, object> { { "key_1", "value_1" }, { "key_2", 4 }, { "key_3", 4.2 }, { "key_4", true } });
            //RSAnalytics.Client.Identify("ios_unity_user_id", new Dictionary<string, object> { { "key_1", "value_1" }, { "key_2", 4 }, { "key_3", 4.2 }, { "key_4", true } }, rudderOptions);
        }

        public void AliasUser()
        {
            RSOptions rudderOptions = new RSOptions();
            rudderOptions.SetIntegration("Firebase", true);

            RSAnalytics.Client.Alias("new_ios_unity_user_id");
            //RSAnalytics.Client.Alias("new_ios_unity_user_id_2", rudderOptions);
        }

        public void ResetUser()    => RSAnalytics.Client.Reset();

        public void Track()
        {
            count++;
            RSOptions rudderOptions = new RSOptions();
            rudderOptions.SetIntegration("Firebase", true);
            //for (int i = 0; i < 60; i++)
            //{
            //    RSAnalytics.Client.Track($"Track {i}");
            //}

            RSAnalytics.Client.Track($"Track {count}");
            //RSAnalytics.Client.Track($"Track {count}", new Dictionary<string, object> { { "key_1", "value_1" }, });
            //RSAnalytics.Client.Track($"Track {count}", new Dictionary<string, object> { { "key_1", "value_1" }, }, rudderOptions);
        }

        public void Page()
        {
            count++;
            RSOptions rudderOptions = new RSOptions();
            rudderOptions.SetIntegration("Firebase", true);

            RSAnalytics.Client.Page($"Page {count}");
            //RSAnalytics.Client.Page($"Page {count}", new Dictionary<string, object> { { "key_1", "value_1" }, { "key_2", 4 }, { "key_3", 4.2 }, { "key_4", true } });
            //RSAnalytics.Client.Page($"Page {count}", new Dictionary<string, object> { { "key_1", "value_1" }, { "key_2", 4 }, { "key_3", 4.2 }, { "key_4", true } }, rudderOptions);
        }

        public void Screen()
        {
            count++;
            RSOptions rudderOptions = new RSOptions();
            rudderOptions.SetIntegration("Firebase", true);

            RSAnalytics.Client.Screen($"Screen {count}");
            //RSAnalytics.Client.Screen($"Screen {count}", new Dictionary<string, object> { { "key_1", "value_1" }, { "key_2", 4 }, { "key_3", 4.2 }, { "key_4", true } });
            //RSAnalytics.Client.Screen($"Screen {count}", new Dictionary<string, object> { { "key_1", "value_1" }, { "key_2", 4 }, { "key_3", 4.2 }, { "key_4", true } }, rudderOptions);
        }

        public void Group()
        {
            RSOptions rudderOptions = new RSOptions();
            rudderOptions.SetIntegration("Firebase", true);

            RSAnalytics.Client.Group("group_id");
            //RSAnalytics.Client.Group("group_id", new Dictionary<string, object> { { "key_1", "value_1" }, { "key_2", 4 }, { "key_3", 4.2 }, { "key_4", true } });
            //RSAnalytics.Client.Group("group_id", new Dictionary<string, object> { { "key_1", "value_1" }, { "key_2", 4 }, { "key_3", 4.2 }, { "key_4", true } }, rudderOptions);
        }

        public void SwitchScene() => SceneManager.LoadScene("RudderStack/Unity/Examples/Example 1");

        public void SwitchBack() => SceneManager.LoadScene("RudderStack/Unity/Examples/Example");
    }
}