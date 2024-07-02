namespace ZotelingsSandbox.Deploy.Actions;
// Token: 0x02000B5C RID: 2908
[ActionCategory(ActionCategory.GameObject)]
[ActionTarget(typeof(GameObject), "gameObject", true)]
[HutongGames.PlayMaker.Tooltip("Creates a Game Object, usually using a Prefab.")]
public class CreateObject : FsmStateAction
{
    // Token: 0x06003E10 RID: 15888 RVA: 0x001632E4 File Offset: 0x001614E4
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
        this.storeObject = null;
        this.networkInstantiate = false;
        this.networkGroup = 0;
    }

    // Token: 0x06003E11 RID: 15889 RVA: 0x00163344 File Offset: 0x00161544
    public override void OnEnter()
    {
        GameObject value = this.gameObject.Value;
        if (value != null)
        {
            Vector3 a = Vector3.zero;
            Vector3 euler = Vector3.zero;
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
            GameObject value2 = UnityEngine.Object.Instantiate<GameObject>(value, a, Quaternion.Euler(euler));
            var instanceInfo = Fsm.GameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
            var properties = new Templates.PlaceConfig
            {
                groupID = instanceInfo.groupID,
                hp = Common.DefaultEnemyHealth,
                damage = instanceInfo.damage,
                hpBar = false
            };
            RewriteInstance.Rewrite(value2, properties, Fsm.GameObject);
            this.storeObject.Value = value2;
        }
        base.Finish();
    }

    // Token: 0x0400422A RID: 16938
    [RequiredField]
    [HutongGames.PlayMaker.Tooltip("GameObject to create. Usually a Prefab.")]
    public FsmGameObject gameObject;

    // Token: 0x0400422B RID: 16939
    [HutongGames.PlayMaker.Tooltip("Optional Spawn Point.")]
    public FsmGameObject spawnPoint;

    // Token: 0x0400422C RID: 16940
    [HutongGames.PlayMaker.Tooltip("Position. If a Spawn Point is defined, this is used as a local offset from the Spawn Point position.")]
    public FsmVector3 position;

    // Token: 0x0400422D RID: 16941
    [HutongGames.PlayMaker.Tooltip("Rotation. NOTE: Overrides the rotation of the Spawn Point.")]
    public FsmVector3 rotation;

    // Token: 0x0400422E RID: 16942
    [UIHint(UIHint.Variable)]
    [HutongGames.PlayMaker.Tooltip("Optionally store the created object.")]
    public FsmGameObject storeObject;

    // Token: 0x0400422F RID: 16943
    [HutongGames.PlayMaker.Tooltip("Use Network.Instantiate to create a Game Object on all clients in a networked game.")]
    public FsmBool networkInstantiate;

    // Token: 0x04004230 RID: 16944
    [HutongGames.PlayMaker.Tooltip("Usually 0. The group number allows you to group together network messages which allows you to filter them if so desired.")]
    public FsmInt networkGroup;
}

