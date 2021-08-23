using watabou.utils;
using watabou.noosa;
using watabou.glwrap;
using spdd.effects;
using spdd.effects.particles;

namespace spdd.sprites
{
    public class GhostSprite : MobSprite
    {
        public GhostSprite()
        {
            Texture(Assets.Sprites.GHOST);

            TextureFilm frames = new TextureFilm(texture, 14, 15);

            idle = new Animation(5, true);
            idle.Frames(frames, 0, 1);

            run = new Animation(10, true);
            run.Frames(frames, 0, 1);

            attack = new Animation(10, false);
            attack.Frames(frames, 0, 2, 3);

            die = new Animation(8, false);
            die.Frames(frames, 0, 4, 5, 6, 7);

            Play(idle);
        }

        public override void Draw()
        {
            Blending.SetLightMode();
            base.Draw();
            Blending.SetNormalMode();
        }

        public override void Die()
        {
            base.Die();
            Emitter().Start(ShaftParticle.Factory, 0.3f, 4);
            Emitter().Start(Speck.Factory(Speck.LIGHT), 0.2f, 3);
        }

        public override Color Blood()
        {
            var color = new Color(0xFF, 0xFF, 0xFF, 0xFF);
            return color;
        }
    }
}