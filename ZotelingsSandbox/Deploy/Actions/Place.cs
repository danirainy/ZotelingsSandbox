namespace ZotelingsSandbox.Deploy.Actions;
internal class Place : FsmStateAction
{
    public Place(bool spriteFacingRight = false)
    {
        this.spriteFacingRight = spriteFacingRight;
    }
    public static Vector3 GetMousePosition(GameObject gameObject)
    {
        var cursorPosition = Input.mousePosition;
        cursorPosition.x = Math.Min(Math.Max(cursorPosition.x, 0), Screen.width);
        cursorPosition.y = Math.Min(Math.Max(cursorPosition.y, 0), Screen.height);
        cursorPosition.z = 38.25f;
        cursorPosition = Camera.main.ScreenToWorldPoint(cursorPosition);
        cursorPosition.z = gameObject.transform.position.z;
        return cursorPosition;
    }
    public override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Fsm.Event("FINISHED");
        }
        else if (Input.GetMouseButtonDown(1) || GameManager.instance.isPaused)
        {
            UnityEngine.Object.Destroy(Fsm.GameObject);
        }
        var instanceInfo = Fsm.GameObject.GetComponent<Behaviors.InstanceInfo>();
        var newPosition = GetMousePosition(Fsm.GameObject);
        newPosition.x += instanceInfo.placingOffset.x;
        newPosition.y += instanceInfo.placingOffset.y;
        Fsm.GameObject.transform.position = newPosition;
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
    }
    private bool spriteFacingRight;
}
