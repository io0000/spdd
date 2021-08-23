using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using watabou.noosa.tweeners;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.utils;
using spdd.ui;
using spdd.items.rings;
using spdd.messages;

namespace spdd.items.artifacts
{
    public class CloakOfShadows : Artifact
    {
        public CloakOfShadows()
        {
            image = ItemSpriteSheet.ARTIFACT_CLOAK;

            exp = 0;
            levelCap = 10;

            charge = Math.Min(GetLevel() + 3, 10);
            partialCharge = 0;
            chargeCap = Math.Min(GetLevel() + 3, 10);

            defaultAction = AC_STEALTH;

            unique = true;
            bones = false;
        }

        private bool stealthed;

        public const string AC_STEALTH = "STEALTH";

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            if (IsEquipped(hero) && !cursed && (charge > 0 || stealthed))
                actions.Add(AC_STEALTH);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_STEALTH))
            {
                if (!stealthed)
                {
                    if (!IsEquipped(hero))
                    {
                        GLog.Information(Messages.Get(typeof(Artifact), "need_to_equip"));
                    }
                    else if (cursed)
                    {
                        GLog.Information(Messages.Get(this, "cursed"));
                    }
                    else if (charge <= 0)
                    {
                        GLog.Information(Messages.Get(this, "no_charge"));
                    }
                    else
                    {
                        stealthed = true;
                        hero.Spend(1f);
                        hero.Busy();
                        Sample.Instance.Play(Assets.Sounds.MELD);
                        activeBuff = ActiveBuff();
                        activeBuff.AttachTo(hero);
                        if (hero.sprite.parent != null)
                            hero.sprite.parent.Add(new AlphaTweener(hero.sprite, 0.4f, 0.4f));
                        else
                            hero.sprite.Alpha(0.4f);
                        hero.sprite.Operate(hero.pos);
                    }
                }
                else
                {
                    stealthed = false;
                    activeBuff.Detach();
                    activeBuff = null;
                    hero.Spend(1f);
                    hero.sprite.Operate(hero.pos);
                }
            }
        }

        public override void Activate(Character ch)
        {
            base.Activate(ch);
            if (stealthed)
            {
                activeBuff = ActiveBuff();
                activeBuff.AttachTo(ch);
            }
        }

        public override bool DoUnequip(Hero hero, bool collect, bool single)
        {
            if (base.DoUnequip(hero, collect, single))
            {
                stealthed = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override ArtifactBuff PassiveBuff()
        {
            return new CloakRecharge(this);
        }

        protected override ArtifactBuff ActiveBuff()
        {
            return new CloakStealth(this);
        }

        public bool RechargeAct(CloakRecharge buff)
        {
            Character target = buff.target;

            if (charge < chargeCap)
            {
                var lockedFloor = target.FindBuff<LockedFloor>();
                if (!stealthed && (lockedFloor == null || lockedFloor.RegenOn()))
                {
                    float missing = (chargeCap - charge);
                    if (GetLevel() > 7)
                        missing += 5 * (GetLevel() - 7) / 3f;
                    float turnsToCharge = (45 - missing);
                    turnsToCharge /= RingOfEnergy.ArtifactChargeMultiplier(target);
                    partialCharge += (1f / turnsToCharge);
                }

                if (partialCharge >= 1)
                {
                    ++charge;
                    partialCharge -= 1;
                    if (charge == chargeCap)
                        partialCharge = 0;
                }
            }
            else
            {
                partialCharge = 0;
            }

            if (cooldown > 0)
                --cooldown;

            UpdateQuickslot();

            buff.Spend(Actor.TICK);

            return true;
        }

        public bool StealthAct(CloakStealth buff)
        {
            var target = buff.target;

            --buff.turnsToCost;

            if (buff.turnsToCost <= 0)
            {
                --charge;
                if (charge < 0)
                {
                    charge = 0;
                    buff.Detach();
                    GLog.Warning(Messages.Get(typeof(CloakStealth), "no_charge"));
                    ((Hero)target).Interrupt();
                }
                else
                {
                    //target hero level is 1 + 2*cloak level
                    int lvlDiffFromTarget = ((Hero)target).lvl - (1 + GetLevel() * 2);
                    //plus an extra one for each level after 6
                    if (GetLevel() >= 7)
                        lvlDiffFromTarget -= GetLevel() - 6;

                    if (lvlDiffFromTarget >= 0)
                        exp += (int)Math.Round(10f * Math.Pow(1.1f, lvlDiffFromTarget), MidpointRounding.AwayFromZero);
                    else
                        exp += (int)Math.Round(10f * Math.Pow(0.75f, -lvlDiffFromTarget), MidpointRounding.AwayFromZero);

                    if (exp >= (GetLevel() + 1) * 50 && GetLevel() < levelCap)
                    {
                        Upgrade();
                        exp -= GetLevel() * 50;
                        GLog.Positive(Messages.Get(typeof(CloakStealth), "levelup"));
                    }

                    buff.turnsToCost = 5;
                }
                Item.UpdateQuickslot();
            }

            buff.Spend(Actor.TICK);

            return true;
        }

        public class CloakRecharge : ArtifactBuff
        {
            public CloakRecharge(Artifact artifact)
                : base(artifact)
            { }

            public override bool Act()
            {
                var cs = (CloakOfShadows)artifact;
                return cs.RechargeAct(this);
            }
        }

        public class CloakStealth : ArtifactBuff
        {
            public CloakStealth(Artifact artifact)
                : base(artifact)
            {
                type = BuffType.POSITIVE;
            }

            public int turnsToCost;

            public override int Icon()
            {
                return BuffIndicator.INVISIBLE;
            }

            public override float IconFadePercent()
            {
                return (5f - turnsToCost) / 5f;
            }

            public override bool AttachTo(Character target)
            {
                if (base.AttachTo(target))
                {
                    ++target.invisible;
                    if (target is Hero && ((Hero)target).subClass == HeroSubClass.ASSASSIN)
                        Buff.Affect<Preparation>(target);

                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override bool Act()
            {
                var cs = (CloakOfShadows)artifact;
                return cs.StealthAct(this);
            }

            public void Dispel()
            {
                Item.UpdateQuickslot();
                Detach();
            }

            public override void Fx(bool on)
            {
                if (on)
                    target.sprite.Add(CharSprite.State.INVISIBLE);
                else if (target.invisible == 0)
                    target.sprite.Remove(CharSprite.State.INVISIBLE);
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc");
            }

            public override void Detach()
            {
                var cs = (CloakOfShadows)artifact;

                if (target.invisible > 0)
                    --target.invisible;
                cs.stealthed = false;

                Item.UpdateQuickslot();
                base.Detach();
            }

            private const string TURNSTOCOST = "turnsToCost";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);

                bundle.Put(TURNSTOCOST, turnsToCost);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);

                turnsToCost = bundle.GetInt(TURNSTOCOST);
            }
        }
    }
}