namespace ZotelingsSandbox.Deploy;
internal class RewriteTarget
{
    private static bool TargetsHero(GameObject gameObject)
    {
        foreach (var fsm in gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
        {
            foreach (var state in fsm.FsmStates)
            {
                foreach (var action in state.Actions)
                {
                    if (action is GetHero)
                    {
                        return true;
                    }
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
                                return true;
                            }
                        }
                        else if (field.FieldType == typeof(FsmOwnerDefault))
                        {
                            var value = field.GetValue(action) as FsmOwnerDefault;
                            if (value.OwnerOption == OwnerDefaultOption.SpecifyGameObject)
                            {
                                if (value.GameObject.Value == HeroController.instance.gameObject)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
    public static void Rewrite(GameObject gameObject)
    {
        if (TargetsHero(gameObject))
        {
            var targetDetector = new GameObject("ZotelingsSandbox.TargetDetector");
            targetDetector.layer = LayerMask.NameToLayer("TransitionGates");
            targetDetector.transform.parent = gameObject.transform;
            targetDetector.transform.localPosition = Vector3.zero;
            targetDetector.transform.localRotation = Quaternion.identity;
            targetDetector.transform.localScale = Vector3.one;
            var circleCollider2D = targetDetector.AddComponent<CircleCollider2D>();
            circleCollider2D.radius = 16;
            circleCollider2D.isTrigger = true;
            var detectTarget = targetDetector.AddComponent<Behaviors.DetectTarget>();
            detectTarget.target = HeroController.instance.gameObject;
            var targerFollower = new GameObject($"{gameObject.name}.TargetFollower");
            var followTarget = targerFollower.AddComponent<Behaviors.FollowTarget>();
            followTarget.targetDetector = detectTarget;
            var instanceInfo = gameObject.GetComponent<Behaviors.InstanceInfo>();
            instanceInfo.targetDetector = detectTarget;
            instanceInfo.targetFollower = followTarget;
        }
    }
}
