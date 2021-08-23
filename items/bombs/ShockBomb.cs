using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.mechanics;
using spdd.effects;
using spdd.effects.particles;
using spdd.tiles;
using spdd.utils;

namespace spdd.items.bombs
{
    public class ShockBomb : Bomb
    {
        public ShockBomb()
        {
            image = ItemSpriteSheet.SHOCK_BOMB;
        }

        public override void Explode(int cell)
        {
            base.Explode(cell);

            List<Character> affected = new List<Character>();
            PathFinder.BuildDistanceMap(cell, BArray.Not(Dungeon.level.solid, null), 3);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue &&
                    Actor.FindChar(i) != null)
                {
                    affected.Add(Actor.FindChar(i));
                }
            }

            foreach (var ch in affected.ToArray())
            {
                Ballistic LOS = new Ballistic(cell, ch.pos, Ballistic.PROJECTILE);
                if (LOS.collisionPos != ch.pos)
                {
                    affected.Remove(ch);
                }
            }

            var arcs = new List<Lightning.Arc>();
            foreach (var ch in affected)
            {
                int power = 16 - 4 * Dungeon.level.Distance(ch.pos, cell);
                if (power > 0)
                {
                    //32% to 8% regular bomb damage
                    int damage = (int)Math.Round(Rnd.NormalIntRange(5 + Dungeon.depth, 10 + 2 * Dungeon.depth) * (power / 50f), MidpointRounding.AwayFromZero);
                    ch.Damage(damage, this);
                    if (ch.IsAlive())
                        Buff.Prolong<Paralysis>(ch, power);
                    arcs.Add(new Lightning.Arc(DungeonTilemap.TileCenterToWorld(cell), ch.sprite.Center()));
                }
            }

            CellEmitter.Center(cell).Burst(SparkParticle.Factory, 20);
            Dungeon.hero.sprite.parent.AddToFront(new Lightning(arcs, null));
            Sample.Instance.Play(Assets.Sounds.LIGHTNING);
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (20 + 30);
        }
    }
}