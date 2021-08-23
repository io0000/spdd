using System.Collections.Generic;
using watabou.utils;
using spdd.actors;
using spdd.items.scrolls;
using spdd.sprites;

namespace spdd.items.weapon.missiles.darts
{
    public class DisplacingDart : TippedDart
    {
        public DisplacingDart()
        {
            image = ItemSpriteSheet.DISPLACING_DART;
        }

        const int distance = 8;

        public override int Proc(Character attacker, Character defender, int damage)
        {
            if (!defender.Properties().Contains(Character.Property.IMMOVABLE))
            {
                int startDist = Dungeon.level.Distance(attacker.pos, defender.pos);

                HashMap<int, List<int>> positions = new HashMap<int, List<int>>();

                for (int pos = 0; pos < Dungeon.level.Length(); ++pos)
                {
                    if (Dungeon.level.heroFOV[pos] &&
                        Dungeon.level.passable[pos] &&
                        (!Character.HasProp(defender, Character.Property.LARGE) || Dungeon.level.openSpace[pos]) &&
                        Actor.FindChar(pos) == null)
                    {
                        int dist = Dungeon.level.Distance(attacker.pos, pos);
                        if (dist > startDist)
                        {
                            if (positions.Get(dist) == null)
                                positions.Add(dist, new List<int>());

                            positions.Get(dist).Add(pos);
                        }
                    }
                }

                float[] probs = new float[distance + 1];

                for (int i = 0; i <= distance; ++i)
                {
                    if (positions.Get(i) != null)
                        probs[i] = i - startDist;
                }

                int chosenDist = Rnd.Chances(probs);
                if (chosenDist != -1)
                {
                    var list = positions.Get(chosenDist);
                    int pos = list[Rnd.Index(list)];
                    ScrollOfTeleportation.Appear(defender, pos);
                    Dungeon.level.OccupyCell(defender);
                }
            }

            return base.Proc(attacker, defender, damage);
        }
    }
}