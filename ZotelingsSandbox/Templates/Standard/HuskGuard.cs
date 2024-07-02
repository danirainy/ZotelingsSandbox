namespace ZotelingsSandbox.Templates.Standard;
internal class HuskGuard : TemplateBase
{
    internal class Behavior : MonoBehaviour
    {
        private void Start()
        {
            instanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
            fsm = gameObject.LocateMyFSM("Zombie Guard");
        }
        private void Update()
        {
            var idleSpotLeft = fsm.FsmVariables.GetFsmFloat("Idle Spot Left").Value;
            var idleSpotRight = fsm.FsmVariables.GetFsmFloat("Idle Spot Right").Value;
            var roamL = fsm.FsmVariables.GetFsmFloat("Roam L").Value;
            var roamR = fsm.FsmVariables.GetFsmFloat("Roam R").Value;
            var oldCenter = (idleSpotLeft + idleSpotRight) / 2;
            if (instanceInfo.targetDetector.target == null)
            {
                return;
            }
            var newCenter = instanceInfo.targetDetector.target.transform.position.x;
            idleSpotLeft -= oldCenter - newCenter;
            idleSpotRight -= oldCenter - newCenter;
            roamL -= oldCenter - newCenter;
            roamR -= oldCenter - newCenter;
            fsm.FsmVariables.GetFsmFloat("Idle Spot Left").Value = idleSpotLeft;
            fsm.FsmVariables.GetFsmFloat("Idle Spot Right").Value = idleSpotRight;
            fsm.FsmVariables.GetFsmFloat("Roam L").Value = roamL;
            fsm.FsmVariables.GetFsmFloat("Roam R").Value = roamR;
        }
        private Deploy.Behaviors.InstanceInfo instanceInfo;
        private PlayMakerFSM fsm;
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("Crossroads_48", "Zombie Guard")
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        prefab = Load.Preload(preloadedObjects, "Crossroads_48", "Zombie Guard");
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("Zombie Guard");
        fsm.RemoveTransition("Start Left", "FINISHED");
        fsm.RemoveTransition("Start Right", "FINISHED");
        fsm.AddState(Deploy.Common.PlacingStateName);
        fsm.AddCustomAction(Deploy.Common.PlacingStateName, () =>
        {
            var animator = fsm.gameObject.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Idle");
        });
        fsm.AddTransition("Start Left", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddTransition("Start Right", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place(true));
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep(true));
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Wake");
        for (var i = 0; i < gameObject.transform.childCount; ++i)
        {
            var child = gameObject.transform.GetChild(i);
            if (child.name == "Dream Gate Set Lock")
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        gameObject.AddComponent<Behavior>();
    }
    private GameObject prefab;
}
