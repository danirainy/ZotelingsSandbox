namespace ZotelingsSandbox.Templates;
internal class TemplateBase
{
    public virtual List<(string, string)> GetPreloadNames()
    {
        Log.LogError("GetPreloadNames not implemented");
        return null;
    }
    public virtual void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Log.LogError("Initialize not implemented");
    }
    protected virtual Dictionary<string, GameObject> Instantiate()
    {
        Log.LogError("Instantiate not implemented");
        return null;
    }
    protected virtual void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        Log.LogError("RewriteInstance not implemented");
    }
    protected virtual void RewriteControllable(string name, GameObject gameObject)
    {
        Log.LogError("RewriteControllable not implemented");
    }
    protected virtual void SetupControllableRoot(GameObject controllableRoot, Dictionary<string, GameObject> controllableChildren)
    {
        Log.LogError("SetupStateMachine not implemented");
    }
    public virtual void Place(PlaceConfig properties)
    {
        var instances = Instantiate();
        if (instances == null || instances.Count != 1)
        {
            Log.LogError("Invalid instance to place");
            return;
        }
        var instance = instances.Values.First();
        instance.name = name;
        var mousePosition = Deploy.Actions.Place.GetMousePosition(instance);
        var hero = HeroController.instance.gameObject;
        instance.transform.position = new Vector3(mousePosition.x, mousePosition.y, hero.transform.position.z - 0.002f);
        RewriteInstance(instance, properties);
        Deploy.RewriteInstance.Rewrite(instance, properties, null);
        var instanceInfo = instance.GetAddComponent<Deploy.Behaviors.InstanceInfo>();
        instanceInfo.status = Deploy.Behaviors.InstanceInfo.Status.Placing;
        ZotelingsSandbox.instance.RefreshInstances().Add(instance);
    }
    public void Control()
    {
        var instances = Instantiate();
        var hero = HeroController.instance.gameObject;
        var controllableRoot = new GameObject(name + ".ControllableRoot");
        controllableRoot.transform.position = new Vector3(
            hero.transform.position.x,
            hero.transform.position.y,
            hero.transform.position.z - 0.001f
        );
        Dictionary<string, GameObject> controllableChildren = new();
        foreach (var pair in instances)
        {
            var instance = pair.Value;
            instance.name = name + ".ControllableChild." + pair.Key;
            instance.transform.position = new Vector3(
                hero.transform.position.x,
                hero.transform.position.y,
                hero.transform.position.z - 0.001f
            );
            RewriteControllable(pair.Key, instance);
            Deploy.RewriteInstance.Rewrite(instance, new PlaceConfig { groupID = -1, hp = 1, damage = 32, hpBar = false }, null);
            Deploy.RewriteControllable.Rewrite(instance);
            controllableChildren.Add(pair.Key, instance);
        }
        var heroHandler = hero.GetComponent<Deploy.Behaviors.HeroHandler>();
        heroHandler.InstallControllable(controllableRoot);
        SetupControllableRoot(controllableRoot, controllableChildren);
    }
    public string name;
}
