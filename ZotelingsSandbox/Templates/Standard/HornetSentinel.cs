namespace ZotelingsSandbox.Templates.Standard;
internal class HornetSentinel : TemplateBase
{
    private class WallCheck : FsmStateAction
    {
        public override void OnUpdate()
        {
            var velocity = Fsm.GameObject.GetComponent<Rigidbody2D>().velocity;
            var col2d = Fsm.GameObject.GetComponent<BoxCollider2D>();
            var leftRays = new List<Vector2>();
            leftRays.Add(col2d.bounds.min);
            leftRays.Add(new Vector2(col2d.bounds.min.x, col2d.bounds.center.y));
            leftRays.Add(new Vector2(col2d.bounds.min.x, col2d.bounds.max.y));
            var leftHit = false;
            for (int l = 0; l < 3; l++)
            {
                RaycastHit2D raycastHit2D4 = Physics2D.Raycast(leftRays[l], -Vector2.right, 1.4f, 1 << 8);
                if (raycastHit2D4.collider != null)
                {
                    leftHit = true;
                    break;
                }
            }
            if (leftHit && velocity.x < 0)
            {
                Fsm.GameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                Fsm.Event("WALL L");
                Fsm.GetFsmFloat("Wall X Left").Value = Fsm.GameObject.transform.position.x - 1;
            }
            var rightRays = new List<Vector2>();
            rightRays.Add(col2d.bounds.max);
            rightRays.Add(new Vector2(col2d.bounds.max.x, col2d.bounds.center.y));
            rightRays.Add(new Vector2(col2d.bounds.max.x, col2d.bounds.min.y));
            var rightHit = false;
            for (int j = 0; j < 3; j++)
            {
                RaycastHit2D raycastHit2D2 = Physics2D.Raycast(rightRays[j], Vector2.right, 1.4f, 1 << 8);
                if (raycastHit2D2.collider != null)
                {
                    rightHit = true;
                    break;
                }
            }
            if (rightHit && velocity.x > 0)
            {
                Fsm.GameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                Fsm.Event("WALL R");
                Fsm.GetFsmFloat("Wall X Right").Value = Fsm.GameObject.transform.position.x + 1;
            }
            var bottomRays = new List<Vector2>();
            bottomRays.Add(new Vector2(col2d.bounds.max.x, col2d.bounds.min.y));
            bottomRays.Add(new Vector2(col2d.bounds.center.x, col2d.bounds.min.y));
            bottomRays.Add(col2d.bounds.min);
            var bottomHit = false;
            for (int k = 0; k < 3; k++)
            {
                RaycastHit2D raycastHit2D3 = Physics2D.Raycast(bottomRays[k], -Vector2.up, 1.4f, 1 << 8);
                if (raycastHit2D3.collider != null)
                {
                    bottomHit = true;
                    break;
                }
            }
            if (bottomHit && velocity.y < 0)
            {
                Fsm.GameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                Fsm.Event("LAND");
                Fsm.GetFsmFloat("Floor Y").Value = Fsm.GameObject.transform.position.y - 1;
            }
            var topRays = new List<Vector2>();
            topRays.Add(new Vector2(col2d.bounds.min.x, col2d.bounds.max.y));
            topRays.Add(new Vector2(col2d.bounds.center.x, col2d.bounds.max.y));
            topRays.Add(col2d.bounds.max);
            var topHit = false;
            for (int i = 0; i < 3; i++)
            {
                RaycastHit2D raycastHit2D = Physics2D.Raycast(topRays[i], Vector2.up, 1.4f, 1 << 8);
                if (raycastHit2D.collider != null)
                {
                    topHit = true;
                    break;
                }
            }
            if (topHit && velocity.y > 0)
            {
                Fsm.GameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                Fsm.Event("ROOF");
                Fsm.GetFsmFloat("Roof Y").Value = Fsm.GameObject.transform.position.y + 1;
            }
        }
    }
    private class LandCheck : FsmStateAction
    {
        public override void OnUpdate()
        {
            var velocity = Fsm.GameObject.GetComponent<Rigidbody2D>().velocity;
            var col2d = Fsm.GameObject.GetComponent<BoxCollider2D>();
            var bottomRays = new List<Vector2>();
            bottomRays.Add(new Vector2(col2d.bounds.max.x, col2d.bounds.min.y));
            bottomRays.Add(new Vector2(col2d.bounds.center.x, col2d.bounds.min.y));
            bottomRays.Add(col2d.bounds.min);
            var bottomHit = false;
            for (int k = 0; k < 3; k++)
            {
                RaycastHit2D raycastHit2D3 = Physics2D.Raycast(bottomRays[k], -Vector2.up, 0.4f, 1 << 8);
                if (raycastHit2D3.collider != null)
                {
                    bottomHit = true;
                    break;
                }
            }
            if (bottomHit && velocity.y <= 0)
            {
                Fsm.GameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                Fsm.Event("LAND");
            }
        }
    }
    private class SmartArena : MonoBehaviour
    {
        public void Build()
        {
            minX = float.MinValue;
            maxX = float.MaxValue;
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
            if (gameObject.transform.position.x < minX - 1)
            {
                gameObject.transform.position = new Vector3(minX, gameObject.transform.position.y, gameObject.transform.position.z);
            }
            if (gameObject.transform.position.x > maxX + 1)
            {
                gameObject.transform.position = new Vector3(maxX, gameObject.transform.position.y, gameObject.transform.position.z);
            }
            if (gameObject.transform.position.y < minY - 1)
            {
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, minY, gameObject.transform.position.z);
            }
        }
        private float minX;
        private float maxX;
        private float minY;
    }
    private class CheckHornet : MonoBehaviour
    {
        private void Update()
        {
            if (gameObject.transform.parent == null)
            {
                if (hornet == null)
                {
                    GameObject.Destroy(gameObject);
                }
            }
        }
        public GameObject hornet;
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Hornet_2", "Boss Holder"),
        ("GG_Hornet_2", "Barb Region")
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        var bossHolder = Load.Preload(preloadedObjects, "GG_Hornet_2", "Boss Holder");
        prefab = bossHolder.transform.Find("Hornet Boss 2").gameObject;
        barb = Load.Preload(preloadedObjects, "GG_Hornet_2", "Barb Region");
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("Control");
        fsm.RemoveTransition("Pause", "FINISHED");
        fsm.AddState(Deploy.Common.PlacingStateName);
        fsm.AddCustomAction(Deploy.Common.PlacingStateName, () =>
        {
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.enabled = true;
            var animator = fsm.gameObject.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Idle");
            var rigidbody = gameObject.GetComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            var collider = fsm.gameObject.GetComponent<BoxCollider2D>();
            collider.enabled = true;
            var colliderSize = fsm.FsmVariables.GetFsmVector2("Box Size Idle");
            var colliderOffset = fsm.FsmVariables.GetFsmVector2("Box Off Idle");
            collider.size = colliderSize.Value;
            collider.offset = colliderOffset.Value;
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
            gameObject.AddComponent<SmartArena>().Build();
            var barbRegion = UnityEngine.Object.Instantiate(barb);
            barbRegion.transform.position = gameObject.transform.position;
            var instanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
            var properties = new PlaceConfig
            {
                groupID = instanceInfo.groupID,
                hp = Deploy.Common.DefaultEnemyHealth,
                damage = instanceInfo.damage,
                hpBar = false
            };
            Deploy.RewriteInstance.Rewrite(barbRegion, properties, gameObject);
            barbRegion.AddComponent<CheckHornet>().hornet = gameObject;
            var barbFsm = barbRegion.LocateMyFSM("Spawn Barbs");
            var fixBarb = () =>
            {
                var barb = barbFsm.FsmVariables.GetFsmGameObject("Barb Obj").Value;
                var barbObjFsm = barb.LocateMyFSM("Control");
                barbObjFsm.InsertCustomAction("Distance Check", () =>
                {
                    barbObjFsm.SetState("Thread");
                }, 0);
            };
            barbFsm.AddCustomAction("Spawn 1", fixBarb);
            barbFsm.AddCustomAction("Spawn 2", fixBarb);
            barbFsm.AddCustomAction("Spawn 3", fixBarb);
            fsm.AccessGameObjectVariable("New Barb Region").Value = barbRegion;
        });
        fsm.RemoveTransition("Init", "FINISHED");
        fsm.AddTransition("Init", "FINISHED", "Refight Ready");
        fsm.RemoveAction("Refight Ready", 5);
        fsm.RemoveTransition("Refight Ready", "WAKE");
        fsm.AddTransition("Refight Ready", "WAKE", "Refight Wake");
        for (var i = 0; i < 7; ++i)
        {
            var state = fsm.GetState("Refight Wake");
            state.RemoveAction(state.Actions.Length - 1);
        }
        fsm.RemoveAction("Refight Wake", 1);
        fsm.RemoveAction("Refight Wake", 0);
        fsm.InsertCustomAction("Idle", () =>
        {
            var currentY = gameObject.transform.position.y;
            fsm.FsmVariables.GetFsmFloat("Sphere Y").Value = currentY + (33.8f - 28.557f);
        }, 0);
        void UpdateLeftRight()
        {
            var currentX = fsm.gameObject.transform.position.x;
            var range = 36.53f - 16.06f;
            fsm.FsmVariables.GetFsmFloat("Left X").Value = currentX - range / 2;
            fsm.FsmVariables.GetFsmFloat("Right X").Value = currentX + range / 2;
        }
        fsm.InsertCustomAction("Aim Jump", UpdateLeftRight, 0);
        fsm.InsertCustomAction("Aim Sphere Jump", UpdateLeftRight, 0);
        var aDashState = fsm.GetState("A Dash");
        for (var i = 0; i < 4; ++i)
        {
            aDashState.RemoveAction(aDashState.Actions.Length - 2);
        }
        fsm.AddAction("A Dash", new WallCheck());
        fsm.RemoveAction("In Air", 0);
        fsm.RemoveAction("In Air", 0);
        fsm.AddAction("In Air", new LandCheck());
        var needle = gameObject.transform.Find("Needle").gameObject;
        needle.AddComponent<CheckHornet>().hornet = gameObject;
        var tink = needle.transform.Find("Needle Tink").gameObject;
        tink.AddComponent<CheckHornet>().hornet = gameObject;
        fsm.AddCustomAction("Throw", () =>
        {
            var instanceInfo = needle.GetAddComponent<Deploy.Behaviors.InstanceInfo>();
            var parentInstanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
            instanceInfo.creator = gameObject;
            instanceInfo.status = Deploy.Behaviors.InstanceInfo.Status.Active;
            instanceInfo.groupID = parentInstanceInfo.groupID;
            instanceInfo.priority = Deploy.Behaviors.InstanceInfo.Priority.SpawnedInstance;
            instanceInfo.damage = parentInstanceInfo.damage;
        });
        fsm.RemoveAction("Barb Throw", 1);
        fsm.AddCustomAction("Barb Throw", () =>
        {
            var barbRegion = fsm.AccessGameObjectVariable("New Barb Region").Value;
            barbRegion.transform.position = gameObject.transform.position;
            barbRegion.transform.Translate(0, 31.18f - 28.557f, 0);
            barbRegion.LocateMyFSM("Spawn Barbs").SendEvent("SPAWN3");
        });
    }
    private GameObject prefab;
    private GameObject barb;
}
