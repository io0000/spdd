using watabou.utils;
using watabou.noosa.audio;
using spdd.items;
using spdd.utils;
using spdd.sprites;
using spdd.levels;
using spdd.levels.features;
using System;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class Skeleton : Mob
    {
        public Skeleton()
        {
            spriteClass = typeof(SkeletonSprite);

            HP = HT = 25;
            defenseSkill = 9;

            EXP = 5;
            maxLvl = 10;

            loot = Generator.Category.WEAPON;
            lootChance = 0.1667f; //by default, see rollToDropLoot()

            properties.Add(Property.UNDEAD);
            properties.Add(Property.INORGANIC);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(2, 10);
        }

        public override void Die(object cause)
        {
            base.Die(cause);

            if (Utils.CheckObjectType(cause, typeof(Chasm)))
                return;

            var heroKilled = false;
            for (var i = 0; i < PathFinder.NEIGHBORS8.Length; ++i)
            {
                var ch = FindChar(pos + PathFinder.NEIGHBORS8[i]);
                if (ch != null && ch.IsAlive())
                {
                    int damage = Rnd.NormalIntRange(6, 12);
                    damage = Math.Max(0, damage - (ch.DrRoll() + ch.DrRoll()));
                    ch.Damage(damage, this);
                    if (ch == Dungeon.hero && !ch.IsAlive())
                        heroKilled = true;
                }
            }

            if (Dungeon.level.heroFOV[pos])
                Sample.Instance.Play(Assets.Sounds.BONES);

            if (heroKilled)
            {
                Dungeon.Fail(GetType());
                GLog.Negative(Messages.Get(this, "explo_kill"));
            }
        }

        public override void RollToDropLoot()
        {
            //each drop makes future drops 1/2 as likely
            // so loot chance looks like: 1/6, 1/12, 1/24, 1/48, etc.
            lootChance *= (float)Math.Pow(1 / 2f, Dungeon.LimitedDrops.SKELE_WEP.count);
            base.RollToDropLoot();
        }

        public override Item CreateLoot()
        {
            ++Dungeon.LimitedDrops.SKELE_WEP.count;
            return base.CreateLoot();
        }

        public override int AttackSkill(Character target)
        {
            return 12;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 5);
        }
    }
}