using watabou.utils;
using spdd.levels.painters;
using spdd.items.food;
using spdd.plants;
using spdd.actors.buffs;

namespace spdd.levels.rooms.secret
{
    public class SecretLarderRoom : SecretRoom
    {
        public override int MinHeight()
        {
            return 6;
        }

        public override int MinWidth()
        {
            return 6;
        }

        public override void Paint(Level level)
        {
            Painter.Fill(level, this, Terrain.WALL);
            Painter.Fill(level, this, 1, Terrain.EMPTY_SP);

            Point c = Center();

            Painter.Fill(level, c.x - 1, c.y - 1, 3, 3, Terrain.WATER);
            Painter.Set(level, c, Terrain.GRASS);

            if (!Dungeon.IsChallenged(Challenges.NO_FOOD))
            {
                level.Plant(new BlandfruitBush.Seed(), level.PointToCell(c));
            }

            int extraFood = (int)(Hunger.STARVING - Hunger.HUNGRY) * (1 + Dungeon.depth / 5);

            while (extraFood > 0)
            {
                Food food;
                if (extraFood >= Hunger.STARVING)
                {
                    food = new Pasty();
                    extraFood -= (int)Hunger.STARVING;
                }
                else
                {
                    food = new ChargrilledMeat();
                    extraFood -= (int)(Hunger.STARVING - Hunger.HUNGRY);
                }
                int foodPos;
                do
                {
                    foodPos = level.PointToCell(Random());
                }
                while (level.map[foodPos] != Terrain.EMPTY_SP || level.heaps[foodPos] != null);

                level.Drop(food, foodPos);
            }

            Entrance().Set(Door.Type.HIDDEN);
        }
    }
}