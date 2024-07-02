namespace ZotelingsSandbox.Deploy;
internal class RewriteControllable
{
    public static void Rewrite(GameObject gameObject)
    {
        var instanceInfo = gameObject.GetAddComponent<Behaviors.InstanceInfo>();
        instanceInfo.status = Behaviors.InstanceInfo.Status.Controlling;
        var damageEnemy = gameObject.transform.Find(Common.DamageEnemyGameObjectName).gameObject;
        if (damageEnemy != null)
        {
            GameObject.Destroy(damageEnemy);
        }
        else
        {
            Log.LogError("DamageEnemy not found");
        }
        foreach (var damageHero in gameObject.GetComponentsInChildren<DamageHero>(true))
        {
            UnityEngine.Object.Destroy(damageHero);
        }
        foreach (var healthManager in gameObject.GetComponentsInChildren<HealthManager>(true))
        {
            UnityEngine.Object.Destroy(healthManager);
        }
    }
}
