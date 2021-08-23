using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.messages;
using spdd.sprites;
using spdd.utils;

namespace spdd.items.weapon.curses
{
    public class Annoying : Weapon.Enchantment
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            if (Rnd.Int(20) == 0)
            {
                foreach (var mob in Dungeon.level.mobs)
                    mob.Beckon(attacker.pos);

                attacker.sprite.CenterEmitter().Start(Speck.Factory(Speck.SCREAM), 0.3f, 3);
                Sample.Instance.Play(Assets.Sounds.MIMIC);
                Invisibility.Dispel();
                GLog.Negative(Messages.Get(this, "msg_" + (Rnd.Int(5) + 1)));
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