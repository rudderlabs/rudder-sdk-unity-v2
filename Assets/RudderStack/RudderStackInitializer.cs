using System.Collections.Generic;
using UnityEngine;
using RudderStack;

public class RudderStackInitializer : MonoBehaviour
{
    [SerializeField] private string dataPlaneUrl;
    [SerializeField] private string writeKey;
    private void Start()
    {
        RudderAnalytics.Initialize(writeKey, new RudderConfig(dataPlaneUrl: dataPlaneUrl)); 
        var thread = new GameObject("UnityMainThread").AddComponent<UnityMainThread>();
        DontDestroyOnLoad(thread);
    }
}