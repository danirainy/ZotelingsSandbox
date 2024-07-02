namespace ZotelingsSandbox.Templates.Standard;
internal class WingedNosk : TemplateBase
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
    private class SwoopLCheck : FsmStateAction
    {
        public override void OnUpdate()
        {
            var myX = Fsm.GameObject.transform.position.x;
            var xMin = Fsm.GetFsmFloat("X Min").Value;
            if (myX < xMin + 3.39f)
            {
                Fsm.Event("FINISHED");
            }
        }
    }
    private class SwoopRCheck : FsmStateAction
    {
        public override void OnUpdate()
        {
            var myX = Fsm.GameObject.transform.position.x;
            var xMax = Fsm.GetFsmFloat("X Max").Value;
            if (myX > xMax - 3.39f)
            {
                Fsm.Event("FINISHED");
            }
        }
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Nosk_Hornet", "Battle Scene"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        var battleScene = Load.Preload(preloadedObjects, "GG_Nosk_Hornet", "Battle Scene");
        prefab = battleScene.transform.Find("Hornet Nosk").gameObject;
        globDropperPrefab = battleScene.transform.Find("Glob Dropper").gameObject;
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("Hornet Nosk");
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
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "HP");
        fsm.AddCustomAction("HP", () =>
        {
            var smartArena = gameObject.AddComponent<SmartArena>();
            smartArena.Build();
            smartArena.minY += 1;
            var smartAttachments = gameObject.AddComponent<SmartAttachments>();
            fsm.FsmVariables.GetFsmFloat("X Min").Value = smartArena.minX;
            fsm.FsmVariables.GetFsmFloat("X Max").Value = smartArena.maxX;
            fsm.FsmVariables.GetFsmFloat("Y Min").Value = smartArena.minY;
            fsm.FsmVariables.GetFsmFloat("Y Max").Value = smartArena.minY + 12;
            fsm.FsmVariables.GetFsmFloat("Swoop Height").Value = fsm.FsmVariables.GetFsmFloat("Y Min").Value + (14.9f - 16.65f);
            var instanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
            var properties = new PlaceConfig
            {
                groupID = instanceInfo.groupID,
                hp = Deploy.Common.DefaultEnemyHealth,
                damage = instanceInfo.damage,
                hpBar = false
            };
            var globDropper = UnityEngine.Object.Instantiate(globDropperPrefab);
            for (int i = 0; i < globDropper.transform.childCount; ++i)
            {
                var dropperChild = globDropper.transform.GetChild(i);
                var originalMinX = 28;
                var originalMaxX = 66;
                var originalMinY = 17;
                var currentMinX = smartArena.minX;
                var currentMaxX = smartArena.maxX;
                var currentMinY = smartArena.minY;
                var currentPos = dropperChild.transform.position;
                currentPos.x = currentMinX + (currentMaxX - currentMinX) * (currentPos.x - originalMinX) / (originalMaxX - originalMinX);
                currentPos.y = currentMinY + (currentPos.y - originalMinY);
                dropperChild.transform.position = currentPos;
                var render = dropperChild.GetComponent<SpriteRenderer>();
                render.color = new Color(1, 1, 1, 0);
            }
            Deploy.RewriteInstance.Rewrite(globDropper, properties, gameObject);
            smartAttachments.attachments.Add(globDropper);
            fsm.FsmVariables.GetFsmGameObject("Glob Dropper").Value = globDropper;
        });
        fsm.RemoveTransition("HP", "FINISHED");
        fsm.AddTransition("HP", "FINISHED", "Idle");
        foreach (var stateName in new List<string> { "Swoop L", "Swoop R" })
        {
            var state = fsm.GetState(stateName);
            state.RemoveAction(5);
            if (stateName.EndsWith("L"))
            {
                state.AddAction(new SwoopLCheck());
            }
            else
            {
                state.AddAction(new SwoopRCheck());
            }
        }
        fsm.RemoveAction("Summon", 1);
        fsm.RemoveAction("Summon", 0);
        fsm.AddCustomAction("Summon", () =>
        {
            var spawned = fsm.FsmVariables.GetFsmGameObject("Spawned Enemy").Value;
            var smartAttachments = gameObject.GetComponent<SmartAttachments>();
            smartAttachments.attachments = smartAttachments.attachments.Where(attachment => attachment != null).ToList();
            if (smartAttachments.attachments.Count >= 5)
            {
                GameObject.Destroy(spawned);
            }
            else
            {
                var healthManager = spawned.GetComponent<HealthManager>();
                healthManager.hp = 1;
                smartAttachments.attachments.Add(spawned);
            }
        });
        fsm.RemoveActionsOfType<SetPosition>("Roof Impact");
        fsm.AddCustomAction("Roof Impact", () =>
        {
            var smartArena = gameObject.GetComponent<SmartArena>();
            var currentPos = gameObject.transform.position;
            currentPos.y = smartArena.minY + 23;
            gameObject.transform.position = currentPos;
        });
        fsm.RemoveActionsOfType<SetPosition>("Roof Return");
        fsm.AddCustomAction("Roof Return", () =>
        {
            var smartArena = gameObject.GetComponent<SmartArena>();
            var currentPos = gameObject.transform.position;
            currentPos.y = smartArena.minY + 10;
            gameObject.transform.position = currentPos;
        });
    }
    private GameObject prefab;
    private GameObject globDropperPrefab;
}
