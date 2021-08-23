using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects;
using spdd.items.bags;
using spdd.items.rings;
using spdd.items.weapon.melee;
using spdd.mechanics;
using spdd.messages;
using spdd.scenes;
using spdd.ui;
using spdd.utils;

namespace spdd.items.wands
{
    public abstract class Wand : Item
    {
        public const string AC_ZAP = "ZAP";

        public const float TIME_TO_ZAP = 1f;

        public int maxCharges; // = initialCharges();
        public int curCharges; // = maxCharges;
        public float partialCharge;

        public Charger charger;

        private bool curChargeKnown;

        public bool curseInfusionBonus;

        private const int USES_TO_ID = 10;
        private int usesLeftToID = USES_TO_ID;
        private float availableUsesToID = USES_TO_ID / 2f;

        protected int collisionProperties = Ballistic.MAGIC_BOLT;

        public Wand()
        {
            maxCharges = InitialCharges();
            curCharges = maxCharges;

            defaultAction = AC_ZAP;
            usesTargeting = true;
            bones = true;
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);
            if (curCharges > 0 || !curChargeKnown)
                actions.Add(AC_ZAP);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_ZAP))
            {
                curUser = hero;
                curItem = this;
                GameScene.SelectCell(zapper);
            }
        }

        protected abstract void OnZap(Ballistic attack);

        public abstract void OnHit(MagesStaff staff, Character attacker, Character defender, int damage);

        public virtual bool TryToZap(Hero owner, int target)
        {
            if (owner.FindBuff<MagicImmune>() != null)
            {
                GLog.Warning(Messages.Get(this, "no_magic"));
                return false;
            }

            if (curCharges >= (cursed ? 1 : ChargesPerCast()))
            {
                return true;
            }
            else
            {
                GLog.Warning(Messages.Get(this, "fizzles"));
                return false;
            }
        }

        public override bool Collect(Bag container)
        {
            if (base.Collect(container))
            {
                if (container.owner != null)
                {
                    if (container is MagicalHolster)
                        Charge(container.owner, MagicalHolster.HOLSTER_SCALE_FACTOR);
                    else
                        Charge(container.owner);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public void GainCharge(float amt)
        {
            partialCharge += amt;
            while (partialCharge >= 1)
            {
                curCharges = Math.Min(maxCharges, curCharges + 1);
                --partialCharge;
                UpdateQuickslot();
            }
        }

        public void Charge(Character owner)
        {
            if (charger == null)
                charger = new Charger(this);

            charger.AttachTo(owner);
        }

        public void Charge(Character owner, float chargeScaleFactor)
        {
            Charge(owner);
            charger.SetScaleFactor(chargeScaleFactor);
        }

        protected void ProcessSoulMark(Character target, int chargesUsed)
        {
            ProcessSoulMark(target, BuffedLvl(), chargesUsed);
        }

        protected static void ProcessSoulMark(Character target, int wandLevel, int chargesUsed)
        {
            if (target != Dungeon.hero &&
                Dungeon.hero.subClass == HeroSubClass.WARLOCK &&
                //standard 1 - 0.92^x chance, plus 7%. Starts at 15% 
                Rnd.Float() > (Math.Pow(0.92f, (wandLevel * chargesUsed) + 1) - 0.07f))
            {
                SoulMark.Prolong<SoulMark>(target, SoulMark.DURATION + wandLevel);
            }
        }

        protected override void OnDetach()
        {
            StopCharging();
        }

        public void StopCharging()
        {
            if (charger == null)
                return;

            charger.Detach();
            charger = null;
        }

        //public void level(int value)
        public override void SetLevel(int value)
        {
            base.SetLevel(value);
            UpdateLevel();
        }

        public override Item Identify()
        {
            curChargeKnown = true;
            base.Identify();

            UpdateQuickslot();

            return this;
        }

        public override void OnHeroGainExp(float levelPercent, Hero hero)
        {
            if (!IsIdentified() && availableUsesToID <= USES_TO_ID / 2f)
            {
                //gains enough uses to ID over 1 level
                availableUsesToID = Math.Min(USES_TO_ID / 2f, availableUsesToID + levelPercent * USES_TO_ID / 2f);
            }
        }

        public override string Info()
        {
            string desc = Desc();

            desc += "\n\n" + StatsDesc();

            if (cursed && cursedKnown)
            {
                desc += "\n\n" + Messages.Get(typeof(Wand), "cursed");
            }
            else if (!IsIdentified() && cursedKnown)
            {
                desc += "\n\n" + Messages.Get(typeof(Wand), "not_cursed");
            }

            return desc;
        }

        public virtual string StatsDesc()
        {
            return Messages.Get(this, "stats_desc");
        }

        public override bool IsIdentified()
        {
            return base.IsIdentified() && curChargeKnown;
        }

        public override string Status()
        {
            if (levelKnown)
                return (curChargeKnown ? curCharges.ToString() : "?") + "/" + maxCharges;
            else
                return null;
        }

        // public int level() 
        public override int GetLevel()
        {
            if (!cursed && curseInfusionBonus)
            {
                curseInfusionBonus = false;
                UpdateLevel();
            }
            return base.GetLevel() + (curseInfusionBonus ? 1 : 0);
        }

        public override Item Upgrade()
        {
            base.Upgrade();

            if (Rnd.Int(3) == 0)
                cursed = false;

            UpdateLevel();
            curCharges = Math.Min(curCharges + 1, maxCharges);
            UpdateQuickslot();

            return this;
        }

        public override Item Degrade()
        {
            base.Degrade();

            UpdateLevel();
            UpdateQuickslot();

            return this;
        }

        public override int BuffedLvl()
        {
            int lvl = base.BuffedLvl();
            if (curUser != null)
            {
                var buff = curUser.FindBuff<WandOfMagicMissile.MagicCharge>();
                if (buff != null && buff.Level() > lvl)
                {
                    return buff.Level();
                }
            }
            return lvl;
        }

        public void UpdateLevel()
        {
            maxCharges = Math.Min(InitialCharges() + GetLevel(), 10);
            curCharges = Math.Min(curCharges, maxCharges);
        }

        protected virtual int InitialCharges()
        {
            return 2;
        }

        protected virtual int ChargesPerCast()
        {
            return 1;
        }

        public virtual void Fx(Ballistic bolt, ICallback callback)
        {
            MagicMissile.BoltFromChar(curUser.sprite.parent,
                            MagicMissile.MAGIC_MISSILE,
                            curUser.sprite,
                            bolt.collisionPos,
                            callback);
            Sample.Instance.Play(Assets.Sounds.ZAP);
        }

        public virtual void StaffFx(MagesStaff.StaffParticle particle)
        {
            particle.SetColor(new Color(0xFF, 0xFF, 0xFF, 0xFF));
            particle.am = 0.3f;
            particle.SetLifespan(1f);
            particle.speed.Polar(Rnd.Float(PointF.PI2), 2f);
            particle.SetSize(1f, 2f);
            particle.RadiateXY(0.5f);
        }

        protected void WandUsed()
        {
            if (!IsIdentified() && availableUsesToID >= 1)
            {
                --availableUsesToID;
                --usesLeftToID;
                if (usesLeftToID <= 0)
                {
                    Identify();
                    GLog.Positive(Messages.Get(typeof(Wand), "identify"));
                    BadgesExtensions.ValidateItemLevelAquired(this);
                }
            }

            curCharges -= cursed ? 1 : ChargesPerCast();

            var buff = curUser.FindBuff<WandOfMagicMissile.MagicCharge>();
            if (buff != null && buff.Level() > base.BuffedLvl())
            {
                buff.Detach();
            }

            Invisibility.Dispel();

            if (curUser.heroClass == HeroClass.MAGE)
                levelKnown = true;
            UpdateQuickslot();

            curUser.SpendAndNext(TIME_TO_ZAP);
        }

        public override Item Random()
        {
            //+0: 66.67% (2/3)
            //+1: 26.67% (4/15)
            //+2: 6.67%  (1/15)
            int n = 0;
            if (Rnd.Int(3) == 0)
            {
                ++n;
                if (Rnd.Int(5) == 0)
                    ++n;
            }
            SetLevel(n);
            curCharges += n;

            //30% chance to be cursed
            if (Rnd.Float() < 0.3f)
                cursed = true;

            return this;
        }

        public override int Value()
        {
            int price = 75;
            if (cursed && cursedKnown)
                price /= 2;

            if (levelKnown)
            {
                var lv = GetLevel();

                if (lv > 0)
                    price *= (lv + 1);
                else if (lv < 0)
                    price /= (1 - lv);
            }

            if (price < 1)
                price = 1;

            return price;
        }

        private const string USES_LEFT_TO_ID = "uses_left_to_id";
        private const string AVAILABLE_USES = "available_uses";
        private const string CUR_CHARGES = "curCharges";
        private const string CUR_CHARGE_KNOWN = "curChargeKnown";
        private const string PARTIALCHARGE = "partialCharge";
        private const string CURSE_INFUSION_BONUS = "curse_infusion_bonus";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(USES_LEFT_TO_ID, usesLeftToID);
            bundle.Put(AVAILABLE_USES, availableUsesToID);
            bundle.Put(CUR_CHARGES, curCharges);
            bundle.Put(CUR_CHARGE_KNOWN, curChargeKnown);
            bundle.Put(PARTIALCHARGE, partialCharge);
            bundle.Put(CURSE_INFUSION_BONUS, curseInfusionBonus);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            usesLeftToID = bundle.GetInt(USES_LEFT_TO_ID);
            availableUsesToID = bundle.GetInt(AVAILABLE_USES);

            curCharges = bundle.GetInt(CUR_CHARGES);
            curChargeKnown = bundle.GetBoolean(CUR_CHARGE_KNOWN);
            partialCharge = bundle.GetFloat(PARTIALCHARGE);
            curseInfusionBonus = bundle.GetBoolean(CURSE_INFUSION_BONUS);
        }

        public override void Reset()
        {
            base.Reset();
            usesLeftToID = USES_TO_ID;
            availableUsesToID = USES_TO_ID / 2f;
        }

        protected virtual int CollisionProperties(int target)
        {
            return collisionProperties;
        }

        protected static ZapperCellSelectorListener zapper = new ZapperCellSelectorListener();

        public class ZapperCellSelectorListener : CellSelector.IListener
        {
            public void OnSelect(int? t)
            {
                if (t != null)
                {
                    int target = t.Value;
                    //FIXME this safety check shouldn't be necessary
                    //it would be better to eliminate the curItem static variable.
                    Wand curWand;
                    if (curItem is Wand)
                        curWand = (Wand)Wand.curItem;
                    else
                        return;

                    var shot = new Ballistic(curUser.pos, target, curWand.CollisionProperties(target));
                    int cell = shot.collisionPos;

                    if (target == curUser.pos || cell == curUser.pos)
                    {
                        GLog.Information(Messages.Get(typeof(Wand), "self_target"));
                        return;
                    }

                    curUser.sprite.Zap(cell);

                    //attempts to target the cell aimed at if something is there, otherwise targets the collision pos.
                    if (Actor.FindChar(target) != null)
                        QuickSlotButton.Target(Actor.FindChar(target));
                    else
                        QuickSlotButton.Target(Actor.FindChar(cell));

                    if (curWand.TryToZap(curUser, target))
                    {
                        curUser.Busy();

                        if (curWand.cursed)
                        {
                            if (!curWand.cursedKnown)
                                GLog.Negative(Messages.Get(typeof(Wand), "curse_discover", curWand.Name()));

                            var callback = new ActionCallback();
                            callback.action = () =>
                            {
                                curWand.WandUsed();
                            };

                            CursedWand.CursedZap(curWand,
                                curUser,
                                new Ballistic(curUser.pos, target, Ballistic.MAGIC_BOLT),
                                callback);
                        }
                        else
                        {
                            var callback = new ActionCallback();
                            callback.action = () =>
                            {
                                curWand.OnZap(shot);
                                curWand.WandUsed();
                            };

                            curWand.Fx(shot, callback);
                        }
                        curWand.cursedKnown = true;
                    }
                }
            }

            public string Prompt()
            {
                return Messages.Get(typeof(Wand), "prompt");
            }
        }

        public class Charger : Buff
        {
            private readonly Wand wand;
            private const float BASE_CHARGE_DELAY = 10f;
            private const float SCALING_CHARGE_ADDITION = 40f;
            private const float NORMAL_SCALE_FACTOR = 0.875f;

            private const float CHARGE_BUFF_BONUS = 0.25f;

            float scalingFactor = NORMAL_SCALE_FACTOR;

            public Charger(Wand wand)
            {
                this.wand = wand;
            }

            public override bool AttachTo(Character target)
            {
                base.AttachTo(target);
                return true;
            }

            public override bool Act()
            {
                if (wand.curCharges < wand.maxCharges)
                    Recharge();

                while (wand.partialCharge >= 1 && wand.curCharges < wand.maxCharges)
                {
                    --wand.partialCharge;
                    ++wand.curCharges;
                    UpdateQuickslot();
                }

                if (wand.curCharges == wand.maxCharges)
                {
                    wand.partialCharge = 0;
                }

                Spend(TICK);

                return true;
            }

            private void Recharge()
            {
                int missingCharges = wand.maxCharges - wand.curCharges;
                missingCharges = Math.Max(0, missingCharges);

                float turnsToCharge = (float)(BASE_CHARGE_DELAY
                        + (SCALING_CHARGE_ADDITION * Math.Pow(scalingFactor, missingCharges)));

                var lockedFloor = target.FindBuff<LockedFloor>();
                if (lockedFloor == null || lockedFloor.RegenOn())
                    wand.partialCharge += (1f / turnsToCharge) * RingOfEnergy.WandChargeMultiplier(target);

                foreach (Recharging bonus in target.Buffs<Recharging>())
                {
                    if (bonus != null && bonus.Remainder() > 0f)
                    {
                        wand.partialCharge += CHARGE_BUFF_BONUS * bonus.Remainder();
                    }
                }
            }

            public Wand Wand()
            {
                return wand;
            }

            public void GainCharge(float charge)
            {
                if (wand.curCharges < wand.maxCharges)
                {
                    wand.partialCharge += charge;
                    while (wand.partialCharge >= 1f)
                    {
                        ++wand.curCharges;
                        --wand.partialCharge;
                    }
                    wand.curCharges = Math.Min(wand.curCharges, wand.maxCharges);
                    UpdateQuickslot();
                }
            }

            public void SetScaleFactor(float value)
            {
                this.scalingFactor = value;
            }
        }
    }
}