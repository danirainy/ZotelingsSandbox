namespace ZotelingsSandbox.Templates.Standard;
internal class BroodingMawlek : TemplateBase
{
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Brooding_Mawlek", "Battle Scene"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        var battleScene = Load.Preload(preloadedObjects, "GG_Brooding_Mawlek", "Battle Scene");
        prefab = battleScene.transform.Find("Mawlek Body").gameObject;
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("Mawlek Control");
        fsm.RemoveTransition("Pause", "FINISHED");
        fsm.AddState(Deploy.Common.PlacingStateName);
        fsm.AddCustomAction(Deploy.Common.PlacingStateName, () =>
        {
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.enabled = true;
            var animator = fsm.gameObject.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Body Idle");
            var rigidbody = gameObject.GetComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            var collider = fsm.gameObject.GetComponent<BoxCollider2D>();
            collider.enabled = true;
            var healthManager = fsm.gameObject.GetComponent<HealthManager>();
            healthManager.hp = Math.Max(healthManager.hp, Deploy.Common.DefaultEnemyHealth);
            healthManager.IsInvincible = false;
            var walker = fsm.gameObject.GetComponent<Walker>();
            walker.Stop(Walker.StopReasons.Controlled);
            var dummy = fsm.gameObject.transform.Find("Dummy");
            var dummyAnimator = dummy.GetComponent<tk2dSpriteAnimator>();
            dummyAnimator.Play("Dummy Blank");
            var mawlekHead = fsm.gameObject.transform.Find("Mawlek Head");
            var mawlekHeadAnimator = mawlekHead.GetComponent<tk2dSpriteAnimator>();
            mawlekHeadAnimator.Play("Head Idle");
        });
        fsm.AddTransition("Pause", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place());
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep());
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Init");
        fsm.RemoveTransition("Init", "FINISHED");
        fsm.AddTransition("Init", "FINISHED", "Start");
        fsm.AddCustomAction("Start", () =>
        {
            var instanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
            var targetFollower = instanceInfo.targetFollower.gameObject;
            fsm.FsmVariables.GetFsmFloat("Start X").Value = targetFollower.transform.position.x;
            fsm.FsmVariables.GetFsmFloat("Start Y").Value = targetFollower.transform.position.y;
            var rigidbody = gameObject.GetComponent<Rigidbody2D>();
            rigidbody.gravityScale = 3;
        });
        var rigidbody = gameObject.GetComponent<Rigidbody2D>();
        rigidbody.gravityScale = 3;
        var mawlekArmL = fsm.gameObject.transform.Find("Mawlek Arm L").gameObject;
        var mawlekArmLFsm = mawlekArmL.LocateMyFSM("Mawlek Arm Control");
        mawlekArmLFsm.AddCustomAction("Dormant", () =>
        {
            var animator = mawlekArmL.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Arm Idle");
        });
        var mawlekArmR = fsm.gameObject.transform.Find("Mawlek Arm R").gameObject;
        var mawlekArmRFsm = mawlekArmR.LocateMyFSM("Mawlek Arm Control");
        mawlekArmRFsm.AddCustomAction("Dormant", () =>
        {
            var animator = mawlekArmR.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Arm Idle");
        });
    }
    private GameObject prefab;
}
