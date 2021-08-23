using System;
using watabou.noosa;
using spdd.ui;
using spdd.messages;
using spdd.items.armor.glyphs;

namespace spdd.actors.buffs
{
    public class MagicImmune : FlavourBuff
    {
        public const float DURATION = 20f;

        public MagicImmune()
        {
            type = BuffType.POSITIVE;
            announced = true;

            immunities.UnionWith(AntiMagic.RESISTS);
        }

        //FIXME what about active buffs/debuffs?, what about rings? what about artifacts?

        public override int Icon()
        {
            return BuffIndicator.COMBO;
        }

        public override void TintIcon(Image icon)
        {
            icon.Hardlight(0.0f, 1.0f, 0.0f);
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns());
        }
    }
}