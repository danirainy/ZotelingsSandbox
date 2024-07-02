namespace ZotelingsSandbox.Templates.Standard;
internal class FalseKnight : TemplateBase
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
    private class CheckHealth : MonoBehaviour
    {
        private void Update()
        {
            if (gameObject.GetComponent<HealthManager>().hp <= 0)
            {
                var fsm = gameObject.LocateMyFSM("FalseyControl");
                fsm.SendEvent("STUN");
                gameObject.GetComponent<HealthManager>().hp = fsm.AccessIntVariable("Stage HP").Value;
            }
        }
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_False_Knight", "Battle Scene"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        var battleScene = Load.Preload(preloadedObjects, "GG_False_Knight", "Battle Scene");
        prefab = battleScene.transform.Find("False Knight New").gameObject;
        rocksPrefab = battleScene.transform.Find("FK Barrel Summon").gameObject;
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("FalseyControl");
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
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Dormant");
        fsm.RemoveAction("Dormant", 1);
        fsm.RemoveAction("Dormant", 0);
        fsm.AddCustomAction("Dormant", () =>
        {
            var smartArena = gameObject.AddComponent<SmartArena>();
            smartArena.Build();
            var xMin = smartArena.minX;
            var xMax = smartArena.maxX;
            fsm.FsmVariables.GetFsmFloat("Final Point X").Value = xMin + (xMax - xMin) * 0.65f;
            fsm.FsmVariables.GetFsmFloat("Rage Point X").Value = xMin + (xMax - xMin) * 0.5f;
            fsm.FsmVariables.GetFsmFloat("Rage Min").Value = Math.Min(xMin + 1, xMax);
            fsm.FsmVariables.GetFsmFloat("Rage Max").Value = Math.Max(xMax - 1, xMin);
            var smartAttachments = gameObject.AddComponent<SmartAttachments>();
            var totalHP = gameObject.GetComponent<HealthManager>().hp;
            var stageHP = totalHP / 3;
            fsm.AccessIntVariable("Stage HP").Value = stageHP;
            gameObject.GetComponent<HealthManager>().hp = stageHP;
            var hpBarFinder = gameObject.GetComponent<HPBar.HPBarFinder>();
            if (hpBarFinder != null)
            {
                var hpBarBehavior = hpBarFinder.hpBar.GetComponent<HPBar.Behavior>();
                hpBarBehavior.maxHP = stageHP;
            }
            gameObject.AddComponent<CheckHealth>();
            var rocks = UnityEngine.Object.Instantiate(rocksPrefab);
            Deploy.RewriteInstance.Rewrite(rocks, properties, gameObject);
            smartAttachments.attachments.Add(rocks);
            rocks.transform.position = new Vector3((smartArena.minX + smartArena.maxX) / 2, smartArena.minX + 8, 0);
            var rocksFsm = rocks.LocateMyFSM("summon");
            rocksFsm.FsmVariables.FindFsmFloat("Summon Min").Value = smartArena.minX;
            rocksFsm.FsmVariables.FindFsmFloat("Summon Max").Value = smartArena.maxX;
            fsm.FsmVariables.GetFsmGameObject("Barrel Summoner").Value = rocks;
        });
        fsm.AddCustomAction("Dormant", () =>
        {
            fsm.SendEvent("BATTLE START");
        });
        fsm.RemoveTransition("Dormant", "BATTLE START");
        fsm.AddTransition("Dormant", "BATTLE START", "Idle");
        fsm.RemoveAction("Check", 0);
        fsm.AddCustomAction("Check", () =>
        {
            fsm.SendEvent("FINISHED");
        });
        fsm.RemoveAction("Check If GG", 1);
        fsm.AddCustomAction("Check If GG", () =>
        {
            fsm.SendEvent("GG BOSS");
        });
        fsm.RemoveAction("Boss Death Sting", 1);
        fsm.RemoveAction("Blow", 1);
        fsm.RemoveAction("Cough", 2);
        fsm.RemoveAction("Cough", 1);
        fsm.AddCustomAction("Cough", () =>
        {
            gameObject.AddComponent<Deploy.Behaviors.ClearCorpse>();
        });
        GameObject.Destroy(gameObject.LocateMyFSM("Check Health"));
        GameObject.Destroy(gameObject.transform.Find("FK Terrain Block").gameObject);
        var rigidbody = gameObject.GetComponent<Rigidbody2D>();
        rigidbody.gravityScale = 1;
        var head = gameObject.transform.Find("Head").gameObject;
        head.SetActive(false);
        fsm.AddCustomAction("Open Uuup", () =>
        {
            head.SetActive(true);
        });
    }
    private GameObject prefab;
    private GameObject rocksPrefab;
}
