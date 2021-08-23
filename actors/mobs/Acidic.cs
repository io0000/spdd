using spdd.sprites;
using spdd.actors.buffs;
using spdd.items;
using spdd.items.potions;

namespace spdd.actors.mobs
{
    public class Acidic : Scorpio
    {
        public Acidic()
        {
            spriteClass = typeof(AcidicSprite);

            properties.Add(Property.ACIDIC);

            loot = new PotionOfExperience();
            lootChance = 1f;
        }

        public override int AttackProc(Character enemy, int damage)
        {
            Buff.Affect<Ooze>(enemy).Set(Ooze.DURATION);
            return base.AttackProc(enemy, damage);
        }

        public override int DefenseProc(Character enemy, int damage)
        {
            if (Dungeon.level.Adjacent(pos, enemy.pos))
            {
                Buff.Affect<Ooze>(enemy).Set(Ooze.DURATION);
            }

            return base.DefenseProc(enemy, damage);
        }

        public override Item CreateLoot()
        {
            return new PotionOfExperience();
        }
    }
}