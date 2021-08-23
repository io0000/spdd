using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.particles;
using spdd.actors;
using spdd.effects;
using spdd.effects.particles;
using spdd.items.wands;

namespace spdd.sprites
{
    public class LotusSprite : MobSprite
    {
        private List<Emitter> grassVfx;

        public LotusSprite()
        {
            perspectiveRaise = 0f;

            Texture(Assets.Sprites.LOTUS);

            var frames = new TextureFilm(texture, 19, 16);

            idle = new Animation(1, true);
            idle.Frames(frames, 0);

            run = new Animation(1, true);
            run.Frames(frames, 0);

            attack = new Animation(1, false);
            attack.Frames(frames, 0);

            die = new Animation(1, false);
            die.Frames(frames, 0);

            Play(idle);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);

            renderShadow = false;

            if (grassVfx == null && ch is WandOfRegrowth.Lotus)
            {
                var l = (WandOfRegrowth.Lotus)ch;
                grassVfx = new List<Emitter>();

                for (int i = 0; i < Dungeon.level.Length(); ++i)
                {
                    if (!Dungeon.level.solid[i] && l.InRange(i))
                    {
                        Emitter e = CellEmitter.Get(i);
                        e.Pour(LeafParticle.LevelSpecific, 0.5f);
                        grassVfx.Add(e);
                    }
                }
            }
        }

        public override void Place(int cell)
        {
            if (parent != null)
                parent.SendToBack(this);
            base.Place(cell);
        }

        public override void TurnTo(int from, int to)
        {
            //do nothing
        }

        public override void Update()
        {
            visible = true;
            base.Update();
        }

        public override void Die()
        {
            base.Die();

            if (grassVfx != null)
            {
                foreach (Emitter e in grassVfx)
                {
                    e.on = false;
                }
                grassVfx = null;
            }
        }

        public override void Kill()
        {
            base.Kill();

            if (grassVfx != null)
            {
                foreach (Emitter e in grassVfx)
                {
                    e.on = false;
                }
                grassVfx = null;
            }
        }
    }
}