namespace ZotelingsSandbox.Templates.Standard;
internal class HiveKnight : TemplateBase
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
    private class SmartAttachments : MonoBehaviour
    {
        private void OnDestroy()
        {
            foreach (var attachment in attachments)
            {
                Destroy(attachment);
            }
        }
        public List<GameObject> attachments = [];
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Hive_Knight", "Battle Scene"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        var battleScene = Load.Preload(preloadedObjects, "GG_Hive_Knight", "Battle Scene");
        prefab = battleScene.transform.Find("Hive Knight").gameObject;
        globsPrefab = battleScene.transform.Find("Globs").gameObject;
        droppersPrefab = battleScene.transform.Find("Droppers").gameObject;
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
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Activate");
        fsm.AddCustomAction("Activate", () =>
        {
            var smartArena = gameObject.AddComponent<SmartArena>();
            smartArena.Build();
            var smartAttachments = gameObject.AddComponent<SmartAttachments>();
            fsm.FsmVariables.GetFsmFloat("Left X").Value = smartArena.minX;
            fsm.FsmVariables.GetFsmFloat("Right X").Value = smartArena.maxX;
            fsm.FsmVariables.GetFsmFloat("Ground Y").Value = smartArena.minY;
            fsm.FsmVariables.GetFsmInt("P2 HP").Value = 9999;
            fsm.FsmVariables.GetFsmInt("P3 HP").Value = 9999;
            var instanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
            var properties = new PlaceConfig
            {
                groupID = instanceInfo.groupID,
                hp = Deploy.Common.DefaultEnemyHealth,
                damage = instanceInfo.damage,
                hpBar = false
            };
            var globs = UnityEngine.Object.Instantiate(globsPrefab);
            for (int i = 0; i < globs.transform.childCount; ++i)
            {
                var glob = globs.transform.GetChild(i).gameObject;
                var childInstanceInfo = glob.GetAddComponent<Deploy.Behaviors.InstanceInfo>();
                childInstanceInfo.creator = gameObject;
                childInstanceInfo.status = Deploy.Behaviors.InstanceInfo.Status.Active;
                childInstanceInfo.groupID = instanceInfo.groupID;
                childInstanceInfo.priority = Deploy.Behaviors.InstanceInfo.Priority.SpawnedInstance;
                childInstanceInfo.damage = instanceInfo.damage;
                smartAttachments.attachments.Add(glob);
            }
            Deploy.RewriteInstance.Rewrite(globs, properties, gameObject);
            smartAttachments.attachments.Add(globs);
            globs.transform.position = new Vector3((smartArena.minX + smartArena.maxX) / 2, smartArena.minY - 2, 0);
            fsm.FsmVariables.GetFsmGameObject("Globs Container").Value = globs;
            var droppers = UnityEngine.Object.Instantiate(droppersPrefab);
            for (int i = 0; i < droppers.transform.childCount; ++i)
            {
                var dropper = droppers.transform.GetChild(i).gameObject;
                dropper.transform.localPosition = gameObject.transform.position;
                var dropperFsm = dropper.LocateMyFSM("Control");
                dropperFsm.AccessFloatVariable("Start Y").Value = smartArena.minY + 15;
                dropperFsm.AccessFloatVariable("X Left").Value = smartArena.minX;
                dropperFsm.AccessFloatVariable("X Right").Value = smartArena.maxX;
                dropperFsm.GetAction<FloatCompare>("Swarm", 3).float2.Value = smartArena.minY - 5;
            }
            Deploy.RewriteInstance.Rewrite(droppers, properties, gameObject);
            smartAttachments.attachments.Add(droppers);
            droppers.transform.position = Vector3.zero;
            fsm.FsmVariables.GetFsmGameObject("Droppers").Value = droppers;
            {
                var mouthSwarm = gameObject.transform.Find("Mouth Swarm").gameObject;
                var childInstanceInfo = mouthSwarm.GetAddComponent<Deploy.Behaviors.InstanceInfo>();
                childInstanceInfo.creator = gameObject;
                childInstanceInfo.status = Deploy.Behaviors.InstanceInfo.Status.Active;
                childInstanceInfo.groupID = instanceInfo.groupID;
                childInstanceInfo.priority = Deploy.Behaviors.InstanceInfo.Priority.SpawnedInstance;
                childInstanceInfo.damage = instanceInfo.damage;
                smartAttachments.attachments.Add(mouthSwarm);
            }
        });
    }
    private GameObject prefab;
    private GameObject globsPrefab;
    private GameObject droppersPrefab;
}
