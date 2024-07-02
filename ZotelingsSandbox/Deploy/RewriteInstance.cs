namespace ZotelingsSandbox.Deploy;
internal class RewriteInstance
{
    public static void Initialize()
    {
        for (int i = 0; i < 32; ++i)
        {
            if (i != LayerMask.NameToLayer("Enemies") && i != LayerMask.NameToLayer("Tinker") && i != Common.ParryEnemiesLayer)
            {
                Physics2D.IgnoreLayerCollision(Common.DamageEnemiesLayer, i);
            }
        }
        for (int i = 0; i < 32; ++i)
        {
            if (i != Common.DamageEnemiesLayer)
            {
                Physics2D.IgnoreLayerCollision(Common.ParryEnemiesLayer, i);
            }
        }
    }
    public static void Rewrite(GameObject gameObject, Templates.PlaceConfig properties, GameObject creator)
    {
        GameObject.Destroy(gameObject.GetComponent<ConstrainPosition>());
        GameObject.Destroy(gameObject.GetComponent<PersistentBoolItem>());
        GameObject.Destroy(gameObject.GetComponent<SetZ>());
        var instanceInfo = gameObject.AddComponent<Behaviors.InstanceInfo>();
        instanceInfo.placingOffset = properties.placingOffset;
        instanceInfo.creator = creator;
        instanceInfo.rootCreator = creator;
        if (creator != null)
        {
            while (true)
            {
                var nextInstanceInfo = instanceInfo.rootCreator.GetComponentInParent<Behaviors.InstanceInfo>(true);
                if (nextInstanceInfo != null && nextInstanceInfo.creator != null)
                {
                    instanceInfo.rootCreator = nextInstanceInfo.creator;
                }
                else
                {
                    break;
                }
            }
        }
        if (creator != null)
        {
            instanceInfo.status = Behaviors.InstanceInfo.Status.Active;
        }
        instanceInfo.groupID = properties.groupID;
        if (creator == null)
        {
            instanceInfo.priority = Behaviors.InstanceInfo.Priority.StandaloneInstance;
        }
        else
        {
            instanceInfo.priority = Behaviors.InstanceInfo.Priority.SpawnedInstance;
        }
        instanceInfo.damage = properties.damage;
        if (gameObject.GetComponent<HealthManager>() != null)
        {
            if (properties.hpBar)
            {
                HPBar.Install(gameObject, properties.groupID);
            }
            else
            {
                gameObject.DisableHPBar();
            }
        }
        if (creator == null)
        {
            var rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
            if (rigidbody2D != null)
            {
                instanceInfo.gravityScale = rigidbody2D.gravityScale;
                rigidbody2D.gravityScale = 0;
            }
        }
        var healthManager = gameObject.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            healthManager.hp = properties.hp;
        }
        RewriteRange.Rewrite(gameObject);
        RewriteTarget.Rewrite(gameObject);
        RewriteComponents.Rewrite(gameObject);
        gameObject.SetActive(true);
    }
}
