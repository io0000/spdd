using System;
using watabou.utils;
using spdd.actors.hero;
using spdd.items.artifacts;
using spdd.ui;
using spdd.utils;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Hunger : Buff, Hero.IDoom
    {
        private const float STEP = 10.0f;

        public const float HUNGRY = 300.0f;
        public const float STARVING = 450.0f;

        private float level;
        private float partialDamage;

        private const string LEVEL = "level";
        private const string PARTIALDAMAGE = "partialDamage";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(LEVEL, level);
            bundle.Put(PARTIALDAMAGE, partialDamage);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            level = bundle.GetFloat(LEVEL);
            partialDamage = bundle.GetFloat(PARTIALDAMAGE);
        }

        public override bool Act()
        {
            if (Dungeon.level.locked || target.FindBuff<WellFed>() != null)
            {
                Spend(STEP);
                return true;
            }

            if (target.IsAlive() && target is Hero)
            {
                var hero = (Hero)target;

                if (IsStarving())
                {
                    partialDamage += STEP * target.HT / 1000f;

                    if (partialDamage > 1)
                    {
                        target.Damage((int)partialDamage, this);
                        partialDamage -= (int)partialDamage;
                    }
                }
                else
                {
                    float newLevel = level + STEP;
                    if (newLevel >= STARVING)
                    {
                        GLog.Negative(Messages.Get(this, "onstarving"));
                        hero.resting = false;
                        hero.Damage(1, this);

                        hero.Interrupt();
                    }
                    else if (newLevel >= HUNGRY && level < HUNGRY)
                    {
                        GLog.Negative(Messages.Get(this, "onhungry"));
                    }

                    level = newLevel;
                }

                Spend(target.FindBuff<Shadows>() == null ? STEP : STEP * 1.5f);
            }
            else
            {
                Deactivate();
            }

            return true;
        }

        public void Satisfy(float energy)
        {
            var buff = target.FindBuff<HornOfPlenty.HornRecharge>();
            if (buff != null && buff.IsCursed())
            {
                energy *= 0.67f;
                GLog.Negative(Messages.Get(this, "cursedhorn"));
            }

            ReduceHunger(energy);
        }

        //directly interacts with hunger, no checks.
        public void ReduceHunger(float energy)
        {
            level -= energy;
            if (level < 0)
            {
                level = 0;
            }
            else if (level > STARVING)
            {
                float excess = level - STARVING;
                level = STARVING;
                partialDamage += excess * (target.HT / 1000f);
            }

            BuffIndicator.RefreshHero();
        }

        public bool IsStarving()
        {
            return level >= STARVING;
        }

        // public int Hunger()
        public int HungerValue()
        {
            return (int)Math.Ceiling(level);
        }

        public override int Icon()
        {
            if (level < HUNGRY)
                return BuffIndicator.NONE;

            if (level < STARVING)
                return BuffIndicator.HUNGER;

            return BuffIndicator.STARVATION;
        }

        public override string ToString()
        {
            if (level < STARVING)
                return Messages.Get(this, "hungry");
            else
                return Messages.Get(this, "starving");
        }

        public override string Desc()
        {
            string result;
            if (level < STARVING)
            {
                result = Messages.Get(this, "desc_intro_hungry");
            }
            else
            {
                result = Messages.Get(this, "desc_intro_starving");
            }

            result += Messages.Get(this, "desc");

            return result;
        }

        // Hero.IDoom
        public void OnDeath()
        {
            BadgesExtensions.ValidateDeathFromHunger();

            Dungeon.Fail(GetType());
            GLog.Negative(Messages.Get(this, "ondeath"));
        }
    }
}