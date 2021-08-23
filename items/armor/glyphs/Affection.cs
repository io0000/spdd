using System;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.sprites;

namespace spdd.items.armor.glyphs
{
    public class Affection : Armor.Glyph
    {
        private static readonly ItemSprite.Glowing PINK = new ItemSprite.Glowing(new Color(0xFF, 0x44, 0x88, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            int level = Math.Max(0, armor.BuffedLvl());

            // lvl 0 - 15%
            // lvl 1 ~ 19%
            // lvl 2 ~ 23%
            if (Rnd.Int(level + 20) >= 17)
            {
                Buff.Affect<Charm>(attacker, Charm.DURATION).obj = defender.Id();
                attacker.sprite.CenterEmitter().Start(Speck.Factory(Speck.HEART), 0.2f, 5);
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return PINK;
        }
    }
}