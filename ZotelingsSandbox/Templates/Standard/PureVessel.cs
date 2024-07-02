namespace ZotelingsSandbox.Templates.Standard;
internal class PureVessel : TemplateBase
{
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
        public float minX;
        public float maxX;
        public float minY;
    }
    private class DestroyFocusBlasts : MonoBehaviour
    {
        private void Update()
        {
            if (pureVessel == null)
            {
                Destroy(gameObject);
            }
        }
        public GameObject pureVessel;
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Hollow_Knight", "Battle Scene")
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        var battleScene = Load.Preload(preloadedObjects, "GG_Hollow_Knight", "Battle Scene");
        prefab = battleScene.transform.Find("HK Prime").gameObject;
        blastPrefab = battleScene.transform.Find("Focus Blasts").gameObject;
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var scale = gameObject.transform.localScale;
        scale.x = Mathf.Abs(scale.x);
        gameObject.transform.localScale = scale;
        var fsm = gameObject.LocateMyFSM("Control");
        fsm.RemoveTransition("Pause", "FINISHED");
        fsm.AddState(Deploy.Common.PlacingStateName);
        fsm.AddCustomAction(Deploy.Common.PlacingStateName, () =>
        {
            var animator = fsm.gameObject.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Idle");
            var rigidbody = gameObject.GetComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            var collider = fsm.gameObject.transform.Find("Colliders").Find("Idle").gameObject;
            collider.SetActive(true);
            var healthManager = fsm.gameObject.GetComponent<HealthManager>();
            healthManager.IsInvincible = false;
        });
        fsm.AddTransition("Pause", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place(true));
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep(true));
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Set Phase HP");
        fsm.RemoveTransition("Set Phase HP", "FINISHED");
        fsm.AddTransition("Set Phase HP", "FINISHED", "Init");
        fsm.InsertCustomAction("Init", () =>
        {
            var smartArena = gameObject.AddComponent<SmartArena>();
            smartArena.Build();
        }, 0);
        fsm.RemoveTransition("Init", "FINISHED");
        fsm.AddTransition("Init", "FINISHED", "Intro Idle");
        fsm.InsertCustomAction("Idle", () =>
        {
            var currentY = fsm.gameObject.transform.position.y;
            var targetY = currentY + (9.2f - 9.1426f) + 0.5f;
            fsm.FsmVariables.GetFsmFloat("Stun Land Y").Value = targetY;
            targetY = currentY + (4.2f - 9.1426f);
            fsm.FsmVariables.GetFsmFloat("Plume Y").Value = targetY;
        }, 0);
        foreach (var i in new[] { 5, 3 })
        {
            fsm.InsertCustomAction("Plume Gen", () =>
            {
                var plume = fsm.FsmVariables.GetFsmGameObject("Plume").Value;
                var plumeFsm = plume.GetComponent<PlayMakerFSM>();
                var state = plumeFsm.GetState("Outside Arena?");
                state.RemoveTransition("OUTSIDE");
                state.AddTransition("OUTSIDE", "Antic");
            }, i);
        }
        fsm.RemoveAction("Pos Check", 3);
        fsm.RemoveAction("Pos Check", 2);
        fsm.AddCustomAction("Pos Check", () =>
        {
            if (UnityEngine.Random.Range(0, 1f) < 1.0f / 2)
            {
                fsm.SetState("Tele Out");
            }
        });
        fsm.InsertCustomAction("Focus Burst", () =>
        {
            var blast = UnityEngine.Object.Instantiate(blastPrefab);
            blast.transform.position = new Vector3(fsm.gameObject.transform.position.x, fsm.gameObject.transform.position.y, blast.transform.position.z);
            foreach (var blastFsm_ in blast.GetComponentsInChildren<PlayMakerFSM>(true))
            {
                var blastFsm = blastFsm_;
                blastFsm.SendEvent("BLAST");
                void UpdateY()
                {
                    var currentY = fsm.gameObject.transform.position.y;
                    blastFsm.FsmVariables.GetFsmFloat("Spawn Y").Value += currentY - 9.1426f;
                }
                blastFsm.InsertCustomAction("Pos Low", UpdateY, 1);
                blastFsm.InsertCustomAction("Pos High", UpdateY, 1);
            }
            var instanceInfo = fsm.gameObject.GetComponentInParent<Deploy.Behaviors.InstanceInfo>(true);
            var properties = new PlaceConfig
            {
                groupID = instanceInfo.groupID,
                hp = Deploy.Common.DefaultEnemyHealth,
                damage = instanceInfo.damage,
                hpBar = false
            };
            Deploy.RewriteInstance.Rewrite(blast, properties, fsm.gameObject);
            blast.transform.position = new Vector3(fsm.gameObject.transform.position.x, fsm.gameObject.transform.position.y, blast.transform.position.z);
            blast.AddComponent<DestroyFocusBlasts>().pureVessel = fsm.gameObject;
            var blastVariable = fsm.AccessGameObjectVariable("ZotelingsSandbox.FocusBlasts");
            if (blastVariable.Value != null)
            {
                UnityEngine.Object.Destroy(blastVariable.Value);
            }
            blastVariable.Value = blast;
        }, 0);
        fsm.AddCustomAction("Tele Out", () =>
        {
            fsm.FsmVariables.GetFsmFloat("TeleRange Min").Value = fsm.FsmVariables.GetFsmFloat("SelfRange Min").Value;
            fsm.FsmVariables.GetFsmFloat("TeleRange Max").Value = fsm.FsmVariables.GetFsmFloat("SelfRangeMax").Value;
        });
        fsm.AddCustomAction("TelePos Focus", () =>
        {
            var heroX = fsm.FsmVariables.GetFsmGameObject("Hero").Value.transform.position.x;
            float targetX;
            do
            {
                targetX = UnityEngine.Random.Range(-6f, 6f) + heroX;
            } while (Mathf.Abs(targetX - heroX) < 3);
            fsm.FsmVariables.GetFsmFloat("Tele X").Value = targetX;
        });
        fsm.RemoveAction("TelePos Dstab", 4);
    }
    private GameObject prefab;
    private GameObject blastPrefab;
}
