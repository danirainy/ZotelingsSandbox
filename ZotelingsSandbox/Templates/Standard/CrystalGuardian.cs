namespace ZotelingsSandbox.Templates.Standard;
internal class CrystalGuardian : TemplateBase
{
    private class SmartArena : MonoBehaviour
    {
        public void Build()
        {
            minX = float.MinValue;
            maxX = float.MaxValue;
            minY = float.MinValue;
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
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Crystal_Guardian", "Mega Zombie Beam Miner (1)"),
        ("GG_Crystal_Guardian", "Laser Turret Mega (1)"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        prefab = Load.Preload(preloadedObjects, "GG_Crystal_Guardian", "Mega Zombie Beam Miner (1)");
        laser = Load.Preload(preloadedObjects, "GG_Crystal_Guardian", "Laser Turret Mega (1)");
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("Beam Miner");
        fsm.RemoveTransition("Pause Frame", "FINISHED");
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
        fsm.AddTransition("Pause Frame", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place());
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep());
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Init");
        fsm.RemoveTransition("Deparents", "FINISHED");
        fsm.AddCustomAction("Deparents", () =>
        {
            var beamImpact = fsm.FsmVariables.GetFsmGameObject("Beam Impact").Value;
            var beamBall = fsm.FsmVariables.GetFsmGameObject("Beam Ball").Value;
            var beam = fsm.FsmVariables.GetFsmGameObject("Beam").Value;
            void process(GameObject obj)
            {
                var instanceInfo = obj.AddComponent<Deploy.Behaviors.InstanceInfo>();
                var parentInstanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
                instanceInfo.creator = gameObject;
                instanceInfo.status = Deploy.Behaviors.InstanceInfo.Status.Active;
                instanceInfo.groupID = parentInstanceInfo.groupID;
                instanceInfo.priority = Deploy.Behaviors.InstanceInfo.Priority.SpawnedInstance;
                instanceInfo.damage = parentInstanceInfo.damage;
                obj.AddComponent<SmartAttachment>().parent = gameObject;
            }
            process(beamImpact);
            process(beamBall);
            process(beam);
            GameObject makeLaser()
            {
                var thisLaser = UnityEngine.Object.Instantiate(laser);
                thisLaser.transform.Find("Beam").gameObject.AddComponent<BoxCollider2D>().enabled = false;
                var instanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
                var properties = new PlaceConfig
                {
                    groupID = instanceInfo.groupID,
                    hp = Deploy.Common.DefaultEnemyHealth,
                    damage = instanceInfo.damage,
                    hpBar = false
                };
                Deploy.RewriteInstance.Rewrite(thisLaser, properties, gameObject);
                thisLaser.AddComponent<SmartAttachment>().parent = gameObject;
                return thisLaser;
            }
            fsm.AccessGameObjectVariable("Laser 1").Value = makeLaser();
            fsm.AccessGameObjectVariable("Laser 2").Value = makeLaser();
            fsm.AccessGameObjectVariable("Laser 3").Value = makeLaser();
            fsm.AccessGameObjectVariable("Laser 4").Value = makeLaser();
            gameObject.AddComponent<SmartArena>().Build();
        });
        fsm.AddTransition("Deparents", "FINISHED", "Battle Init");
        fsm.RemoveAction("Battle Init", 3);
        fsm.RemoveAction("Battle Init", 1);
        fsm.RemoveTransition("Battle Init", "FINISHED");
        fsm.AddTransition("Battle Init", "FINISHED", "Idle");
        fsm.InsertCustomAction("Idle", () =>
        {
            var currentX = fsm.gameObject.transform.position.x;
            fsm.FsmVariables.GetFsmFloat("Jump Min X").Value = currentX - (40.01f - 16.72f) / 2;
            fsm.FsmVariables.GetFsmFloat("Jump Max X").Value = currentX + (40.01f - 16.72f) / 2;
        }, 0);
        fsm.RemoveAction("Roar Start", 1);
        fsm.RemoveAction("Lasers", 0);
        fsm.AddCustomAction("Lasers", () =>
        {
            var currentX = fsm.gameObject.transform.position.x;
            var currentY = fsm.gameObject.transform.position.y;
            var baseY = 12.4803f;
            var oldXs = new List<float> { 19.91f, 25.97f, 33.06f, 38.35f };
            var oldY = 24.62f;
            for (int i = 1; i <= 4; ++i)
            {
                var thisLaser = fsm.AccessGameObjectVariable("Laser " + i).Value;
                var position = thisLaser.transform.position;
                position.x = currentX + oldXs[i - 1] - (33.06f + 25.97f) / 2;
                position.y = currentY + oldY - baseY;
                thisLaser.transform.position = position;
                thisLaser.LocateMyFSM("Laser Bug Mega").SendEvent("LASER SHOOT");
            }
        });
        fsm.InsertCustomAction("Range Check", () => { fsm.SendEvent("FINISHED"); }, 0);
        var camLock = gameObject.transform.Find("Cam Lock");
        camLock.gameObject.SetActive(false);
        var beam = gameObject.transform.Find("Beam").gameObject;
        beam.AddComponent<BoxCollider2D>().enabled = false;
        var beamPointL = gameObject.transform.Find("Beam Point L");
        beamPointL.gameObject.AddComponent<SmartAttachment>().parent = gameObject;
        var beamPointR = gameObject.transform.Find("Beam Point R");
        beamPointR.gameObject.AddComponent<SmartAttachment>().parent = gameObject;
        var crystalRain = gameObject.transform.Find("Crystal Rain");
        crystalRain.gameObject.AddComponent<SmartAttachment>().parent = gameObject;
    }
    private GameObject prefab;
    private GameObject laser;
}
