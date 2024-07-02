namespace ZotelingsSandbox.Templates.Controllable;
internal class WingedZote : TemplateBase
{
    private class Idle : StateMachine.State
    {
        public override void Enter(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var animator = main.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Buzzer Idle");
        }
        public override string Update(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var inputActions = HeroController.instance.Reflect().inputHandler.inputActions;
            var direction = Vector2.zero;
            if (inputActions.left.IsPressed)
            {
                direction.x -= 1;
            }
            if (inputActions.right.IsPressed)
            {
                direction.x += 1;
            }
            if (inputActions.up.IsPressed)
            {
                direction.y += 1;
            }
            if (inputActions.down.IsPressed)
            {
                direction.y -= 1;
            }
            var velocityMax = 10;
            var accleleration = 30;
            if (direction != Vector2.zero)
            {
                var velocity = main.GetComponent<Rigidbody2D>().velocity;
                velocity += direction.normalized * accleleration * Time.deltaTime;
                if (velocity.magnitude > velocityMax)
                {
                    velocity = velocity.normalized * velocityMax;
                }
                main.GetComponent<Rigidbody2D>().velocity = velocity;
            }
            else
            {
                var velocity = main.GetComponent<Rigidbody2D>().velocity;
                var oldVelocity = velocity;
                velocity -= velocity.normalized * accleleration * Time.deltaTime;
                if (Vector2.Dot(oldVelocity, velocity) <= 0)
                {
                    velocity = Vector2.zero;
                }
                main.GetComponent<Rigidbody2D>().velocity = velocity;
            }
            var scale = main.transform.localScale;
            if (main.GetComponent<Rigidbody2D>().velocity.x < 0)
            {
                scale.x = Math.Abs(scale.x);
            }
            else if (main.GetComponent<Rigidbody2D>().velocity.x > 0)
            {
                scale.x = -Math.Abs(scale.x);
            }
            main.transform.localScale = scale;
            return null;
        }
    }
    private class Controller : StateMachine.StateMachine
    {
        public Controller()
        {
            AddState(new Idle());
            StartState = "Idle";
        }
        public override bool CanTakeDamage() => false;
        public override void TakeDamage(GameObject source, CollisionSide damageSide, int damageAmount, int hazardType)
        {
        }
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Grey_Prince_Zote","Zoteling"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        prefab = Load.Preload(preloadedObjects, "GG_Grey_Prince_Zote", "Zoteling");
    }
    protected override Dictionary<string, GameObject> Instantiate()
    {
        return new Dictionary<string, GameObject> { ["Main"] = GameObject.Instantiate(prefab) };
    }
    protected override void RewriteControllable(string name, GameObject gameObject)
    {
        if (name == "Main")
        {
            var fsm = gameObject.LocateMyFSM("Control");
            fsm.RemoveTransition("Pause", "FINISHED");
            fsm.AddState(Deploy.Common.ControllingStateName);
            fsm.AddCustomAction(Deploy.Common.ControllingStateName, () =>
            {
                var rigidbody = gameObject.GetComponent<Rigidbody2D>();
                rigidbody.bodyType = RigidbodyType2D.Dynamic;
                rigidbody.gravityScale = 0;
                var hitboxBuzzer = gameObject.transform.Find("Hitbox Buzzer").gameObject;
                hitboxBuzzer.SetActive(true);
                var damageEnemy = gameObject.transform.Find(Deploy.Common.DamageEnemyGameObjectName).gameObject;
                damageEnemy.SetActive(false);
                var controllable = HeroController.instance.gameObject.GetComponent<Deploy.Behaviors.HeroHandler>().Controllable;
                var control = controllable.GetComponent<StateMachine.StateMachine>();
                control.enabled = true;
            });
            fsm.AddTransition("Pause", "FINISHED", Deploy.Common.ControllingStateName);
            fsm.Fsm.GlobalTransitions = [];
            var direction = HeroController.instance.gameObject.transform.localScale.x < 0 ? -1 : 1;
            gameObject.transform.localScale = new Vector3(direction * Math.Abs(gameObject.transform.localScale.x), gameObject.transform.localScale.y, gameObject.transform.localScale.z);
        }
    }
    protected override void SetupControllableRoot(GameObject controllableRoot, Dictionary<string, GameObject> controllableChildren)
    {
        controllableRoot.AddComponent<Controller>().enabled = false;
        var childManager = controllableRoot.AddComponent<Deploy.Behaviors.ChildManager>();
        childManager.controllableChildren = controllableChildren;
        childManager.SwitchTo("Main");
    }
    private GameObject prefab;
}
