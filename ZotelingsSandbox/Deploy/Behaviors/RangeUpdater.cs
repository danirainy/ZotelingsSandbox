namespace ZotelingsSandbox.Deploy.Behaviors;
internal class RangeUpdater : MonoBehaviour
{
    public class RangeInfo
    {
        public string rangeGameObjectName;
        public string rangeVariableName;
        public string fsmName;
    }
    private void FixedUpdate()
    {
        if (UnityEngine.Random.Range(0f, 1) > 0.2f)
        {
            return;
        }
        var instanceInfo = gameObject.GetComponent<InstanceInfo>();
        if (instanceInfo.status != InstanceInfo.Status.Active)
        {
            return;
        }
        if (instanceInfo.targetDetector.target == null)
        {
            return;
        }
        foreach (var range in ranges)
        {
            var rangeGameObject = gameObject.transform.Find(range.rangeGameObjectName).gameObject;
            var rangeCollider = rangeGameObject.GetComponent<Collider2D>();
            var overlappedColliders = new List<Collider2D>();
            var contactFilter2D = new ContactFilter2D
            {
                layerMask = 1 << instanceInfo.targetDetector.target.layer,
            };
            rangeCollider.OverlapCollider(contactFilter2D, overlappedColliders);
            var collide = false;
            foreach (var overlappedCollider in overlappedColliders)
            {
                var targetCandidate = overlappedCollider.gameObject;
                while (targetCandidate.transform.parent != null)
                {
                    targetCandidate = targetCandidate.transform.parent.gameObject;
                }
                if (targetCandidate == instanceInfo.targetDetector.target)
                {
                    collide = true;
                    break;
                }
            }
            var fsm = gameObject.LocateMyFSM(range.fsmName);
            if (fsm == null)
            {
                continue;
            }
            fsm.FsmVariables.GetFsmBool(range.rangeVariableName).Value = collide;
        }
    }
    public List<RangeInfo> ranges;
}
