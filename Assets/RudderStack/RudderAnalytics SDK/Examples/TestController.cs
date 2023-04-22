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
            RSAnalytics.Initialize("2OmDuHamX06zSuHObnMf8QQbvSW",
                new RSConfig(dataPlaneUrl: "https://rudderstacz.dataplane.rudderstack.com")
                    .SetAutoCollectAdvertId(true)
                    .SetGzip(true)
                    .SetRecordScreenViews(true));

        public void IdentifyUser()
        {
            RSAnalytics.Client.Identify("ios_unity_user_id", new Dict());
        }

        public void AliasUser()    => RSAnalytics.Client.Alias("new_ios_unity_user_id");
        public void ResetUser()    => RSAnalytics.Client.Reset();

        public void Track()
        {
            count++;
            RSAnalytics.Client.Track(
                $"Track {count}",
                new Dictionary<string, object> { { "key_1", "value_1" }, });
        }

        public void Page()
        {
            count++;
            RSAnalytics.Client.Page(
                $"Page {count}",
                new Dictionary<string, object> { { "key_1", "value_1" }, });
        }

        public void Screen()
        {
            count++;
            RSAnalytics.Client.Screen(
                $"Screen {count}",
                new Dictionary<string, object> { { "key_1", "value_1" }, });
        }

        public void Group()
        {
            RSAnalytics.Client.Group("group_id", new Dict());
        }

        public void SwitchScene() => SceneManager.LoadScene("RudderStack/RudderAnalytics SDK/Examples/Example 1");

        public void SwitchBack() => SceneManager.LoadScene("RudderStack/RudderAnalytics SDK/Examples/Example");
    }
}