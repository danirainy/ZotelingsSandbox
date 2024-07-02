namespace ZotelingsSandbox.Common;
internal static class Lang
{
    public static string Translate(string key)
    {
        var language = Language.Language.CurrentLanguage();
        if (!translations.TryGetValue(language, out var translation))
        {
            return key;
        }
        if (translation.TryGetValue(key, out string value))
        {
            return value;
        }
        else
        {
            Log.LogError("Unexpected key: " + key);
            return key;
        }
    }
    private static Dictionary<Language.LanguageCode, Dictionary<string, string>> translations = new()
    {
        {
            Language.LanguageCode.EN,
            new Dictionary<string, string>
            {
                {"Zoteling's Sandbox", "Zoteling's Sandbox"},
                {"Zote is adorably cute!", "Zote is adorably cute!"},
            }
        },
        {
            Language.LanguageCode.ZH,
            new Dictionary<string, string>
            {
                {"Zoteling's Sandbox", "沙盒模式"},
                {"Zote is adorably cute!", "左宝可爱捏！"},
            }
        }
    };
}
