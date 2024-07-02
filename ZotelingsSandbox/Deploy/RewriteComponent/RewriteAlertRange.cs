namespace ZotelingsSandbox.Deploy.RewriteComponent;
internal class RewriteAlertRange
{
    public static void Rewrite(AlertRange alertRange)
    {
        Log.LogKey("Rewrite", $"Rewriting alert range {alertRange.gameObject.name}");
        alertRange.gameObject.layer = LayerMask.NameToLayer("TransitionGates");
    }
}
