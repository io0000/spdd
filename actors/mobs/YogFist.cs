using System;
using watabou.utils;
using spdd.messages;
using spdd.actors.buffs;
using spdd.actors.blobs;
using spdd.mechanics;
using spdd.levels;
using spdd.utils;
using spdd.sprites;
using spdd.items.scrolls;
using spdd.items.armor.glyphs;
using spdd.scenes;
using spdd.effects;
using spdd.effects.particles;


namespace spdd.actors.mobs
{
    public abstract class YogFist : Mob
    {
        public YogFist()
        {
            HP = HT = 300;
            defenseSkill = 20;

            viewDistance = Light.DISTANCE;

            //for doomed resistance
            EXP = 25;
            maxLvl = -2;

            state = HUNTING;

            properties.Add(Property.BOSS);
            properties.Add(Property.DEMONIC);
        }

        private float rangedCooldown;
        protected bool canRangedInMelee = true;

        protected virtual void IncrementRangedCooldown()
        {
            rangedCooldown += Rnd.NormalFloat(8, 12);
        }

        public override bool Act()
        {
            if (paralysed <= 0 && rangedCooldown > 0)
                --rangedCooldown;
            return base.Act();
        }

        protected override bool CanAttack(Character _enemy)
        {
            if (rangedCooldown <= 0)
            {
                return new Ballistic(pos, _enemy.pos, Ballistic.MAGIC_BOLT).collisionPos == _enemy.pos;
            }
            else
            {
                return base.CanAttack(_enemy);
            }
        }

        private bool invulnWarned;

        protected bool IsNearYog()
        {
            int yogPos = Dungeon.level.exit + 3 * Dungeon.level.Width();
            return Dungeon.level.Distance(pos, yogPos) <= 4;
        }

        public override bool IsInvulnerable(Type effect)
        {
            if (IsNearYog() && !invulnWarned)
            {
                invulnWarned = true;
                GLog.Warning(Messages.Get(this, "invuln_warn"));
            }
            return IsNearYog();
        }

        protected override bool DoAttack(Character enemyChar)
        {
            if (Dungeon.level.Adjacent(pos, enemyChar.pos) && (!canRangedInMelee || rangedCooldown > 0))
            {
                return base.DoAttack(enemyChar);
            }
            else
            {
                IncrementRangedCooldown();
                if (sprite != null && (sprite.visible || enemyChar.sprite.visible))
                {
                    sprite.Zap(enemyChar.pos);
                    return false;
                }
                else
                {
                    Zap();
                    return true;
                }
            }
        }

        protected abstract void Zap();

        public void OnZapComplete()
        {
            Zap();
            Next();
        }

        public override int AttackSkill(Character target)
        {
            return 36;
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(18, 36);
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 15);
        }

        public override string Description()
        {
            return Messages.Get(typeof(YogFist), "desc") + "\n\n" + Messages.Get(this, "desc");
        }

        public const string RANGED_COOLDOWN = "ranged_cooldown";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(RANGED_COOLDOWN, rangedCooldown);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            rangedCooldown = bundle.GetFloat(RANGED_COOLDOWN);
        }

        [SPDStatic]
        public class BurningFist : YogFist
        {
            public BurningFist()
            {
                spriteClass = typeof(FistSprite.Burning);

                properties.Add(Property.FIERY);
            }

            public override bool Act()
            {
                bool result = base.Act();

                if (Dungeon.level.map[pos] == Terrain.WATER)
                {
                    Level.Set(pos, Terrain.EMPTY);
                    GameScene.UpdateMap(pos);
                    CellEmitter.Get(pos).Burst(Speck.Factory(Speck.STEAM), 10);
                }

                //1.33 evaporated tiles on average
                int evaporatedTiles = Rnd.Chances(new float[] { 0, 2, 1 });

                for (int i = 0; i < evaporatedTiles; ++i)
                {
                    int cell = pos + PathFinder.NEIGHBORS8[Rnd.Int(8)];
                    if (Dungeon.level.map[cell] == Terrain.WATER)
                    {
                        Level.Set(cell, Terrain.EMPTY);
                        GameScene.UpdateMap(cell);
                        CellEmitter.Get(cell).Burst(Speck.Factory(Speck.STEAM), 10);
                    }
                }

                foreach (int i in PathFinder.NEIGHBORS9)
                {
                    int vol = Fire.VolumeAt(pos + i, typeof(Fire));
                    if (vol < 4 && !Dungeon.level.water[pos + i] && !Dungeon.level.solid[pos + i])
                    {
                        GameScene.Add(Blob.Seed(pos + i, 4 - vol, typeof(Fire)));
                    }
                }

                return result;
            }

            protected override void Zap()
            {
                Spend(1f);

                if (Dungeon.level.map[enemy.pos] == Terrain.WATER)
                {
                    Level.Set(enemy.pos, Terrain.EMPTY);
                    GameScene.UpdateMap(enemy.pos);
                    CellEmitter.Get(enemy.pos).Burst(Speck.Factory(Speck.STEAM), 10);
                }
                else
                {
                    Buff.Affect<Burning>(enemy).Reignite(enemy);
                }

                foreach (int i in PathFinder.NEIGHBORS9)
                {
                    if (!Dungeon.level.water[enemy.pos + i] && !Dungeon.level.solid[enemy.pos + i])
                    {
                        int vol = Fire.VolumeAt(enemy.pos + i, typeof(Fire));
                        if (vol < 4)
                        {
                            GameScene.Add(Blob.Seed(enemy.pos + i, 4 - vol, typeof(Fire)));
                        }
                    }
                }
            }
        }

        [SPDStatic]
        public class SoiledFist : YogFist
        {
            public SoiledFist()
            {
                InitInstance();
                spriteClass = typeof(FistSprite.Soiled);
            }

            public override bool Act()
            {
                bool result = base.Act();

                //1.33 grass tiles on average
                int furrowedTiles = Rnd.Chances(new float[] { 0, 2, 1 });

                for (int i = 0; i < furrowedTiles; ++i)
                {
                    int cell = pos + PathFinder.NEIGHBORS9[Rnd.Int(9)];
                    if (Dungeon.level.map[cell] == Terrain.GRASS)
                    {
                        Level.Set(cell, Terrain.FURROWED_GRASS);
                        GameScene.UpdateMap(cell);
                        CellEmitter.Get(cell).Burst(LeafParticle.General, 10);
                    }
                }

                Dungeon.Observe();

                foreach (int i in PathFinder.NEIGHBORS9)
                {
                    int cell = pos + i;
                    if (CanSpreadGrass(cell))
                    {
                        Level.Set(pos + i, Terrain.GRASS);
                        GameScene.UpdateMap(pos + i);
                    }
                }

                return result;
            }

            public override void Damage(int dmg, object src)
            {
                int grassCells = 0;
                foreach (int i in PathFinder.NEIGHBORS9)
                {
                    if (Dungeon.level.map[pos + i] == Terrain.FURROWED_GRASS ||
                        Dungeon.level.map[pos + i] == Terrain.HIGH_GRASS)
                    {
                        ++grassCells;
                    }
                }
                if (grassCells > 0)
                    dmg = (int)Math.Round(dmg * (6 - grassCells) / 6f, MidpointRounding.AwayFromZero);

                base.Damage(dmg, src);
            }

            protected override void Zap()
            {
                Spend(1f);

                if (Hit(this, enemy, true))
                {
                    Buff.Affect<Roots>(enemy, 3f);
                }
                else
                {
                    enemy.sprite.ShowStatus(CharSprite.NEUTRAL, enemy.DefenseVerb());
                }

                foreach (int i in PathFinder.NEIGHBORS9)
                {
                    int cell = enemy.pos + i;
                    if (CanSpreadGrass(cell))
                    {
                        if (Rnd.Int(5) == 0)
                        {
                            Level.Set(cell, Terrain.FURROWED_GRASS);
                            GameScene.UpdateMap(cell);
                        }
                        else
                        {
                            Level.Set(cell, Terrain.GRASS);
                            GameScene.UpdateMap(cell);
                        }
                        CellEmitter.Get(cell).Burst(LeafParticle.General, 10);
                    }
                }
                Dungeon.Observe();
            }

            private bool CanSpreadGrass(int cell)
            {
                int yogPos = Dungeon.level.exit + Dungeon.level.Width() * 3;
                return Dungeon.level.Distance(cell, yogPos) > 4 && !Dungeon.level.solid[cell]
                        && !(Dungeon.level.map[cell] == Terrain.FURROWED_GRASS || Dungeon.level.map[cell] == Terrain.HIGH_GRASS);
            }

            private void InitInstance()
            {
                resistances.Add(typeof(Burning));
            }

        } // SoiledFist

        [SPDStatic]
        public class RottingFist : YogFist
        {
            public RottingFist()
            {
                InitInstance();
                spriteClass = typeof(FistSprite.Rotting);
                properties.Add(Property.ACIDIC);
            }

            public override bool Act()
            {
                //ensures toxic gas acts at the appropriate time when added
                GameScene.Add(Blob.Seed(pos, 0, typeof(ToxicGas)));

                if (Dungeon.level.water[pos] && HP < HT)
                {
                    sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 3);
                    HP += HT / 50;
                }

                return base.Act();
            }

            public override void Damage(int dmg, object src)
            {
                if (!IsInvulnerable(src.GetType()) && !(src is Bleeding))
                {
                    Bleeding b = FindBuff<Bleeding>();
                    if (b == null)
                    {
                        b = new Bleeding();
                    }
                    b.announced = false;
                    b.Set((int)(dmg * .67f));
                    b.AttachTo(this);
                    sprite.ShowStatus(CharSprite.WARNING, b.ToString() + " " + (int)b.level);
                }
                else
                {
                    base.Damage(dmg, src);
                }
            }

            protected override void Zap()
            {
                Spend(1f);
                GameScene.Add(Blob.Seed(enemy.pos, 100, typeof(ToxicGas)));
            }

            public override int AttackProc(Character enemy, int damage)
            {
                damage = base.AttackProc(enemy, damage);

                if (Rnd.Int(2) == 0)
                {
                    Buff.Affect<Ooze>(enemy).Set(Ooze.DURATION);
                    enemy.sprite.Burst(new Color(0x00, 0x00, 0x00, 0xFF), 5);
                }

                return damage;
            }

            private void InitInstance()
            {
                immunities.Add(typeof(ToxicGas));
            }
        } // RottingFist

        [SPDStatic]
        public class RustedFist : YogFist
        {
            public RustedFist()
            {
                spriteClass = typeof(FistSprite.Rusted);

                properties.Add(Property.LARGE);
                properties.Add(Property.INORGANIC);
            }

            public override int DamageRoll()
            {
                return Rnd.NormalIntRange(22, 44);
            }

            public override void Damage(int dmg, object src)
            {
                if (!IsInvulnerable(src.GetType()) && !(src is Viscosity.DeferedDamage))
                {
                    Buff.Affect<Viscosity.DeferedDamage>(this).Prolong(dmg);
                    sprite.ShowStatus(CharSprite.WARNING, Messages.Get(typeof(Viscosity), "deferred", dmg));
                }
                else
                {
                    base.Damage(dmg, src);
                }
            }

            protected override void Zap()
            {
                Spend(1f);
                Buff.Affect<Cripple>(enemy, 4f);
            }
        } // RustedFist

        [SPDStatic]
        public class BrightFist : YogFist
        {
            public BrightFist()
            {
                spriteClass = typeof(FistSprite.Bright);

                properties.Add(Property.ELECTRIC);

                canRangedInMelee = false;
            }

            protected override void IncrementRangedCooldown()
            {
                //ranged attack has no cooldown
            }

            //used so resistances can differentiate between melee and magical attacks
            public class LightBeam
            { }

            protected override void Zap()
            {
                Spend(1f);

                if (Hit(this, enemy, true))
                {
                    enemy.Damage(Rnd.NormalIntRange(10, 20), new LightBeam());
                    Buff.Prolong<Blindness>(enemy, Blindness.DURATION / 2f);

                    if (!enemy.IsAlive() && enemy == Dungeon.hero)
                    {
                        Dungeon.Fail(GetType());
                        GLog.Negative(Messages.Get(typeof(Character), "kill", Name()));
                    }
                }
                else
                {
                    enemy.sprite.ShowStatus(CharSprite.NEUTRAL, enemy.DefenseVerb());
                }
            }

            public override void Damage(int dmg, object src)
            {
                int beforeHP = HP;
                base.Damage(dmg, src);

                if (IsAlive() && beforeHP > HT / 2 && HP < HT / 2)
                {
                    HP = HT / 2;
                    Buff.Prolong<Blindness>(Dungeon.hero, Blindness.DURATION * 1.5f);
                    int i;
                    do
                    {
                        i = Rnd.Int(Dungeon.level.Length());
                    } 
                    while (Dungeon.level.heroFOV[i]
                            || Dungeon.level.solid[i]
                            || Actor.FindChar(i) != null
                            || PathFinder.GetStep(i, Dungeon.level.exit, Dungeon.level.passable) == -1);
                    ScrollOfTeleportation.Appear(this, i);
                    state = WANDERING;
                    GameScene.Flash(new Color(0xFF, 0xFF, 0xFF, 0xFF));
                    GLog.Warning(Messages.Get(this, "teleport"));
                }
                else if (!IsAlive())
                {
                    Buff.Prolong<Blindness>(Dungeon.hero, Blindness.DURATION * 3f);
                    GameScene.Flash(new Color(0xFF, 0xFF, 0xFF, 0xFF));
                }
            }
        }  // BrightFist

        [SPDStatic]
        public class DarkFist : YogFist
        {
            public DarkFist()
            {
                spriteClass = typeof(FistSprite.Dark);

                canRangedInMelee = false;
            }

            protected override void IncrementRangedCooldown()
            {
                //ranged attack has no cooldown
            }

            //used so resistances can differentiate between melee and magical attacks
            public class DarkBolt 
            { }

            protected override void Zap()
            {
                Spend(1f);

                if (Hit(this, enemy, true))
                {
                    enemy.Damage(Rnd.NormalIntRange(10, 20), new DarkBolt());

                    Light l = enemy.FindBuff<Light>();
                    if (l != null)
                    {
                        l.Weaken(50);
                    }

                    if (!enemy.IsAlive() && enemy == Dungeon.hero)
                    {
                        Dungeon.Fail(GetType());
                        GLog.Negative(Messages.Get(typeof(Character), "kill", Name()));
                    }
                }
                else
                {
                    enemy.sprite.ShowStatus(CharSprite.NEUTRAL, enemy.DefenseVerb());
                }
            }

            public override void Damage(int dmg, object src)
            {
                int beforeHP = HP;
                base.Damage(dmg, src);
                if (IsAlive() && beforeHP > HT / 2 && HP < HT / 2)
                {
                    HP = HT / 2;
                    Light l = Dungeon.hero.FindBuff<Light>();
                    if (l != null)
                    {
                        l.Detach();
                    }
                    int i;
                    do
                    {
                        i = Rnd.Int(Dungeon.level.Length());
                    } 
                    while (Dungeon.level.heroFOV[i] || 
                           Dungeon.level.solid[i] || 
                           Actor.FindChar(i) != null || 
                           PathFinder.GetStep(i, Dungeon.level.exit, Dungeon.level.passable) == -1);

                    ScrollOfTeleportation.Appear(this, i);
                    state = WANDERING;
                    GameScene.Flash(new Color(0x00, 0x00, 0x00, 0xFF), false);
                    GLog.Warning(Messages.Get(this, "teleport"));
                }
                else if (!IsAlive())
                {
                    Light l = Dungeon.hero.FindBuff<Light>();
                    if (l != null)
                    {
                        l.Detach();
                    }
                    GameScene.Flash(new Color(0x00, 0x00, 0x00, 0xFF), false);
                }
            }
        }  // DarkFist
    }
}