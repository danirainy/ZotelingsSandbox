namespace ZotelingsSandbox.Deploy.Actions;
// Token: 0x020009C5 RID: 2501
[ActionCategory(ActionCategory.GameObject)]
[HutongGames.PlayMaker.Tooltip("Spawns a random amount of chosen GameObject from global pool and fires them off in random directions.")]
public class FlingObjectsFromGlobalPoolVel : RigidBody2dActionBase
{
    // Token: 0x060036B9 RID: 14009 RVA: 0x0014343C File Offset: 0x0014163C
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
        this.speedMinX = null;
        this.speedMaxX = null;
        this.speedMinY = null;
        this.speedMaxY = null;
        this.originVariationX = null;
        this.originVariationY = null;
    }

    // Token: 0x060036BA RID: 14010 RVA: 0x001434A4 File Offset: 0x001416A4
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
                float x2 = UnityEngine.Random.Range(this.speedMinX.Value, this.speedMaxX.Value);
                float y2 = UnityEngine.Random.Range(this.speedMinY.Value, this.speedMaxY.Value);
                Vector2 velocity = new Vector2(x2, y2);
                this.rb2d.velocity = velocity;
            }
        }
        base.Finish();
    }

    // Token: 0x040038CC RID: 14540
    [RequiredField]
    [HutongGames.PlayMaker.Tooltip("GameObject to spawn.")]
    public FsmGameObject gameObject;

    // Token: 0x040038CD RID: 14541
    [HutongGames.PlayMaker.Tooltip("GameObject to spawn at (optional).")]
    public FsmGameObject spawnPoint;

    // Token: 0x040038CE RID: 14542
    [HutongGames.PlayMaker.Tooltip("Position. If a Spawn Point is defined, this is used as a local offset from the Spawn Point position.")]
    public FsmVector3 position;

    // Token: 0x040038CF RID: 14543
    [HutongGames.PlayMaker.Tooltip("Minimum amount of objects to be spawned.")]
    public FsmInt spawnMin;

    // Token: 0x040038D0 RID: 14544
    [HutongGames.PlayMaker.Tooltip("Maximum amount of objects to be spawned.")]
    public FsmInt spawnMax;

    // Token: 0x040038D1 RID: 14545
    public FsmFloat speedMinX;

    // Token: 0x040038D2 RID: 14546
    public FsmFloat speedMaxX;

    // Token: 0x040038D3 RID: 14547
    public FsmFloat speedMinY;

    // Token: 0x040038D4 RID: 14548
    public FsmFloat speedMaxY;

    // Token: 0x040038D5 RID: 14549
    [HutongGames.PlayMaker.Tooltip("Randomises spawn points of objects within this range. Leave as 0 and all objects will spawn at same point.")]
    public FsmFloat originVariationX;

    // Token: 0x040038D6 RID: 14550
    public FsmFloat originVariationY;

    // Token: 0x040038D9 RID: 14553
    private bool originAdjusted;
}