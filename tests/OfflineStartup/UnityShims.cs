// Only Unity platform APIs are substituted. The SDK source, bundled RudderAnalytics.dll,
// JSON serialization, encryption, disk persistence, and HTTP requests are real.
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;

namespace UnityEngine
{
    public sealed class WaitUntil
    {
        public Func<bool> Predicate { get; }
        public WaitUntil(Func<bool> predicate) => Predicate = predicate;
    }

    public static class PlayerPrefs
    {
        private static readonly ConcurrentDictionary<string, string> Values = new ConcurrentDictionary<string, string>();
        public static bool HasKey(string key) => Values.ContainsKey(key);
        public static string GetString(string key) => Values[key];
        public static void SetString(string key, string value) => Values[key] = value ?? "";
        public static void DeleteKey(string key) => Values.TryRemove(key, out _);
        internal static void Reset() => Values.Clear();
    }

    public static class Resources
    {
        public static object Load(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, name + ".json"));
    }

    public static class Debug
    {
        internal static readonly ConcurrentQueue<string> Messages = new ConcurrentQueue<string>();
        public static void Log(object message) => Messages.Enqueue(message.ToString());
        public static void LogWarning(object message) => Messages.Enqueue(message.ToString());
        public static void LogError(object message) => Messages.Enqueue(message.ToString());
    }

    public static class SystemInfo
    {
        public static string operatingSystem => "Test OS";
        public static string deviceModel => "Test Device";
        public static string deviceName => "Test Device";
        public static string deviceUniqueIdentifier => "test-device";
    }

    public static class Screen
    {
        public static float dpi => 96;
        public static int width => 640;
        public static int height => 480;
    }

    public static class Application
    {
        public static string productName => "SDK regression test";
        public static string version => "1.0.0";
    }
}

namespace UnityEngine.Device
{
    public static class Application
    {
        public static string persistentDataPath { get; set; }
    }
}

namespace RudderStack.Unity
{
    public sealed class RSLifecycleEvents
    {
        public static RSLifecycleEvents Instance { get; } = new RSLifecycleEvents();
        public TestPlayerSettings settings = new TestPlayerSettings();
        internal IEnumerator ScheduledRoutine;
        public void StartCoroutine(IEnumerator routine) => ScheduledRoutine = routine;
    }

    public sealed class TestPlayerSettings
    {
        public string packageName = "test.package";
    }
}
