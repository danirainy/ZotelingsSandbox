namespace ZotelingsSandbox.Deploy.Behaviors;
internal class ColliderCopier : MonoBehaviour
{
    public void Update()
    {
        myCollider2D.GetCopyOf(parentCollider2D);
        myCollider2D.isTrigger = isTrigger;
    }
    public Collider2D myCollider2D;
    public Collider2D parentCollider2D;
    public bool isTrigger;
}