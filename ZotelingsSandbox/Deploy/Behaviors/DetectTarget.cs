namespace ZotelingsSandbox.Deploy.Behaviors;
internal class DetectTarget : MonoBehaviour
{
    private void Start()
    {
        groupID = GetComponentInParent<InstanceInfo>().groupID;
    }
    private InstanceInfo.Priority Priority(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return InstanceInfo.Priority.Invalid;
        }
        var instanceInfo = gameObject.GetComponent<InstanceInfo>();
        if (instanceInfo != null)
        {
            if (instanceInfo.status != InstanceInfo.Status.Active || instanceInfo.groupID == groupID)
            {
                return InstanceInfo.Priority.Invalid;
            }
            return instanceInfo.priority;
        }
        if (gameObject.GetComponent<HeroController>())
        {
            return InstanceInfo.Priority.Knight;
        }
        if (gameObject.GetComponent<HealthManager>())
        {
            return InstanceInfo.Priority.NativeEnemy;
        }
        return InstanceInfo.Priority.Invalid;
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < 0.1f)
        {
            return;
        }
        timer = 0;
        var collider2D = GetComponent<Collider2D>();
        var overlappedColliders = new List<Collider2D>();
        var contactFilter2D = new ContactFilter2D
        {
            layerMask = 1 << LayerMask.NameToLayer("Player") + 1 << LayerMask.NameToLayer("Enemies")
        };
        collider2D.OverlapCollider(contactFilter2D, overlappedColliders);
        var currentPriority = Priority(target);
        foreach (var overlappedCollider in overlappedColliders)
        {
            var targetCandidate = overlappedCollider.gameObject;
            while (targetCandidate.transform.parent != null)
            {
                targetCandidate = targetCandidate.transform.parent.gameObject;
            }
            var priorityCandidate = Priority(targetCandidate);
            if (priorityCandidate > currentPriority || (priorityCandidate == currentPriority && targetCandidate == lastHitter))
            {
                target = targetCandidate;
                currentPriority = priorityCandidate;
            }
        }
        if (currentPriority == InstanceInfo.Priority.Invalid)
        {
            target = HeroController.instance.gameObject;
        }
    }
    public GameObject target;
    public GameObject lastHitter;
    private int groupID;
    private float timer;
}
