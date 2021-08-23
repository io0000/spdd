using System;
using watabou.utils;
using spdd.actors.hero;
using spdd.sprites;
using spdd.items.rings;
using spdd.messages;

namespace spdd.items.artifacts
{
    public class MasterThievesArmband : Artifact
    {
        public MasterThievesArmband()
        {
            image = ItemSpriteSheet.ARTIFACT_ARMBAND;

            levelCap = 10;

            charge = 0;
        }

        //private int exp = 0;

        protected override ArtifactBuff PassiveBuff()
        {
            return new Thievery(this);
        }

        public override void Charge(Hero target)
        {
            if (charge < chargeCap)
            {
                charge += 10;
                UpdateQuickslot();
            }
        }

        public override string Desc()
        {
            string desc = base.Desc();

            if (IsEquipped(Dungeon.hero))
            {
                if (cursed)
                {
                    desc += "\n\n" + Messages.Get(this, "desc_cursed");
                }
                else
                {
                    desc += "\n\n" + Messages.Get(this, "desc_worn");
                }
            }

            return desc;
        }

        public class Thievery : ArtifactBuff
        {
            public Thievery(Artifact artifact)
                : base(artifact)
            { }

            public void Collect(int gold)
            {
                if (!artifact.cursed)
                {
                    artifact.charge += (int)(gold / 2 * RingOfEnergy.ArtifactChargeMultiplier(target));
                }
            }

            public override void Detach()
            {
                artifact.charge = (int)(artifact.charge * 0.95f);
                base.Detach();
            }

            public override bool Act()
            {
                if (artifact.cursed)
                {
                    if (Dungeon.gold > 0 && Rnd.Int(6) == 0)
                        --Dungeon.gold;

                    Spend(TICK);
                    return true;
                }
                else
                {
                    return base.Act();
                }
            }

            public bool Steal(int value)
            {
                var mta = (MasterThievesArmband)artifact;

                if (value <= mta.charge)
                {
                    mta.charge -= value;
                    mta.exp += value;
                }
                else
                {
                    float chance = StealChance(value);
                    if (Rnd.Float() > chance)
                    {
                        return false;
                    }
                    else
                    {
                        if (chance <= 1)
                        {
                            mta.charge = 0;
                        }
                        else
                        {
                            //removes the charge it took you to reach 100%
                            mta.charge -= (int)(mta.charge / chance);
                        }

                        mta.exp += value;
                    }
                }
                while (mta.exp >= (250 + 50 * mta.GetLevel()) && mta.GetLevel() < mta.levelCap)
                {
                    mta.exp -= (250 + 50 * mta.GetLevel());
                    mta.Upgrade();
                }
                return true;
            }

            public float StealChance(int value)
            {
                //get lvl*50 gold or lvl*3.33% item value of free charge, whichever is less.
                int chargeBonus = Math.Min(artifact.GetLevel() * 50, (value * artifact.GetLevel()) / 30);
                return (((float)artifact.charge + chargeBonus) / value);
            }
        }
    }
}