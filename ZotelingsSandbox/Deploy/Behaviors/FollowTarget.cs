namespace ZotelingsSandbox.Deploy.Behaviors;
internal class FollowTarget : MonoBehaviour
{
    private void Update()
    {
        if (targetDetector == null)
        {
            GameObject.Destroy(gameObject);
            return;
        }
        var target = targetDetector.target;
        if (target == null)
        {
            return;
        }
        transform.position = target.transform.position;
    }
    public DetectTarget targetDetector;
}
