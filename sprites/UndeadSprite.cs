using watabou.noosa;
using watabou.utils;
using spdd.effects;

namespace spdd.sprites
{
    public class UndeadSprite : MobSprite
    {
        public UndeadSprite()
        {
            Texture(Assets.Sprites.UNDEAD);

            var frames = new TextureFilm(texture, 12, 16);

            idle = new Animation(12, true);
            idle.Frames(frames, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3);

            run = new Animation(15, true);
            run.Frames(frames, 4, 5, 6, 7, 8, 9);

            attack = new Animation(15, false);
            attack.Frames(frames, 14, 15, 16);

            die = new Animation(12, false);
            die.Frames(frames, 10, 11, 12, 13);

            Play(idle);
        }

        public override void Die()
        {
            base.Die();

            if (Dungeon.level.heroFOV[ch.pos])
                Emitter().Burst(Speck.Factory(Speck.BONE), 3);
        }

        public override Color Blood()
        {
            return new Color(0xcc, 0xcc, 0xcc, 0xFF);
        }
    }
}