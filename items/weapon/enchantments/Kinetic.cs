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
    public class Kinetic : Weapon.Enchantment
    {
        private static ItemSprite.Glowing YELLOW = new ItemSprite.Glowing(new Color(0xFF, 0xFF, 0x00, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            int conservedDamage = 0;
            var buff = attacker.FindBuff<ConservedDamage>();
            if (buff != null)
            {
                conservedDamage = buff.DamageBonus();
                buff.Detach();
            }

            if (damage > defender.HP)
            {
                int extraDamage = damage - defender.HP;

                Buff.Affect<ConservedDamage>(attacker).SetBonus(extraDamage);
            }

            return damage + conservedDamage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return YELLOW;
        }

        [SPDStatic]
        public class ConservedDamage : Buff
        {
            public override int Icon()
            {
                return BuffIndicator.WEAPON;
            }

            public override void TintIcon(Image icon)
            {
                if (preservedDamage >= 10)
                {
                    icon.Hardlight(1f, 0f, 0f);
                }
                else if (preservedDamage >= 5)
                {
                    icon.Hardlight(1f, 1f - (preservedDamage - 5f) * .2f, 0f);
                }
                else
                {
                    icon.Hardlight(1f, 1f, 1f - preservedDamage * .2f);
                }
            }

            float preservedDamage;

            public void SetBonus(int value)
            {
                preservedDamage = value;
            }

            public int DamageBonus()
            {
                return (int)Math.Ceiling(preservedDamage);
            }

            public override bool Act()
            {
                preservedDamage -= Math.Max(preservedDamage * .025f, 0.1f);
                if (preservedDamage <= 0)
                {
                    Detach();
                }

                Spend(TICK);
                return true;
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc", DamageBonus());
            }

            const string PRESERVED_DAMAGE = "preserve_damage";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(PRESERVED_DAMAGE, preservedDamage);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                if (bundle.Contains(PRESERVED_DAMAGE))
                {
                    preservedDamage = bundle.GetFloat(PRESERVED_DAMAGE);
                }
                else
                {
                    preservedDamage = Cooldown() / 10;
                    Spend(Cooldown());
                }
            }
        }
    }
}