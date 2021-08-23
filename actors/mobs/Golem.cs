using System;
using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.actors.mobs.npcs;
using spdd.items;
using spdd.items.scrolls;
using spdd.sprites;

namespace spdd.actors.mobs
{
    public class Golem : Mob
    {
        public Golem()
        {
            spriteClass = typeof(GolemSprite);

            HP = HT = 100;
            defenseSkill = 12;

            EXP = 12;
            maxLvl = 22;

            loot = Rnd.OneOf(Generator.Category.WEAPON, Generator.Category.ARMOR);
            lootChance = 0.125f; //initially, see rollToDropLoot

            properties.Add(Property.INORGANIC);
            properties.Add(Property.LARGE);

            WANDERING = new WanderingGolem(this);
            HUNTING = new HuntingGolem(this);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(15, 35);
        }

        public override int AttackSkill(Character target)
        {
            return 28;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 12);
        }

        public override void RollToDropLoot()
        {
            Imp.Quest.Process(this);

            //each drop makes future drops 1/2 as likely
            // so loot chance looks like: 1/8, 1/16, 1/32, 1/64, etc.
            lootChance *= (float)Math.Pow(1 / 2f, Dungeon.LimitedDrops.GOLEM_EQUIP.count);
            base.RollToDropLoot();
        }

        public override Item CreateLoot()
        {
            ++Dungeon.LimitedDrops.GOLEM_EQUIP.count;
            //uses probability tables for demon halls
            if (loot is Generator.Category && (Generator.Category)loot == Generator.Category.WEAPON)
            {
                return Generator.RandomWeapon(5);
            }
            else
            {
                return Generator.RandomArmor(5);
            }
        }

        private bool teleporting;
        private int selfTeleCooldown;
        private int enemyTeleCooldown;

        private const string TELEPORTING = "teleporting";
        private const string SELF_COOLDOWN = "self_cooldown";
        private const string ENEMY_COOLDOWN = "enemy_cooldown";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(TELEPORTING, teleporting);
            bundle.Put(SELF_COOLDOWN, selfTeleCooldown);
            bundle.Put(ENEMY_COOLDOWN, enemyTeleCooldown);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            teleporting = bundle.GetBoolean(TELEPORTING);
            selfTeleCooldown = bundle.GetInt(SELF_COOLDOWN);
            enemyTeleCooldown = bundle.GetInt(ENEMY_COOLDOWN);
        }

        public override bool Act()
        {
            --selfTeleCooldown;
            --enemyTeleCooldown;
            if (teleporting)
            {
                ((GolemSprite)sprite).TeleParticles(false);
                if (Actor.FindChar(target) == null && Dungeon.level.openSpace[target])
                {
                    ScrollOfTeleportation.Appear(this, target);
                    selfTeleCooldown = 30;
                }
                else
                {
                    target = Dungeon.level.RandomDestination(this);
                }
                teleporting = false;
                Spend(TICK);
                return true;
            }
            return base.Act();
        }

        public void OnZapComplete()
        {
            TeleportEnemy();
            Next();
        }

        public void TeleportEnemy()
        {
            Spend(TICK);

            int bestPos = enemy.pos;
            foreach (int i in PathFinder.NEIGHBORS8)
            {
                if (Dungeon.level.passable[pos + i] &&
                    Actor.FindChar(pos + i) == null &&
                    Dungeon.level.TrueDistance(pos + i, enemy.pos) > Dungeon.level.TrueDistance(bestPos, enemy.pos))
                {
                    bestPos = pos + i;
                }
            }

            if (enemy.FindBuff<MagicImmune>() != null)
                bestPos = enemy.pos;

            if (bestPos != enemy.pos)
            {
                ScrollOfTeleportation.Appear(enemy, bestPos);
                if (enemy is Hero)
                {
                    ((Hero)enemy).Interrupt();
                    Dungeon.Observe();
                }
            }

            enemyTeleCooldown = 20;
        }

        class WanderingGolem : Mob.Wandering
        {
            public WanderingGolem(Mob mob)
                : base(mob)
            { }

            protected override bool ContinueWandering()
            {
                Golem golem = (Golem)mob;

                golem.enemySeen = false;

                int oldPos = golem.pos;
                if (golem.target != -1 && golem.GetCloser(golem.target))
                {
                    golem.Spend(1 / golem.Speed());
                    return golem.MoveSprite(oldPos, golem.pos);
                }
                else if (golem.target != -1 && golem.target != golem.pos && golem.selfTeleCooldown <= 0)
                {
                    var s = (GolemSprite)golem.sprite;
                    s.TeleParticles(true);
                    golem.teleporting = true;
                    golem.Spend(2 * TICK);
                }
                else
                {
                    golem.target = Dungeon.level.RandomDestination(golem);
                    golem.Spend(TICK);
                }

                return true;
            }
        }

        class HuntingGolem : Mob.Hunting
        {
            public HuntingGolem(Mob mob)
                : base(mob)
            { }

            public override bool Act(bool enemyInFOV, bool justAlerted)
            {
                Golem golem = (Golem)mob;
                bool callBase = false;

                bool result = golem.HuntingGolemAct(enemyInFOV, justAlerted, ref callBase);
                if (callBase)
                    return base.Act(enemyInFOV, justAlerted);
                else
                    return result;
            }
        }

        public bool HuntingGolemAct(bool enemyInFOV, bool justAlerted, ref bool callBase)
        {
            if (!enemyInFOV || CanAttack(enemy))
            {
                //return super.act(enemyInFOV, justAlerted);
                callBase = true;
                return false;
            }
            else
            {
                enemySeen = true;
                target = enemy.pos;

                int oldPos = pos;

                if (enemyTeleCooldown <= 0 &&
                    Rnd.Int(100 / Distance(enemy)) == 0 &&
                    !Character.HasProp(enemy, Property.IMMOVABLE))
                {
                    if (sprite != null && (sprite.visible || enemy.sprite.visible))
                    {
                        sprite.Zap(enemy.pos);
                        return false;
                    }
                    else
                    {
                        TeleportEnemy();
                        return true;
                    }
                }
                else if (GetCloser(target))
                {
                    Spend(1 / Speed());
                    return MoveSprite(oldPos, pos);
                }
                else if (enemyTeleCooldown <= 0 && !Character.HasProp(enemy, Property.IMMOVABLE))
                {
                    if (sprite != null && (sprite.visible || enemy.sprite.visible))
                    {
                        sprite.Zap(enemy.pos);
                        return false;
                    }
                    else
                    {
                        TeleportEnemy();
                        return true;
                    }
                }
                else
                {
                    Spend(TICK);
                    return true;
                }
            }
        }
    }
}