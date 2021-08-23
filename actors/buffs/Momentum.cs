using System;
using watabou.utils;
using watabou.noosa;
using spdd.ui;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Momentum : Buff
    {
        public Momentum()
        {
            type = BuffType.POSITIVE;
        }

        private int stacks;
        private int turnsSinceMove;

        public override bool Act()
        {
            ++turnsSinceMove;
            if (turnsSinceMove > 0)
            {
                stacks = Math.Max(0, stacks - turnsSinceMove);
                if (stacks == 0)
                    Detach();
            }
            Spend(TICK);
            return true;
        }

        public void GainStack()
        {
            stacks = Math.Min(stacks + 1, 10);
            turnsSinceMove = -1;
        }

        public int Stacks()
        {
            return stacks;
        }

        public float SpeedMultiplier()
        {
            //1.33x speed at max stacks
            return 1.0f + (stacks / 30.0f);
        }

        public int EvasionBonus(int excessArmorStr)
        {
            //8 evasion, +2 evasion per excess str, at max stacks
            return (int)Math.Round((0.8f + 0.2f * excessArmorStr) * stacks, MidpointRounding.AwayFromZero);
        }

        public override int Icon()
        {
            return BuffIndicator.MOMENTUM;
        }
        public override void TintIcon(Image icon)
        {
            icon.Invert();
        }

        public override float IconFadePercent()
        {
            return (10 - stacks) / 10f;
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", stacks * 10);
        }

        private const string STACKS = "stacks";
        private const string TURNS_SINCE = "turnsSinceMove";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(STACKS, stacks);
            bundle.Put(TURNS_SINCE, turnsSinceMove);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            stacks = bundle.GetInt(STACKS);
            turnsSinceMove = bundle.GetInt(TURNS_SINCE);
        }
    }
}