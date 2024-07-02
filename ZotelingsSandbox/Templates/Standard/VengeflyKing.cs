namespace ZotelingsSandbox.Templates.Standard;
internal class VengeflyKing : TemplateBase
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
            minX += 1;
            maxX -= 1;
            minY += 1;
            maxY = minY + 12;
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
    internal class SummonManager : MonoBehaviour
    {
        private void Update()
        {
            summons = summons.Where(zoteling => zoteling != null).ToList();
        }
        public List<GameObject> summons;
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Vengefly", "Giant Buzzer Col"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        prefab = Load.Preload(preloadedObjects, "GG_Vengefly", "Giant Buzzer Col");
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("Big Buzzer");
        fsm.RemoveAction("Init", 3);
        fsm.RemoveTransition("Init", "FINISHED");
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
        fsm.AddTransition("Init", "FINISHED", Deploy.Common.PlacingStateName);
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place());
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep());
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Set GG");
        fsm.RemoveAction("Set GG", 3);
        fsm.RemoveAction("Set GG", 0);
        fsm.AddCustomAction("Set GG", () =>
        {
            var smartArena = gameObject.AddComponent<SmartArena>();
            smartArena.Build();
            fsm.FsmVariables.GetFsmFloat("Swoop Height").Value = smartArena.minY + 1;
            var zotelingManager = gameObject.AddComponent<SummonManager>();
            zotelingManager.summons = new List<GameObject>();
        });
        fsm.RemoveTransition("Set GG", "FINISHED");
        fsm.AddTransition("Set GG", "FINISHED", "Swoop Rise");
        fsm.RemoveAction("Check Summon", 4);
        fsm.InsertCustomAction("Check Summon", () =>
        {
            var n = gameObject.GetComponent<SummonManager>().summons.Count;
            if (n >= 4)
            {
                if (fsm.FsmVariables.GetFsmInt("Swoops in A Row").Value > 3)
                {
                    fsm.FsmVariables.GetFsmInt("Swoops in A Row").Value = 3;
                }
                fsm.SendEvent("CANCEL");
            }
        }, 0);
        fsm.RemoveTransition("Check Summon", "FINISHED");
        fsm.AddTransition("Check Summon", "FINISHED", "Summon Antic");
        summonPrefab = (fsm.GetState("Summon").Actions[1] as CreateObject).gameObject.Value;
        fsm.RemoveAction("Summon", 3);
        fsm.RemoveAction("Summon", 2);
        fsm.RemoveAction("Summon", 1);
        fsm.RemoveAction("Summon", 0);
        void summon(Vector2 position, int scale)
        {
            var smartArena = gameObject.GetComponent<SmartArena>();
            var lX = smartArena.minX + 1;
            var rX = smartArena.maxX - 1;
            if (lX <= rX)
            {
                position.x = Mathf.Clamp(position.x, lX, rX);
            }
            position.y = Mathf.Max(position.y, smartArena.minY);
            var thisSummon = UnityEngine.Object.Instantiate(summonPrefab);
            var oldPosition = thisSummon.transform.position;
            oldPosition.x = position.x;
            oldPosition.y = position.y;
            thisSummon.transform.position = oldPosition;
            var oldScale = thisSummon.transform.localScale;
            oldScale.x = Mathf.Abs(oldScale.x) * scale;
            thisSummon.transform.localScale = oldScale;
            var instanceInfo = gameObject.GetComponent<Deploy.Behaviors.InstanceInfo>();
            var properties = new PlaceConfig
            {
                groupID = instanceInfo.groupID,
                hp = Deploy.Common.DefaultEnemyHealth,
                damage = instanceInfo.damage,
                hpBar = false
            };
            Deploy.RewriteInstance.Rewrite(thisSummon, properties, gameObject);
            var summonManager = gameObject.GetComponent<SummonManager>();
            summonManager.summons.Add(thisSummon);
            var thisArena = thisSummon.AddComponent<SmartArena>();
            var parentArena = gameObject.GetComponent<SmartArena>();
            thisArena.minX = parentArena.minX;
            thisArena.maxX = parentArena.maxX;
            thisArena.minY = parentArena.minY;
            thisArena.maxY = parentArena.maxY;
            thisSummon.AddComponent<SmartAttachment>().parent = gameObject;
        }
        fsm.InsertCustomAction("Summon", () =>
        {
            var currentX = gameObject.transform.position.x;
            var currentY = gameObject.transform.position.y;
            if (gameObject.transform.localScale.x > 0)
            {
                summon(new Vector2(currentX - 3, currentY + 3), 1);
                summon(new Vector2(currentX - 22, currentY + 3), -1);
            }
            else
            {
                summon(new Vector2(currentX + 3, currentY + 3), -1);
                summon(new Vector2(currentX + 22, currentY + 3), 1);
            }
        }, 0);
    }
    private GameObject prefab;
    private GameObject summonPrefab;
}
