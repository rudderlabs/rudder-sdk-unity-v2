using System;
using System.Globalization;
using Newtonsoft.Json;
using RudderStack.Model;
using UnityEngine;

namespace RudderStack.Unity.Utility
{
    public static class Converter
    {
        public static RudderOptions ToOptions(this BaseAction action)
        {
            var options = new RudderOptions();
            options.SetContext(action.Context);
            options.SetTimestamp(DateTime.Parse(action.Timestamp, CultureInfo.InvariantCulture));
            options.SetAnonymousId(action.AnonymousId);
            // TODO: Solve problem with Integrations
            return options;
        }
    }
}
