namespace ZotelingsSandbox.Deploy.RewriteComponent;
internal class RewriteDamage
{
    private static void RewriteDamageEnemy(DamageHero damageHero)
    {
        Log.LogKey("Rewrite", $"    Adding damage enemy to {damageHero.gameObject.name}");
        var damageEnemy = new GameObject(Common.DamageEnemyGameObjectName);
        damageEnemy.layer = Common.DamageEnemiesLayer;
        damageEnemy.transform.parent = damageHero.transform;
        damageEnemy.transform.localPosition = Vector3.zero;
        damageEnemy.transform.localRotation = Quaternion.identity;
        damageEnemy.transform.localScale = Vector3.one;
        var parentCollider2D = damageHero.GetComponent<Collider2D>();
        if (parentCollider2D == null)
        {
            Log.LogError("Collider2D not found");
            return;
        }
        var myCollider2D = damageEnemy.AddComponent(parentCollider2D.GetType());
        var colliderCopier = damageEnemy.AddComponent<Behaviors.ColliderCopier>();
        colliderCopier.myCollider2D = myCollider2D as Collider2D;
        colliderCopier.parentCollider2D = parentCollider2D;
        colliderCopier.isTrigger = true;
        colliderCopier.Update();
        var damageEnemies = damageEnemy.AddComponent<Behaviors.DamageEnemies>();
        damageEnemies.attackType = AttackTypes.Generic;
        damageEnemies.specialType = SpecialTypes.None;
        damageEnemies.direction = 0;
        damageEnemies.circleDirection = true;
        damageEnemies.moveDirection = false;
        damageEnemies.ignoreInvuln = false;
        damageEnemies.magnitudeMult = 1;
    }
    private static void RewriteParryEnemy(DamageHero damageHero)
    {
        bool filter(PlayMakerFSM fsm)
        {
            foreach (var state in fsm.FsmStates)
            {
                if (state.Name == "Blocked Hit")
                {
                    return true;
                }
            }
            return false;
        }
        var fsms = damageHero.GetComponents<PlayMakerFSM>().Where(filter);
        if (fsms.Count() == 0)
        {
            return;
        }
        Log.LogKey("Rewrite", $"    Adding parry enemy to {damageHero.gameObject.name}");
        damageHero.gameObject.AddComponent<Behaviors.CanParry>();
        foreach (var fsm in fsms)
        {
            UnityEngine.GameObject.Destroy(fsm);
        }
        var parryEnemy = new GameObject("ZotelingsSandbox.ParryEnemey");
        parryEnemy.layer = Common.ParryEnemiesLayer;
        parryEnemy.transform.parent = damageHero.transform;
        parryEnemy.transform.localPosition = Vector3.zero;
        parryEnemy.transform.localRotation = Quaternion.identity;
        parryEnemy.transform.localScale = Vector3.one;
        var parentCollider2D = damageHero.GetComponent<Collider2D>();
        if (parentCollider2D == null)
        {
            Log.LogError("Collider2D not found");
            return;
        }
        var myCollider2D = parryEnemy.AddComponent(parentCollider2D.GetType());
        var colliderCopier = parryEnemy.AddComponent<Behaviors.ColliderCopier>();
        colliderCopier.myCollider2D = myCollider2D as Collider2D;
        colliderCopier.parentCollider2D = parentCollider2D;
        colliderCopier.isTrigger = false;
        colliderCopier.Update();
    }
    public static void Rewrite(DamageHero damageHero)
    {
        Log.LogKey("Rewrite", $"Rewriting damage {damageHero.gameObject.name}");
        RewriteDamageEnemy(damageHero);
        RewriteParryEnemy(damageHero);
    }
}
