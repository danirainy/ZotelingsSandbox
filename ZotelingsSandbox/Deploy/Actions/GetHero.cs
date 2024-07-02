namespace ZotelingsSandbox.Deploy.Actions;
// Token: 0x02000025 RID: 37
[ActionCategory("Hollow Knight")]
public class GetHero : FsmStateAction
{
    // Token: 0x060000FC RID: 252 RVA: 0x0000615F File Offset: 0x0000435F
    public override void Reset()
    {
        base.Reset();
        storeResult = new FsmGameObject();
    }

    // Token: 0x060000FD RID: 253 RVA: 0x00006174 File Offset: 0x00004374
    public override void OnEnter()
    {
        base.OnEnter();
        HeroController instance = HeroController.instance;
        var instanceInfo = Fsm.GameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
        storeResult.Value = instanceInfo.targetFollower.gameObject;
        Finish();
    }

    // Token: 0x040000B0 RID: 176
    [UIHint(UIHint.Variable)]
    public FsmGameObject storeResult;
}
