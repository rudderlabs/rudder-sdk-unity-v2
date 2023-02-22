using System.Collections.Generic;
using RudderStack;
using TMPro;
using UnityEngine;
using Logger = RudderStack.Logger;

public class TestController : MonoBehaviour
{
    [Header("Initialize")]
    [SerializeField] private TMP_InputField dataPlaneUrl;
    [SerializeField] private TMP_InputField writeKey;
    [Header("Track")]
    [SerializeField] private TMP_InputField userID;
    [SerializeField] private TMP_InputField eventName;
    [SerializeField] private TMP_InputField propertyType;
    [SerializeField] private TMP_InputField propertyValue;

    public void Initialize()
    {
        RudderAnalytics.Initialize(writeKey.text, new RudderConfig(dataPlaneUrl.text));
        RudderStackLogger.LoggingHandler(Logger.Level.INFO, "RudderAnalytics Initialized", null);
    }
    
    public void Track()
    {
        RudderAnalytics.Client.Track(
            userID.text,
            eventName.text,
            new Dictionary<string, object> { {propertyType.text, propertyValue.text}, }
        );
    }
}
