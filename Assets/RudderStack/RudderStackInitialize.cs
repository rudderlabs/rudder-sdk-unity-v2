using System.Collections.Generic;
using UnityEngine;
using RudderStack;

public class RudderStackInitialize : MonoBehaviour
{
    [SerializeField] private string dataPlaneUrl;
    [SerializeField] private string writeKey;
    private void Start()
    {
        RudderAnalytics.Initialize(writeKey, new RudderConfig(dataPlaneUrl: dataPlaneUrl));
    }
}