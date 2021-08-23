using System;
using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.messages;
using spdd.sprites;
using spdd.ui;

namespace spdd.items.weapon.enchantments
{
    public class Blocking : Weapon.Enchantment
    {
        private static ItemSprite.Glowing BLUE = new ItemSprite.Glowing(new Color(0x00, 0x00, 0xFF, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            int level = Math.Max(0, weapon.BuffedLvl());

            Buff.Prolong<BlockBuff>(attacker, 2 + level / 2).SetBlocking(level + 1);

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return BLUE;
        }

        [SPDStatic]
        public class BlockBuff : FlavourBuff
        {
            private int blocking;

            public void SetBlocking(int value)
            {
                this.blocking = value;
            }

            public int BlockingRoll()
            {
                return Rnd.NormalIntRange(0, blocking);
            }

            public override int Icon()
            {
                return BuffIndicator.ARMOR;
            }

            public override void TintIcon(Image icon)
            {
                icon.Hardlight(0.5f, 1f, 2f);
            }

            public override float IconFadePercent()
            {
                return Math.Max(0, (5f - Visualcooldown()) / 5f);
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc", blocking, DispTurns());
            }

            const string BLOCKING = "blocking";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(BLOCKING, blocking);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                blocking = bundle.GetInt(BLOCKING);
            }
        }
    }
}
