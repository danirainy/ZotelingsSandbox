namespace ZotelingsSandbox.Templates.Standard;
internal class LostKin : TemplateBase
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
        ("GG_Lost_Kin", "Lost Kin"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        prefab = Load.Preload(preloadedObjects, "GG_Lost_Kin", "Lost Kin");
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("IK Control");
        fsm.RemoveTransition("Pause", "FINISHED");
        fsm.RemoveAction("Pause", 1);
        fsm.RemoveAction("Pause", 0);
        fsm.AddCustomAction("Pause", () =>
        {
            fsm.SendEvent("FINISHED");
        });
        var ridigbody = gameObject.GetComponent<Rigidbody2D>();
        ridigbody.gravityScale = 3.25f;
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
        fsm.AddAction(Deploy.Common.PlacingStateName, new Deploy.Actions.Place());
        fsm.AddState(Deploy.Common.SleepingStateName);
        fsm.AddTransition(Deploy.Common.PlacingStateName, "FINISHED", Deploy.Common.SleepingStateName);
        fsm.AddAction(Deploy.Common.SleepingStateName, new Deploy.Actions.Sleep());
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Init");
        fsm.InsertCustomAction("Init", () =>
        {
            var smartArena = gameObject.AddComponent<SmartArena>();
            smartArena.Build();
            var smartAttachments = gameObject.AddComponent<SmartAttachments>();
            fsm.FsmVariables.GetFsmFloat("Left X").Value = smartArena.minX + 1;
            fsm.FsmVariables.GetFsmFloat("Right X").Value = smartArena.maxX - 1;
            fsm.FsmVariables.GetFsmFloat("Air Dash Height").Value = smartArena.minY + 3;
            fsm.FsmVariables.GetFsmFloat("Min Dstab Height").Value = smartArena.minY + 5;
            var maxHP = gameObject.GetComponent<HealthManager>().hp;
            var spawnBalloon = gameObject.LocateMyFSM("Spawn Balloon");
            spawnBalloon.FsmVariables.GetFsmFloat("X Min").Value = smartArena.minX + 1;
            spawnBalloon.FsmVariables.GetFsmFloat("X Max").Value = smartArena.maxX - 1;
            spawnBalloon.FsmVariables.GetFsmFloat("Y Min").Value = smartArena.minY + 4;
            spawnBalloon.FsmVariables.GetFsmFloat("Y Max").Value = smartArena.minY + 9;
            spawnBalloon.AccessIntVariable("HP Threshold").Value = (int)(maxHP * (1150 / 1500.0f));
            spawnBalloon.RemoveAction("Spawn", 3);
            spawnBalloon.RemoveAction("Spawn", 2);
            spawnBalloon.RemoveAction("Spawn", 1);
            spawnBalloon.RemoveAction("Spawn", 0);
            spawnBalloon.InsertCustomAction("Spawn", () =>
            {
                if (smartAttachments.attachments.Count > 3)
                {
                    spawnBalloon.SendEvent("FINISHED");
                }
                var hp = gameObject.GetComponent<HealthManager>().hp;
                if (hp > spawnBalloon.AccessIntVariable("HP Threshold").Value)
                {
                    spawnBalloon.SendEvent("FINISHED");
                }
            }, 0);
            spawnBalloon.AddCustomAction("Spawn", () =>
            {
                var spawned = spawnBalloon.FsmVariables.GetFsmGameObject("Spawned Enemy").Value;
                smartAttachments.attachments.Add(spawned);
            });
            fsm.SendEvent("ACTIVE");
        }, 2);
        fsm.RemoveTransition("Init", "FINISHED");
        fsm.AddTransition("Init", "FINISHED", "Idle");
        fsm.InsertCustomAction("Aim Jump 2", () =>
        {
            var smartArena = gameObject.GetComponent<SmartArena>();
            var midX = (smartArena.minX + smartArena.maxX) / 2;
            fsm.FsmVariables.GetFsmFloat("Jump X").Value = UnityEngine.Random.Range(midX - 1, midX + 1);
        }, 1);
    }
    private GameObject prefab;
}
