namespace ZotelingsSandbox.Deploy.Behaviors;
// Token: 0x02000176 RID: 374
public class DamageEnemies : MonoBehaviour
{
    // Token: 0x06000895 RID: 2197 RVA: 0x0002F0F0 File Offset: 0x0002D2F0
    private void Reset()
    {
        foreach (PlayMakerFSM playMakerFSM in GetComponents<PlayMakerFSM>())
        {
            if (playMakerFSM.FsmName == "damages_enemy")
            {
                attackType = (AttackTypes)playMakerFSM.FsmVariables.GetFsmInt("attackType").Value;
                circleDirection = playMakerFSM.FsmVariables.GetFsmBool("circleDirection").Value;
                damageDealt = playMakerFSM.FsmVariables.GetFsmInt("damageDealt").Value;
                direction = playMakerFSM.FsmVariables.GetFsmFloat("direction").Value;
                ignoreInvuln = playMakerFSM.FsmVariables.GetFsmBool("Ignore Invuln").Value;
                magnitudeMult = playMakerFSM.FsmVariables.GetFsmFloat("magnitudeMult").Value;
                moveDirection = playMakerFSM.FsmVariables.GetFsmBool("moveDirection").Value;
                specialType = (SpecialTypes)playMakerFSM.FsmVariables.GetFsmInt("Special Type").Value;
                return;
            }
        }
    }

    // Token: 0x06000896 RID: 2198 RVA: 0x0002F20A File Offset: 0x0002D40A
    private void OnCollisionEnter2D(Collision2D collision)
    {
        DoDamage(collision.gameObject);
    }

    // Token: 0x06000897 RID: 2199 RVA: 0x0002F218 File Offset: 0x0002D418
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var sender = gameObject;
        var receiver = collision.gameObject;
        var senderInstanceInfo = sender.GetComponentInParent<InstanceInfo>(true);
        var receiverInstanceInfo = receiver.GetComponentInParent<InstanceInfo>(true);
        if (senderInstanceInfo == null)
        {
            Log.LogError($"DamageEnemies: Sender {sender.name} does not have InstanceInfo");
            Debug.PrintInstance(sender, Log.LogError);
            return;
        }
        if (senderInstanceInfo.status != InstanceInfo.Status.Active && senderInstanceInfo.status != InstanceInfo.Status.Controlling)
        {
            return;
        }
        if (receiverInstanceInfo != null && receiverInstanceInfo.status != InstanceInfo.Status.Active && receiverInstanceInfo.status != InstanceInfo.Status.Controlling)
        {
            return;
        }
        if (receiverInstanceInfo != null && senderInstanceInfo.groupID == receiverInstanceInfo.groupID)
        {
            return;
        }
        var layer = receiver.layer;
        if (layer == LayerMask.NameToLayer("Tinker") || layer == Common.ParryEnemiesLayer)
        {
            if (SharedObjects.parryEffect == null)
            {
                return;
            }
            if (sender.GetComponentInParent<CanParry>(true) == null)
            {
                return;
            }
            sender = sender.GetComponentInParent<CanParry>(true).gameObject;
            if (layer == Common.ParryEnemiesLayer)
            {
                receiver = receiver.GetComponentInParent<CanParry>(true).gameObject;
                if (sender.GetInstanceID() < receiver.GetInstanceID())
                {
                    return;
                }
                if (UnityEngine.Random.Range(0, 1.0f) > 0.5f)
                {
                    (receiver, sender) = (sender, receiver);
                }
            }
            var parryEffect = SharedObjects.parryEffect.LocateMyFSM("FSM");
            parryEffect.transform.position = sender.transform.position;
            parryEffect.GetState("Blocked Hit").Actions[0].OnEnter();
            senderInstanceInfo.invincibleTimer = HeroController.instance.Reflect().INVUL_TIME_PARRY * 2;
            if (layer == LayerMask.NameToLayer("Tinker"))
            {
                parryEffect.GetState("Blocked Hit").Actions[1].OnEnter();
            }
            else
            {
                receiverInstanceInfo.invincibleTimer = HeroController.instance.Reflect().INVUL_TIME_PARRY * 2;
                if (senderInstanceInfo.status == InstanceInfo.Status.Controlling || receiverInstanceInfo.status == InstanceInfo.Status.Controlling)
                {
                    parryEffect.GetState("Blocked Hit").Actions[1].OnEnter();
                }
            }
            parryEffect.GetState("Blocked Hit").Actions[4].OnEnter();
            parryEffect.GetState("Blocked Hit").Actions[5].OnEnter();
            var parryVisual = sender.transform.position.x < receiver.transform.position.x ? parryEffect.GetState("No Box Right") : parryEffect.GetState("No Box Left");
            (parryVisual.Actions[1] as SpawnObjectFromGlobalPool).spawnPoint = sender;
            parryVisual.Actions[1].OnEnter();
            Log.LogKey("Damage", $"Parry, Sender: {sender.name}, Receiver: {receiver.name}");
            return;
        }
        if (layer != LayerMask.NameToLayer("Enemies"))
        {
            Log.LogError("DamageEnemies: Invalid layer for collision: " + LayerMask.LayerToName(layer));
            return;
        }
        if (receiverInstanceInfo != null && receiverInstanceInfo.status != InstanceInfo.Status.Active)
        {
            return;
        }
        var healthManager = receiver.GetComponentInParent<HealthManager>(true);
        if (healthManager == null)
        {
            return;
        }
        receiver = healthManager.gameObject;
        Log.LogKey("Damage", $"Collision, Sender: {sender.name}, Receiver: {receiver.name}");
        var logger = (string msg) =>
        {
            if (Config.logKeys.Contains("Damage"))
            {
                Log.LogKey("Damage", msg);
            }
        };
        Log.LogKey("Damage", "Sender:");
        Debug.PrintInstance(sender, logger);
        Log.LogKey("Damage", "Receiver:");
        Debug.PrintInstance(receiver, logger);
        Log.LogKey("Damage", "");
        damageDealt = senderInstanceInfo.damage;
        DoDamage(receiver);
        if (receiverInstanceInfo != null && receiverInstanceInfo.targetDetector != null)
        {
            var currentInstanceInfo = senderInstanceInfo;
            while (currentInstanceInfo.creator != null)
            {
                var creator = currentInstanceInfo.creator;
                currentInstanceInfo = creator.GetComponentInParent<InstanceInfo>(true);
            }
            receiverInstanceInfo.targetDetector.lastHitter = currentInstanceInfo.gameObject;
            Log.LogKey("Damage", $"Setting last hitter of {receiverInstanceInfo.gameObject.name} to {currentInstanceInfo.gameObject.name}");
        }
        var enemyBullet = sender.GetComponentInParent<EnemyBullet>(true);
        if (enemyBullet != null)
        {
            enemyBullet.Reflect().active = false;
            enemyBullet.StartCoroutine(enemyBullet.Reflect().Collision(Vector2.zero, false));
        }
    }

    // Token: 0x06000898 RID: 2200 RVA: 0x0002F277 File Offset: 0x0002D477
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (enteredColliders.Contains(collision))
        {
            enteredColliders.Remove(collision);
        }
    }

    // Token: 0x06000899 RID: 2201 RVA: 0x0002F294 File Offset: 0x0002D494
    private void OnDisable()
    {
        enteredColliders.Clear();
    }

    // Token: 0x0600089A RID: 2202 RVA: 0x0002F2A4 File Offset: 0x0002D4A4
    private void FixedUpdate()
    {
        for (int i = enteredColliders.Count - 1; i >= 0; i--)
        {
            Collider2D collider2D = enteredColliders[i];
            if (collider2D == null || !collider2D.isActiveAndEnabled)
            {
                enteredColliders.RemoveAt(i);
            }
            else
            {
                DoDamage(collider2D.gameObject);
            }
        }
    }

    // Token: 0x0600089B RID: 2203 RVA: 0x0002F304 File Offset: 0x0002D504
    private void DoDamage(GameObject target)
    {
        if (damageDealt <= 0)
        {
            return;
        }
        FSMUtility.SendEventToGameObject(target, "TAKE DAMAGE", false);
        HitTaker.Hit(target, new HitInstance
        {
            Source = gameObject,
            AttackType = attackType,
            CircleDirection = circleDirection,
            DamageDealt = damageDealt,
            Direction = direction,
            IgnoreInvulnerable = ignoreInvuln,
            MagnitudeMultiplier = magnitudeMult,
            MoveAngle = 0f,
            MoveDirection = moveDirection,
            Multiplier = 1f,
            SpecialType = specialType,
            IsExtraDamage = false
        }, 3);
    }

    // Token: 0x0600089C RID: 2204 RVA: 0x0002F3CC File Offset: 0x0002D5CC
    public DamageEnemies()
    {
        attackType = AttackTypes.Generic;
        ignoreInvuln = true;
        enteredColliders = new List<Collider2D>();
    }

    // Token: 0x0400097C RID: 2428
    public AttackTypes attackType;

    // Token: 0x0400097D RID: 2429
    public bool circleDirection;

    // Token: 0x0400097E RID: 2430
    public int damageDealt;

    // Token: 0x0400097F RID: 2431
    public float direction;

    // Token: 0x04000980 RID: 2432
    public bool ignoreInvuln;

    // Token: 0x04000981 RID: 2433
    public float magnitudeMult;

    // Token: 0x04000982 RID: 2434
    public bool moveDirection;

    // Token: 0x04000983 RID: 2435
    public SpecialTypes specialType;

    // Token: 0x04000984 RID: 2436
    private List<Collider2D> enteredColliders;
}
