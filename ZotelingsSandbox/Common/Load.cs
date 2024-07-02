namespace ZotelingsSandbox.Common;
internal static class Load
{
    public static GameObject Preload(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects, string scene, string name)
    {
        if (preloadedObjects == null)
        {
            Log.LogError("preloadedObjects is null");
            return null;
        }
        if (!preloadedObjects.ContainsKey(scene))
        {
            Log.LogError($"{scene} not found");
            return null;
        }
        if (!preloadedObjects[scene].ContainsKey(name))
        {
            Log.LogError($"{name} not found");
            return null;
        }
        return preloadedObjects[scene][name];
    }
    public static Sprite LoadSprite(string name)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        MemoryStream memoryStream = new((int)stream.Length);
        stream.CopyTo(memoryStream);
        stream.Close();
        var bytes = memoryStream.ToArray();
        memoryStream.Close();
        var texture2D = new Texture2D(0, 0);
        texture2D.LoadImage(bytes, true);
        return Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), Vector2.one / 2, 100.0f);
    }
}
