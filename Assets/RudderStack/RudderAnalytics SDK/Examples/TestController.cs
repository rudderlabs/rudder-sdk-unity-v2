using System.Collections.Generic;
using RudderStack.Model;
using RudderStack.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Examples.RudderStack.Unity
{

    public class TestController : MonoBehaviour
    {
        private int count = 0;

        private void Start()
        {
            Initialize();
        }

        public void PutAnonymousId() => RSAnalytics.Client.SetAnonymousId("anonymous_id");

        public void PutAdvertisingId() => RSClient.PutAdvertisingId("ios_advertisement_id");

        public void PutDeviceToken() => RSClient.PutDeviceToken("ios_device_token");

        public void Initialize() =>
            StartCoroutine(RSAnalytics.Initialize("2OmDuHamX06zSuHObnMf8QQbvSW",
                new RSConfig(dataPlaneUrl: "https://rudderstacz.dataplane.rudderstack.com")
                    .SetTrackLifeCycleEvents(false)));

        public void IdentifyUser()
        {
            RudderOptions rudderOptions = new RudderOptions();
            rudderOptions.SetIntegration("Firebase", true);

            RSAnalytics.Client.Identify("ios_unity_user_id",
                new Dictionary<string, object> { { "key_1", "value_1" }, { "key_2", 4 }, { "key_3", 4.2 }, { "key_4", true } },
                rudderOptions);
        }

        public void AliasUser()
        {
            RudderOptions rudderOptions = new RudderOptions();
            rudderOptions.SetIntegration("Firebase", true);

            RSAnalytics.Client.Alias("new_ios_unity_user_id", rudderOptions);
        }

        public void ResetUser()    => RSAnalytics.Client.Reset();

        public void Track()
        {
            count++;
            RudderOptions rudderOptions = new RudderOptions();
            rudderOptions.SetIntegration("Firebase", true);

            RSAnalytics.Client.Track(
                $"Track {count}",
                new Dictionary<string, object> { { "key_1", "value_1" }, },
                rudderOptions);
        }

        public void Page()
        {
            count++;
            RudderOptions rudderOptions = new RudderOptions();
            rudderOptions.SetIntegration("Firebase", true);

            RSAnalytics.Client.Page(
                $"Page {count}",
                new Dictionary<string, object> { { "key_1", "value_1" }, { "key_2", 4 }, { "key_3", 4.2 }, { "key_4", true } },
                rudderOptions);
        }

        public void Screen()
        {
            count++;
            RudderOptions rudderOptions = new RudderOptions();
            rudderOptions.SetIntegration("Firebase", true);

            RSAnalytics.Client.Screen(
                $"Screen {count}",
                new Dictionary<string, object> { { "key_1", "value_1" }, { "key_2", 4 }, { "key_3", 4.2 }, { "key_4", true } });
        }

        public void Group()
        {
            RudderOptions rudderOptions = new RudderOptions();
            rudderOptions.SetIntegration("Firebase", true);

            RSAnalytics.Client.Group("group_id",
                new Dictionary<string, object> { { "key_1", "value_1" }, { "key_2", 4 }, { "key_3", 4.2 }, { "key_4", true } },
                rudderOptions);
        }

        public void SwitchScene() => SceneManager.LoadScene("RudderStack/RudderAnalytics SDK/Examples/Example 1");

        public void SwitchBack() => SceneManager.LoadScene("RudderStack/RudderAnalytics SDK/Examples/Example");
    }
}