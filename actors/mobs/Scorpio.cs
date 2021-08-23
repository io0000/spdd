using System;
using watabou.utils;
using spdd.actors.buffs;
using spdd.items;
using spdd.items.potions;
using spdd.mechanics;
using spdd.sprites;

namespace spdd.actors.mobs
{
    public class Scorpio : Mob
    {
        public Scorpio()
        {
            spriteClass = typeof(ScorpioSprite);

            HP = HT = 95;
            defenseSkill = 24;
            viewDistance = Light.DISTANCE;

            EXP = 14;
            maxLvl = 27;

            loot = Generator.Category.POTION;
            lootChance = 0.5f;

            properties.Add(Property.DEMONIC);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(26, 36);
        }

        public override int AttackSkill(Character target)
        {
            return 36;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 16);
        }

        protected override bool CanAttack(Character enemy)
        {
            Ballistic attack = new Ballistic(pos, enemy.pos, Ballistic.PROJECTILE);
            return !Dungeon.level.Adjacent(pos, enemy.pos) && attack.collisionPos == enemy.pos;
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);
            if (Rnd.Int(2) == 0)
                Buff.Prolong<Cripple>(enemy, Cripple.DURATION);

            return damage;
        }

        public override bool GetCloser(int target)
        {
            if (state == HUNTING)
                return enemySeen && GetFurther(target);

            return base.GetCloser(target);
        }

        public override Item CreateLoot()
        {
            Type loot;
            do
            {
                loot = (Type)Rnd.OneOf(Generator.Category.POTION.GetClasses());
            } 
            while (loot.Equals(typeof(PotionOfHealing)) || loot.Equals(typeof(PotionOfStrength)));

            return (Item)Reflection.NewInstance(loot);
        }
    }
}