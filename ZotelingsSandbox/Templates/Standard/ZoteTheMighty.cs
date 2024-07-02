namespace ZotelingsSandbox.Templates.Standard;
internal class ZoteTheMighty : TemplateBase
{
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Mighty_Zote","Battle Control"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        var battleControl = Load.Preload(preloadedObjects, "GG_Mighty_Zote", "Battle Control");
        prefab = battleControl.transform.Find("Dormant Warriors").transform.Find("Zote Crew Normal (1)").gameObject;
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("Control");
        fsm.RemoveTransition("Init", "FINISHED");
        fsm.AddState(Deploy.Common.PlacingStateName);
        fsm.AddTransition("Init", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddCustomAction(Deploy.Common.PlacingStateName, () =>
        {
            var animator = fsm.gameObject.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Idle");
        });
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place());
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep());
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Dormant");
        fsm.AddTransition("Dormant", "FINISHED", "Multiply");
        fsm.RemoveAction("Spawn Antic", 4);
        fsm.RemoveAction("Spawn Antic", 1);
        fsm.AddCustomAction("Spawn Antic", () => { fsm.SendEvent("FINISHED"); });
        fsm.RemoveAction("Activate", 3);
        fsm.RemoveAction("Tumble Out", 2);
        fsm.RemoveAction("Death", 0);
        fsm.AddCustomAction("Death Reset", () =>
        {
            UnityEngine.Object.Destroy(fsm.gameObject);
        });
    }
    private GameObject prefab;
}
