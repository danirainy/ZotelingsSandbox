namespace ZotelingsSandbox.Common;
internal static class FSM
{
    public static FsmInt AccessIntVariable(this PlayMakerFSM fsm, string name)
    {
        FsmInt fsmInt = fsm.FsmVariables.IntVariables.FirstOrDefault(x => x.Name == name);
        if (fsmInt != null)
            return fsmInt;
        fsmInt = new FsmInt(name);
        fsm.FsmVariables.IntVariables = fsm.FsmVariables.IntVariables.Append(fsmInt).ToArray();
        return fsmInt;
    }
    public static FsmFloat AccessFloatVariable(this PlayMakerFSM fsm, string name)
    {
        FsmFloat fsmFloat = fsm.FsmVariables.FloatVariables.FirstOrDefault(x => x.Name == name);
        if (fsmFloat != null)
            return fsmFloat;
        fsmFloat = new FsmFloat(name);
        fsm.FsmVariables.FloatVariables = fsm.FsmVariables.FloatVariables.Append(fsmFloat).ToArray();
        return fsmFloat;
    }
    public static FsmGameObject AccessGameObjectVariable(this PlayMakerFSM fsm, string name)
    {
        FsmGameObject fsmGameObject = fsm.FsmVariables.GameObjectVariables.FirstOrDefault(x => x.Name == name);
        if (fsmGameObject != null)
            return fsmGameObject;
        fsmGameObject = new FsmGameObject(name);
        fsm.FsmVariables.GameObjectVariables = fsm.FsmVariables.GameObjectVariables.Append(fsmGameObject).ToArray();
        return fsmGameObject;
    }
    public static void RemoveActionsOfType<T>(this PlayMakerFSM fsm, string stateName)
    {
        var state = fsm.GetState(stateName);
        state.Actions = state.Actions.Where(action => action is not T).ToArray();
    }
}
