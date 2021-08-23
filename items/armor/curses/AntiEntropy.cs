using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.effects.particles;
using spdd.sprites;

namespace spdd.items.armor.curses
{
    public class AntiEntropy : Armor.Glyph
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            if (Rnd.Int(8) == 0)
            {
                if (Dungeon.level.Adjacent(attacker.pos, defender.pos))
                {
                    Buff.Prolong<Frost>(attacker, Frost.DURATION);
                    CellEmitter.Get(attacker.pos).Start(SnowParticle.Factory, 0.2f, 6);
                }

                Buff.Affect<Burning>(defender).Reignite(defender);
                defender.sprite.Emitter().Burst(FlameParticle.Factory, 5);

            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return BLACK;
        }

        public override bool Curse()
        {
            return true;
        }
    }
}