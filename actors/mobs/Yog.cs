using System;
using System.Linq;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.effects;
using spdd.items.scrolls;
using spdd.items.scrolls.exotic;
using spdd.items.artifacts;
using spdd.items.weapon.enchantments;
using spdd.levels.traps;
using spdd.mechanics;
using spdd.scenes;
using spdd.sprites;
using spdd.items.keys;
using spdd.utils;
using spdd.ui;
using spdd.effects.particles;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class Yog : Mob
    {
        public Yog()
        {
            InitInstance();

            spriteClass = typeof(YogSprite);

            HP = HT = 300;

            EXP = 50;

            state = PASSIVE;

            properties.Add(Property.BOSS);
            properties.Add(Property.IMMOVABLE);
            properties.Add(Property.DEMONIC);
        }

        public void SpawnFists()
        {
            var fist1 = new RottingFist();
            var fist2 = new BurningFist();

            do
            {
                fist1.pos = pos + PathFinder.NEIGHBORS8[Rnd.Int(8)];
                fist2.pos = pos + PathFinder.NEIGHBORS8[Rnd.Int(8)];
            }
            while (!Dungeon.level.passable[fist1.pos] ||
                   !Dungeon.level.passable[fist2.pos] ||
                   fist1.pos == fist2.pos);

            GameScene.Add(fist1);
            GameScene.Add(fist2);

            Notice();
        }

        public override bool Act()
        {
            //heals 1 health per turn
            HP = Math.Min(HT, HP + 1);

            return base.Act();
        }

        public override void Damage(int dmg, object src)
        {
            HashSet<Mob> fists = new HashSet<Mob>();

            foreach (Mob mob in Dungeon.level.mobs)
            {
                if (mob is RottingFist || mob is BurningFist)
                    fists.Add(mob);
            }

            dmg >>= fists.Count;

            base.Damage(dmg, src);

            var lockedFloor = Dungeon.hero.FindBuff<LockedFloor>();
            if (lockedFloor != null)
                lockedFloor.AddTime(dmg * 0.5f);
        }

        public override int DefenseProc(Character enemy, int damage)
        {
            var spawnPoints = new List<int>();

            for (var i = 0; i < PathFinder.NEIGHBORS8.Length; ++i)
            {
                var p = pos + PathFinder.NEIGHBORS8[i];
                if (FindChar(p) == null &&
                    (Dungeon.level.passable[p] || Dungeon.level.avoid[p]))
                {
                    spawnPoints.Add(p);
                }
            }

            if (spawnPoints.Count > 0)
            {
                var larva = new Larva();
                larva.pos = Rnd.Element(spawnPoints);

                GameScene.Add(larva);
                AddDelayed(new Pushing(larva, pos, larva.pos), -1);
            }

            foreach (Mob mob in Dungeon.level.mobs)
            {
                if (mob is BurningFist || mob is RottingFist || mob is Larva)
                    mob.Aggro(enemy);
            }

            return base.DefenseProc(enemy, damage);
        }

        public override void Beckon(int cell)
        { }

        public override void Die(object cause)
        {
            //for (Mob mob : (Iterable<Mob>)Dungeon.level.mobs.clone())
            foreach (var mob in Dungeon.level.mobs.ToArray())
            {
                if (mob is BurningFist || mob is RottingFist)
                    mob.Die(cause);
            }

            GameScene.BossSlain();
            Dungeon.level.Drop(new SkeletonKey(Dungeon.depth), pos).sprite.Drop();
            base.Die(cause);

            Yell(Messages.Get(this, "defeated"));
        }

        public override void Notice()
        {
            base.Notice();
            if (!BossHealthBar.IsAssigned())
            {
                BossHealthBar.AssignBoss(this);
                Yell(Messages.Get(this, "notice"));
                foreach (var ch in Actor.Chars())
                {
                    if (ch is DriedRose.GhostHero)
                        ((DriedRose.GhostHero)ch).SayBoss();
                }
            }
        }

        private void InitInstance()
        {
            immunities.Add(typeof(Grim));
            immunities.Add(typeof(GrimTrap));
            immunities.Add(typeof(Terror));
            immunities.Add(typeof(Amok));
            immunities.Add(typeof(Charm));
            immunities.Add(typeof(Sleep));
            immunities.Add(typeof(Burning));
            immunities.Add(typeof(ToxicGas));
            immunities.Add(typeof(ScrollOfRetribution));
            immunities.Add(typeof(ScrollOfPsionicBlast));
            immunities.Add(typeof(Vertigo));
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            BossHealthBar.AssignBoss(this);
        }

        [SPDStatic]
        public class RottingFist : Mob
        {
            private const int REGENERATION = 4;

            public RottingFist()
            {
                InitInstance();

                spriteClass = typeof(FistSprite.Rotting);

                HP = HT = 300;
                defenseSkill = 25;

                EXP = 0;

                state = WANDERING;

                properties.Add(Property.MINIBOSS);
                properties.Add(Property.DEMONIC);
                properties.Add(Property.ACIDIC);
            }

            public override int AttackSkill(Character target)
            {
                return 36;
            }

            public override int DamageRoll()
            {
                return Rnd.NormalIntRange(20, 50);
            }

            public override int DrRoll()
            {
                return Rnd.NormalIntRange(0, 15);
            }

            public override int AttackProc(Character enemy, int damage)
            {
                damage = base.AttackProc(enemy, damage);

                if (Rnd.Int(3) == 0)
                {
                    Buff.Affect<Ooze>(enemy).Set(Ooze.DURATION);
                    var color = new Color(0x00, 0x00, 0x00, 0xFF);
                    enemy.sprite.Burst(color, 5);
                }

                return damage;
            }

            public override bool Act()
            {
                if (Dungeon.level.water[pos] && HP < HT)
                {
                    sprite.Emitter().Burst(ShadowParticle.Up, 2);
                    HP += REGENERATION;
                }

                return base.Act();
            }

            public override void Damage(int dmg, object src)
            {
                base.Damage(dmg, src);
                LockedFloor lockedFloor = Dungeon.hero.FindBuff<LockedFloor>();
                if (lockedFloor != null)
                    lockedFloor.AddTime(dmg * 0.5f);
            }

            private void InitInstance()
            {
                immunities.Add(typeof(Paralysis));
                immunities.Add(typeof(Amok));
                immunities.Add(typeof(Sleep));
                immunities.Add(typeof(Terror));
                immunities.Add(typeof(Poison));
                immunities.Add(typeof(Vertigo));
            }
        }

        [SPDStatic]
        public class BurningFist : Mob
        {
            public BurningFist()
            {
                InitInstance();

                spriteClass = typeof(FistSprite.Burning);

                HP = HT = 200;
                defenseSkill = 25;

                EXP = 0;

                state = WANDERING;

                properties.Add(Property.MINIBOSS);
                properties.Add(Property.DEMONIC);
                properties.Add(Property.FIERY);
            }

            public override int AttackSkill(Character target)
            {
                return 36;
            }

            public override int DamageRoll()
            {
                return Rnd.NormalIntRange(26, 32);
            }

            public override int DrRoll()
            {
                return Rnd.NormalIntRange(0, 15);
            }

            protected override bool CanAttack(Character enemy)
            {
                return new Ballistic(pos, enemy.pos, Ballistic.MAGIC_BOLT).collisionPos == enemy.pos;
            }

            //used so resistances can differentiate between melee and magical attacks
            public class DarkBolt { }

            protected override bool DoAttack(Character enemy)
            {
                if (Dungeon.level.Adjacent(pos, enemy.pos))
                {
                    return base.DoAttack(enemy);
                }
                else
                {
                    if (sprite != null && (sprite.visible || enemy.sprite.visible))
                    {
                        sprite.Zap(enemy.pos);
                        return false;
                    }
                    else
                    {
                        Zap();
                        return true;
                    }
                }
            }

            private void Zap()
            {
                Spend(1f);

                if (Hit(this, enemy, true))
                {
                    int dmg = DamageRoll();
                    enemy.Damage(dmg, new DarkBolt());

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

            public void OnZapComplete()
            {
                Zap();
                Next();
            }

            public override bool Act()
            {
                for (int i = 0; i < PathFinder.NEIGHBORS9.Length; ++i)
                {
                    GameScene.Add(Blob.Seed(pos + PathFinder.NEIGHBORS9[i], 2, typeof(Fire)));
                }

                return base.Act();
            }

            public override void Damage(int dmg, object src)
            {
                base.Damage(dmg, src);
                LockedFloor lockedFloor = Dungeon.hero.FindBuff<LockedFloor>();
                if (lockedFloor != null)
                    lockedFloor.AddTime(dmg * 0.5f);
            }

            private void InitInstance()
            {
                immunities.Add(typeof(Amok));
                immunities.Add(typeof(Sleep));
                immunities.Add(typeof(Terror));
                immunities.Add(typeof(Vertigo));
            }
        }

        [SPDStatic]
        public class Larva : Mob
        {
            public Larva()
            {
                spriteClass = typeof(LarvaSprite);

                HP = HT = 25;
                defenseSkill = 20;

                EXP = 0;

                state = HUNTING;

                properties.Add(Property.DEMONIC);
            }

            public override int AttackSkill(Character target)
            {
                return 30;
            }

            public override int DamageRoll()
            {
                return Rnd.NormalIntRange(22, 30);
            }

            public override int DrRoll()
            {
                return Rnd.NormalIntRange(0, 8);
            }
        }
    }
}