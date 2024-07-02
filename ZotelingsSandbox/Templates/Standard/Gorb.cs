namespace ZotelingsSandbox.Templates.Standard;
internal class Gorb : TemplateBase
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
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Ghost_Gorb", "Warrior"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        var warriors = Load.Preload(preloadedObjects, "GG_Ghost_Gorb", "Warrior");
        prefab = warriors.transform.Find("Ghost Warrior Slug").gameObject;
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = UnityEngine.Object.Instantiate(prefab) };
    }
    protected override void RewriteInstance(GameObject gameObject, PlaceConfig properties)
    {
        var fsm = gameObject.LocateMyFSM("Movement");
        fsm.RemoveTransition("Init", "FINISHED");
        fsm.AddState(Deploy.Common.PlacingStateName);
        fsm.AddCustomAction(Deploy.Common.PlacingStateName, () =>
        {
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.enabled = true;
            var animator = fsm.gameObject.GetComponent<tk2dSpriteAnimator>();
            animator.Play("TurnToIdle");
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
        fsm.AddTransition(Deploy.Common.SleepingStateName, "FINISHED", "Init Choice");
        fsm.InsertCustomAction("Init Choice", () =>
        {
            var smartArena = gameObject.AddComponent<SmartArena>();
            smartArena.Build();
            Vector2 convert(Vector2 old)
            {
                var minX = smartArena.minX;
                var maxX = smartArena.maxX;
                var minY = smartArena.minY;
                var oldMinX = 44.41f;
                var oldMaxX = 67.78f;
                var oldMinY = 33;
                var rx = (old.x - oldMinX) / (oldMaxX - oldMinX);
                var newx = minX + rx * (maxX - minX);
                var newY = minY + (old.y - oldMinY);
                return new Vector2(newx, newY);
            }
            fsm.FsmVariables.GetFsmVector3("P1").Value = convert(new Vector2(56.06f, 41));
            fsm.FsmVariables.GetFsmVector3("P2").Value = convert(new Vector2(56.06f, 36));
            fsm.FsmVariables.GetFsmVector3("P3").Value = convert(new Vector2(46.41f, 36));
            fsm.FsmVariables.GetFsmVector3("P4").Value = convert(new Vector2(65.78f, 36));
            fsm.FsmVariables.GetFsmVector3("P5").Value = convert(new Vector2(56.06f, 37.7f));
            fsm.FsmVariables.GetFsmVector3("P6").Value = convert(new Vector2(62.42f, 37.7f));
            fsm.FsmVariables.GetFsmVector3("P7").Value = convert(new Vector2(49.8f, 37.7f));
        }, 0);
        fsm.RemoveTransition("Hover", "RETURN");
        fsm.InsertCustomAction("Set Warp", () =>
        {
            if (UnityEngine.Random.Range(0f, 1) < 0.5f)
            {
                fsm.SendEvent("WARP L");
            }
            else
            {
                fsm.SendEvent("WARP R");
            }
        }, 0);
        var fsm2 = gameObject.LocateMyFSM("Distance Attack");
        fsm2.RemoveTransition("Init", "FINISHED");
    }
    private GameObject prefab;
}
