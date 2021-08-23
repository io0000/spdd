using System;
using spdd.sprites;
using spdd.items;
using spdd.items.weapon.melee;
using watabou.utils;

namespace spdd.actors.mobs
{
    public class Slime : Mob
    {
        public Slime()
        {
            spriteClass = typeof(SlimeSprite);

            HP = HT = 20;
            defenseSkill = 5;

            EXP = 4;
            maxLvl = 9;

            lootChance = 0.2f; //by default, see rollToDropLoot()
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(2, 5);
        }

        public override int AttackSkill(Character target)
        {
            return 12;
        }

        public override void Damage(int dmg, object src)
        {
            if (dmg >= 5)
            {
                //takes 5/6/7/8/9/10 dmg at 5/7/10/14/19/25 incoming dmg
                dmg = 4 + (int)(Math.Sqrt(8 * (dmg - 4) + 1) - 1) / 2;
            }
            base.Damage(dmg, src);
        }

        public override void RollToDropLoot()
        {
            //each drop makes future drops 1/3 as likely
            // so loot chance looks like: 1/5, 1/15, 1/45, 1/135, etc.
            lootChance *= (float)Math.Pow(1 / 3f, Dungeon.LimitedDrops.SLIME_WEP.count);
            base.RollToDropLoot();
        }

        public override Item CreateLoot()
        {
            ++Dungeon.LimitedDrops.SLIME_WEP.count;
            Generator.Category c = Generator.Category.WEP_T2;
            var classes = c.GetClasses();
            var probs = c.GetProbs();
            var w = (MeleeWeapon)Reflection.NewInstance(classes[Rnd.Chances(probs)]);
            w.Random();
            w.SetLevel(0);
            return w;
        }
    }
}