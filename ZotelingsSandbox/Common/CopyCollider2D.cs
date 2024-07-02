// Credit to https://discussions.unity.com/t/how-to-get-a-component-from-an-object-and-add-it-to-another-copy-components-at-runtime/80939/4
namespace ZotelingsSandbox.Common;
internal static class CopyCollider2D
{
    public static T GetCopyOf<T>(this Collider2D comp, T other) where T : Collider2D
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    if (pinfo.Name == "name" || pinfo.Name == "density" || pinfo.Name == "usedByComposite")
                    {
                        continue;
                    }
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }
    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Collider2D
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }
}
