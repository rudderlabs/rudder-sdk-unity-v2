using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Logger = RudderStack.Logger;

public class RudderStackLogger : MonoBehaviour
{
    [SerializeField] private TMP_Text log;
    [SerializeField] private bool writeInUnityConsole;
    private static TMP_Text s_log;
    private static bool s_writeInUnityConsole;

    private void OnEnable() => Logger.Handlers += LoggingHandler;
    private void OnDisable() => Logger.Handlers -= LoggingHandler;

    private void Awake()
    {
        s_log = log;
        s_writeInUnityConsole = writeInUnityConsole;
    }

    public static void LoggingHandler(Logger.Level level, string message, IDictionary<string, object> args)
    {
        if (args != null)
        {
            foreach (var key in args.Keys)
            {
                message += $" {"" + key}: {"" + args[key]},";
            }
        }

        var logText = $"[RudderAnalytics] [{level}] {message}";
        if(s_writeInUnityConsole)
            Debug.Log(logText);

        UnityMainThread._Wkr.AddJob(() =>
        {
            if (s_log)
                s_log.text += logText + "\n";
        });
    }
}