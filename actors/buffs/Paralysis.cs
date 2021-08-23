using System;
using watabou.utils;
using spdd.ui;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Paralysis : FlavourBuff
    {
        public const float DURATION = 10.0f;

        public Paralysis()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override bool AttachTo(Character target)
        {
            if (base.AttachTo(target))
            {
                ++target.paralysed;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ProcessDamage(int damage)
        {
            if (target == null)
                return;

            var resist = target.FindBuff<ParalysisResist>();
            if (resist == null)
            {
                resist = Buff.Affect<ParalysisResist>(target);
            }

            resist.damage += damage;

            if (Rnd.NormalIntRange(0, resist.damage) >= Rnd.NormalIntRange(0, target.HP))
            {
                if (Dungeon.level.heroFOV[target.pos])
                {
                    target.sprite.ShowStatus(CharSprite.NEUTRAL, Messages.Get(this, "out"));
                }
                Detach();
            }
        }

        public override void Detach()
        {
            base.Detach();
            if (target.paralysed > 0)
                --target.paralysed;
        }

        public override int Icon()
        {
            return BuffIndicator.PARALYSIS;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
        }

        public override void Fx(bool on)
        {
            if (on)
                target.sprite.Add(CharSprite.State.PARALYSED);
            else
                target.sprite.Remove(CharSprite.State.PARALYSED);
        }

        public override string HeroMessage()
        {
            return Messages.Get(this, "heromsg");
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns());
        }

        [SPDStatic]
        public class ParalysisResist : Buff
        {
            public ParalysisResist()
            {
                type = BuffType.POSITIVE;
            }
            public int damage;

            public override bool Act()
            {
                if (target.FindBuff<Paralysis>() == null)
                {
                    damage -= (int)Math.Ceiling(damage / 10f);
                    if (damage >= 0)
                        Detach();
                }
                Spend(TICK);
                return true;
            }

            private const string DAMAGE = "damage";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                damage = bundle.GetInt(DAMAGE);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                bundle.Put(DAMAGE, damage);
            }
        }
    }
}