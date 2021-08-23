using System;
using spdd.ui;
using spdd.plants;
using spdd.sprites;
using spdd.items.artifacts;
using spdd.actors.hero;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Invisibility : FlavourBuff
    {
        public const float DURATION = 20f;

        public Invisibility()
        {
            type = BuffType.POSITIVE;
            announced = true;
        }

        public override bool AttachTo(Character target)
        {
            if (!base.AttachTo(target))
                return false;

            ++target.invisible;

            if (target is Hero && ((Hero)target).subClass == HeroSubClass.ASSASSIN)
            {
                Buff.Affect<Preparation>(target);
            }
            return true;
        }

        public override void Detach()
        {
            if (target.invisible > 0)
                --target.invisible;
            base.Detach();
        }

        public override int Icon()
        {
            return BuffIndicator.INVISIBLE;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
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
            return Messages.Get(this, "desc", DispTurns());
        }

        public static void Dispel()
        {
            foreach (var invis in Dungeon.hero.Buffs<Invisibility>())
            {
                invis.Detach();
            }

            var cloakBuff = Dungeon.hero.FindBuff<CloakOfShadows.CloakStealth>();
            if (cloakBuff != null)
                cloakBuff.Dispel();

            //these aren't forms of invisibilty, but do dispel at the same time as it.
            var timeFreeze = Dungeon.hero.FindBuff<TimekeepersHourglass.TimeFreeze>();
            if (timeFreeze != null)
                timeFreeze.Detach();

            var prep = Dungeon.hero.FindBuff<Preparation>();
            if (prep != null)
                prep.Detach();

            var bubble = Dungeon.hero.FindBuff<Swiftthistle.TimeBubble>();
            if (bubble != null)
                bubble.Detach();
        }
    }
}