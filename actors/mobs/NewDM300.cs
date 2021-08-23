using System;
using watabou.utils;
using watabou.noosa.audio;
using watabou.noosa;
using spdd.sprites;
using spdd.actors.buffs;
using spdd.actors.blobs;
using spdd.scenes;
using spdd.mechanics;
using spdd.utils;
using spdd.ui;
using spdd.effects;
using spdd.items.wands;
using spdd.items.quest;
using spdd.levels;
using spdd.effects.particles;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class NewDM300 : Mob
    {
        public NewDM300()
        {
            InitInstance();

            //TODO improved sprite
            spriteClass = typeof(DM300Sprite);

            HP = HT = 300;
            EXP = 30;
            defenseSkill = 15;

            properties.Add(Property.BOSS);
            properties.Add(Property.INORGANIC);
            properties.Add(Property.LARGE);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(15, 25);
        }

        public override int AttackSkill(Character target)
        {
            return 20;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 10);
        }

        public int pylonsActivated;
        public bool supercharged;
        public bool chargeAnnounced;

        private int turnsSinceLastAbility = -1;
        private int abilityCooldown = Rnd.NormalIntRange(MIN_COOLDOWN, MAX_COOLDOWN);

        private const int MIN_COOLDOWN = 5;
        private const int MAX_COOLDOWN = 9;

        private int lastAbility;
        private const int NONE = 0;
        private const int GAS = 1;
        private const int ROCKS = 2;

        private const string PYLONS_ACTIVATED = "pylons_activated";
        private const string SUPERCHARGED = "supercharged";
        private const string CHARGE_ANNOUNCED = "charge_announced";
        private const string TURNS_SINCE_LAST_ABILITY = "turns_since_last_ability";
        private const string ABILITY_COOLDOWN = "ability_cooldown";
        private const string LAST_ABILITY = "last_ability";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(PYLONS_ACTIVATED, pylonsActivated);
            bundle.Put(SUPERCHARGED, supercharged);
            bundle.Put(CHARGE_ANNOUNCED, chargeAnnounced);
            bundle.Put(TURNS_SINCE_LAST_ABILITY, turnsSinceLastAbility);
            bundle.Put(ABILITY_COOLDOWN, abilityCooldown);
            bundle.Put(LAST_ABILITY, lastAbility);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            pylonsActivated = bundle.GetInt(PYLONS_ACTIVATED);
            supercharged = bundle.GetBoolean(SUPERCHARGED);
            chargeAnnounced = bundle.GetBoolean(CHARGE_ANNOUNCED);
            turnsSinceLastAbility = bundle.GetInt(TURNS_SINCE_LAST_ABILITY);
            abilityCooldown = bundle.GetInt(ABILITY_COOLDOWN);
            lastAbility = bundle.GetInt(LAST_ABILITY);

            if (turnsSinceLastAbility != -1)
            {
                BossHealthBar.AssignBoss(this);
                if (!supercharged && pylonsActivated == 2)
                    BossHealthBar.Bleed(true);
            }
        }

        public override bool Act()
        {
            GameScene.Add(Blob.Seed(pos, 0, typeof(FallingRocks)));
            GameScene.Add(Blob.Seed(pos, 0, typeof(ToxicGas)));

            //ability logic only triggers if DM is not supercharged
            if (!supercharged)
            {
                if (turnsSinceLastAbility >= 0) 
                    ++turnsSinceLastAbility;

                //in case DM-300 hasn't been able to act yet
                if (fieldOfView == null || fieldOfView.Length != Dungeon.level.Length())
                {
                    fieldOfView = new bool[Dungeon.level.Length()];
                    Dungeon.level.UpdateFieldOfView(this, fieldOfView);
                }

                //determine if DM can reach its enemy
                bool canReach;
                if (enemy == null)
                {
                    if (Dungeon.level.Adjacent(pos, Dungeon.hero.pos))
                    {
                        canReach = true;
                    }
                    else
                    {
                        canReach = (Dungeon.FindStep(this, Dungeon.hero.pos, Dungeon.level.openSpace, fieldOfView, true) != -1);
                    }
                }
                else
                {
                    if (Dungeon.level.Adjacent(pos, enemy.pos))
                    {
                        canReach = true;
                    }
                    else
                    {
                        canReach = (Dungeon.FindStep(this, enemy.pos, Dungeon.level.openSpace, fieldOfView, true) != -1);
                    }
                }

                if (state != HUNTING)
                {
                    if (Dungeon.hero.invisible <= 0 && canReach)
                    {
                        Beckon(Dungeon.hero.pos);
                    }
                }
                else
                {

                    if (enemy == null)
                        enemy = Dungeon.hero;

                    if (!canReach)
                    {

                        if (fieldOfView[enemy.pos] && turnsSinceLastAbility >= MIN_COOLDOWN)
                        {

                            lastAbility = GAS;
                            turnsSinceLastAbility = 0;
                            Spend(TICK);

                            GLog.Warning(Messages.Get(this, "vent"));
                            if (sprite != null && (sprite.visible || enemy.sprite.visible))
                            {
                                sprite.Zap(enemy.pos);
                                return false;
                            }
                            else
                            {
                                VentGas(enemy);
                                Sample.Instance.Play(Assets.Sounds.GAS);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (turnsSinceLastAbility > abilityCooldown)
                        {
                            if (lastAbility == NONE)
                            {
                                //50/50 either ability
                                lastAbility = Rnd.Int(2) == 0 ? GAS : ROCKS;
                            }
                            else if (lastAbility == GAS)
                            {
                                //more likely to use rocks
                                lastAbility = Rnd.Int(4) == 0 ? GAS : ROCKS;
                            }
                            else
                            {
                                //more likely to use gas
                                lastAbility = Rnd.Int(4) != 0 ? GAS : ROCKS;
                            }

                            //doesn't spend a turn if _enemy is at a distance
                            if (Dungeon.level.Adjacent(pos, enemy.pos))
                            {
                                Spend(TICK);
                            }

                            turnsSinceLastAbility = 0;
                            abilityCooldown = Rnd.NormalIntRange(MIN_COOLDOWN, MAX_COOLDOWN);

                            if (lastAbility == GAS)
                            {
                                GLog.Warning(Messages.Get(this, "vent"));
                                if (sprite != null && (sprite.visible || enemy.sprite.visible))
                                {
                                    sprite.Zap(enemy.pos);
                                    return false;
                                }
                                else
                                {
                                    VentGas(enemy);
                                    Sample.Instance.Play(Assets.Sounds.GAS);
                                    return true;
                                }
                            }
                            else
                            {
                                GLog.Warning(Messages.Get(this, "rocks"));
                                if (sprite != null && (sprite.visible || enemy.sprite.visible))
                                {
                                    ((DM300Sprite)sprite).Slam(enemy.pos);
                                    return false;
                                }
                                else
                                {
                                    DropRocks(enemy);
                                    Sample.Instance.Play(Assets.Sounds.ROCKS);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (!chargeAnnounced)
                {
                    Yell(Messages.Get(this, "supercharged"));
                    chargeAnnounced = true;
                }

                if (state == WANDERING && Dungeon.hero.invisible <= 0)
                {
                    Beckon(Dungeon.hero.pos);
                    state = HUNTING;
                    enemy = Dungeon.hero;
                }
            }

            return base.Act();
        }

        protected override Character ChooseEnemy()
        {
            Character enemy = base.ChooseEnemy();
            if (supercharged && enemy == null)
            {
                enemy = Dungeon.hero;
            }
            return enemy;
        }

        public override void Move(int step)
        {
            base.Move(step);

            Camera.main.Shake(supercharged ? 3 : 1, 0.25f);

            if (Dungeon.level.map[step] == Terrain.INACTIVE_TRAP && state == HUNTING)
            {
                //don't gain energy from cells that are energized
                if (NewCavesBossLevel.PylonEnergy.VolumeAt(pos, typeof(NewCavesBossLevel.PylonEnergy)) > 0)
                    return;
            }

            if (Dungeon.level.heroFOV[step])
            {
                if (FindBuff<Barrier>() == null)
                {
                    GLog.Warning(Messages.Get(this, "shield"));
                }
                Sample.Instance.Play(Assets.Sounds.LIGHTNING);
                sprite.Emitter().Start(SparkParticle.Static, 0.05f, 20);
            }

            Buff.Affect<Barrier>(this).SetShield(30 + (HT - HP) / 10);
        }

        public override float Speed()
        {
            return base.Speed() * (supercharged ? 2 : 1);
        }

        public override void Notice()
        {
            base.Notice();
            if (!BossHealthBar.IsAssigned())
            {
                BossHealthBar.AssignBoss(this);
                turnsSinceLastAbility = 0;
                Yell(Messages.Get(this, "notice"));
                foreach (var ch in Actor.Chars())
                {
                    if (ch is items.artifacts.DriedRose.GhostHero)
                        ((items.artifacts.DriedRose.GhostHero)ch).SayBoss();
                }
            }
        }

        public void OnZapComplete()
        {
            VentGas(enemy);
            Next();
        }

        public void VentGas(Character target)
        {
            Dungeon.hero.Interrupt();

            int gasVented = 0;

            Ballistic trajectory = new Ballistic(pos, target.pos, Ballistic.STOP_TARGET);

            foreach (int i in trajectory.SubPath(0, trajectory.dist))
            {
                GameScene.Add(Blob.Seed(i, 20, typeof(ToxicGas)));
                gasVented += 20;
            }

            GameScene.Add(Blob.Seed(trajectory.collisionPos, 100, typeof(ToxicGas)));

            if (gasVented < 250)
            {
                int toVentAround = (int)Math.Ceiling((250 - gasVented) / 8f);
                foreach (int i in PathFinder.NEIGHBORS8)
                {
                    GameScene.Add(Blob.Seed(pos + i, toVentAround, typeof(ToxicGas)));
                }
            }
        }

        public void OnSlamComplete()
        {
            DropRocks(enemy);
            Next();
        }

        public void DropRocks(Character target)
        {
            Dungeon.hero.Interrupt();
            int rockCenter;

            if (Dungeon.level.Adjacent(pos, target.pos))
            {
                int oppositeAdjacent = target.pos + (target.pos - pos);
                Ballistic trajectory = new Ballistic(target.pos, oppositeAdjacent, Ballistic.MAGIC_BOLT);
                WandOfBlastWave.ThrowChar(target, trajectory, 2, false, false);
                if (target == Dungeon.hero)
                {
                    Dungeon.hero.Interrupt();
                }
                rockCenter = trajectory.path[Math.Min(trajectory.dist, 2)];
            }
            else
            {
                rockCenter = target.pos;
            }

            var a = new NewDM300Actor(this, rockCenter);

            Actor.AddDelayed(a, Math.Min(target.Cooldown(), 3 * TICK));
        }

        public class NewDM300Actor : Actor
        {
            NewDM300 dm300;
            int rockCenter;

            public NewDM300Actor(NewDM300 dm300, int rockCenter)
            {
                actPriority = HERO_PRIO + 1;
                this.dm300 = dm300;
                this.rockCenter = rockCenter;
            }

            public override bool Act()
            {
                //pick an adjacent cell to the hero as a safe cell. This cell is less likely to be in a wall or containing hazards
                int safeCell;
                do
                {
                    safeCell = rockCenter + PathFinder.NEIGHBORS8[Rnd.Int(8)];
                } while (safeCell == dm300.pos ||
                        (Dungeon.level.solid[safeCell] && Rnd.Int(2) == 0) ||
                        (Blob.VolumeAt(safeCell, typeof(NewCavesBossLevel.PylonEnergy)) > 0 && Rnd.Int(2) == 0));

                int start = rockCenter - Dungeon.level.Width() * 3 - 3;
                int pos;
                for (int y = 0; y < 7; ++y)
                {
                    pos = start + Dungeon.level.Width() * y;
                    for (int x = 0; x < 7; ++x)
                    {
                        if (!Dungeon.level.InsideMap(pos))
                        {
                            ++pos;
                            continue;
                        }
                        //add rock cell to pos, if it is not solid, and isn't the safecell
                        if (!Dungeon.level.solid[pos] && pos != safeCell && Rnd.Int(Dungeon.level.Distance(rockCenter, pos)) == 0)
                        {
                            //don't want to overly punish players with slow move or attack speed
                            GameScene.Add(Blob.Seed(pos, 1, typeof(FallingRocks)));
                        }
                        ++pos;
                    }
                }
                Actor.Remove(this);
                return true;
            }
        }

        private bool invulnWarned;

        public override void Damage(int dmg, object src)
        {
            base.Damage(dmg, src);
            if (IsInvulnerable(src.GetType()))
            {
                return;
            }

            LockedFloor lockedFloor = Dungeon.hero.FindBuff<LockedFloor>();
            if (lockedFloor != null && !IsImmune(src.GetType()))
                lockedFloor.AddTime(dmg);

            int threshold = HT / 3 * (2 - pylonsActivated);

            if (HP < threshold)
            {
                HP = threshold;
                Supercharge();
            }
        }

        public override bool IsInvulnerable(Type effect)
        {
            if (supercharged && !invulnWarned)
            {
                invulnWarned = true;
                GLog.Warning(Messages.Get(this, "charging_hint"));
            }
            return supercharged;
        }

        public void Supercharge()
        {
            supercharged = true;
            ((NewCavesBossLevel)Dungeon.level).ActivatePylon();
            ++pylonsActivated;

            Spend(3f);
            Yell(Messages.Get(this, "charging"));
            sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(this, "invulnerable"));
            ((DM300Sprite)sprite).Charge();
            chargeAnnounced = false;
        }

        public bool IsSupercharged()
        {
            return supercharged;
        }

        public void LoseSupercharge()
        {
            supercharged = false;
            sprite.ResetColor();

            if (pylonsActivated < 2)
            {
                Yell(Messages.Get(this, "charge_lost"));
            }
            else
            {
                Yell(Messages.Get(this, "pylons_destroyed"));
                BossHealthBar.Bleed(true);
            }
        }

        public override bool IsAlive()
        { 
            return HP > 0 || pylonsActivated < 2; 
        }

        public override void Die(object cause)
        {
            base.Die(cause);

            GameScene.BossSlain();
            Dungeon.level.Unseal();

            //60% chance of 2 shards, 30% chance of 3, 10% chance for 4. Average of 2.5
            int shards = Rnd.Chances(new float[] { 0, 0, 6, 3, 1 });
            for (int i = 0; i < shards; ++i)
            {
                int ofs;
                do
                {
                    ofs = PathFinder.NEIGHBORS8[Rnd.Int(8)];
                } while (!Dungeon.level.passable[pos + ofs]);
                Dungeon.level.Drop(new MetalShard(), pos + ofs).sprite.Drop(pos);
            }

            BadgesExtensions.ValidateBossSlain();

            var beacon = Dungeon.hero.belongings.GetItem<items.artifacts.LloydsBeacon>();
            if (beacon != null)
                beacon.Upgrade();

            Yell(Messages.Get(this, "defeated"));
        }

        public override bool GetCloser(int target)
        {
            if (base.GetCloser(target))
            {
                return true;
            }
            else
            {
                if (rooted || target == pos)
                {
                    return false;
                }

                int bestpos = pos;
                foreach (int i in PathFinder.NEIGHBORS8)
                {
                    if (Dungeon.level.openSpace[pos + i] &&
                        Actor.FindChar(pos + i) == null &&
                        Dungeon.level.Distance(bestpos, target) > Dungeon.level.Distance(pos + i, target))
                    {
                        bestpos = pos + i;
                    }
                }
                if (bestpos != pos)
                {
                    Move(bestpos);
                    return true;
                }

                if (!supercharged || state != HUNTING || Dungeon.level.Adjacent(pos, target))
                {
                    return false;
                }

                foreach (int i in PathFinder.NEIGHBORS8)
                {
                    if (Actor.FindChar(pos + i) == null &&
                            Dungeon.level.TrueDistance(bestpos, target) > Dungeon.level.TrueDistance(pos + i, target))
                    {
                        bestpos = pos + i;
                    }
                }
                if (bestpos != pos)
                {
                    Sample.Instance.Play(Assets.Sounds.ROCKS);

                    Rect gate = NewCavesBossLevel.gate;
                    foreach (int i in PathFinder.NEIGHBORS9)
                    {
                        if (Dungeon.level.map[pos + i] == Terrain.WALL || Dungeon.level.map[pos + i] == Terrain.WALL_DECO)
                        {
                            Point p = Dungeon.level.CellToPoint(pos + i);
                            if (p.y < gate.bottom && p.x > gate.left - 2 && p.x < gate.right + 2)
                            {
                                continue; //don't break the gate or walls around the gate
                            }
                            Level.Set(pos + i, Terrain.EMPTY_DECO);
                            GameScene.UpdateMap(pos + i);
                        }
                    }
                    Dungeon.level.CleanWalls();
                    Dungeon.Observe();
                    Spend(3f);

                    bestpos = pos;
                    foreach (int i in PathFinder.NEIGHBORS8)
                    {
                        if (Actor.FindChar(pos + i) == null && Dungeon.level.openSpace[pos + i] &&
                                Dungeon.level.TrueDistance(bestpos, target) > Dungeon.level.TrueDistance(pos + i, target))
                        {
                            bestpos = pos + i;
                        }
                    }

                    if (bestpos != pos)
                    {
                        Move(bestpos);
                    }
                    Camera.main.Shake(5, 1f);

                    return true;
                }

                return false;
            }
        }

        public override string Description()
        {
            string desc = base.Description();
            if (supercharged)
            {
                desc += "\n\n" + Messages.Get(this, "desc_supercharged");
            }
            return desc;
        }

        private void InitInstance()
        {
            immunities.Add(typeof(Sleep));

            resistances.Add(typeof(Terror));
            resistances.Add(typeof(Charm));
            resistances.Add(typeof(Vertigo));
            resistances.Add(typeof(Cripple));
            resistances.Add(typeof(Chill));
            resistances.Add(typeof(Frost));
            resistances.Add(typeof(Roots));
            resistances.Add(typeof(Slow));
        }

        [SPDStatic]
        public class FallingRocks : Blob
        {
            public FallingRocks()
            {
                alwaysVisible = true;
            }

            protected override void Evolve()
            {
                bool rocksFell = false;

                int cell;
                for (int i = area.left; i < area.right; ++i)
                {
                    for (int j = area.top; j < area.bottom; ++j)
                    {
                        cell = i + j * Dungeon.level.Width();
                        off[cell] = cur[cell] > 0 ? cur[cell] - 1 : 0;

                        if (off[cell] > 0)
                        {
                            volume += off[cell];
                        }

                        if (cur[cell] > 0 && off[cell] == 0)
                        {
                            CellEmitter.Get(cell).Start(Speck.Factory(Speck.ROCK), 0.07f, 10);

                            var ch = Actor.FindChar(cell);
                            if (ch != null && !(ch is NewDM300))
                            {
                                Buff.Prolong<Paralysis>(ch, 3);
                            }

                            rocksFell = true;
                        }
                    }
                }

                if (rocksFell)
                {
                    Camera.main.Shake(3, 0.7f);
                    Sample.Instance.Play(Assets.Sounds.ROCKS);
                }
            }

            public override void Use(BlobEmitter emitter)
            {
                base.Use(emitter);

                emitter.bound = new RectF(0, -0.2f, 1, 0.4f);
                emitter.Pour(effects.particles.EarthParticle.Falling, 0.1f);
            }

            public override string TileDesc()
            {
                return Messages.Get(this, "desc");
            }
        } // FallingRocks
    } // NewDM300
}
