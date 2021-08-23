using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.particles;
using spdd.actors;
using spdd.effects;

namespace spdd.sprites
{
    public class BlacksmithSprite : MobSprite
    {
        private Emitter emitter;

        public BlacksmithSprite()
        {
            Texture(Assets.Sprites.TROLL);

            var frames = new TextureFilm(texture, 13, 16);

            idle = new Animation(15, true);
            idle.Frames(frames, 0, 0, 0, 0, 0, 0, 0, 1, 2, 2, 2, 3);

            run = new Animation(20, true);
            run.Frames(frames, 0);

            die = new Animation(20, false);
            die.Frames(frames, 0);

            Play(idle);
        }

        public override void Link(Character ch)
        {
            base.Link(ch);

            emitter = new Emitter();
            emitter.autoKill = false;
            emitter.Pos(x + 7, y + 12);
            parent.Add(emitter);
        }

        public override void Update()
        {
            base.Update();

            if (emitter != null)
                emitter.visible = visible;
        }

        public override void OnComplete(Animation anim)
        {
            base.OnComplete(anim);

            //FIXME should figure out why onComplete is called constantly when an animation is paused
            if (visible && emitter != null && anim == idle && !paused)
            {
                emitter.Burst(Speck.Factory(Speck.FORGE), 3);
                var volume = 0.2f / (Dungeon.level.Distance(ch.pos, Dungeon.hero.pos));
                Sample.Instance.Play(Assets.Sounds.EVOKE, volume, volume, 0.8f);
            }
        }
    }
}