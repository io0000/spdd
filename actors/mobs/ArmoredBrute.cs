using watabou.utils;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.items;
using spdd.items.armor;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class ArmoredBrute : Brute
    {
        public ArmoredBrute()
        {
            spriteClass = typeof(ShieldedSprite);

            //see rollToDropLoot
            loot = Generator.Category.ARMOR;
            lootChance = 1f;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(6, 10);
        }

        protected override void TriggerEnrage()
        {
            Buff.Affect<ArmoredRage>(this).SetShield(HT / 2 + 1);
            if (Dungeon.level.heroFOV[pos])
            {
                sprite.ShowStatus(CharSprite.NEGATIVE, Messages.Get(this, "enraged"));
            }
            Spend(TICK);
            hasRaged = true;
        }

        public override Item CreateLoot()
        {
            if (Rnd.Int(4) == 0)
            {
                return new PlateArmor().Random();
            }
            return new ScaleArmor().Random();
        }

        [SPDStatic]
        public class ArmoredRage : Brute.BruteRage
        {
            public override bool Act()
            {
                if (target.HP > 0)
                {
                    Detach();
                    return true;
                }

                AbsorbDamage(1);

                if (Shielding() <= 0)
                    target.Die(null);

                Spend(3 * TICK);

                return true;
            }
        }
    }
}