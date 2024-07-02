namespace ZotelingsSandbox.Templates.Standard;
internal class SoulWarrior : TemplateBase
{
    private class SmartArena : MonoBehaviour
    {
        public void Build()
        {

            var col2d = gameObject.GetComponent<BoxCollider2D>();
            minX = float.MinValue;
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
            minY = gameObject.transform.position.y;
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
    internal class SmartAttachment : MonoBehaviour
    {
        private void Update()
        {
            if (gameObject.transform.parent == null)
            {
                if (parent == null)
                {
                    GameObject.Destroy(gameObject);
                }
            }
        }
        public GameObject parent;
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Mage_Knight", "Mage Knight"),
        ("GG_Mage_Knight_V", "Balloon Spawner"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        prefab = Load.Preload(preloadedObjects, "GG_Mage_Knight", "Mage Knight");
        balloon = Load.Preload(preloadedObjects, "GG_Mage_Knight_V", "Balloon Spawner");
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("Mage Knight");
        fsm.RemoveTransition("Pause", "FINISHED");
        fsm.AddState(Deploy.Common.PlacingStateName);
        fsm.AddCustomAction(Deploy.Common.PlacingStateName, () =>
        {
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.enabled = true;
            var animator = fsm.gameObject.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Run");
            var rigidbody = gameObject.GetComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            var collider = fsm.gameObject.GetComponent<BoxCollider2D>();
            collider.enabled = true;
            var healthManager = fsm.gameObject.GetComponent<HealthManager>();
            healthManager.IsInvincible = false;
        });
        fsm.AddTransition("Pause", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place());
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep());
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Init");
        fsm.AddCustomAction("Init", () =>
        {
            var smartArena = gameObject.AddComponent<SmartArena>();
            smartArena.Build();
            var thisBalloon = UnityEngine.Object.Instantiate(balloon);
            var thisBalloonFsm = thisBalloon.LocateMyFSM("Battle Control");
            thisBalloonFsm.AddCustomAction("Init", () =>
            {
                thisBalloonFsm.AccessGameObjectVariable("Mage Knight").Value = gameObject;
            });
            thisBalloon.AddComponent<SmartAttachment>().parent = gameObject;
            var instanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
            var properties = new PlaceConfig
            {
                groupID = instanceInfo.groupID,
                hp = Deploy.Common.DefaultEnemyHealth,
                damage = instanceInfo.damage,
                hpBar = false
            };
            Deploy.RewriteInstance.Rewrite(thisBalloon, properties, gameObject);
            var myX = (smartArena.minX + smartArena.maxX) / 2;
            var myY = gameObject.transform.position.y;
            thisBalloon.transform.position = new Vector3(myX - (47.2813f - 11.63f), myY - (7.1369f - 46.35f), 0);
        });
        fsm.RemoveTransition("Init", "FINISHED");
        fsm.AddTransition("Init", "FINISHED", "Wake");
        fsm.RemoveTransition("Wake", "FINISHED");
        fsm.AddTransition("Wake", "FINISHED", "Idle");
        fsm.AddCustomAction("Idle", () =>
        {
            var instanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
            var targetFollower = instanceInfo.targetFollower.gameObject;
            var targetX = targetFollower.transform.position.x;
            fsm.FsmVariables.GetFsmFloat("Tele X Min").Value = targetX - (56.79521f - 35.46665f) / 2;
            fsm.FsmVariables.GetFsmFloat("Tele X Max").Value = targetX + (56.79521f - 35.46665f) / 2;
            fsm.FsmVariables.GetFsmFloat("Floor Y").Value = gameObject.transform.position.y;
        });
        fsm.AddCustomAction("Cancel Frame", () =>
        {
            fsm.SetState("Side Tele Aim");
        });
        fsm.AddCustomAction("Up Tele Aim", () =>
        {
            var targetY = fsm.FsmVariables.GetFsmFloat("Target Y").Value;
            targetY = Mathf.Min(targetY, fsm.FsmVariables.GetFsmFloat("Floor Y").Value + 12);
            fsm.FsmVariables.GetFsmFloat("Target Y").Value = targetY;
        });
    }
    private GameObject prefab;
    private GameObject balloon;
}
