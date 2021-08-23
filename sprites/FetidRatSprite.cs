using watabou.noosa;
using watabou.noosa.particles;
using spdd.actors;
using spdd.effects;

namespace spdd.sprites
{
    public class FetidRatSprite : RatSprite
    {
        private Emitter cloud;

        public FetidRatSprite()
        {
            Texture(Assets.Sprites.RAT);

            var frames = new TextureFilm(texture, 16, 15);

            idle = new Animation(2, true);
            idle.Frames(frames, 32, 32, 32, 33);

            run = new Animation(10, true);
            run.Frames(frames, 38, 39, 40, 41, 42);

            attack = new Animation(15, false);
            attack.Frames(frames, 34, 35, 36, 37, 32);

            die = new Animation(10, false);
            die.Frames(frames, 43, 44, 45, 46);

            Play(idle);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);

            if (cloud == null)
            {
                cloud = Emitter();
                cloud.Pour(Speck.Factory(Speck.STENCH), 0.7f);
            }
        }

        public override void Update()
        {
            base.Update();

            if (cloud != null)
                cloud.visible = visible;
        }

        public override void Kill()
        {
            base.Kill();

            if (cloud != null)
                cloud.on = false;
        }
    }
}