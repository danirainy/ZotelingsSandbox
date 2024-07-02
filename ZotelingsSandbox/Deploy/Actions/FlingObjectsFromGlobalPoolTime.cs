namespace ZotelingsSandbox.Deploy.Actions;
// Token: 0x020009C4 RID: 2500
[ActionCategory(ActionCategory.GameObject)]
[HutongGames.PlayMaker.Tooltip("Spawns a random amount of chosen GameObject from global pool and fires them off in random directions.")]
public class FlingObjectsFromGlobalPoolTime : RigidBody2dActionBase
{
    // Token: 0x060036B5 RID: 14005 RVA: 0x00143158 File Offset: 0x00141358
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
    }

    // Token: 0x060036B6 RID: 14006 RVA: 0x00003603 File Offset: 0x00001803
    public override void OnEnter()
    {
    }

    // Token: 0x060036B7 RID: 14007 RVA: 0x001431C0 File Offset: 0x001413C0
    public override void OnUpdate()
    {
        this.timer += Time.deltaTime;
        if (this.timer >= this.frequency.Value)
        {
            this.timer = 0f;
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
                }
            }
        }
    }

    // Token: 0x040038BB RID: 14523
    [RequiredField]
    [HutongGames.PlayMaker.Tooltip("GameObject to spawn.")]
    public FsmGameObject gameObject;

    // Token: 0x040038BC RID: 14524
    [HutongGames.PlayMaker.Tooltip("GameObject to spawn at (optional).")]
    public FsmGameObject spawnPoint;

    // Token: 0x040038BD RID: 14525
    [HutongGames.PlayMaker.Tooltip("Position. If a Spawn Point is defined, this is used as a local offset from the Spawn Point position.")]
    public FsmVector3 position;

    // Token: 0x040038BE RID: 14526
    [HutongGames.PlayMaker.Tooltip("How often, in seconds, spawn occurs.")]
    public FsmFloat frequency;

    // Token: 0x040038BF RID: 14527
    [HutongGames.PlayMaker.Tooltip("Minimum amount of objects to be spawned.")]
    public FsmInt spawnMin;

    // Token: 0x040038C0 RID: 14528
    [HutongGames.PlayMaker.Tooltip("Maximum amount of objects to be spawned.")]
    public FsmInt spawnMax;

    // Token: 0x040038C1 RID: 14529
    [HutongGames.PlayMaker.Tooltip("Minimum speed objects are fired at.")]
    public FsmFloat speedMin;

    // Token: 0x040038C2 RID: 14530
    [HutongGames.PlayMaker.Tooltip("Maximum speed objects are fired at.")]
    public FsmFloat speedMax;

    // Token: 0x040038C3 RID: 14531
    [HutongGames.PlayMaker.Tooltip("Minimum angle objects are fired at.")]
    public FsmFloat angleMin;

    // Token: 0x040038C4 RID: 14532
    [HutongGames.PlayMaker.Tooltip("Maximum angle objects are fired at.")]
    public FsmFloat angleMax;

    // Token: 0x040038C5 RID: 14533
    [HutongGames.PlayMaker.Tooltip("Randomises spawn points of objects within this range. Leave as 0 and all objects will spawn at same point.")]
    public FsmFloat originVariationX;

    // Token: 0x040038C6 RID: 14534
    public FsmFloat originVariationY;

    // Token: 0x040038C7 RID: 14535
    private float vectorX;

    // Token: 0x040038C8 RID: 14536
    private float vectorY;

    // Token: 0x040038C9 RID: 14537
    private float timer;

    // Token: 0x040038CA RID: 14538
    private bool originAdjusted;
}
