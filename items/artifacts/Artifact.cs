using System;
using watabou.utils;
using spdd.actors.hero;
using spdd.utils;
using spdd.actors.buffs;
using spdd.actors;
using spdd.messages;

namespace spdd.items.artifacts
{
    public class Artifact : KindofMisc
    {
        public Buff passiveBuff;
        public Buff activeBuff;

        //level is used internally to track upgrades to artifacts, size/logic varies per artifact.
        //already inherited from item superclass
        //exp is used to count progress towards levels for some artifacts
        public int exp;
        //levelCap is the artifact's maximum level
        public int levelCap;

        //the current artifact charge
        public int charge;
        //the build towards next charge, usually rolls over at 1.
        //better to keep charge as an int and use a separate float than casting.
        public float partialCharge;
        //the maximum charge, varies per artifact, not all artifacts use this.
        public int chargeCap;

        //used by some artifacts to keep track of duration of effects or cooldowns to use.
        public int cooldown;

        public override bool DoEquip(Hero hero)
        {
            if ((hero.belongings.artifact != null && hero.belongings.artifact.GetType() == this.GetType()) ||
                (hero.belongings.misc != null && hero.belongings.misc.GetType() == this.GetType()))
            {
                GLog.Warning(Messages.Get(typeof(Artifact), "cannot_wear_two"));
                return false;
            }
            else
            {
                if (base.DoEquip(hero))
                {
                    Identify();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override void Activate(Character ch)
        {
            passiveBuff = PassiveBuff();
            passiveBuff.AttachTo(ch);
        }

        public override bool DoUnequip(Hero hero, bool collect, bool single)
        {
            if (base.DoUnequip(hero, collect, single))
            {
                passiveBuff.Detach();
                passiveBuff = null;

                if (activeBuff != null)
                {
                    activeBuff.Detach();
                    activeBuff = null;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override int VisiblyUpgraded()
        {
            return levelKnown ? (int)Math.Round((GetLevel() * 10) / (float)levelCap, MidpointRounding.AwayFromZero) : 0;
        }

        public override int BuffedVisiblyUpgraded()
        {
            return VisiblyUpgraded();
        }

        public override int BuffedLvl()
        {
            //level isn't affected by buffs/debuffs
            return GetLevel();
        }

        public void TransferUpgrade(int transferLvl)
        {
            Upgrade((int)(Math.Round((float)(transferLvl * levelCap) / 10, MidpointRounding.AwayFromZero)));
        }

        public override string Info()
        {
            if (cursed && cursedKnown && !IsEquipped(Dungeon.hero))
            {
                return Desc() + "\n\n" + Messages.Get(typeof(Artifact), "curse_known");

            }
            else if (!IsIdentified() && cursedKnown && !IsEquipped(Dungeon.hero))
            {
                return Desc() + "\n\n" + Messages.Get(typeof(Artifact), "not_cursed");
            }
            else
            {
                return Desc();
            }
        }

        public override string Status()
        {
            //if the artifact isn't IDed, or is cursed, don't display anything
            if (!IsIdentified() || cursed)
                return null;

            //display the current cooldown
            if (cooldown != 0)
                return Messages.Format("%d", cooldown);

            //display as percent
            if (chargeCap == 100)
                return Messages.Format("%d%%", charge);

            //display as #/#
            if (chargeCap > 0)
                return Messages.Format("%d/%d", charge, chargeCap);

            //if there's no cap -
            //- but there is charge anyway, display that charge
            if (charge != 0)
                return Messages.Format("%d", charge);

            //otherwise, if there's no charge, return null.
            return null;
        }

        public override Item Random()
        {
            //always +0

            //30% chance to be cursed
            if (Rnd.Float() < 0.3f)
                cursed = true;

            return this;
        }

        public override int Value()
        {
            int price = 100;
            if (GetLevel() > 0)
                price += 20 * VisiblyUpgraded();

            if (cursed && cursedKnown)
                price /= 2;

            if (price < 1)
                price = 1;

            return price;
        }

        protected virtual ArtifactBuff PassiveBuff()
        {
            return null;
        }

        protected virtual ArtifactBuff ActiveBuff()
        {
            return null;
        }

        public virtual void Charge(Hero target)
        {
            //do nothing by default;
        }

        public class ArtifactBuff : Buff
        {
            protected Artifact artifact;

            public ArtifactBuff(Artifact artifact)
            {
                this.artifact = artifact;
            }

            public int ItemLevel()
            {
                return artifact.GetLevel();
            }

            public bool IsCursed()
            {
                return artifact.cursed;
            }
        }

        private const string EXP = "exp";
        private const string CHARGE = "charge";
        private const string PARTIALCHARGE = "partialcharge";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(EXP, exp);
            bundle.Put(CHARGE, charge);
            bundle.Put(PARTIALCHARGE, partialCharge);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            exp = bundle.GetInt(EXP);
            if (chargeCap > 0)
                charge = Math.Min(chargeCap, bundle.GetInt(CHARGE));
            else
                charge = bundle.GetInt(CHARGE);
            partialCharge = bundle.GetFloat(PARTIALCHARGE);
        }
    }
}
