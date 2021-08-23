using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.sprites;

namespace spdd.items.weapon.curses
{
    public class Friendly : Weapon.Enchantment
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            if (Rnd.Int(10) == 0)
            {
                Buff.Affect<Charm>(attacker, Charm.DURATION).obj = defender.Id();
                attacker.sprite.CenterEmitter().Start(Speck.Factory(Speck.HEART), 0.2f, 5);

                Charm c = Buff.Affect<Charm>(defender, Charm.DURATION / 2);
                c.ignoreNextHit = true;
                c.obj = attacker.Id();
                defender.sprite.CenterEmitter().Start(Speck.Factory(Speck.HEART), 0.2f, 5);
            }

            return damage;
        }

        public override bool Curse()
        {
            return true;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return BLACK;
        }
    }
}