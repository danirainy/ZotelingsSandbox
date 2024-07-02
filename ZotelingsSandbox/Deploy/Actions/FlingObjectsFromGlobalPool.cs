namespace ZotelingsSandbox.Deploy.Actions;
// Token: 0x020009C3 RID: 2499
[ActionCategory(ActionCategory.GameObject)]
[HutongGames.PlayMaker.Tooltip("Spawns a random amount of chosen GameObject from global pool and fires them off in random directions.")]
public class FlingObjectsFromGlobalPool : RigidBody2dActionBase
{
    // Token: 0x060036B2 RID: 14002 RVA: 0x00142E50 File Offset: 0x00141050
    public override void Reset()
    {
        this.gameObject = null;
        this.spawnPoint = null;
        this.position = new FsmVector3
        {
            UseVariable = true
        };
        this.spawnMin = null;
        this.spawnMax = null;
        this.speedMin = null;
        this.speedMax = null;
        this.angleMin = null;
        this.angleMax = null;
        this.originVariationX = null;
        this.originVariationY = null;
        this.FSM = new FsmString
        {
            UseVariable = true
        };
        this.FSMEvent = new FsmString
        {
            UseVariable = true
        };
    }

    // Token: 0x060036B3 RID: 14003 RVA: 0x00142EDC File Offset: 0x001410DC
    public override void OnEnter()
    {
        if (this.gameObject.Value != null)
        {
            Vector3 a = Vector3.zero;
            Vector3 zero = Vector3.zero;
            if (this.spawnPoint.Value != null)
            {
                a = this.spawnPoint.Value.transform.position;
                if (!this.position.IsNone)
                {
                    a += this.position.Value;
                }
            }
            else if (!this.position.IsNone)
            {
                a = this.position.Value;
            }
            int num = UnityEngine.Random.Range(this.spawnMin.Value, this.spawnMax.Value + 1);
            for (int i = 1; i <= num; i++)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate(this.gameObject.Value, a, Quaternion.Euler(zero));
                var instanceInfo = Fsm.GameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
                var properties = new Templates.PlaceConfig
                {
                    groupID = instanceInfo.groupID,
                    hp = Common.DefaultEnemyHealth,
                    damage = instanceInfo.damage,
                    hpBar = false
                };
                RewriteInstance.Rewrite(gameObject, properties, Fsm.GameObject);
                float x = gameObject.transform.position.x;
                float y = gameObject.transform.position.y;
                float z = gameObject.transform.position.z;
                if (this.originVariationX != null)
                {
                    x = gameObject.transform.position.x + UnityEngine.Random.Range(-this.originVariationX.Value, this.originVariationX.Value);
                    this.originAdjusted = true;
                }
                if (this.originVariationY != null)
                {
                    y = gameObject.transform.position.y + UnityEngine.Random.Range(-this.originVariationY.Value, this.originVariationY.Value);
                    this.originAdjusted = true;
                }
                if (this.originAdjusted)
                {
                    gameObject.transform.position = new Vector3(x, y, z);
                }
                base.CacheRigidBody2d(gameObject);
                float num2 = UnityEngine.Random.Range(this.speedMin.Value, this.speedMax.Value);
                float num3 = UnityEngine.Random.Range(this.angleMin.Value, this.angleMax.Value);
                this.vectorX = num2 * Mathf.Cos(num3 * 0.017453292f);
                this.vectorY = num2 * Mathf.Sin(num3 * 0.017453292f);
                Vector2 velocity;
                velocity.x = this.vectorX;
                velocity.y = this.vectorY;
                this.rb2d.velocity = velocity;
                if (!this.FSM.IsNone)
                {
                    FSMUtility.LocateFSM(gameObject, this.FSM.Value).SendEvent(this.FSMEvent.Value);
                }
            }
        }
        base.Finish();
    }

    // Token: 0x040038AB RID: 14507
    [RequiredField]
    [HutongGames.PlayMaker.Tooltip("GameObject to spawn.")]
    public FsmGameObject gameObject;

    // Token: 0x040038AC RID: 14508
    [HutongGames.PlayMaker.Tooltip("GameObject to spawn at (optional).")]
    public FsmGameObject spawnPoint;

    // Token: 0x040038AD RID: 14509
    [HutongGames.PlayMaker.Tooltip("Position. If a Spawn Point is defined, this is used as a local offset from the Spawn Point position.")]
    public FsmVector3 position;

    // Token: 0x040038AE RID: 14510
    [HutongGames.PlayMaker.Tooltip("Minimum amount of objects to be spawned.")]
    public FsmInt spawnMin;

    // Token: 0x040038AF RID: 14511
    [HutongGames.PlayMaker.Tooltip("Maximum amount of objects to be spawned.")]
    public FsmInt spawnMax;

    // Token: 0x040038B0 RID: 14512
    [HutongGames.PlayMaker.Tooltip("Minimum speed objects are fired at.")]
    public FsmFloat speedMin;

    // Token: 0x040038B1 RID: 14513
    [HutongGames.PlayMaker.Tooltip("Maximum speed objects are fired at.")]
    public FsmFloat speedMax;

    // Token: 0x040038B2 RID: 14514
    [HutongGames.PlayMaker.Tooltip("Minimum angle objects are fired at.")]
    public FsmFloat angleMin;

    // Token: 0x040038B3 RID: 14515
    [HutongGames.PlayMaker.Tooltip("Maximum angle objects are fired at.")]
    public FsmFloat angleMax;

    // Token: 0x040038B4 RID: 14516
    [HutongGames.PlayMaker.Tooltip("Randomises spawn points of objects within this range. Leave as 0 and all objects will spawn at same point.")]
    public FsmFloat originVariationX;

    // Token: 0x040038B5 RID: 14517
    public FsmFloat originVariationY;

    // Token: 0x040038B6 RID: 14518
    [HutongGames.PlayMaker.Tooltip("Optional: Name of FSM on object you want to send an event to after spawn")]
    public FsmString FSM;

    // Token: 0x040038B7 RID: 14519
    [HutongGames.PlayMaker.Tooltip("Optional: Event you want to send to object after spawn")]
    public FsmString FSMEvent;

    // Token: 0x040038B8 RID: 14520
    private float vectorX;

    // Token: 0x040038B9 RID: 14521
    private float vectorY;

    // Token: 0x040038BA RID: 14522
    private bool originAdjusted;
}
