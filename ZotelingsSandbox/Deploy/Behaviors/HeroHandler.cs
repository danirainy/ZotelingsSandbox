namespace ZotelingsSandbox.Deploy.Behaviors;
internal class HeroHandler : MonoBehaviour
{
    private void DestroyControllable()
    {
        if (!HasControllable)
        {
            Log.LogError("No controllable to destroy");
            return;
        }
        if (Controllable != null)
        {
            var childManger = Controllable.GetComponent<ChildManager>();
            foreach (var child in childManger.controllableChildren)
            {
                GameObject.Destroy(child.Value);
            }
            GameObject.Destroy(Controllable);
        }
    }
    public void InstallControllable(GameObject gameObject)
    {
        if (HasControllable)
        {
            DestroyControllable();
        }
        else
        {
            var heroController = HeroController.instance;
            if (heroController.controlReqlinquished)
            {
                Log.LogError("Hero control was relinquished");
                return;
            }
            heroController.RelinquishControl();
            originalGravityScale = heroController.GetComponent<Rigidbody2D>().gravityScale;
            GetComponent<Rigidbody2D>().gravityScale = 0;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Terrain"));
            GetComponent<tk2dSprite>().color = Vector4.zero;
            var heroCollider = GetComponent<BoxCollider2D>();
            originalColliderOffset = heroCollider.offset;
            originalColliderSize = heroCollider.size;
            var heroBoxCollider = transform.Find("HeroBox").GetComponent<BoxCollider2D>();
            originalHeroBoxColliderOffset = heroBoxCollider.offset;
            originalHeroBoxColliderSize = heroBoxCollider.size;
            HasControllable = true;
        }
        Controllable = gameObject;
    }
    public void UninstallControllable()
    {
        if (!HasControllable)
        {
            Log.LogError("No controllable to uninstall");
            return;
        }
        DestroyControllable();
        HasControllable = false;
        var heroController = HeroController.instance;
        heroController.RegainControl();
        GetComponent<Rigidbody2D>().gravityScale = originalGravityScale;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Terrain"), false);
        GetComponent<tk2dSprite>().color = Vector4.one;
        var heroCollider = GetComponent<BoxCollider2D>();
        heroCollider.offset = originalColliderOffset;
        heroCollider.size = originalColliderSize;
        var heroBoxCollider = transform.Find("HeroBox").GetComponent<BoxCollider2D>();
        heroBoxCollider.offset = originalHeroBoxColliderOffset;
        heroBoxCollider.size = originalHeroBoxColliderSize;
    }
    private void Update()
    {
        if (HasControllable)
        {
            if (Controllable == null)
            {
                UninstallControllable();
            }
            else
            {
                transform.position = Controllable.transform.position;
                var rigidbody2D = GetComponent<Rigidbody2D>();
                rigidbody2D.velocity = Vector2.zero;
                GetComponent<tk2dSprite>().color = Vector4.zero;
            }
        }
    }
    public bool HasControllable { get; private set; }
    public GameObject Controllable { get; private set; }
    private float originalGravityScale;
    private Vector2 originalColliderOffset;
    private Vector2 originalColliderSize;
    private Vector2 originalHeroBoxColliderOffset;
    private Vector2 originalHeroBoxColliderSize;
}
