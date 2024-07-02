namespace ZotelingsSandbox.Templates.Standard;
internal class MossKnight : TemplateBase
{
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("Fungus1_26", "Moss Knight")
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        prefab = Load.Preload(preloadedObjects, "Fungus1_26", "Moss Knight");
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("Moss Knight Control");
        fsm.RemoveTransition("Pause Frame", "FINISHED");
        fsm.AddState(Deploy.Common.PlacingStateName);
        fsm.AddTransition("Pause Frame", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddCustomAction(Deploy.Common.PlacingStateName, () =>
        {
            var walker = fsm.gameObject.GetComponent<Walker>();
            walker.Stop(Walker.StopReasons.Controlled);
            var animator = fsm.gameObject.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Idle");
        });
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place());
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep());
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Initialise");
        fsm.AddCustomAction("Lake", () => { fsm.SendEvent("WAKE"); });
        var animator = gameObject.GetComponent<tk2dSpriteAnimator>();
        animator.Play("Idle");
    }
    private GameObject prefab;
}
