using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors;
using spdd.actors.blobs;
using spdd.sprites;
using spdd.scenes;
using spdd.effects;
using spdd.effects.particles;
using spdd.utils;

namespace spdd.items.bombs
{
    public class ArcaneBomb : Bomb
    {
        public ArcaneBomb()
        {
            image = ItemSpriteSheet.ARCANE_BOMB;
        }

        public override void OnThrow(int cell)
        {
            base.OnThrow(cell);
            if (fuse != null)
            {
                PathFinder.BuildDistanceMap(cell, BArray.Not(Dungeon.level.solid, null), 2);
                for (int i = 0; i < PathFinder.distance.Length; ++i)
                {
                    if (PathFinder.distance[i] < int.MaxValue)
                        GameScene.Add(Blob.Seed(i, 3, typeof(GooWarn)));
                }
            }
        }

        public override bool ExplodesDestructively()
        {
            return false;
        }

        public override void Explode(int cell)
        {
            base.Explode(cell);

            List<Character> affected = new List<Character>();

            PathFinder.BuildDistanceMap(cell, BArray.Not(Dungeon.level.solid, null), 2);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    if (Dungeon.level.heroFOV[i])
                        CellEmitter.Get(i).Burst(ElmoParticle.Factory, 10);

                    var ch = Actor.FindChar(i);
                    if (ch != null)
                        affected.Add(ch);
                }
            }

            foreach (var ch in affected)
            {
                // 100%/83%/67% bomb damage based on distance, but pierces armor.
                int damage = Rnd.NormalIntRange(Dungeon.depth + 5, 10 + Dungeon.depth * 2);
                float multiplier = 1f - (.16667f * Dungeon.level.Distance(cell, ch.pos));
                ch.Damage((int)Math.Round(damage * multiplier, MidpointRounding.AwayFromZero), this);
                if (ch == Dungeon.hero && !ch.IsAlive())
                {
                    Dungeon.Fail(typeof(Bomb));
                }
            }
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (20 + 50);
        }
    }
}