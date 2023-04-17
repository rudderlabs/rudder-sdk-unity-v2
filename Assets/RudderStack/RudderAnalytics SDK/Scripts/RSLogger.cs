using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RudderStack.Unity
{
    public class RSLogger : MonoBehaviour
    {
        public Logger.Level minLevel = Logger.Level.DEBUG;

        private void OnEnable()  => Logger.Handlers += LoggingHandler;
        private void OnDisable() => Logger.Handlers -= LoggingHandler;

        private void LoggingHandler(Logger.Level level, string message, IDictionary<string, object> args)
        {
            if (level < minLevel)
                return;

            if (args != null) 
                message += string.Concat(args.Keys.Select(x => $"\n\t {x}: {args[x]},"));

            Debug.Log($"[RudderAnalytics] [{level}] {message}");
        }
    }
}