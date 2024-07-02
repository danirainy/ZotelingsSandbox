namespace ZotelingsSandbox.Templates.Standard;
internal class TraitorLord : TemplateBase
{
    private class LandCheck : FsmStateAction
    {
        public override void OnUpdate()
        {
            var boxCollider = Fsm.GameObject.GetComponent<BoxCollider2D>();
            var bottomRays = new List<Vector2>(3)
            {
                new Vector2(boxCollider.bounds.max.x, boxCollider.bounds.min.y),
                new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y),
                boxCollider.bounds.min
            };
            var bottomHit = false;
            for (int k = 0; k < 3; k++)
            {
                RaycastHit2D raycastHit2D3 = Physics2D.Raycast(bottomRays[k], -Vector2.up, 1.5f, 1 << 8);
                if (raycastHit2D3.collider != null)
                {
                    bottomHit = true;
                    break;
                }
            }
            if (bottomHit)
            {
                Fsm.Event("LAND");
            }
        }
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Traitor_Lord", "Battle Scene")
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        var battleScene = Load.Preload(preloadedObjects, "GG_Traitor_Lord", "Battle Scene");
        var wave3 = battleScene.transform.Find("Wave 3").gameObject;
        prefab = wave3.transform.Find("Mantis Traitor Lord").gameObject;
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("Mantis");
        fsm.RemoveTransition("Init", "FINISHED");
        fsm.AddState(Deploy.Common.PlacingStateName);
        fsm.AddCustomAction(Deploy.Common.PlacingStateName, () =>
        {
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.enabled = true;
            var animator = fsm.gameObject.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Idle");
            var rigidbody = gameObject.GetComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
        });
        fsm.AddTransition("Init", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place(true));
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep(true));
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Active");
        fsm.RemoveAction("Active", 6);
        fsm.RemoveAction("Active", 5);
        fsm.RemoveAction("Active", 4);
        fsm.RemoveAction("Roar Recover", 1);
        fsm.RemoveAction("DSlash", 13);
        fsm.AddAction("DSlash", new LandCheck());
        fsm.RemoveAction("Land", 0);
    }
    private GameObject prefab;
}
