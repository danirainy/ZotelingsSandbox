namespace ZotelingsSandbox.Templates.Standard;
internal class Nosk : TemplateBase
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
        }
        public float minX;
        public float maxX;
        public float minY;
    }
    private class RSCheck : FsmStateAction
    {
        public override void OnUpdate()
        {
            var velocity = Fsm.GameObject.GetComponent<Rigidbody2D>().velocity;
            if (velocity.y == 0)
            {
                Fsm.Event("LAND");
            }
        }
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Nosk", "Mimic Spider"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        prefab = Load.Preload(preloadedObjects, "GG_Nosk", "Mimic Spider");
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("Mimic Spider");
        fsm.RemoveAction("Init", 2);
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
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Roar Finish");
        fsm.RemoveAction("Roar Finish", 2);
        fsm.RemoveAction("Roar Finish", 1);
        fsm.RemoveAction("Roar Finish", 0);
        fsm.InsertCustomAction("Roar Finish", () =>
        {
            var smartArena = gameObject.AddComponent<SmartArena>();
            smartArena.Build();
            var l = smartArena.minX;
            var r = smartArena.maxX;
            var padding = 2;
            if (l + padding < r - padding)
            {
                l += padding;
                r -= padding;
            }
            fsm.FsmVariables.GetFsmFloat("Jump Min X").Value = l;
            fsm.FsmVariables.GetFsmFloat("Jump Max X").Value = r;
            fsm.FsmVariables.GetFsmFloat("Roof Y").Value = smartArena.minY + 17;
            fsm.AccessIntVariable("Max HP").Value = gameObject.GetComponent<HealthManager>().hp;
        }, 0);
        fsm.RemoveAction("Roof Jump?", 1);
        fsm.InsertCustomAction("Roof Jump?", () =>
        {
            var currentHP = gameObject.GetComponent<HealthManager>().hp;
            var maxHP = fsm.AccessIntVariable("Max HP").Value;
            if (currentHP > (560f / 980) * maxHP)
            {
                fsm.SendEvent("FINISHED");
            }
        }, 1);
        fsm.RemoveAction("Idle", 3);
        fsm.InsertCustomAction("Idle", () =>
        {
            var smartArena = gameObject.GetComponent<SmartArena>();
            var l = smartArena.minX;
            var r = smartArena.maxX;
            var originalL = 73.39f;
            var originalR = 115.55f;
            var originalRangeL = 88.32f;
            var originalRangeR = 103.78f;
            var newRangeL = (originalRangeL - originalL) / (originalR - originalL) * (r - l) + l;
            var newRangeR = (originalRangeR - originalL) / (originalR - originalL) * (r - l) + l;
            var currentX = gameObject.transform.position.x;
            if (currentX >= newRangeL && currentX <= newRangeR)
            {
                fsm.SendEvent("NO SPIT");
            }
        }, 3);
        fsm.AddCustomAction("Set Spit", () =>
        {
            var smartArena = gameObject.GetComponent<SmartArena>();
            var l = smartArena.minX;
            var r = smartArena.maxX;
            var originalL = 73.39f;
            var originalR = 115.55f;
            var originalRangeL = 92.7f;
            var originalRangeR = 99.6f;
            var newRangeL = (originalRangeL - originalL) / (originalR - originalL) * (r - l) + l;
            var newRangeR = (originalRangeR - originalL) / (originalR - originalL) * (r - l) + l;
            fsm.FsmVariables.GetFsmFloat("Spit X").Value = UnityEngine.Random.Range(newRangeL, newRangeR);
        });
        fsm.RemoveAction("Spit Antic", 0);
        fsm.InsertCustomAction("Spit Antic", () =>
        {
            gameObject.transform.position = new Vector3(fsm.FsmVariables.GetFsmFloat("Spit X").Value, gameObject.transform.position.y, gameObject.transform.position.z);
        }, 0);
        fsm.AddAction("RS Jump", new RSCheck());
        fsm.RemoveAction("Roof Antic", 5);
        fsm.RemoveAction("Roof Antic", 0);
        fsm.InsertCustomAction("Roof Antic", () =>
        {
            var currentY = gameObject.transform.position.y;
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, currentY - 1, gameObject.transform.position.z);
        }, 0);
        UnityEngine.Object.Destroy(gameObject.LocateMyFSM("constrain_x"));
        gameObject.transform.Find("Knight Idle").gameObject.SetActive(false);
    }
    private GameObject prefab;
}
