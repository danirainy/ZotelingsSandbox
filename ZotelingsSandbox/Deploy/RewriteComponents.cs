namespace ZotelingsSandbox.Deploy;
internal class RewriteComponents
{
    public static void Rewrite(GameObject gameObject)
    {
        foreach (var fsm in gameObject.GetComponentsInChildren<PlayMakerFSM>(true))
        {
            RewriteComponent.RewriteFSM.Rewrite(fsm);
        }
        foreach (var damageHero in gameObject.GetComponentsInChildren<DamageHero>(true))
        {
            RewriteComponent.RewriteDamage.Rewrite(damageHero);
        }
        foreach (var alertRange in gameObject.GetComponentsInChildren<AlertRange>(true))
        {
            RewriteComponent.RewriteAlertRange.Rewrite(alertRange);
        }
        if (Config.renderColider)
        {
            foreach (var collider2D in gameObject.GetComponentsInChildren<Collider2D>(true))
            {
                collider2D.gameObject.GetAddComponent<Behaviors.ColliderRender>();
            }
        }
    }
}
