namespace ZotelingsSandbox.Common;
internal static class Log
{
    public static void LogKey(string key, string message)
    {
        if (Config.logKeys.Contains(key))
        {
            ZotelingsSandbox.instance.Log($"[{key}] - {message}");
        }
    }
    public static void LogError(string message)
    {
        ZotelingsSandbox.instance.LogError(message);
    }
}
