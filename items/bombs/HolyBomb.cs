using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors;
using spdd.sprites;
using spdd.effects;
using spdd.effects.particles;
using spdd.tiles;
using spdd.utils;

namespace spdd.items.bombs
{
    public class HolyBomb : Bomb
    {
        public HolyBomb()
        {
            image = ItemSpriteSheet.HOLY_BOMB;
        }

        public override void Explode(int cell)
        {
            base.Explode(cell);

            if (Dungeon.level.heroFOV[cell])
            {
                new Flare(10, 64).Show(Dungeon.hero.sprite.parent, DungeonTilemap.TileCenterToWorld(cell), 2f);
            }

            List<Character> affected = new List<Character>();

            PathFinder.BuildDistanceMap(cell, BArray.Not(Dungeon.level.solid, null), 2);
            for (int i = 0; i < PathFinder.distance.Length; ++i)
            {
                if (PathFinder.distance[i] < int.MaxValue)
                {
                    var ch = Actor.FindChar(i);
                    if (ch != null)
                        affected.Add(ch);
                }
            }

            foreach (var ch in affected)
            {
                if (ch.Properties().Contains(Character.Property.UNDEAD) || ch.Properties().Contains(Character.Property.DEMONIC))
                {
                    ch.sprite.Emitter().Start(ShadowParticle.Up, 0.05f, 10);

                    //bomb deals an additional 67% damage to unholy enemies in a 5x5 range
                    int damage = (int)Math.Round(Rnd.NormalIntRange(Dungeon.depth + 5, 10 + Dungeon.depth * 2) * 0.67f, MidpointRounding.AwayFromZero);
                    ch.Damage(damage, this);
                }
            }

            Sample.Instance.Play(Assets.Sounds.READ);
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (20 + 30);
        }
    }
}