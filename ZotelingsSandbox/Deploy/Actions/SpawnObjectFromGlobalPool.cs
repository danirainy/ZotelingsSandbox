namespace ZotelingsSandbox.Deploy.Actions;
// Token: 0x02000A63 RID: 2659
[ActionCategory(ActionCategory.GameObject)]
[HutongGames.PlayMaker.Tooltip("Spawns a prefab Game Object from the Global Object Pool on the Game Manager.")]
public class SpawnObjectFromGlobalPool : FsmStateAction
{
    // Token: 0x0600396E RID: 14702 RVA: 0x0014E84F File Offset: 0x0014CA4F
    public override void Reset()
    {
        gameObject = null;
        spawnPoint = null;
        position = new FsmVector3
        {
            UseVariable = true
        };
        rotation = new FsmVector3
        {
            UseVariable = true
        };
        storeObject = null;
    }

    // Token: 0x0600396F RID: 14703 RVA: 0x0014E88C File Offset: 0x0014CA8C
    public override void OnEnter()
    {
        if (gameObject.Value != null)
        {
            Vector3 a = Vector3.zero;
            Vector3 euler = Vector3.up;
            if (spawnPoint.Value != null)
            {
                a = spawnPoint.Value.transform.position;
                if (!position.IsNone)
                {
                    a += position.Value;
                }
                euler = !rotation.IsNone ? rotation.Value : spawnPoint.Value.transform.eulerAngles;
            }
            else
            {
                if (!position.IsNone)
                {
                    a = position.Value;
                }
                if (!rotation.IsNone)
                {
                    euler = rotation.Value;
                }
            }
            if (gameObject != null)
            {
                GameObject value = UnityEngine.Object.Instantiate(gameObject.Value, a, Quaternion.Euler(euler));
                var instanceInfo = Fsm.GameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
                var properties = new Templates.PlaceConfig
                {
                    groupID = instanceInfo.groupID,
                    hp = Common.DefaultEnemyHealth,
                    damage = instanceInfo.damage,
                    hpBar = false
                };
                RewriteInstance.Rewrite(value, properties, Fsm.GameObject);
                storeObject.Value = value;
            }
        }
        Finish();
    }

    // Token: 0x04003C2A RID: 15402
    [RequiredField]
    [HutongGames.PlayMaker.Tooltip("GameObject to create. Usually a Prefab.")]
    public FsmGameObject gameObject;

    // Token: 0x04003C2B RID: 15403
    [HutongGames.PlayMaker.Tooltip("Optional Spawn Point.")]
    public FsmGameObject spawnPoint;

    // Token: 0x04003C2C RID: 15404
    [HutongGames.PlayMaker.Tooltip("Position. If a Spawn Point is defined, this is used as a local offset from the Spawn Point position.")]
    public FsmVector3 position;

    // Token: 0x04003C2D RID: 15405
    [HutongGames.PlayMaker.Tooltip("Rotation. NOTE: Overrides the rotation of the Spawn Point.")]
    public FsmVector3 rotation;

    // Token: 0x04003C2E RID: 15406
    [UIHint(UIHint.Variable)]
    [HutongGames.PlayMaker.Tooltip("Optionally store the created object.")]
    public FsmGameObject storeObject;
}
