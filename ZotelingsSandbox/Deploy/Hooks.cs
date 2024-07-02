namespace ZotelingsSandbox.Deploy;
internal class Hooks
{
    public delegate bool orig_IsHeroInRange(AlertRange self);
    public static void Initialize()
    {
        On.HeroBox.CheckForDamage += HeroBoxCheckForDamage;
        On.HeroController.TakeDamage += HeroControllerTakeDamage;
        On.HeroController.RegainControl += HeroControllerRegainControl;
        On.HealthManager.Hit += HealthManagerHit;
        On.HealthManager.Die += HealthManagerDie;
        On.PlayerData.TakeHealth += PlayerDataTakeHealth;
        new Hook(
            typeof(AlertRange).GetProperty("IsHeroInRange", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
            typeof(Hooks).GetMethod("AlertRangeIsHeroInRange", BindingFlags.Static | BindingFlags.NonPublic)
        );
        On.Walker.UpdateWaitingForConditions += WalkerUpdateWaitingForConditions;
        On.Walker.UpdateWalking += WalkerUpdateWalking;
        On.EnemyDeathEffects.EmitCorpse += EnemyDeathEffectsEmitCorpse;
        On.HutongGames.PlayMaker.Fsm.EnterState += FsmEnterState;
        On.GeoCounter.AddGeo += GeoCounterAddGeo;
        On.HutongGames.PlayMaker.Actions.CheckTargetDirection.DoCheckDirection += CheckTargetDirectionDoCheckDirection;
        On.PaintBullet.Update += PaintBulletUpdate;
        On.PaintBullet.Collision += PaintBulletCollision;
        On.EnemyBullet.Update += EnemyBulletUpdate;
        On.EnemySpawner.Update += EnemySpawnerUpdate;
    }
    private static void HeroBoxCheckForDamage(On.HeroBox.orig_CheckForDamage orig, HeroBox self, Collider2D otherCollider)
    {
        var instanceInfo = otherCollider.gameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
        if (instanceInfo != null && instanceInfo.status != Behaviors.InstanceInfo.Status.Active)
        {
            return;
        }
        var heroHandler = self.gameObject.GetComponentInParent<Behaviors.HeroHandler>();
        if (heroHandler != null && heroHandler.HasControllable)
        {
            var stateMachine = heroHandler.Controllable.GetComponent<StateMachine.StateMachine>();
            if (!stateMachine.CanTakeDamage())
            {
                return;
            }
        }
        orig(self, otherCollider);
    }
    private static void HeroControllerTakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, CollisionSide damageSide, int damageAmount, int hazardType)
    {
        var heroHandler = self.gameObject.GetComponent<Behaviors.HeroHandler>();
        if (heroHandler != null && heroHandler.HasControllable)
        {
            var stateMachine = heroHandler.Controllable.GetComponent<StateMachine.StateMachine>();
            stateMachine.TakeDamage(go, damageSide, damageAmount, hazardType);
        }
        orig(self, go, damageSide, damageAmount, hazardType);
    }
    private static void HeroControllerRegainControl(On.HeroController.orig_RegainControl orig, HeroController self)
    {
        var heroHandler = self.gameObject.GetComponent<Behaviors.HeroHandler>();
        if (heroHandler != null && heroHandler.HasControllable)
        {
            return;
        }
        orig(self);
    }
    private static void HealthManagerHit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
    {
        var instanceInfo = self.gameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
        if (instanceInfo != null && instanceInfo.status != Behaviors.InstanceInfo.Status.Active)
        {
            return;
        }
        var sender = hitInstance.Source;
        var receiver = self.gameObject;
        var damage = hitInstance.DamageDealt;
        if (instanceInfo != null && instanceInfo.invincibleTimer > 0)
        {
            Log.LogKey("Damage", $"Ignored damage to {receiver.name} due to it being invincible");
            return;
        }
        if (sender != null && receiver != null)
        {
            Log.LogKey("Damage", $"Damage, Sender: {sender.name}, Receiver: {receiver.name}, Damage Dealt: {damage}");
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
        }
        else
        {
            Log.LogKey("Damage", "Null sender or receiver");
        }
        orig(self, hitInstance);
    }
    private static void HealthManagerDie(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        var base_ = self;
        var this_ = self.Reflect();
        if (this_.isDead)
        {
            return;
        }
        if (this_.sprite)
        {
            this_.sprite.color = Color.white;
        }
        FSMUtility.SendEventToGameObject(base_.gameObject, "ZERO HP", false);
        if (this_.showGodfinderIcon)
        {
            GodfinderIcon.ShowIcon(this_.showGodFinderDelay, this_.unlockBossScene);
        }
        if (this_.unlockBossScene && !GameManager.instance.playerData.GetVariable<List<string>>("unlockedBossScenes").Contains(this_.unlockBossScene.name))
        {
            GameManager.instance.playerData.GetVariable<List<string>>("unlockedBossScenes").Add(this_.unlockBossScene.name);
        }
        if (this_.hasSpecialDeath)
        {
            this_.NonFatalHit(ignoreEvasion);
            return;
        }
        this_.isDead = true;
        if (this_.damageHero != null)
        {
            this_.damageHero.damageDealt = 0;
        }
        if (this_.battleScene != null && !this_.notifiedBattleScene)
        {
            PlayMakerFSM playMakerFSM = FSMUtility.LocateFSM(this_.battleScene, "Battle Control");
            if (playMakerFSM != null)
            {
                FsmInt fsmInt = playMakerFSM.FsmVariables.GetFsmInt("Battle Enemies");
                if (fsmInt != null)
                {
                    fsmInt.Value--;
                    this_.notifiedBattleScene = true;
                }
            }
        }
        if (this_.deathAudioSnapshot != null)
        {
            this_.deathAudioSnapshot.TransitionTo(6f);
        }
        if (this_.sendKilledTo != null)
        {
            FSMUtility.SendEventToGameObject(this_.sendKilledTo, "KILLED", false);
        }
        if (attackType == AttackTypes.Splatter)
        {
            GameCameras.instance.cameraShakeFSM.SendEvent("AverageShake");
            UnityEngine.Object.Instantiate<GameObject>(this_.corpseSplatPrefab, base_.transform.position + this_.effectOrigin, Quaternion.identity);
            if (this_.enemyDeathEffects)
            {
                this_.enemyDeathEffects.EmitSound();
            }
            UnityEngine.Object.Destroy(base_.gameObject);
            return;
        }
        var instanceInfo = self.gameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
        if (attackType != AttackTypes.RuinsWater && instanceInfo == null)
        {
            float angleMin = (float)(this_.megaFlingGeo ? 65 : 80);
            float angleMax = (float)(this_.megaFlingGeo ? 115 : 100);
            float speedMin = (float)(this_.megaFlingGeo ? 30 : 15);
            float speedMax = (float)(this_.megaFlingGeo ? 45 : 30);
            int num = this_.smallGeoDrops;
            int num2 = this_.mediumGeoDrops;
            int num3 = this_.largeGeoDrops;
            bool flag = false;
            if (GameManager.instance.playerData.GetBool("equippedCharm_24") && !GameManager.instance.playerData.GetBool("brokenCharm_24"))
            {
                num += Mathf.CeilToInt((float)num * 0.2f);
                num2 += Mathf.CeilToInt((float)num2 * 0.2f);
                num3 += Mathf.CeilToInt((float)num3 * 0.2f);
                flag = true;
            }
            GameObject[] gameObjects = FlingUtils.SpawnAndFling(new FlingUtils.Config
            {
                Prefab = this_.smallGeoPrefab,
                AmountMin = num,
                AmountMax = num,
                SpeedMin = speedMin,
                SpeedMax = speedMax,
                AngleMin = angleMin,
                AngleMax = angleMax
            }, base_.transform, this_.effectOrigin);
            if (flag)
            {
                this_.SetGeoFlashing(gameObjects, this_.smallGeoDrops);
            }
            gameObjects = FlingUtils.SpawnAndFling(new FlingUtils.Config
            {
                Prefab = this_.mediumGeoPrefab,
                AmountMin = num2,
                AmountMax = num2,
                SpeedMin = speedMin,
                SpeedMax = speedMax,
                AngleMin = angleMin,
                AngleMax = angleMax
            }, base_.transform, this_.effectOrigin);
            if (flag)
            {
                this_.SetGeoFlashing(gameObjects, this_.mediumGeoDrops);
            }
            gameObjects = FlingUtils.SpawnAndFling(new FlingUtils.Config
            {
                Prefab = this_.largeGeoPrefab,
                AmountMin = num3,
                AmountMax = num3,
                SpeedMin = speedMin,
                SpeedMax = speedMax,
                AngleMin = angleMin,
                AngleMax = angleMax
            }, base_.transform, this_.effectOrigin);
            if (flag)
            {
                this_.SetGeoFlashing(gameObjects, this_.largeGeoDrops);
            }
        }
        if (this_.enemyDeathEffects != null)
        {
            if (attackType == AttackTypes.RuinsWater || attackType == AttackTypes.Acid || attackType == AttackTypes.Generic)
            {
                this_.enemyDeathEffects.doKillFreeze = false;
            }
            this_.enemyDeathEffects.RecieveDeathEvent(attackDirection, this_.deathReset, attackType == AttackTypes.Spell, attackType == AttackTypes.RuinsWater || attackType == AttackTypes.Acid);
        }
        this_.SendDeathEvent();
    }
    private static void PlayerDataTakeHealth(On.PlayerData.orig_TakeHealth orig, PlayerData self, int amount)
    {
        if (Config.heroInfiniteHealth)
        {
            amount = 0;
        }
        orig(self, amount);
    }
    private static bool AlertRangeIsHeroInRange(orig_IsHeroInRange orig, AlertRange self)
    {
        var instanceInfo = self.gameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
        if (instanceInfo != null)
        {
            if (instanceInfo.targetDetector.target == null)
            {
                return false;
            }
            foreach (var myCollider in self.Reflect().colliders)
            {
                var overlappedColliders = new List<Collider2D>();
                var contactFilter2D = new ContactFilter2D
                {
                    layerMask = 1 << instanceInfo.targetDetector.target.layer,
                };
                myCollider.OverlapCollider(contactFilter2D, overlappedColliders);
                foreach (var overlappedCollider in overlappedColliders)
                {
                    var targetCandidate = overlappedCollider.gameObject;
                    while (targetCandidate.transform.parent != null)
                    {
                        targetCandidate = targetCandidate.transform.parent.gameObject;
                    }
                    if (targetCandidate == instanceInfo.targetDetector.target)
                    {
                        return true;
                    }
                }
            }
            return false;
        };
        return orig(self);
    }
    private static void WalkerUpdateWaitingForConditions(On.Walker.orig_UpdateWaitingForConditions orig, Walker self)
    {
        var base_ = self;
        var this_ = self.Reflect();
        var thisHero = HeroController.instance.gameObject;
        var instanceInfo = self.gameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
        if (instanceInfo != null)
        {
            thisHero = instanceInfo.targetFollower.gameObject;
        }
        if (!this_.didFulfilCameraDistanceCondition && (this_.mainCamera.transform.position - base_.transform.position).sqrMagnitude < 3600f)
        {
            this_.didFulfilCameraDistanceCondition = true;
        }
        if (this_.didFulfilCameraDistanceCondition && !this_.didFulfilHeroXCondition && thisHero != null && Mathf.Abs(thisHero.transform.GetPositionX() - this_.waitHeroX) < 1f)
        {
            this_.didFulfilHeroXCondition = true;
        }
        if (this_.didFulfilCameraDistanceCondition && (!this_.waitForHeroX || this_.didFulfilHeroXCondition) && !this_.startInactive && !this_.ambush)
        {
            this_.BeginStopped(Walker.StopReasons.Bored);
            this_.StartMoving();
        }
    }
    private static void WalkerUpdateWalking(On.Walker.orig_UpdateWalking orig, Walker self)
    {
        var base_ = self;
        var this_ = self.Reflect();
        var thisHero = HeroController.instance.gameObject;
        var instanceInfo = self.gameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
        if (instanceInfo != null)
        {
            thisHero = instanceInfo.targetFollower.gameObject;
        }
        if (this_.turnCooldownRemaining <= 0f)
        {
            Sweep sweep = new Sweep(this_.bodyCollider, 1 - this_.currentFacing, 3, 0.1f);
            if (sweep.Check(base_.transform.position, this_.bodyCollider.bounds.extents.x + 0.5f, 256))
            {
                this_.BeginTurning(-this_.currentFacing);
                return;
            }
            if (!this_.preventTurningToFaceHero && (thisHero != null && thisHero.transform.GetPositionX() > base_.transform.GetPositionX() != this_.currentFacing > 0) && this_.lineOfSightDetector != null && this_.lineOfSightDetector.CanSeeHero && this_.alertRange != null && this_.alertRange.IsHeroInRange)
            {
                this_.BeginTurning(-this_.currentFacing);
                return;
            }
            if (!this_.ignoreHoles)
            {
                Sweep sweep2 = new Sweep(this_.bodyCollider, 3, 3, 0.1f);
                if (!sweep2.Check((Vector2)base_.transform.position + new Vector2((this_.bodyCollider.bounds.extents.x + 0.5f + this_.edgeXAdjuster) * (float)this_.currentFacing, 0f), 0.25f, 256))
                {
                    this_.BeginTurning(-this_.currentFacing);
                    return;
                }
            }
        }
        if (this_.pauses)
        {
            this_.walkTimeRemaining -= Time.deltaTime;
            if (this_.walkTimeRemaining <= 0f)
            {
                this_.BeginStopped(Walker.StopReasons.Bored);
                return;
            }
        }
        this_.body.velocity = new Vector2((this_.currentFacing > 0) ? this_.walkSpeedR : this_.walkSpeedL, this_.body.velocity.y);
    }
    private static void EnemyDeathEffectsEmitCorpse(On.EnemyDeathEffects.orig_EmitCorpse orig, EnemyDeathEffects self, float? attackDirection, bool isWatery, bool spellBurn)
    {
        var instanceInfo = self.GetComponent<Behaviors.InstanceInfo>();
        if (instanceInfo != null && self.Reflect().corpse != null)
        {
            self.Reflect().corpse.AddComponent<Behaviors.ClearCorpse>();
        }
        orig(self, attackDirection, isWatery, spellBurn);
    }
    private static void FsmEnterState(On.HutongGames.PlayMaker.Fsm.orig_EnterState orig, Fsm self, FsmState state)
    {
        var this_ = self;
        if (state.loopCount >= this_.MaxLoopCount)
        {
            Log.LogError($"Loop count exceeded in state {state.Name} for game object {self.Owner.gameObject.name}");
        }
        orig(self, state);
    }
    private static void GeoCounterAddGeo(On.GeoCounter.orig_AddGeo orig, GeoCounter self, int geo)
    {
        if (self.Reflect().geoSpriteFsm == null)
        {
            return;
        }
        orig(self, geo);
    }
    private static void CheckTargetDirectionDoCheckDirection(On.HutongGames.PlayMaker.Actions.CheckTargetDirection.orig_DoCheckDirection orig, CheckTargetDirection self)
    {
        var base_ = self;
        var this_ = self.Reflect();
        float num = this_.self.Value.transform.position.x;
        float num2 = this_.self.Value.transform.position.y;
        float num3;
        float num4;
        if (this_.target.Value == null)
        {
            num3 = 0;
            num4 = 0;
        }
        else
        {
            num3 = this_.target.Value.transform.position.x;
            num4 = this_.target.Value.transform.position.y;
        }
        if (num < num3)
        {
            base_.Fsm.Event(this_.rightEvent);
            this_.rightBool.Value = true;
        }
        else
        {
            this_.rightBool.Value = false;
        }
        if (num > num3)
        {
            base_.Fsm.Event(this_.leftEvent);
            this_.leftBool.Value = true;
        }
        else
        {
            this_.leftBool.Value = false;
        }
        if (num2 < num4)
        {
            base_.Fsm.Event(this_.aboveEvent);
            this_.aboveBool.Value = true;
        }
        else
        {
            this_.aboveBool.Value = false;
        }
        if (num2 > num4)
        {
            base_.Fsm.Event(this_.belowEvent);
            this_.belowBool.Value = true;
            return;
        }
        this_.belowBool.Value = false;
    }
    private static void PaintBulletUpdate(On.PaintBullet.orig_Update orig, PaintBullet self)
    {
        var instanceInfo = self.gameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
        if (instanceInfo != null && instanceInfo.creator != null && self.name.StartsWith("Paint Shot P Down"))
        {
            self.Reflect().col.isTrigger = self.transform.position.y > instanceInfo.creator.transform.position.y;
        }
        orig(self);
    }
    private static System.Collections.IEnumerator PaintBulletCollision(On.PaintBullet.orig_Collision orig, PaintBullet self, Vector2 normal, bool doRotation)
    {
        var this_ = self.Reflect();
        self.transform.localScale = new Vector3(this_.scale, this_.scale, self.transform.localScale.z);
        this_.body.isKinematic = true;
        this_.body.velocity = Vector2.zero;
        this_.body.angularVelocity = 0f;
        this_.sprite.enabled = false;
        this_.impactParticle.Play();
        this_.trailParticle.Stop();
        this_.splatEffect.SetActive(true);
        if (!doRotation || (normal.y >= 0.75f && Mathf.Abs(normal.x) < 0.5f))
        {
            self.transform.SetRotation2D(0f);
        }
        else if (normal.y <= 0.75f && Mathf.Abs(normal.x) < 0.5f)
        {
            self.transform.SetRotation2D(180f);
        }
        else if (normal.x >= 0.75f && Mathf.Abs(normal.y) < 0.5f)
        {
            self.transform.SetRotation2D(270f);
        }
        else if (normal.x <= 0.75f && Mathf.Abs(normal.y) < 0.5f)
        {
            self.transform.SetRotation2D(90f);
        }
        AudioClip clip = this_.splatClips[UnityEngine.Random.Range(0, this_.splatClips.Count - 1)];
        UnityEngine.Random.Range(0.9f, 1.1f);
        this_.audioSourcePrefab.PlayOneShot(clip);
        this_.chance = 100;
        this_.painting = true;
        foreach (SpriteRenderer spriteRenderer in this_.splatList)
        {
            if (spriteRenderer == null)
            {
                continue;
            }
            if (this_.painting)
            {
                if (UnityEngine.Random.Range(1, 100) <= this_.chance)
                {
                    spriteRenderer.color = this_.sprite.color;
                    this_.chance /= 2;
                }
                else
                {
                    this_.painting = false;
                }
            }
        }
        yield return null;
        this_.col.enabled = false;
        yield return new WaitForSeconds(1f);
        self.gameObject.Recycle();
        yield break;
    }
    private static void EnemyBulletUpdate(On.EnemyBullet.orig_Update orig, EnemyBullet self)
    {
        var instanceInfo = self.gameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
        if (instanceInfo != null && instanceInfo.creator != null && self.name.StartsWith("Shot Mawlek NoDrip"))
        {
            self.Reflect().col.isTrigger = self.transform.position.y > instanceInfo.creator.transform.position.y;
        }
        orig(self);
    }
    private static void EnemySpawnerUpdate(On.EnemySpawner.orig_Update orig, EnemySpawner self)
    {
        var instanceInfo = self.gameObject.GetComponentInParent<Behaviors.InstanceInfo>(true);
        var this_ = self.Reflect();
        if (!this_.isComplete)
        {
            this_.elapsed += Time.deltaTime;
            this_.sprite.color = Color.Lerp(this_.startColor, this_.endColor, Mathf.Clamp(this_.elapsed / this_.easeTime, 0f, 1f));
            if (this_.elapsed > this_.easeTime)
            {
                this_.isComplete = true;
                this_.spawnedEnemy.transform.position = self.transform.position;
                this_.spawnedEnemy.transform.localScale = self.transform.localScale;
                this_.spawnedEnemy.SetActive(true);
                if (instanceInfo != null)
                {
                    var properties = new Templates.PlaceConfig
                    {
                        groupID = instanceInfo.groupID,
                        hp = Common.DefaultEnemyHealth,
                        damage = instanceInfo.damage,
                        hpBar = false
                    };
                    RewriteInstance.Rewrite(this_.spawnedEnemy, properties, self.gameObject);
                    if (this_.spawnedEnemy.name.StartsWith("Buzzer"))
                    {
                        var thisInstanceInfo = this_.spawnedEnemy.GetComponent<Behaviors.InstanceInfo>();
                        this_.spawnedEnemy.AddComponent<Templates.Standard.VengeflyKing.SmartAttachment>().parent = thisInstanceInfo.rootCreator;
                        if (thisInstanceInfo.rootCreator != null)
                        {
                            var summonManager = thisInstanceInfo.rootCreator.GetComponent<Templates.Standard.VengeflyKing.SummonManager>();
                            if (summonManager != null)
                            {
                                summonManager.summons.Add(this_.spawnedEnemy);
                            }
                        }
                    }
                }
                if (this_.OnEnemySpawned != null)
                {
                    this_.OnEnemySpawned(this_.spawnedEnemy);
                }
                PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(this_.spawnedEnemy, "chaser");
                if (playMakerFSM)
                {
                    playMakerFSM.FsmVariables.FindFsmBool("Start Alert").Value = true;
                }
                if (instanceInfo != null)
                {
                    GameObject.Destroy(self.gameObject);
                }
                else
                {
                    self.gameObject.SetActive(false);
                }
            }
        }
    }
}
