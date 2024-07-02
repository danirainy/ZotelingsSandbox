namespace ZotelingsSandbox.Templates.Standard;
internal class Oblobble : TemplateBase
{
    private class SmartArena : MonoBehaviour
    {
        public void Build()
        {
            minX = float.MinValue;
            var col2d = gameObject.GetComponent<BoxCollider2D>();
            var leftRays = new List<Vector2>();
            leftRays.Add(col2d.bounds.min);
            leftRays.Add(new Vector2(col2d.bounds.min.x, col2d.bounds.center.y));
            leftRays.Add(new Vector2(col2d.bounds.min.x, col2d.bounds.max.y));
            for (int l = 0; l < 3; l++)
            {
                RaycastHit2D raycastHit2D4 = Physics2D.Raycast(leftRays[l], -Vector2.right, float.MaxValue, 1 << 8);
                if (raycastHit2D4.collider != null)
                {
                    minX = Mathf.Max(minX, raycastHit2D4.point.x);
                }
            }
            maxX = float.MaxValue;
            var rightRays = new List<Vector2>();
            rightRays.Add(col2d.bounds.max);
            rightRays.Add(new Vector2(col2d.bounds.max.x, col2d.bounds.center.y));
            rightRays.Add(new Vector2(col2d.bounds.max.x, col2d.bounds.min.y));
            for (int j = 0; j < 3; j++)
            {
                RaycastHit2D raycastHit2D2 = Physics2D.Raycast(rightRays[j], Vector2.right, float.MaxValue, 1 << 8);
                if (raycastHit2D2.collider != null)
                {
                    maxX = Mathf.Min(maxX, raycastHit2D2.point.x);
                }
            }
            minY = float.MinValue;
            var bottomRays = new List<Vector2>();
            bottomRays.Add(new Vector2(col2d.bounds.max.x, col2d.bounds.min.y));
            bottomRays.Add(new Vector2(col2d.bounds.center.x, col2d.bounds.min.y));
            bottomRays.Add(col2d.bounds.min);
            for (int k = 0; k < 3; k++)
            {
                RaycastHit2D raycastHit2D3 = Physics2D.Raycast(bottomRays[k], -Vector2.up, float.MaxValue, 1 << 8);
                if (raycastHit2D3.collider != null)
                {
                    minY = Mathf.Max(minY, raycastHit2D3.point.y);
                }
            }
        }
        private void FixedUpdate()
        {
            if (gameObject.transform.position.x < minX)
            {
                gameObject.transform.position = new Vector3(minX, gameObject.transform.position.y, gameObject.transform.position.z);
            }
            if (gameObject.transform.position.x > maxX)
            {
                gameObject.transform.position = new Vector3(maxX, gameObject.transform.position.y, gameObject.transform.position.z);
            }
            if (gameObject.transform.position.y < minY)
            {
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, minY, gameObject.transform.position.z);
            }
        }
        public float minX;
        public float maxX;
        public float minY;
    }
    private class OblobbleTag : MonoBehaviour
    {
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Oblobbles", "Mega Fat Bee"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        prefab = Load.Preload(preloadedObjects, "GG_Oblobbles", "Mega Fat Bee");
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("fat fly bounce");
        fsm.RemoveTransition("Initialise", "FINISHED");
        fsm.AddState(Deploy.Common.PlacingStateName);
        fsm.AddCustomAction(Deploy.Common.PlacingStateName, () =>
        {
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.enabled = true;
            var animator = fsm.gameObject.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Fly");
            var rigidbody = gameObject.GetComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            var collider = fsm.gameObject.GetComponent<BoxCollider2D>();
            collider.enabled = true;
            var healthManager = fsm.gameObject.GetComponent<HealthManager>();
            healthManager.IsInvincible = false;
        });
        fsm.AddTransition("Initialise", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place());
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep());
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Activate");
        fsm.InsertCustomAction("Activate", () =>
        {
            var smartArena = gameObject.AddComponent<SmartArena>();
            smartArena.Build();
            fsm.FsmVariables.GetFsmFloat("X Min").Value = smartArena.minX;
            fsm.FsmVariables.GetFsmFloat("X Max").Value = smartArena.maxX;
            fsm.AccessIntVariable("Max HP").Value = gameObject.GetComponent<HealthManager>().hp;
        }, 0);
        var setRage = gameObject.LocateMyFSM("Set Rage");
        setRage.InsertCustomAction("Set", () =>
        {
            setRage.FsmVariables.GetFsmInt("HP Max").Value = fsm.AccessIntVariable("Max HP").Value;
            setRage.FsmVariables.GetFsmInt("HP Add").Value = (int)(200f / 750 * fsm.AccessIntVariable("Max HP").Value);
        }, 0);
        gameObject.AddComponent<OblobbleTag>();
        var rager = gameObject.LocateMyFSM("Rager");
        rager.RemoveAction("State 2", 0);
        rager.AddCustomAction("State 2", () =>
        {
            foreach (var instance in ZotelingsSandbox.instance.RefreshInstances())
            {
                if (instance.GetComponent<OblobbleTag>() != null)
                {
                    var myInstanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
                    var instanceInfo = instance.GetComponent<Deploy.Behaviors.InstanceInfo>();
                    if (myInstanceInfo.status == Deploy.Behaviors.InstanceInfo.Status.Active
                        && instanceInfo.status == Deploy.Behaviors.InstanceInfo.Status.Active
                        && myInstanceInfo.groupID == instanceInfo.groupID)
                    {
                        instance.LocateMyFSM("Set Rage").SendEvent("OBLOBBLE RAGE");
                    }
                }
            }
        });
    }
    private GameObject prefab;
}
