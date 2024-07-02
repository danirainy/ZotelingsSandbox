namespace ZotelingsSandbox.Deploy.RewriteComponent;
internal class RewriteFSM
{
    private static void RewriteActions(PlayMakerFSM fsm)
    {
        foreach (var state in fsm.FsmStates)
        {
            for (int i = 0; i < state.Actions.Length; ++i)
            {
                var action = state.Actions[i];
                var type = action.GetType();
                if (action is GetHero)
                {
                    var oldAction = action as GetHero;
                    var newAction = new Actions.GetHero()
                    {
                        storeResult = oldAction.storeResult
                    };
                    state.Actions[i] = newAction;
                    Log.LogKey("Rewrite", $"    Rewrote {state.Name} : {i} of type {type.Name}");
                }
                else if (action is CheckCanSeeHero)
                {
                    var oldAction = action as CheckCanSeeHero;
                    var newAction = new Actions.CheckCanSeeHero
                    {
                        storeResult = oldAction.storeResult,
                        everyFrame = oldAction.everyFrame
                    };
                    state.Actions[i] = newAction;
                    Log.LogKey("Rewrite", $"    Rewrote {state.Name} : {i} of type {type.Name}");
                }
                else if (action is SpawnObjectFromGlobalPool)
                {
                    var oldAction = action as SpawnObjectFromGlobalPool;
                    var newAction = new Actions.SpawnObjectFromGlobalPool
                    {
                        gameObject = oldAction.gameObject,
                        spawnPoint = oldAction.spawnPoint,
                        position = oldAction.position,
                        rotation = oldAction.rotation,
                        storeObject = oldAction.storeObject
                    };
                    state.Actions[i] = newAction;
                    Log.LogKey("Rewrite", $"    Rewrote {state.Name} : {i} of type {type.Name}");
                }
                else if (action is SpawnObjectFromGlobalPoolOverTimeV2)
                {
                    var oldAction = action as SpawnObjectFromGlobalPoolOverTimeV2;
                    var newAction = new Actions.SpawnObjectFromGlobalPoolOverTimeV2
                    {
                        gameObject = oldAction.gameObject,
                        spawnPoint = oldAction.spawnPoint,
                        position = oldAction.position,
                        rotation = oldAction.rotation,
                        frequency = oldAction.frequency,
                        scaleMin = oldAction.scaleMin,
                        scaleMax = oldAction.scaleMax,
                    };
                    state.Actions[i] = newAction;
                    Log.LogKey("Rewrite", $"    Rewrote {state.Name} : {i} of type {type.Name}");
                }
                else if (action is FlingObjectsFromGlobalPool)
                {
                    var oldAction = action as FlingObjectsFromGlobalPool;
                    var newAction = new Actions.FlingObjectsFromGlobalPool
                    {
                        gameObject = oldAction.gameObject,
                        spawnPoint = oldAction.spawnPoint,
                        position = oldAction.position,
                        spawnMin = oldAction.spawnMin,
                        spawnMax = oldAction.spawnMax,
                        speedMin = oldAction.speedMin,
                        speedMax = oldAction.speedMax,
                        angleMin = oldAction.angleMin,
                        angleMax = oldAction.angleMax,
                        originVariationX = oldAction.originVariationX,
                        originVariationY = oldAction.originVariationY,
                        FSM = oldAction.FSM,
                        FSMEvent = oldAction.FSMEvent,
                    };
                    state.Actions[i] = newAction;
                    Log.LogKey("Rewrite", $"    Rewrote {state.Name} : {i} of type {type.Name}");
                }
                else if (action is FlingObjectsFromGlobalPoolVel)
                {
                    var oldAction = action as FlingObjectsFromGlobalPoolVel;
                    var newAction = new Actions.FlingObjectsFromGlobalPoolVel
                    {
                        gameObject = oldAction.gameObject,
                        spawnPoint = oldAction.spawnPoint,
                        position = oldAction.position,
                        spawnMin = oldAction.spawnMin,
                        spawnMax = oldAction.spawnMax,
                        speedMinX = oldAction.speedMinX,
                        speedMaxX = oldAction.speedMaxX,
                        speedMinY = oldAction.speedMinY,
                        speedMaxY = oldAction.speedMaxY,
                        originVariationX = oldAction.originVariationX,
                        originVariationY = oldAction.originVariationY,
                    };
                    state.Actions[i] = newAction;
                    Log.LogKey("Rewrite", $"    Rewrote {state.Name} : {i} of type {type.Name}");
                }
                else if (action is FlingObjectsFromGlobalPoolTime)
                {
                    var oldAction = action as FlingObjectsFromGlobalPoolTime;
                    var newAction = new Actions.FlingObjectsFromGlobalPoolTime
                    {
                        gameObject = oldAction.gameObject,
                        spawnPoint = oldAction.spawnPoint,
                        position = oldAction.position,
                        frequency = oldAction.frequency,
                        spawnMin = oldAction.spawnMin,
                        spawnMax = oldAction.spawnMax,
                        speedMin = oldAction.speedMin,
                        speedMax = oldAction.speedMax,
                        angleMin = oldAction.angleMin,
                        angleMax = oldAction.angleMax,
                        originVariationX = oldAction.originVariationX,
                        originVariationY = oldAction.originVariationY,
                    };
                    state.Actions[i] = newAction;
                    Log.LogKey("Rewrite", $"    Rewrote {state.Name} : {i} of type {type.Name}");
                }
                else if (action is CreateObject)
                {
                    var oldAction = action as CreateObject;
                    var newAction = new Actions.CreateObject
                    {
                        gameObject = oldAction.gameObject,
                        spawnPoint = oldAction.spawnPoint,
                        position = oldAction.position,
                        rotation = oldAction.rotation,
                        storeObject = oldAction.storeObject,
                        networkInstantiate = oldAction.networkInstantiate,
                        networkGroup = oldAction.networkGroup
                    };
                    state.Actions[i] = newAction;
                    Log.LogKey("Rewrite", $"    Rewrote {state.Name} : {i} of type {type.Name}");
                }
                else if (action is SpawnRandomObjects)
                {
                    var oldAction = action as SpawnRandomObjects;
                    var newAction = new Actions.SpawnRandomObjects
                    {
                        gameObject = oldAction.gameObject,
                        spawnPoint = oldAction.spawnPoint,
                        position = oldAction.position,
                        spawnMin = oldAction.spawnMin,
                        spawnMax = oldAction.spawnMax,
                        speedMin = oldAction.speedMin,
                        speedMax = oldAction.speedMax,
                        angleMin = oldAction.angleMin,
                        angleMax = oldAction.angleMax,
                        originVariation = oldAction.originVariation,
                    };
                    state.Actions[i] = newAction;
                    Log.LogKey("Rewrite", $"    Rewrote {state.Name} : {i} of type {type.Name}");
                }
            }
        }
    }
    private static void RewriteHero(PlayMakerFSM fsm)
    {
        var instanceInfo = fsm.Fsm.GameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
        var targetFollower = fsm.FsmVariables.GetFsmGameObject("ZotelingsSandbox.Hero");
        if (instanceInfo.targetFollower != null)
        {
            targetFollower.Value = instanceInfo.targetFollower.gameObject;
        }
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
                    if (field.FieldType == typeof(FsmGameObject))
                    {
                        var value = field.GetValue(action) as FsmGameObject;
                        if (value.Value == HeroController.instance.gameObject)
                        {
                            field.SetValue(action, targetFollower);
                            Log.LogKey("Rewrite", $"    Redirected {state.Name} : {i} of type {type.Name}");
                        }
                    }
                    else if (field.FieldType == typeof(FsmOwnerDefault))
                    {
                        var value = field.GetValue(action) as FsmOwnerDefault;
                        if (value.OwnerOption == OwnerDefaultOption.SpecifyGameObject)
                        {
                            if (value.GameObject.Value == HeroController.instance.gameObject)
                            {
                                value.GameObject = targetFollower;
                                Log.LogKey("Rewrite", $"    Redirected {state.Name} : {i} of type {type.Name}");
                            }
                        }
                    }
                }
            }
        }
    }
    private static void RewriteAudio(PlayMakerFSM fsm)
    {
        foreach (var state in fsm.FsmStates)
        {
            fsm.RemoveActionsOfType<TransitionToAudioSnapshot>(state.Name);
            fsm.RemoveActionsOfType<ApplyMusicCue>(state.Name);
        }
    }
    public static void Rewrite(PlayMakerFSM fsm)
    {
        Log.LogKey("Rewrite", $"Rewriting FSM {fsm.gameObject.name} : {fsm.FsmName}");
        RewriteActions(fsm);
        RewriteHero(fsm);
        RewriteAudio(fsm);
    }
}
