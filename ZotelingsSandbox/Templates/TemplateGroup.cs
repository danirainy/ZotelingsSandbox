namespace ZotelingsSandbox.Templates;
internal class TemplateGroup : TemplateBase
{
    public override List<(string, string)> GetPreloadNames()
    {
        var preloadNames = new List<(string, string)>();
        foreach (var (template, _) in templates)
        {
            preloadNames.AddRange(template.GetPreloadNames());
        }
        return preloadNames;
    }
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        foreach (var (template, _) in templates)
        {
            template.Initialize(preloadedObjects);
        }
    }
    public override void Place(PlaceConfig properties)
    {
        foreach (var (template, offset) in templates)
        {
            properties.placingOffset = offset;
            template.Place(properties);
        }
    }
    public List<(TemplateBase, Vector2)> templates;
}
