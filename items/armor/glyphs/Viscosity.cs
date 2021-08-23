using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.ui;
using spdd.utils;
using spdd.messages;
using spdd.items.weapon.missiles;

namespace spdd.items.armor.glyphs
{
    public class Viscosity : Armor.Glyph
    {
        private static ItemSprite.Glowing PURPLE = new ItemSprite.Glowing(new Color(0x88, 0x44, 0xCC, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            //FIXME this glyph should really just proc after DR is accounted for.
            //should build in functionality for that, but this works for now
            int realDamage = damage - defender.DrRoll();

            if (attacker is Hero &&
                ((Hero)attacker).belongings.weapon is MissileWeapon &&
                ((Hero)attacker).subClass == HeroSubClass.SNIPER &&
                !Dungeon.level.Adjacent(attacker.pos, defender.pos))
            {
                realDamage = damage;
            }

            if (realDamage <= 0)
                return 0;

            int level = Math.Max(0, armor.BuffedLvl());

            float percent = (level + 1) / (float)(level + 6);
            int amount = (int)Math.Ceiling(realDamage * percent);

            DeferedDamage deferred = Buff.Affect<DeferedDamage>(defender);
            deferred.Prolong(amount);

            defender.sprite.ShowStatus(CharSprite.WARNING, Messages.Get(this, "deferred", amount));

            return damage - amount;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return PURPLE;
        }

        [SPDStatic]
        public class DeferedDamage : Buff
        {
            public DeferedDamage()
            {
                type = BuffType.NEGATIVE;
            }

            protected int damage;

            private const string DAMAGE = "damage";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(DAMAGE, damage);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                damage = bundle.GetInt(DAMAGE);
            }

            public override bool AttachTo(Character target)
            {
                if (base.AttachTo(target))
                {
                    Postpone(TICK);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Prolong(int damage)
            {
                this.damage += damage;
            }

            public override int Icon()
            {
                return BuffIndicator.DEFERRED;
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override bool Act()
            {
                if (target.IsAlive())
                {
                    int damageThisTick = Math.Max(1, (int)(damage * 0.1f));
                    target.Damage(damageThisTick, this);
                    if (target == Dungeon.hero && !target.IsAlive())
                    {
                        Dungeon.Fail(GetType());
                        GLog.Negative(Messages.Get(this, "ondeath"));

                        BadgesExtensions.ValidateDeathFromGlyph();
                    }
                    Spend(TICK);

                    damage -= damageThisTick;
                    if (damage <= 0)
                        Detach();
                }
                else
                {
                    Detach();
                }

                return true;
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc", damage);
            }
        }
    }
}