namespace ZotelingsSandbox;
public class ZotelingsSandbox : Mod
{
    public ZotelingsSandbox()
    {
        instance = this;
        templateLists = Templates.TemplateLists.Get();
        plugins = new List<Plugins.PluginBase>()
        {
            new Plugins.NailgodArena(),
        };
    }
    public override string GetVersion() => "1.0.5.7";
    public override List<(string, string)> GetPreloadNames()
    {
        var templatePreloadNames = templateLists.SelectMany(templateList => templateList.Item2)
                                            .SelectMany(template => template.GetPreloadNames());
        var sharedObjectPreloadNames = Deploy.SharedObjects.GetPreloadNames();
        var pluginPreloadNames = plugins.SelectMany(plugin => plugin.GetPreloadNames());
        return templatePreloadNames.Concat(sharedObjectPreloadNames).Concat(pluginPreloadNames).ToList();
    }
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Config.logKeys = ["Rewrite", "Damage", "StateMachine", "FSM", "Plugin"];
        Config.renderColider = false;
        Config.mainInterfaceActive = true;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManagerActiveSceneChanged;
        On.HeroController.Update += HeroControllerUpdate;
        On.AudioManager.BeginApplyMusicCue += AudioManagerBeginApplyMusicCue;
        Deploy.RewriteInstance.Initialize();
        Deploy.Hooks.Initialize();
        Deploy.SharedObjects.Initialize(preloadedObjects);
        Interface.MainInterface.Initialize();
        foreach (var template in templateLists.SelectMany(templateList => templateList.Item2))
        {
            template.Initialize(preloadedObjects);
        }
        foreach (var plugin in plugins)
        {
            plugin.Initialize(preloadedObjects);
        }
    }
    private void SceneManagerActiveSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
    {
        if (HeroController.instance != null)
        {
            var knight = HeroController.instance.gameObject;
            var instanceInfo = knight.GetAddComponent<Deploy.Behaviors.InstanceInfo>();
            instanceInfo.status = Deploy.Behaviors.InstanceInfo.Status.Active;
            instanceInfo.groupID = -1;
            instanceInfo.priority = Deploy.Behaviors.InstanceInfo.Priority.Knight;
            if (Config.renderColider)
            {
                foreach (var collider2D in knight.GetComponentsInChildren<Collider2D>(true))
                {
                    collider2D.gameObject.GetAddComponent<Deploy.Behaviors.ColliderRender>();
                }
            }
            knight.GetAddComponent<Deploy.Behaviors.HeroHandler>();
        }
        if (to.name != "Menu_Title")
        {
            Deploy.SharedObjects.ActiveSceneChanged();
            mainInterface = new Interface.MainInterface();
        }
        else
        {
            mainInterface = null;
        }
        foreach (var plugin in plugins)
        {
            plugin.ActiveSceneChanged(from, to);
        }
    }
    private void HeroControllerUpdate(On.HeroController.orig_Update orig, HeroController self)
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Config.mainInterfaceActive = !Config.mainInterfaceActive;
        }
        orig(self);
    }
    private System.Collections.IEnumerator AudioManagerBeginApplyMusicCue(On.AudioManager.orig_BeginApplyMusicCue orig, AudioManager self, MusicCue musicCue, float delayTime, float transitionTime, bool applySnapshot)
    {
        foreach (var plugin in plugins)
        {
            plugin.BeginApplyMusicCue(orig, self, musicCue, delayTime, transitionTime, applySnapshot);
        }
        yield return orig(self, musicCue, delayTime, transitionTime, applySnapshot);
    }
    public List<GameObject> RefreshInstances()
    {
        instances = instances.Where(gameObject => gameObject != null).ToList();
        return instances;
    }
    public static ZotelingsSandbox instance;
    public Interface.MainInterface mainInterface;
    private List<(string, List<Templates.TemplateBase>)> templateLists;
    private List<GameObject> instances = [];
    private List<Plugins.PluginBase> plugins;
}
