namespace ZotelingsSandbox.Templates.Standard;
internal class GreyPrinceZote : TemplateBase
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
    private class JSCheck : FsmStateAction
    {
        public override void OnUpdate()
        {
            var currentY = Fsm.GetFsmFloat("Current Y").Value;
            var targetY = currentY + (17.5f - 8.6885f);
            if (Fsm.GameObject.transform.position.y > targetY)
            {
                Fsm.Event("JUMP SLASH");
            }
        }
    }
    private class ZotelingManager : MonoBehaviour
    {
        private void Update()
        {
            zotelings = zotelings.Where(zoteling => zoteling != null).ToList();
        }
        public List<GameObject> zotelings;
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Grey_Prince_Zote", "Grey Prince"),
        ("GG_Grey_Prince_Zote", "Zote Balloon"),
        ("GG_Grey_Prince_Zote", "Zoteling"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        prefab = Load.Preload(preloadedObjects, "GG_Grey_Prince_Zote", "Grey Prince");
        balloon = Load.Preload(preloadedObjects, "GG_Grey_Prince_Zote", "Zote Balloon");
        zoteling = Load.Preload(preloadedObjects, "GG_Grey_Prince_Zote", "Zoteling");
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        UnityEngine.Object.Destroy(gameObject.LocateMyFSM("Constrain X"));
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
            var healthManager = fsm.gameObject.GetComponent<HealthManager>();
            healthManager.IsInvincible = false;
        });
        fsm.AddTransition("Pause", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place(true));
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep(true));
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Init");
        fsm.InsertCustomAction("Init", () =>
        {
            var smartArena = gameObject.AddComponent<SmartArena>();
            smartArena.Build();
            var zotelingManager = gameObject.AddComponent<ZotelingManager>();
            zotelingManager.zotelings = new List<GameObject>();
            var chargeTink = gameObject.transform.Find("Charge Tink").gameObject;
            chargeTink.AddComponent<SmartAttachment>().parent = gameObject;
        }, 0);
        fsm.RemoveTransition("Init", "FINISHED");
        fsm.AddTransition("Init", "FINISHED", "Level 3");
        fsm.RemoveAction("Level 3", 0);
        fsm.RemoveTransition("Level 3", "FINISHED");
        fsm.AddTransition("Level 3", "FINISHED", "Activate");
        fsm.InsertCustomAction("Idle Start", () =>
        {
            var currentX = fsm.gameObject.transform.position.x;
            fsm.FsmVariables.GetFsmFloat("Left X").Value = currentX - (45.61f - 7.19f) / 2;
            fsm.FsmVariables.GetFsmFloat("Right X").Value = currentX + (45.61f - 7.19f) / 2;
            var currentY = fsm.gameObject.transform.position.y;
            fsm.AccessFloatVariable("Current Y").Value = currentY;
            fsm.FsmVariables.GetFsmFloat("Shockwave Y").Value = currentY + (4.9f - 8.6885f);
        }, 0);
        fsm.AddCustomAction("FT Through", () =>
        {
            var velocity = fsm.gameObject.GetComponent<Rigidbody2D>().velocity;
            velocity.y = 50;
            fsm.gameObject.GetComponent<Rigidbody2D>().velocity = velocity;
            var collider = fsm.gameObject.GetComponent<BoxCollider2D>();
            collider.isTrigger = false;
            var rigidbody = fsm.gameObject.GetComponent<Rigidbody2D>();
            fsm.AccessFloatVariable("Old Gravity").Value = rigidbody.gravityScale;
            rigidbody.gravityScale = 0;
            var tk2dSprite = fsm.gameObject.GetComponent<tk2dSprite>();
            tk2dSprite.color = Color.clear;
        });
        fsm.RemoveAction("FT Fall", 4);
        fsm.AddCustomAction("FT Fall", () =>
        {
            var selfX = fsm.FsmVariables.GetFsmFloat("Self X").Value;
            fsm.gameObject.transform.position = new Vector3(selfX, fsm.gameObject.transform.position.y, fsm.gameObject.transform.position.z);
            var rigidbody = fsm.gameObject.GetComponent<Rigidbody2D>();
            rigidbody.gravityScale = fsm.AccessFloatVariable("Old Gravity").Value;
            var tk2dSprite = fsm.gameObject.GetComponent<tk2dSprite>();
            tk2dSprite.color = Color.white;
        });
        var fixShockwave = () =>
        {
            var shockWave = fsm.FsmVariables.GetFsmGameObject("Shockwave").Value;
            shockWave.transform.position = new Vector3(shockWave.transform.position.x, fsm.FsmVariables.GetFsmFloat("Shockwave Y").Value - 0.1f, shockWave.transform.position.z);
        };
        fsm.InsertCustomAction("Ft Waves", fixShockwave, 11);
        fsm.InsertCustomAction("Ft Waves", fixShockwave, 5);
        fsm.InsertCustomAction("Slash Waves L", fixShockwave, 3);
        fsm.InsertCustomAction("Slash Waves R", fixShockwave, 3);
        fsm.RemoveAction("JS In Air", 3);
        fsm.AddAction("JS In Air", new JSCheck());
        fsm.RemoveAction("B Roar", 4);
        fsm.AddCustomAction("B Roar", () =>
        {
            var refZoteX = 26;
            var refZoteY = 8.6885f;
            var refBalloonY = 9.55f;
            var actualZoteX = fsm.gameObject.transform.position.x;
            var actualZoteY = fsm.gameObject.transform.position.y;
            foreach (var refBalloonX in new List<float> { 12.48f, 21.48f, 31.48f, 39.47f })
            {
                var thisBallonX = actualZoteX + (refBalloonX - refZoteX);
                var thisBallonY = actualZoteY + (refBalloonY - refZoteY);
                var thisBallon = UnityEngine.Object.Instantiate(balloon, new Vector3(thisBallonX, thisBallonY, 0), Quaternion.Euler(Vector3.zero));
                var balloonFsm = thisBallon.LocateMyFSM("Control");
                balloonFsm.RemoveAction("GG Damage", 0);
                balloonFsm.AddCustomAction("Dormant", () =>
                {
                    balloonFsm.SendEvent("BALLOON SPAWN");
                });
                balloonFsm.AddCustomAction("Reset", () =>
                {
                    UnityEngine.GameObject.Destroy(thisBallon);
                });
                var instanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
                var properties = new PlaceConfig
                {
                    groupID = instanceInfo.groupID,
                    hp = Deploy.Common.DefaultEnemyHealth,
                    damage = instanceInfo.damage,
                    hpBar = false
                };
                Deploy.RewriteInstance.Rewrite(thisBallon, properties, gameObject);
                thisBallon.AddComponent<SmartAttachment>().parent = gameObject;
            }
        });
        fsm.RemoveAction("Spit Antic", 0);
        fsm.RemoveAction("Spit Antic", 0);
        fsm.RemoveAction("Spit Antic", 0);
        fsm.RemoveAction("Spit Antic", 0);
        fsm.InsertCustomAction("Spit Antic", () =>
        {
            var zotelingManager = gameObject.GetComponent<ZotelingManager>();
            if (zotelingManager.zotelings.Count >= 3)
            {
                fsm.SendEvent("CANCEL");
            }
        }, 0);
        void summonZoteling()
        {
            var thisZoteling = UnityEngine.Object.Instantiate(zoteling);
            thisZoteling.transform.position = gameObject.transform.position;
            var zotelingFsm = thisZoteling.LocateMyFSM("Control");
            zotelingFsm.RemoveAction("GG Damage", 0);
            zotelingFsm.AddCustomAction("Reset", () =>
            {
                UnityEngine.GameObject.Destroy(thisZoteling);
            });
            var instanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
            var properties = new PlaceConfig
            {
                groupID = instanceInfo.groupID,
                hp = Deploy.Common.DefaultEnemyHealth,
                damage = instanceInfo.damage,
                hpBar = false
            };
            Deploy.RewriteInstance.Rewrite(thisZoteling, properties, gameObject);
            fsm.FsmVariables.GetFsmGameObject("Zoteling").Value = thisZoteling;
            var zotelingManager = gameObject.GetComponent<ZotelingManager>();
            zotelingManager.zotelings.Add(thisZoteling);
            var thisArena = thisZoteling.AddComponent<SmartArena>();
            var parentArena = gameObject.GetComponent<SmartArena>();
            thisArena.minX = parentArena.minX;
            thisArena.maxX = parentArena.maxX;
            thisArena.minY = parentArena.minY - 3;
            thisZoteling.AddComponent<SmartAttachment>().parent = gameObject;
        }
        fsm.InsertCustomAction("Spit L", summonZoteling, 1);
        fsm.InsertCustomAction("Spit R", summonZoteling, 1);
        fsm.AddCustomAction("Stun Start", () =>
        {
            var tk2dSprite = fsm.gameObject.GetComponent<tk2dSprite>();
            tk2dSprite.color = Color.white;
            var stompHit = fsm.FsmVariables.GetFsmGameObject("Stomp Hit").Value;
            stompHit.SetActive(false);
        });
    }
    private GameObject prefab;
    private GameObject balloon;
    private GameObject zoteling;
}
