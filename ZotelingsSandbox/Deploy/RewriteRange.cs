namespace ZotelingsSandbox.Deploy;
internal class RewriteRange
{
    private static bool IsRange(GameObject gameObject)
    {
        if (gameObject.layer != LayerMask.NameToLayer("Hero Detector"))
        {
            return false;
        }
        var colloider = gameObject.GetComponent<Collider2D>();
        if (colloider == null)
        {
            return false;
        }
        if (!colloider.isTrigger)
        {
            return false;
        }
        var fsms = gameObject.GetComponents<PlayMakerFSM>();
        if (fsms.Length != 1)
        {
            return false;
        }
        var fsm = fsms[0];
        var strings = fsm.FsmVariables.StringVariables;
        if (strings.Length != 2)
        {
            return false;
        }
        if (strings[0].Name != "Bool Name" && strings[1].Name != "Bool Name")
        {
            return false;
        }
        if (strings[0].Name != "FSM Name" && strings[1].Name != "FSM Name")
        {
            return false;
        }
        return true;
    }
    public static void Rewrite(GameObject gameObject)
    {
        List<Behaviors.RangeUpdater.RangeInfo> ranges = [];
        for (var i = 0; i < gameObject.transform.childCount; ++i)
        {
            var child = gameObject.transform.GetChild(i).gameObject;
            if (IsRange(child.gameObject))
            {
                Log.LogKey("Rewrite", $"Rewriting range {child.name}");
                var fsm = child.GetComponent<PlayMakerFSM>();
                var boolName = fsm.FsmVariables.GetFsmString("Bool Name").Value;
                var fsmName = fsm.FsmVariables.GetFsmString("FSM Name").Value;
                ranges.Add(new Behaviors.RangeUpdater.RangeInfo
                {
                    rangeGameObjectName = child.name,
                    rangeVariableName = boolName,
                    fsmName = fsmName
                });
                child.layer = LayerMask.NameToLayer("TransitionGates");
                UnityEngine.Object.Destroy(fsm);
            }
        }
        if (ranges.Count != 0)
        {
            var rangeUpdater = gameObject.AddComponent<Behaviors.RangeUpdater>();
            rangeUpdater.ranges = ranges;
        }
    }
}
