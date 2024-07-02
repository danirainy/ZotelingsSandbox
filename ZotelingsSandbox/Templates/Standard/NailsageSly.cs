namespace ZotelingsSandbox.Templates.Standard;
internal class NailsageSly : TemplateBase
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
            maxY = float.MaxValue;
            var topRays = new List<Vector2>();
            topRays.Add(new Vector2(col2d.bounds.min.x, col2d.bounds.max.y));
            topRays.Add(new Vector2(col2d.bounds.center.x, col2d.bounds.max.y));
            topRays.Add(col2d.bounds.max);
            for (int i = 0; i < 3; i++)
            {
                RaycastHit2D raycastHit2D = Physics2D.Raycast(topRays[i], Vector2.up, 0.08f, 1 << 8);
                if (raycastHit2D.collider != null)
                {
                    maxY = Mathf.Min(maxY, raycastHit2D.point.y);
                }
            }
            minX += 0.5f;
            maxX -= 0.5f;
            minY += 0.5f;
            maxY -= 0.5f;
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
            if (gameObject.transform.position.y > maxY)
            {
                gameObject.transform.position = new Vector3(gameObject.transform.position.x, maxY, gameObject.transform.position.z);
            }
        }
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
    }
    private class SmartAttachment : MonoBehaviour
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
    private class CycDownCheck : FsmStateAction
    {
        public override void OnUpdate()
        {
            var smartArena = Fsm.GameObject.GetComponent<SmartArena>();
            var targetY = smartArena.minY + (15 - 5.19f);
            if (Fsm.GameObject.transform.position.y < targetY)
            {
                Fsm.Event("END");
            }
        }
    }
    private class NailFallCheck : FsmStateAction
    {
        public override void OnUpdate()
        {
            var deathNail = Fsm.GetFsmGameObject("Death Nail").Value;
            var smartArena = Fsm.GameObject.GetComponent<SmartArena>();
            var targetY = smartArena.minY + (12.32f - 5.19f);
            if (deathNail.transform.position.y <= targetY)
            {
                Fsm.Event("GRABBED");
            }
        }
    }
    private class RageDashCheck : FsmStateAction
    {
        public override void OnUpdate()
        {
            var smartArena = Fsm.GameObject.GetComponent<SmartArena>();
            var myY = Fsm.GameObject.transform.position.y;
            if (myY > smartArena.minY + 12)
            {
                Fsm.Event("ROOF");
            }
        }
    }
    private class CycloneTinkFixer : MonoBehaviour
    {
        private void Start()
        {
            var cycloneTinkFsm = gameObject.LocateMyFSM("Follow");
            cycloneTinkFsm.AddCustomAction("Follow 2", () =>
            {
                var instanceInfo = gameObject.GetAddComponent<Deploy.Behaviors.InstanceInfo>();
                var parentInstanceInfo = parent.GetComponent<Deploy.Behaviors.InstanceInfo>();
                instanceInfo.creator = parent;
                instanceInfo.status = Deploy.Behaviors.InstanceInfo.Status.Active;
                instanceInfo.groupID = parentInstanceInfo.groupID;
                instanceInfo.priority = Deploy.Behaviors.InstanceInfo.Priority.SpawnedInstance;
                instanceInfo.damage = parentInstanceInfo.damage;
                gameObject.AddComponent<SmartAttachment>().parent = parent;
            });
        }
        public GameObject parent;
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Sly", "Battle Scene"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        var battleScene = Load.Preload(preloadedObjects, "GG_Sly", "Battle Scene");
        prefab = battleScene.transform.Find("Sly Boss").gameObject;
        stunNailPrefab = battleScene.transform.Find("Stun Nail").gameObject;
        deathNailPrefab = battleScene.transform.Find("Death Nail").gameObject;
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
            var healthManager = fsm.gameObject.GetComponent<HealthManager>();
            healthManager.IsInvincible = false;
        });
        fsm.AddTransition("Init", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place());
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep());
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Phase HP");
        fsm.AddCustomAction("Phase HP", () =>
        {
            var smartArena = gameObject.AddComponent<SmartArena>();
            smartArena.Build();
            var stunNail = UnityEngine.Object.Instantiate(stunNailPrefab);
            stunNail.SetActive(false);
            stunNail.transform.position = gameObject.transform.position;
            stunNail.AddComponent<SmartAttachment>().parent = gameObject;
            var stunNailFsm = stunNail.LocateMyFSM("Stun Nail");
            stunNailFsm.FsmVariables.GetFsmGameObject("Sly").Value = gameObject;
            fsm.FsmVariables.FindFsmGameObject("Stun Nail").Value = stunNail;
            var deathNail = UnityEngine.Object.Instantiate(deathNailPrefab);
            deathNail.SetActive(false);
            deathNail.transform.position = gameObject.transform.position;
            deathNail.AddComponent<SmartAttachment>().parent = gameObject;
            fsm.FsmVariables.FindFsmGameObject("Death Nail").Value = deathNail;
            var wallspotL = new GameObject("Wallspot L");
            wallspotL.transform.position = new Vector3(smartArena.minX + 2, smartArena.minY, 0);
            wallspotL.AddComponent<SmartAttachment>().parent = gameObject;
            fsm.FsmVariables.FindFsmGameObject("Wallspot L").Value = wallspotL;
            var wallspotR = new GameObject("Wallspot R");
            wallspotR.transform.position = new Vector3(smartArena.maxX - 2, smartArena.minY, 0);
            wallspotR.AddComponent<SmartAttachment>().parent = gameObject;
            fsm.FsmVariables.FindFsmGameObject("Wallspot R").Value = wallspotR;
            var totalHP = gameObject.GetComponent<HealthManager>().hp;
            var stage1HP = totalHP / 3 * 2;
            var stage2HP = totalHP - stage1HP;
            gameObject.GetComponent<HealthManager>().hp = stage1HP;
            fsm.AccessIntVariable("ZotelingsSandbox.Stage2HP").Value = stage2HP;
            var hpBarFinder = gameObject.GetComponent<HPBar.HPBarFinder>();
            if (hpBarFinder != null)
            {
                var hpBarBehavior = hpBarFinder.hpBar.GetComponent<HPBar.Behavior>();
                hpBarBehavior.maxHP = stage1HP;
            }
            var topSlashY = smartArena.minY + 10;
            if (topSlashY > smartArena.maxY)
            {
                topSlashY = smartArena.maxY;
            }
            fsm.FsmVariables.GetFsmFloat("Topslash Y").Value = topSlashY;
        });
        fsm.RemoveTransition("Phase HP", "FINISHED");
        fsm.AddTransition("Phase HP", "FINISHED", "Idle");
        fsm.InsertCustomAction("Dash Jump Launch", () =>
        {
            var smartArena = gameObject.GetComponent<SmartArena>();
            float convertX(float x)
            {
                float oldL = 30.5f;
                float oldR = 62.5f;
                float newL = smartArena.minX;
                float newR = smartArena.maxX;
                return newL + (x - oldL) / (oldR - oldL) * (newR - newL);
            }
            fsm.FsmVariables.GetFsmFloat("End X").Value = convertX(fsm.FsmVariables.GetFsmFloat("End X").Value);
            fsm.FsmVariables.GetFsmFloat("Target X").Value = convertX(fsm.FsmVariables.GetFsmFloat("Target X").Value);
        }, 0);
        fsm.InsertCustomAction("Cyc Jump Launch", () =>
        {
            var smartArena = gameObject.GetComponent<SmartArena>();
            var centerX = (smartArena.minX + smartArena.maxX) / 2;
            var myX = gameObject.transform.position.x;
            fsm.AccessFloatVariable("Jump X").Value = centerX - myX;
        }, 3);
        fsm.RemoveAction("Cyc Down", 1);
        fsm.AddAction("Cyc Down", new CycDownCheck());
        fsm.RemoveAction("Nail Fall", 3);
        fsm.RemoveAction("Nail Fall", 2);
        fsm.RemoveAction("Nail Fall", 0);
        fsm.AddCustomAction("Death Reset", () =>
        {
            var hpBarFinder = gameObject.GetComponent<HPBar.HPBarFinder>();
            if (hpBarFinder != null)
            {
                var hpBarBehavior = hpBarFinder.hpBar.GetComponent<HPBar.Behavior>();
                hpBarBehavior.SetVisible(false);
            }
        });
        fsm.InsertCustomAction("Nail Fall", () =>
        {
            var deathNail = fsm.FsmVariables.FindFsmGameObject("Death Nail").Value;
            var smartArena = gameObject.GetComponent<SmartArena>();
            var centerX = (smartArena.minX + smartArena.maxX) / 2;
            var bottomY = smartArena.minY;
            var targetY = bottomY + (55.8f - 5.19f);
            var oldPos = deathNail.transform.position;
            oldPos.x = centerX;
            oldPos.y = targetY;
            deathNail.transform.position = oldPos;
        }, 0);
        fsm.AddAction("Nail Fall", new NailFallCheck());
        fsm.RemoveAction("Air Catch", 6);
        fsm.InsertCustomAction("Air Catch", () =>
        {
            var smartArena = gameObject.GetComponent<SmartArena>();
            var centerX = (smartArena.minX + smartArena.maxX) / 2;
            var bottomY = smartArena.minY;
            var targetY = bottomY + (11.32f - 5.19f);
            targetY = Mathf.Min(targetY, smartArena.maxY);
            var oldPos = gameObject.transform.position;
            oldPos.x = centerX;
            oldPos.y = targetY;
            gameObject.transform.position = oldPos;
        }, 6);
        fsm.RemoveAction("Air Roar", 3);
        fsm.RemoveAction("Acended HP", 1);
        fsm.RemoveAction("Acended HP", 0);
        fsm.AddCustomAction("Acended HP", () =>
        {
            var healthManager = gameObject.GetComponent<HealthManager>();
            healthManager.hp = fsm.AccessIntVariable("ZotelingsSandbox.Stage2HP").Value;
            var hpBarFinder = gameObject.GetComponent<HPBar.HPBarFinder>();
            if (hpBarFinder != null)
            {
                var hpBarBehavior = hpBarFinder.hpBar.GetComponent<HPBar.Behavior>();
                hpBarBehavior.SetVisible(true);
            }
        });
        fsm.AddAction("Rage Dash", new RageDashCheck());
        foreach (var stateName in new[] { "Bounce L", "Bounce R", "Bounce U", "Bounce D" })
        {
            var state = fsm.GetState(stateName);
            state.Actions = state.Actions.Where(action => action is not SetPosition).ToArray();
            var thisStateName = stateName;
            state.AddCustomAction(() =>
            {
                var myPos = gameObject.transform.position;
                switch (thisStateName)
                {
                    case "Bounce L":
                        myPos.x += 1;
                        break;
                    case "Bounce R":
                        myPos.x -= 1;
                        break;
                    case "Bounce U":
                        myPos.y -= 1;
                        break;
                    case "Bounce D":
                        myPos.y += 1;
                        break;
                }
                gameObject.transform.position = myPos;
            });
        }
        fsm.AddCustomAction("Explosion", () =>
        {
            GameObject.Destroy(gameObject.transform.Find("ZotelingsSandbox.DamageEnemy").gameObject);
            gameObject.RemoveComponent<DamageHero>();
        });
        fsm.RemoveAction("Battle End", 0);
        fsm.AddCustomAction("Battle End", () =>
        {
            GameObject.Destroy(gameObject);
        });
        var spinTink = gameObject.transform.Find("Spin Tink").gameObject;
        var spinTinkFsm = spinTink.LocateMyFSM("Follow");
        spinTinkFsm.AddCustomAction("Follow 2", () =>
        {
            var instanceInfo = spinTink.GetAddComponent<Deploy.Behaviors.InstanceInfo>();
            var parentInstanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
            instanceInfo.creator = gameObject;
            instanceInfo.status = Deploy.Behaviors.InstanceInfo.Status.Active;
            instanceInfo.groupID = parentInstanceInfo.groupID;
            instanceInfo.priority = Deploy.Behaviors.InstanceInfo.Priority.SpawnedInstance;
            instanceInfo.damage = parentInstanceInfo.damage;
            spinTink.AddComponent<SmartAttachment>().parent = gameObject;
        });
        var cycloneTink = gameObject.transform.Find("Cyclone Tink").gameObject;
        cycloneTink.AddComponent<CycloneTinkFixer>().parent = gameObject;
    }
    private GameObject prefab;
    private GameObject stunNailPrefab;
    private GameObject deathNailPrefab;
}
