using watabou.utils;
using spdd.actors;
using spdd.actors.mobs;
using spdd.effects;
using spdd.sprites;

namespace spdd.items.weapon.curses
{
    public class Displacing : Weapon.Enchantment
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Weapon weapon, Character attacker, Character defender, int damage)
        {
            if (Rnd.Int(12) == 0 && !defender.Properties().Contains(Character.Property.IMMOVABLE))
            {
                int count = 10;
                int newPos;
                do
                {
                    newPos = Dungeon.level.RandomRespawnCell(defender);
                    if (count-- <= 0)
                        break;

                }
                while (newPos == -1);

                if (newPos != -1 && !Dungeon.BossLevel())
                {
                    if (Dungeon.level.heroFOV[defender.pos])
                    {
                        CellEmitter.Get(defender.pos).Start(Speck.Factory(Speck.LIGHT), 0.2f, 3);
                    }

                    defender.pos = newPos;
                    if (defender is Mob && ((Mob)defender).state == ((Mob)defender).HUNTING)
                    {
                        ((Mob)defender).state = ((Mob)defender).WANDERING;
                    }
                    defender.sprite.Place(defender.pos);
                    defender.sprite.visible = Dungeon.level.heroFOV[defender.pos];

                    return 0;
                }
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