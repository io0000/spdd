using System.Collections.Generic;
using watabou.noosa;
using spdd.actors;
using spdd.scenes;

namespace spdd.effects
{
    public class SpellSprite : Image
    {
        public const int FOOD = 0;
        public const int MAP = 1;
        public const int CHARGE = 2;
        public const int MASTERY = 3;
        public const int BERSERK = 4;

        private const int SIZE = 16;

        private enum Phase
        {
            FADE_IN,
            STATIC,
            FADE_OUT
        }

        private const float FADE_IN_TIME = 0.2f;
        private const float STATIC_TIME = 0.8f;
        private const float FADE_OUT_TIME = 0.4f;

        private static TextureFilm film;

        private Character target;

        private Phase phase;
        private float duration;
        private float passed;

        private static Dictionary<Character, SpellSprite> all = new Dictionary<Character, SpellSprite>();

        public SpellSprite()
            : base(Assets.Effects.SPELL_ICONS)
        {
            if (film == null)
                film = new TextureFilm(texture, SIZE);
        }

        public void Reset(int index)
        {
            Frame(film.Get(index));
            origin.Set(width / 2, height / 2);

            phase = Phase.FADE_IN;

            duration = FADE_IN_TIME;
            passed = 0;
        }

        public override void Update()
        {
            base.Update();

            if (target.sprite != null)
            {
                x = target.sprite.Center().x - SIZE / 2;
                y = target.sprite.y - SIZE;
            }

            switch (phase)
            {
                case Phase.FADE_IN:
                    Alpha(passed / duration);
                    scale.Set(passed / duration);
                    break;
                case Phase.STATIC:
                    break;
                case Phase.FADE_OUT:
                    Alpha(1 - passed / duration);
                    break;
            }

            if ((passed += Game.elapsed) > duration)
            {
                switch (phase)
                {
                    case Phase.FADE_IN:
                        phase = Phase.STATIC;
                        duration = STATIC_TIME;
                        break;
                    case Phase.STATIC:
                        phase = Phase.FADE_OUT;
                        duration = FADE_OUT_TIME;
                        break;
                    case Phase.FADE_OUT:
                        Kill();
                        break;
                }

                passed = 0;
            }
        }

        public override void Kill()
        {
            base.Kill();
            all.Remove(target);
        }

        public static void Show(Character ch, int index)
        {
            if (!ch.sprite.visible)
                return;

            SpellSprite old;
            if (all.TryGetValue(ch, out old) == true)
                old.Kill();

            var sprite = GameScene.SpellSprite();
            sprite.Revive();
            sprite.Reset(index);
            sprite.target = ch;
            all.Add(ch, sprite);
        }
    }
}