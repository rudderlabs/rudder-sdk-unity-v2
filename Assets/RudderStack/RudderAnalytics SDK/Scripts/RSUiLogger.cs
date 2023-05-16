using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RudderStack.Unity
{
    public class RSUiLogger : MonoBehaviour
    {
        [SerializeField] private Text     uiLogger;
        [SerializeField] private TMP_Text textMeshProLogger;
        [Space]
        [SerializeField] private Logger.Level minLevel = Logger.Level.DEBUG;

        private static readonly ConcurrentQueue<Action> Jobs = new ConcurrentQueue<Action>();

        private void OnEnable()  => Logger.Handlers += LoggingHandler;
        private void OnDisable() => Logger.Handlers -= LoggingHandler;

        private void Update()
        {
            while (Jobs.TryDequeue(out var job))
                job.Invoke();
        }


        private void LoggingHandler(Logger.Level level, string message, IDictionary<string, object> args)
        {
            if (level < minLevel)
                return;

            if (args != null)
                message += string.Concat(args.Keys.Select(x => $"\n\t {x}: {args[x]},"));

            var logText = $"[RudderAnalytics] [{level}] {message} \n";

            Jobs.Enqueue(() =>
            {
                if (uiLogger)
                {
                    var text = (uiLogger.text + logText);
                    if (text.Length > 1000)
                        text = text.Substring(text.Length - 1000, 1000);
                    uiLogger.text = text;
                }
                if (textMeshProLogger)
                {
                    var text = (textMeshProLogger.text + logText);
                    if (text.Length > 1000)
                        text = text.Substring(text.Length - 1000, 1000);
                    textMeshProLogger.text = text;
                }
            });
        }
    }
}