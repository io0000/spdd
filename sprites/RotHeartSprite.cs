using watabou.noosa;
using watabou.noosa.particles;
using spdd.actors;
using spdd.effects;

namespace spdd.sprites
{
    public class RotHeartSprite : MobSprite
    {
        private Emitter cloud;

        public RotHeartSprite()
        {
            perspectiveRaise = 0.2f;

            Texture(Assets.Sprites.ROT_HEART);

            var frames = new TextureFilm(texture, 16, 16);

            idle = new Animation(1, true);
            idle.Frames(frames, 0);

            run = new Animation(1, true);
            run.Frames(frames, 0);

            attack = new Animation(1, false);
            attack.Frames(frames, 0);

            die = new Animation(8, false);
            die.Frames(frames, 1, 2, 3, 4, 5, 6, 7, 7, 7);

            Play(idle);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);

            renderShadow = false;

            if (cloud == null)
            {
                cloud = Emitter();
                cloud.Pour(Speck.Factory(Speck.TOXIC), 0.7f);
            }
        }

        public override void TurnTo(int from, int to)
        {
            //do nothing
        }

        public override void Update()
        {
            base.Update();

            if (cloud != null)
            {
                cloud.visible = visible;
            }
        }

        public override void Die()
        {
            base.Die();

            if (cloud != null)
            {
                cloud.on = false;
            }
        }
    }
}