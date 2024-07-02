namespace ZotelingsSandbox.Templates.Standard;
internal class PaintmasterSheo : TemplateBase
{
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Painter", "Battle Scene")
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        var battleScene = Load.Preload(preloadedObjects, "GG_Painter", "Battle Scene");
        prefab = battleScene.transform.Find("Sheo Boss").gameObject;
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("nailmaster_sheo");
        fsm.RemoveTransition("Init", "FINISHED");
        fsm.AddState(Deploy.Common.PlacingStateName);
        fsm.AddCustomAction(Deploy.Common.PlacingStateName, () =>
        {
            var animator = fsm.gameObject.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Idle");
            var rigidbody = gameObject.GetComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            var collider = gameObject.GetComponent<Collider2D>();
            collider.enabled = true;
        });
        fsm.AddTransition("Init", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place());
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep());
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Set Paint HP");
        fsm.RemoveTransition("Set Paint HP", "FINISHED");
        fsm.AddTransition("Set Paint HP", "FINISHED", "Battle Start");
        fsm.InsertCustomAction("Idle", () =>
        {
            var currentY = fsm.gameObject.transform.position.y;
            var targetY = currentY + (16.1f - 6.8763f);
            fsm.FsmVariables.GetFsmFloat("Topslash Y").Value = targetY;
        }, 0);
    }
    private GameObject prefab;
}
