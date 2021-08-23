using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.effects;
using spdd.effects.particles;
using spdd.utils;

namespace spdd.items.stones
{
    public class StoneOfShock : Runestone
    {
        public StoneOfShock()
        {
            image = ItemSpriteSheet.STONE_SHOCK;
        }

        protected override void Activate(int cell)
        {
            Sample.Instance.Play(Assets.Sounds.LIGHTNING);

            List<Lightning.Arc> arcs = new List<Lightning.Arc>();
            int hits = 0;

            PathFinder.BuildDistanceMap(cell, BArray.Not(Dungeon.level.solid, null), 2);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    var n = Actor.FindChar(i);
                    if (n != null)
                    {
                        arcs.Add(new Lightning.Arc(cell, n.sprite.Center()));
                        Buff.Prolong<Paralysis>(n, 1f);
                        ++hits;
                    }
                }
            }

            CellEmitter.Center(cell).Burst(SparkParticle.Factory, 3);

            if (hits > 0)
            {
                curUser.sprite.parent.AddToFront(new Lightning(arcs, null));
                curUser.sprite.CenterEmitter().Burst(EnergyParticle.Factory, 10);
                Sample.Instance.Play(Assets.Sounds.LIGHTNING);

                curUser.belongings.Charge(1f + hits);
            }
        }
    }
}