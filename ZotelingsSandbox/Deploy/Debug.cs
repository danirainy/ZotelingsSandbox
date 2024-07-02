namespace ZotelingsSandbox.Deploy;
internal class Debug
{
    private static void PrintVariableUsage(GameObject gameObject, string variableName)
    {
        foreach (var fsm in gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
        {
            foreach (var state in fsm.FsmStates)
            {
                for (int i = 0; i < state.Actions.Length; ++i)
                {
                    var action = state.Actions[i];
                    var type = action.GetType();
                    var flags = BindingFlags.Public | BindingFlags.Instance;
                    var fields = type.GetFields(flags);
                    foreach (var field in fields)
                    {
                        if (field.GetValue(action) is NamedVariable)
                        {
                            var value = field.GetValue(action) as NamedVariable;
                            if (value == null)
                            {
                                continue;
                            }
                            bool found = false;
                            if (value.Name == variableName)
                            {
                                found = true;
                            }
                            else if (value is FsmString)
                            {
                                var fsmString = value as FsmString;
                                if (fsmString.Value == variableName)
                                {
                                    found = true;
                                }
                            }
                            if (found)
                            {
                                Log.LogError($"{fsm.gameObject.name} : {fsm.FsmName} : {state.Name} : {i} of type {type.Name} is using {variableName}");
                            }
                        }
                    }
                }
            }
        }
    }
    public static void PrintVariablesUsage(GameObject gameObject)
    {
        HashSet<string> variableNames = new();
        foreach (var fsm in gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
        {
            foreach (var variable in fsm.FsmVariables.GetAllNamedVariables())
            {
                variableNames.Add(variable.Name);
            }
        }
        foreach (var variableName in variableNames)
        {
            PrintVariableUsage(gameObject, variableName);
        }
    }
    public static void PrintFSMExecution(PlayMakerFSM fsm)
    {
        foreach (var state in fsm.FsmStates)
        {
            for (var i = state.Actions.Length; i >= 0; --i)
            {
                var actionType = i - 1 >= 0 ? state.Actions[i - 1].GetType().Name : "Start";
                state.InsertCustomAction(() =>
                {
                    Log.LogError($"{fsm.gameObject.GetInstanceID()} : {fsm.gameObject.name} : {fsm.FsmName} : {state.Name} : {actionType}");
                    Log.LogError($"    Current position: {fsm.gameObject.transform.position}");
                    Log.LogError($"    Current scale: {fsm.gameObject.transform.localScale}");
                    var ridigbody = fsm.gameObject.GetComponent<Rigidbody2D>();
                    if (ridigbody != null)
                    {
                        Log.LogError($"    Current velocity: {ridigbody.velocity}");
                    }
                    var healthManger = fsm.gameObject.GetComponent<HealthManager>();
                    if (healthManger != null)
                    {
                        Log.LogError($"    Current health: {healthManger.hp}");
                    }
                    var instanceInfo = fsm.gameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
                    if (instanceInfo != null)
                    {
                        var targetDetector = instanceInfo.targetDetector;
                        if (targetDetector != null)
                        {
                            var target = targetDetector.target;
                            Log.LogError($"    Current target instance ID: {target.GetInstanceID()}");
                            Log.LogError($"    Current target name: {target.name}");
                            Log.LogError($"    Current target position: {target.transform.position}");
                        }
                    }
                    Log.LogError("    Variables:");
                    foreach (var variable in fsm.FsmVariables.GetAllNamedVariables())
                    {
                        Log.LogError($"        {variable.Name} : {variable.RawValue}");
                    }
                }, i);
            }
        }
    }
    public static void PrintGameObjectTree(GameObject gameObject, int level = 0)
    {
        Log.LogError($"{new String(' ', level * 4)}{gameObject.name}");
        for (int i = 0; i < gameObject.transform.childCount; ++i)
        {
            var child = gameObject.transform.GetChild(i).gameObject;
            PrintGameObjectTree(child, level + 1);
        }
    }
    public static void PrintInstance(GameObject gameObject, Action<string> logger)
    {
        logger($"    Instance : {gameObject.name}, Instance ID: {gameObject.GetInstanceID()}");
        logger($"    Layer : {gameObject.layer}, Position: {gameObject.transform.position}");
        var collider2D = gameObject.GetComponent<Collider2D>();
        if (collider2D != null)
        {
            logger($"    Collider 2D : {collider2D.bounds.center}, {collider2D.bounds.extents}");
        }
        if (gameObject.transform.parent != null)
        {
            logger($"    Parent : {gameObject.transform.parent.name}, Instance ID: {gameObject.transform.parent.gameObject.GetInstanceID()}");
        }
        var root = gameObject;
        while (root.transform.parent != null)
        {
            root = root.transform.parent.gameObject;
        }
        if (root != gameObject)
        {
            logger($"    Root : {root.name}, Instance ID: {root.GetInstanceID()}");
        }
        var instanceInfo = gameObject.GetComponentInParent<Deploy.Behaviors.InstanceInfo>(true);
        if (instanceInfo != null)
        {
            logger($"    Status : {instanceInfo.status}");
            logger($"    Group ID : {instanceInfo.groupID}");
            var creator = instanceInfo.creator;
            if (creator != null)
            {
                logger($"    Creator : {creator.name}, Instance ID: {creator.GetInstanceID()}");
            }
            if (instanceInfo.targetDetector != null)
            {
                var target = instanceInfo.targetDetector.target;
                logger($"    Target : {target.name}, Instance ID: {target.GetInstanceID()}");
            }
        }
    }
}
