using System;
using System.Globalization;
using UnityEngine;

namespace RudderStack.Unity
{
    public class RudderStackInitializer : MonoBehaviour
    {
        [SerializeField] private string dataPlaneUrl;
        [SerializeField] private string writeKey;
        [Space]
        [SerializeField] private string encryptionKey = "b14ca5898a4e4133bbce2ea2315a1916";

        private void Start()
        {
            DontDestroyOnLoad(new GameObject("UnityMainThread").AddComponent<UnityMainThread>());
            
            if (!string.IsNullOrEmpty(dataPlaneUrl) && !string.IsNullOrEmpty(writeKey))
            {
                RSAnalytics.Initialize(writeKey, new RSConfig(dataPlaneUrl: dataPlaneUrl).SetAutoCollectAdvertId(true));
                RSFailureRequestManager.Init(RSAnalytics.Client, encryptionKey);
            }
        }

        private void OnDestroy()
        {
            RSFailureRequestManager.Stop();
        }
    }
}