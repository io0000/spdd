using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.mechanics;
using spdd.actors;
using spdd.sprites;
using spdd.effects;
using spdd.effects.particles;

namespace spdd.items.bombs
{
    public class ShrapnelBomb : Bomb
    {
        public ShrapnelBomb()
        {
            image = ItemSpriteSheet.SHRAPNEL_BOMB;
        }

        public override bool ExplodesDestructively()
        {
            return false;
        }

        public override void Explode(int cell)
        {
            base.Explode(cell);

            var FOV = new bool[Dungeon.level.Length()];
            Point c = Dungeon.level.CellToPoint(cell);
            ShadowCaster.CastShadow(c.x, c.y, FOV, Dungeon.level.losBlocking, 8);

            List<Character> affected = new List<Character>();

            for (int i = 0; i < FOV.Length; ++i)
            {
                if (FOV[i])
                {
                    if (Dungeon.level.heroFOV[i] && !Dungeon.level.solid[i])
                    {
                        //TODO better vfx?
                        CellEmitter.Center(i).Burst(BlastParticle.Factory, 5);
                    }
                    var ch = Actor.FindChar(i);
                    if (ch != null)
                        affected.Add(ch);
                }
            }

            foreach (var ch in affected)
            {
                //regular bomb damage, which falls off at a rate of 5% per tile of distance
                int damage = Rnd.NormalIntRange(Dungeon.depth + 5, 10 + Dungeon.depth * 2);
                damage = (int)Math.Round(damage * (1f - .05f * Dungeon.level.Distance(cell, ch.pos)), MidpointRounding.AwayFromZero);
                damage -= ch.DrRoll();
                ch.Damage(damage, this);
                if (ch == Dungeon.hero && !ch.IsAlive())
                {
                    Dungeon.Fail(typeof(Bomb));
                }
            }
        }

        public override int Value()
        {
            //prices of ingredients
            return quantity * (20 + 100);
        }
    }
}