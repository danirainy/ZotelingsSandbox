namespace ZotelingsSandbox.Templates.Controllable;
internal class ZoteTheMighty : TemplateBase
{
    private class Idle : StateMachine.State
    {
        public override void Enter(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var animator = main.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Idle");
        }
        public override string Update(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var inputActions = HeroController.instance.Reflect().inputHandler.inputActions;
            if (inputActions.jump.IsPressed)
            {
                return "JumpAntic";
            }
            if (inputActions.left.IsPressed || inputActions.right.IsPressed)
            {
                var direction = inputActions.left.IsPressed ? 1 : -1;
                main.transform.localScale = new Vector3(direction * Math.Abs(main.transform.localScale.x), main.transform.localScale.y, main.transform.localScale.z);
                return "ChargeAntic";
            }
            return null;
        }
    }
    private class ChargeAntic : StateMachine.State
    {
        public override void Enter(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var animator = main.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Antic");
            var voice = main.transform.Find("Attack Voice").gameObject.GetComponent<AudioSource>();
            voice.volume = 1;
            voice.Play();
            timer = 0.2f;
        }
        public override string Update(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var inputActions = HeroController.instance.Reflect().inputHandler.inputActions;
            if (inputActions.jump.IsPressed)
            {
                var voice = main.transform.Find("Attack Voice").gameObject.GetComponent<AudioSource>();
                voice.Stop();
                return "JumpAntic";
            }
            if (!inputActions.left.IsPressed && !inputActions.right.IsPressed)
            {
                var voice = main.transform.Find("Attack Voice").gameObject.GetComponent<AudioSource>();
                voice.Stop();
                return "Idle";
            }
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                return "Charge";
            }
            return null;
        }
        public override void Exit(StateMachine.StateMachine stateMachine, bool interrupted)
        {
            if (interrupted)
            {
                var root = stateMachine.gameObject;
                var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
                var voice = main.transform.Find("Attack Voice").gameObject.GetComponent<AudioSource>();
                voice.Stop();
            }
        }
        private float timer;
    }
    private class Charge : StateMachine.State
    {
        public override void Enter(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var animator = main.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Attack");
            var bonker = main.transform.Find("Bonker").gameObject;
            bonker.SetActive(true);
            var pt = main.transform.Find("Pt Run").gameObject.GetComponent<ParticleSystem>();
            pt.Play();
        }
        public override string Update(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var accel = 20f;
            var speed = 9f;
            var inputActions = HeroController.instance.Reflect().inputHandler.inputActions;
            var velocity = main.GetComponent<Rigidbody2D>().velocity;
            if (inputActions.jump.IsPressed)
            {
                return "JumpAntic";
            }
            if (!inputActions.left.IsPressed && !inputActions.right.IsPressed)
            {
                if (velocity.x != 0)
                {
                    var sign = velocity.x > 0 ? 1 : -1;
                    velocity.x -= sign * accel * Time.deltaTime;
                    if (sign * velocity.x < 0)
                    {
                        velocity.x = 0;
                    }
                    main.GetComponent<Rigidbody2D>().velocity = velocity;
                }
            }
            else if (inputActions.left.IsPressed)
            {
                velocity.x -= accel * Time.deltaTime;
                if (velocity.x < -speed)
                {
                    velocity.x = -speed;
                }
                main.GetComponent<Rigidbody2D>().velocity = velocity;
            }
            else
            {
                velocity.x += accel * Time.deltaTime;
                if (velocity.x > speed)
                {
                    velocity.x = speed;
                }
                main.GetComponent<Rigidbody2D>().velocity = velocity;
            }
            if (velocity.x == 0)
            {
                return "Idle";
            }
            var direction = velocity.x < 0 ? 1 : -1;
            main.transform.localScale = new Vector3(direction * Math.Abs(main.transform.localScale.x), main.transform.localScale.y, main.transform.localScale.z);
            return null;
        }
        public override void Exit(StateMachine.StateMachine stateMachine, bool interrupted)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var bonker = main.transform.Find("Bonker").gameObject;
            bonker.SetActive(false);
            var pt = main.transform.Find("Pt Run").gameObject.GetComponent<ParticleSystem>();
            pt.Stop();
            var voice = main.transform.Find("Attack Voice").gameObject.GetComponent<AudioSource>();
            voice.Stop();
        }
    }
    private class Fall : StateMachine.State
    {
        public override string Update(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var velocity = main.GetComponent<Rigidbody2D>().velocity;
            if (velocity.y != 0)
            {
                return null;
            }
            var col2d = main.GetComponent<Collider2D>();
            var bottomRays = new List<Vector2>(3)
            {
                new Vector2(col2d.bounds.max.x, col2d.bounds.min.y),
                new Vector2(col2d.bounds.center.x, col2d.bounds.min.y),
                col2d.bounds.min
            };
            var bottomHit = false;
            for (int k = 0; k < 3; k++)
            {
                RaycastHit2D raycastHit2D3 = Physics2D.Raycast(bottomRays[k], -Vector2.up, 0.08f, 1 << 8);
                if (raycastHit2D3.collider != null)
                {
                    bottomHit = true;
                    break;
                }
            }
            if (bottomHit)
            {
                main.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                return "GetUp";
            }
            return null;
        }
        public override void Exit(StateMachine.StateMachine stateMachine, bool interrupted)
        {
        }
    }
    private class GetUp : StateMachine.State
    {
        public override void Enter(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var animator = main.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Get Up");
            timer = 0.2f;
        }
        public override string Update(StateMachine.StateMachine stateMachine)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                return "Idle";
            }
            return null;
        }
        private float timer;
    }
    private class JumpAntic : StateMachine.State
    {
        public override void Enter(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var animator = main.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Antic");
            foreach (var action in main.LocateMyFSM("Control").GetState("Jump Antic").Actions)
            {
                if (action is AudioPlayerOneShotSingle)
                {
                    action.OnEnter();
                }
            }
            main.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            timer = 0.02f;
        }
        public override string Update(StateMachine.StateMachine stateMachine)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                return "Jump";
            }
            return null;
        }
        private float timer;
    }
    private class Jump : StateMachine.State
    {
        public override void Enter(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var animator = main.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Roll");
            foreach (var action in main.LocateMyFSM("Control").GetState("Jump Launch").Actions)
            {
                if (action is AudioPlayerOneShot)
                {
                    action.OnEnter();
                }
            }
            float velocityX = 0;
            var inputActions = HeroController.instance.Reflect().inputHandler.inputActions;
            if (inputActions.left.IsPressed)
            {
                velocityX = -9;
            }
            else if (inputActions.right.IsPressed)
            {
                velocityX = 9;
            }
            main.GetComponent<Rigidbody2D>().velocity = new Vector2(velocityX, 20);
        }
        public override string Update(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var accel = 20f;
            var speed = 18f;
            var inputActions = HeroController.instance.Reflect().inputHandler.inputActions;
            var velocity = main.GetComponent<Rigidbody2D>().velocity;
            if (!inputActions.left.IsPressed && !inputActions.right.IsPressed)
            {
                if (velocity.x != 0)
                {
                    var sign = velocity.x > 0 ? 1 : -1;
                    velocity.x -= sign * accel * Time.deltaTime;
                    if (sign * velocity.x < 0)
                    {
                        velocity.x = 0;
                    }
                    main.GetComponent<Rigidbody2D>().velocity = velocity;
                }
            }
            else if (inputActions.left.IsPressed)
            {
                velocity.x -= accel * Time.deltaTime;
                if (velocity.x < -speed)
                {
                    velocity.x = -speed;
                }
                main.GetComponent<Rigidbody2D>().velocity = velocity;
            }
            else
            {
                velocity.x += accel * Time.deltaTime;
                if (velocity.x > speed)
                {
                    velocity.x = speed;
                }
                main.GetComponent<Rigidbody2D>().velocity = velocity;
            }
            if (velocity.y >= -10 && velocity.y <= 10 && inputActions.jump.IsPressed)
            {
                main.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0.5f;
            }
            if (velocity.y != 0)
            {
                return null;
            }
            var col2d = main.GetComponent<Collider2D>();
            var bottomRays = new List<Vector2>(3)
        {
            new Vector2(col2d.bounds.max.x, col2d.bounds.min.y),
            new Vector2(col2d.bounds.center.x, col2d.bounds.min.y),
            col2d.bounds.min
        };
            var bottomHit = false;
            for (int k = 0; k < 3; k++)
            {
                RaycastHit2D raycastHit2D3 = Physics2D.Raycast(bottomRays[k], -Vector2.up, 0.08f, 1 << 8);
                if (raycastHit2D3.collider != null)
                {
                    bottomHit = true;
                    break;
                }
            }
            if (bottomHit)
            {
                main.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                return "JumpEnd";
            }
            return null;
        }
        public override void Exit(StateMachine.StateMachine stateMachine, bool interrupted)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            main.gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
        }
    }
    private class JumpEnd : StateMachine.State
    {
        public override void Enter(StateMachine.StateMachine stateMachine)
        {
            var root = stateMachine.gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var animator = main.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Get Up");
            timer = 0.02f;
        }
        public override string Update(StateMachine.StateMachine stateMachine)
        {
            var inputActions = HeroController.instance.Reflect().inputHandler.inputActions;
            if (inputActions.jump.IsPressed)
            {
                return "JumpAntic";
            }
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                return "Idle";
            }
            return null;
        }
        private float timer;
    }
    private class Controller : StateMachine.StateMachine
    {
        public Controller()
        {
            AddState(new Idle());
            AddState(new ChargeAntic());
            AddState(new Charge());
            AddState(new Fall());
            AddState(new GetUp());
            AddState(new JumpAntic());
            AddState(new Jump());
            AddState(new JumpEnd());
            StartState = "Idle";
        }
        public override bool CanTakeDamage() => true;
        public override void TakeDamage(GameObject source, CollisionSide damageSide, int damageAmount, int hazardType)
        {
            SetState("Fall");
            var root = gameObject;
            var main = root.GetComponent<Deploy.Behaviors.ChildManager>().controllableChildren["Main"];
            var animator = main.GetComponent<tk2dSpriteAnimator>();
            animator.Play("Roll");
            var velocity = 25f;
            float angle;
            if (damageSide == CollisionSide.left)
            {
                angle = UnityEngine.Random.Range(50, 60);
            }
            else
            {
                angle = UnityEngine.Random.Range(120, 130);
            }
            var velocityX = velocity * Mathf.Cos(angle * 0.017453292f);
            var velocityY = velocity * Mathf.Sin(angle * 0.017453292f);
            main.GetComponent<Rigidbody2D>().velocity = new Vector2(velocityX, velocityY);
            foreach (var action in main.LocateMyFSM("Control").GetState("Hit Left").Actions)
            {
                if (action is AudioPlayerOneShot)
                {
                    action.OnEnter();
                }
            }
            var direction = damageSide == CollisionSide.left ? 1 : -1;
            main.transform.localScale = new Vector3(direction * Math.Abs(main.transform.localScale.x), main.transform.localScale.y, main.transform.localScale.z);
        }
    }
    public override List<(string, string)> GetPreloadNames() => new()
    {
        ("GG_Mighty_Zote","Battle Control"),
    };
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        var battleControl = Load.Preload(preloadedObjects, "GG_Mighty_Zote", "Battle Control");
        prefab = battleControl.transform.Find("Dormant Warriors").transform.Find("Zote Crew Normal (1)").gameObject;
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
            fsm.RemoveTransition("Init", "FINISHED");
            fsm.AddState(Deploy.Common.ControllingStateName);
            fsm.AddCustomAction(Deploy.Common.ControllingStateName, () =>
            {
                var rigidbody = gameObject.GetComponent<Rigidbody2D>();
                rigidbody.gravityScale = 1;
                var controllable = HeroController.instance.gameObject.GetComponent<Deploy.Behaviors.HeroHandler>().Controllable;
                var control = controllable.GetComponent<StateMachine.StateMachine>();
                control.enabled = true;
            });
            fsm.AddTransition("Init", "FINISHED", Deploy.Common.ControllingStateName);
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
