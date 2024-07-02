namespace ZotelingsSandbox.Deploy.Behaviors;
internal class InstanceInfo : MonoBehaviour
{
    public enum Status
    {
        Placing,
        Sleeping,
        Awaking,
        Active,
        Controlling
    }
    public enum Priority
    {
        Invalid,
        NativeEnemy,
        Knight,
        SpawnedInstance,
        StandaloneInstance
    }
    private void Update()
    {
        if (invincibleTimer > 0)
        {
            invincibleTimer -= Time.deltaTime;
        }
    }
    public Vector2 placingOffset;
    public GameObject creator;
    public GameObject rootCreator;
    public Status status;
    public int groupID;
    public Priority priority;
    public DetectTarget targetDetector;
    public FollowTarget targetFollower;
    public float gravityScale;
    public float invincibleTimer;
    public int damage;
}
