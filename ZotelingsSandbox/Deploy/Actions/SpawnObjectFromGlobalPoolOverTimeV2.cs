namespace ZotelingsSandbox.Deploy.Actions;
// Token: 0x02000A65 RID: 2661
[ActionCategory(ActionCategory.GameObject)]
[HutongGames.PlayMaker.Tooltip("Spawns a prefab Game Object from the Global Object Pool on the Game Manager.")]
public class SpawnObjectFromGlobalPoolOverTimeV2 : FsmStateAction
{
    // Token: 0x06003974 RID: 14708 RVA: 0x0014EB02 File Offset: 0x0014CD02
    public override void Reset()
    {
        this.gameObject = null;
        this.spawnPoint = null;
        this.position = new FsmVector3
        {
            UseVariable = true
        };
        this.rotation = new FsmVector3
        {
            UseVariable = true
        };
        this.frequency = null;
    }

    // Token: 0x06003975 RID: 14709 RVA: 0x0014EB40 File Offset: 0x0014CD40
    public override void OnUpdate()
    {
        this.timer += Time.deltaTime;
        if (this.timer >= this.frequency.Value)
        {
            this.timer = 0f;
            if (this.gameObject.Value != null)
            {
                Vector3 a = Vector3.zero;
                Vector3 euler = Vector3.up;
                if (this.spawnPoint.Value != null)
                {
                    a = this.spawnPoint.Value.transform.position;
                    if (!this.position.IsNone)
                    {
                        a += this.position.Value;
                    }
                    euler = ((!this.rotation.IsNone) ? this.rotation.Value : this.spawnPoint.Value.transform.eulerAngles);
                }
                else
                {
                    if (!this.position.IsNone)
                    {
                        a = this.position.Value;
                    }
                    if (!this.rotation.IsNone)
                    {
                        euler = this.rotation.Value;
                    }
                }
                if (this.gameObject != null)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate(this.gameObject.Value, a, Quaternion.Euler(euler));
                    var instanceInfo = Fsm.GameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
                    var properties = new Templates.PlaceConfig
                    {
                        groupID = instanceInfo.groupID,
                        hp = Common.DefaultEnemyHealth,
                        damage = instanceInfo.damage,
                        hpBar = false
                    };
                    RewriteInstance.Rewrite(gameObject, properties, Fsm.GameObject);
                    if (this.scaleMin != null && this.scaleMax != null)
                    {
                        float num = UnityEngine.Random.Range(this.scaleMin.Value, this.scaleMax.Value);
                        if (num != 1f)
                        {
                            gameObject.transform.localScale = new Vector3(num, num, num);
                        }
                    }
                }
            }
        }
    }

    // Token: 0x06003976 RID: 14710 RVA: 0x0014ECB5 File Offset: 0x0014CEB5
    public SpawnObjectFromGlobalPoolOverTimeV2()
    {
        this.scaleMin = 1f;
        this.scaleMax = 1f;
    }

    // Token: 0x04003C35 RID: 15413
    [RequiredField]
    [HutongGames.PlayMaker.Tooltip("GameObject to create. Usually a Prefab.")]
    public FsmGameObject gameObject;

    // Token: 0x04003C36 RID: 15414
    [HutongGames.PlayMaker.Tooltip("Optional Spawn Point.")]
    public FsmGameObject spawnPoint;

    // Token: 0x04003C37 RID: 15415
    [HutongGames.PlayMaker.Tooltip("Position. If a Spawn Point is defined, this is used as a local offset from the Spawn Point position.")]
    public FsmVector3 position;

    // Token: 0x04003C38 RID: 15416
    [HutongGames.PlayMaker.Tooltip("Rotation. NOTE: Overrides the rotation of the Spawn Point.")]
    public FsmVector3 rotation;

    // Token: 0x04003C39 RID: 15417
    [HutongGames.PlayMaker.Tooltip("How often, in seconds, spawn occurs.")]
    public FsmFloat frequency;

    // Token: 0x04003C3A RID: 15418
    [HutongGames.PlayMaker.Tooltip("Minimum scale of clone.")]
    public FsmFloat scaleMin;

    // Token: 0x04003C3B RID: 15419
    [HutongGames.PlayMaker.Tooltip("Maximum scale of clone.")]
    public FsmFloat scaleMax;

    // Token: 0x04003C3C RID: 15420
    private float timer;
}
