namespace ZotelingsSandbox.Deploy.Actions;
// Token: 0x020001C4 RID: 452
[ActionCategory("Hollow Knight")]
public class CheckCanSeeHero : FsmStateAction
{
    // Token: 0x060009FB RID: 2555 RVA: 0x000372B1 File Offset: 0x000354B1
    public override void Reset()
    {
        storeResult = new FsmBool();
    }

    // Token: 0x060009FC RID: 2556 RVA: 0x000372BE File Offset: 0x000354BE
    public override void OnEnter()
    {
        source = Owner.GetComponent<LineOfSightDetector>();
        Apply();
        if (!everyFrame)
        {
            Finish();
        }
    }

    // Token: 0x060009FD RID: 2557 RVA: 0x000372E5 File Offset: 0x000354E5
    public override void OnUpdate()
    {
        Apply();
    }

    // Token: 0x060009FE RID: 2558 RVA: 0x000372ED File Offset: 0x000354ED
    private void Apply()
    {
        if (source != null)
        {
            storeResult.Value = true;
            return;
        }
        storeResult.Value = false;
    }

    // Token: 0x04000B1C RID: 2844
    [UIHint(UIHint.Variable)]
    public FsmBool storeResult;

    // Token: 0x04000B1D RID: 2845
    public bool everyFrame;

    // Token: 0x04000B1E RID: 2846
    private LineOfSightDetector source;
}
