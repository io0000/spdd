using System;
using System.Linq;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.items.rings;
using spdd.sprites;
using spdd.scenes;
using spdd.utils;
using spdd.actors;
using spdd.windows;
using spdd.messages;

namespace spdd.items.artifacts
{
    public class TimekeepersHourglass : Artifact
    {
        public TimekeepersHourglass()
        {
            image = ItemSpriteSheet.ARTIFACT_HOURGLASS;

            levelCap = 5;

            charge = 5 + GetLevel();
            partialCharge = 0;
            chargeCap = 5 + GetLevel();

            defaultAction = AC_ACTIVATE;
        }

        public const string AC_ACTIVATE = "ACTIVATE";

        //keeps track of generated sandbags.
        public int sandBags;

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            if (IsEquipped(hero) && charge > 0 && !cursed)
                actions.Add(AC_ACTIVATE);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_ACTIVATE))
            {
                if (!IsEquipped(hero))
                {
                    GLog.Information(Messages.Get(typeof(Artifact), "need_to_equip"));
                }
                else if (activeBuff != null)
                {
                    if (activeBuff is TimeStasis)
                    {
                        //do nothing
                    }
                    else
                    {
                        activeBuff.Detach();
                        GLog.Information(Messages.Get(this, "deactivate"));
                    }
                }
                else if (charge <= 0)
                {
                    GLog.Information(Messages.Get(this, "no_charge"));
                }
                else if (cursed)
                {
                    GLog.Information(Messages.Get(this, "cursed"));
                }
                else
                {
                    var wnd = new WndOptions(
                        Messages.Get(this, "name"),
                        Messages.Get(this, "prompt"),
                        Messages.Get(this, "stasis"),
                        Messages.Get(this, "freeze"));

                    wnd.selectAction = (index) =>
                    {
                        if (index == 0)
                        {
                            GLog.Information(Messages.Get(typeof(TimekeepersHourglass), "onstasis"));
                            GameScene.Flash(new Color(0xFF, 0xFF, 0xFF, 0xFF));
                            Sample.Instance.Play(Assets.Sounds.TELEPORT);

                            activeBuff = new TimeStasis(this);
                            activeBuff.AttachTo(Dungeon.hero);
                        }
                        else if (index == 1)
                        {
                            GLog.Information(Messages.Get(typeof(TimekeepersHourglass), "onfreeze"));
                            GameScene.Flash(new Color(0xFF, 0xFF, 0xFF, 0xFF));
                            Sample.Instance.Play(Assets.Sounds.TELEPORT);

                            activeBuff = new TimeFreeze(this);
                            activeBuff.AttachTo(Dungeon.hero);
                            ((TimeFreeze)activeBuff).ProcessTime(0f);
                        }
                    };
                    GameScene.Show(wnd);
                }
            }
        }

        public override void Activate(Character ch)
        {
            base.Activate(ch);
            if (activeBuff != null)
                activeBuff.AttachTo(ch);
        }

        public override bool DoUnequip(Hero hero, bool collect, bool single)
        {
            if (base.DoUnequip(hero, collect, single))
            {
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

        protected override ArtifactBuff PassiveBuff()
        {
            return new HourglassRecharge(this);
        }

        public override void Charge(Hero target)
        {
            if (charge < chargeCap)
            {
                partialCharge += 0.25f;
                if (partialCharge >= 1)
                {
                    --partialCharge;
                    ++charge;
                    UpdateQuickslot();
                }
            }
        }

        public override Item Upgrade()
        {
            chargeCap += 1;

            //for artifact transmutation.
            while (GetLevel() + 1 > sandBags)
                ++sandBags;

            return base.Upgrade();
        }

        public override string Desc()
        {
            string desc = base.Desc();

            if (IsEquipped(Dungeon.hero))
            {
                if (!cursed)
                {
                    if (GetLevel() < levelCap)
                        desc += "\n\n" + Messages.Get(this, "desc_hint");
                }
                else
                {
                    desc += "\n\n" + Messages.Get(this, "desc_cursed");
                }
            }
            return desc;
        }

        private const string SANDBAGS = "sandbags";
        private const string BUFF = "buff";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(SANDBAGS, sandBags);

            if (activeBuff != null)
                bundle.Put(BUFF, activeBuff);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            sandBags = bundle.GetInt(SANDBAGS);

            //these buffs belong to hourglass, need to handle unbundling within the hourglass class.
            if (bundle.Contains(BUFF))
            {
                Bundle buffBundle = bundle.GetBundle(BUFF);

                if (buffBundle.Contains(TimeFreeze.PRESSES))
                    activeBuff = new TimeFreeze(this);
                else
                    activeBuff = new TimeStasis(this);

                activeBuff.RestoreFromBundle(buffBundle);
            }
        }

        public class HourglassRecharge : ArtifactBuff
        {
            public HourglassRecharge(Artifact artifact)
                : base(artifact)
            { }

            public override bool Act()
            {
                var th = (TimekeepersHourglass)artifact;
                return th.HourglassRechargeAct(this);
            }
        }

        public bool HourglassRechargeAct(HourglassRecharge buff)
        {
            var target = buff.target;

            var lockedFloor = target.FindBuff<LockedFloor>();
            if (charge < chargeCap && !cursed && (lockedFloor == null || lockedFloor.RegenOn()))
            {
                //90 turns to charge at full, 60 turns to charge at 0/10
                float chargeGain = 1 / (90f - (chargeCap - charge) * 3f);
                chargeGain *= RingOfEnergy.ArtifactChargeMultiplier(target);
                partialCharge += chargeGain;

                if (partialCharge >= 1)
                {
                    --partialCharge;
                    ++charge;

                    if (charge == chargeCap)
                        partialCharge = 0;
                }
            }
            else if (cursed && Rnd.Int(10) == 0)
                ((Hero)target).Spend(Actor.TICK);

            UpdateQuickslot();

            buff.Spend(Actor.TICK);

            return true;
        }

        public class TimeStasis : ArtifactBuff
        {
            public TimeStasis(Artifact artifact)
                : base(artifact)
            {
                type = BuffType.POSITIVE;
            }

            public override bool AttachTo(Character target)
            {
                var th = (TimekeepersHourglass)artifact;

                if (base.AttachTo(target))
                {
                    int usedCharge = Math.Min(th.charge, 2);
                    //buffs always act last, so the stasis buff should end a turn early.
                    Spend((5 * usedCharge) - 1);
                    ((Hero)target).SpendAndNext(5 * usedCharge);

                    //shouldn't punish the player for going into stasis frequently
                    Hunger hunger = Buff.Affect<Hunger>(target);
                    if (hunger != null && !hunger.IsStarving())
                        hunger.Satisfy(5 * usedCharge);

                    th.charge -= usedCharge;

                    ++target.invisible;

                    UpdateQuickslot();

                    Dungeon.Observe();

                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override bool Act()
            {
                Detach();
                return true;
            }

            public override void Detach()
            {
                if (target.invisible > 0)
                    --target.invisible;
                base.Detach();
                artifact.activeBuff = null;
                Dungeon.Observe();
            }

            public override void Fx(bool on)
            {
                if (on)
                    target.sprite.Add(CharSprite.State.INVISIBLE);
                else if (target.invisible == 0)
                    target.sprite.Remove(CharSprite.State.INVISIBLE);
            }
        }

        public class TimeFreeze : ArtifactBuff
        {
            public TimeFreeze(Artifact artifact)
                : base(artifact)
            {
                type = BuffType.POSITIVE;
            }

            float turnsToCost;

            List<int> presses = new List<int>();

            public void ProcessTime(float time)
            {
                turnsToCost -= time;

                while (turnsToCost < 0f)
                {
                    turnsToCost += 2f;
                    --artifact.charge;
                }

                UpdateQuickslot();

                if (artifact.charge < 0)
                {
                    artifact.charge = 0;
                    Detach();
                }
            }

            public void SetDelayedPress(int cell)
            {
                if (!presses.Contains(cell))
                    presses.Add(cell);
            }

            private void TriggerPresses()
            {
                foreach (int cell in presses)
                    Dungeon.level.PressCell(cell);

                presses = new List<int>();
            }

            public override void Detach()
            {
                UpdateQuickslot();
                base.Detach();
                artifact.activeBuff = null;
                TriggerPresses();
                target.Next();
            }

            public override void Fx(bool on)
            {
                watabou.noosa.particles.Emitter.freezeEmitters = on;
                if (on)
                {
                    foreach (var mob in Dungeon.level.mobs.ToArray())
                    {
                        if (mob.sprite != null)
                            mob.sprite.Add(CharSprite.State.PARALYSED);
                    }
                }
                else
                {
                    foreach (var mob in Dungeon.level.mobs.ToArray())
                    {
                        if (mob.paralysed <= 0)
                            mob.sprite.Remove(CharSprite.State.PARALYSED);
                    }
                }
            }

            public const string PRESSES = "presses";
            public const string TURNSTOCOST = "turnsToCost";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);

                int[] values = new int[presses.Count];
                for (int i = 0; i < values.Length; ++i)
                    values[i] = presses[i];
                bundle.Put(PRESSES, values);

                bundle.Put(TURNSTOCOST, turnsToCost);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);

                int[] values = bundle.GetIntArray(PRESSES);
                foreach (int value in values)
                    presses.Add(value);

                turnsToCost = bundle.GetFloat(TURNSTOCOST);
            }
        }

        [SPDStatic]
        public class SandBag : Item
        {
            public SandBag()
            {
                image = ItemSpriteSheet.SANDBAG;
            }

            public override bool DoPickUp(Hero hero)
            {
                var hourglass = hero.belongings.GetItem<TimekeepersHourglass>();
                if (hourglass != null && !hourglass.cursed)
                {
                    hourglass.Upgrade();
                    Sample.Instance.Play(Assets.Sounds.DEWDROP);
                    if (hourglass.GetLevel() == hourglass.levelCap)
                        GLog.Positive(Messages.Get(this, "maxlevel"));
                    else
                        GLog.Information(Messages.Get(this, "levelup"));
                    hero.SpendAndNext(TIME_TO_PICK_UP);
                    return true;
                }
                else
                {
                    GLog.Warning(Messages.Get(this, "no_hourglass"));
                    return false;
                }
            }

            public override int Value()
            {
                return 20;
            }
        }
    }
}