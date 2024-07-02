namespace ZotelingsSandbox.Deploy.Actions;
internal class Sleep : FsmStateAction
{
    public Sleep(bool spriteFacingRight = false)
    {
        this.spriteFacingRight = spriteFacingRight;
    }
    public override void OnEnter()
    {
        var instanceInfo = Fsm.GameObject.GetComponent<Behaviors.InstanceInfo>();
        instanceInfo.status = Behaviors.InstanceInfo.Status.Sleeping;
        var rigidbody2D = Fsm.GameObject.GetComponent<Rigidbody2D>();
        if (rigidbody2D != null)
        {
            rigidbody2D.gravityScale = instanceInfo.gravityScale;
        }
    }
    public override void OnUpdate()
    {
        var rigidbody2D = Fsm.GameObject.GetComponent<Rigidbody2D>();
        if (rigidbody2D != null)
        {
            if (rigidbody2D.velocity.y == 0)
            {
                rigidbody2D.velocity = Vector2.zero;
            }
        }
        var instanceInfo = Fsm.GameObject.GetComponent<Behaviors.InstanceInfo>();
        var targetFollower = instanceInfo.targetFollower.gameObject;
        var scale = Fsm.GameObject.transform.localScale;
        if (targetFollower.transform.position.x < Fsm.GameObject.transform.position.x)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else
        {
            scale.x = -Mathf.Abs(scale.x);
        }
        if (spriteFacingRight)
        {
            scale.x *= -1;
        }
        Fsm.GameObject.transform.localScale = scale;
        if (instanceInfo.status == Behaviors.InstanceInfo.Status.Awaking && rigidbody2D.velocity == Vector2.zero)
        {
            Fsm.Event("FINISHED");
        }
    }
    public override void OnExit()
    {
        var instanceInfo = Fsm.GameObject.GetComponent<Behaviors.InstanceInfo>();
        instanceInfo.status = Behaviors.InstanceInfo.Status.Active;
    }
    private bool spriteFacingRight;
}
