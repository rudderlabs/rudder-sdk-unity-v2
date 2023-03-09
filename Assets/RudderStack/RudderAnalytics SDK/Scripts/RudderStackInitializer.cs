using UnityEngine;

namespace RudderStack.Unity
{
    public class RudderStackInitializer : MonoBehaviour
    {
        [SerializeField] private string dataPlaneUrl;
        [SerializeField] private string writeKey;

        private void Start()
        {
            DontDestroyOnLoad(new GameObject("UnityMainThread").AddComponent<UnityMainThread>());
            if (!string.IsNullOrEmpty(dataPlaneUrl) && !string.IsNullOrEmpty(writeKey))
            {
                RSAnalytics.Initialize(writeKey, new RSConfig(dataPlaneUrl: dataPlaneUrl).SetAutoCollectAdvertId(true));
            }
        }
    }
}