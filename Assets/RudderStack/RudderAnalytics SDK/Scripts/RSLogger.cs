using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RudderStack.Unity
{
    public class RSLogger : MonoBehaviour
    {

        private void OnEnable()  => Logger.Handlers += LoggingHandler;
        private void OnDisable() => Logger.Handlers -= LoggingHandler;

        private void LoggingHandler(Logger.Level level, string message, IDictionary<string, object> args)
        {
            if (level < RSAnalytics.Client.Config.GetLogLevel())
                return;

            if (args != null) 
                message += string.Concat(args.Keys.Select(x => $"\n\t {x}: {args[x]},"));

            Debug.Log($"[RudderAnalytics] [{level}] {message}");
        }
    }
}