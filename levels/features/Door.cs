using watabou.noosa.audio;
using spdd.scenes;
using spdd.actors;

namespace spdd.levels.features
{
    public class Door
    {
        public static void Enter(int pos)
        {
            Level.Set(pos, Terrain.OPEN_DOOR);
            GameScene.UpdateMap(pos);

            if (Dungeon.level.heroFOV[pos])
            {
                Dungeon.Observe();
                Sample.Instance.Play(Assets.Sounds.OPEN);
            }
        }

        public static void Leave(int pos)
        {
            int chars = 0;

            foreach (var ch in Actor.Chars())
            {
                if (ch.pos == pos)
                    ++chars;
            }

            //door does not shut if anything else is also on it
            if (Dungeon.level.heaps[pos] == null && chars <= 1)
            {
                Level.Set(pos, Terrain.DOOR);
                GameScene.UpdateMap(pos);
                if (Dungeon.level.heroFOV[pos])
                    Dungeon.Observe();
            }
        }
    }
}