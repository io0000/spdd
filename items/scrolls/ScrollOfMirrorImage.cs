using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs.npcs;
using spdd.scenes;
using spdd.sprites;

namespace spdd.items.scrolls
{
    public class ScrollOfMirrorImage : Scroll
    {
        public ScrollOfMirrorImage()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_MIRRORIMG;
        }

        private const int NIMAGES = 2;

        public override void DoRead()
        {
            int spawnedImages = SpawnImages(curUser, NIMAGES);

            if (spawnedImages > 0)
                SetKnown();

            Sample.Instance.Play(Assets.Sounds.READ);

            ReadAnimation();
        }


        //returns the number of images spawned
        public static int SpawnImages(Hero hero, int nImages)
        {
            var respawnPoints = new List<int>();

            for (int i = 0; i < PathFinder.NEIGHBORS8.Length; ++i)
            {
                int p = hero.pos + PathFinder.NEIGHBORS8[i];
                if (Actor.FindChar(p) == null && Dungeon.level.passable[p])
                {
                    respawnPoints.Add(p);
                }
            }

            int spawned = 0;
            while (nImages > 0 && respawnPoints.Count > 0)
            {
                int index = Rnd.Index(respawnPoints);

                MirrorImage mob = new MirrorImage();
                mob.Duplicate(hero);
                GameScene.Add(mob);
                ScrollOfTeleportation.Appear(mob, respawnPoints[index]);

                respawnPoints.RemoveAt(index);
                --nImages;
                ++spawned;
            }

            return spawned;
        }

        // DelayedImageSpawner <- 사용하지 않음
    }
}