using watabou.noosa;
using watabou.noosa.tweeners;
using watabou.utils;
using spdd.actors.mobs;
using spdd.tiles;

namespace spdd.sprites
{
    public class MobSprite : CharSprite
    {
        private const float FADE_TIME = 3.0f;
        private const float FALL_TIME = 1.0f;

        public override void Update()
        {
            sleeping = ch != null && ((Mob)ch).state == ((Mob)ch).SLEEPING;
            base.Update();
        }

        public override void OnComplete(Animation anim)
        {
            base.OnComplete(anim);

            if (anim != die)
                return;

            var tweener = new MobAlphaTweener(this, 0, FADE_TIME);
            parent.Add(tweener);
        }

        public void Fall()
        {
            origin.Set(width / 2, height - DungeonTilemap.SIZE / 2);
            angularSpeed = Rnd.Int(2) == 0 ? -720 : 720;
            am = 1.0f;

            HideEmo();

            if (health != null)
                health.KillAndErase();

            var tweener = new MobScaleTweener(this, new PointF(0, 0), FALL_TIME);
            parent.Add(tweener);
        }
    }

    public class MobAlphaTweener : AlphaTweener
    {
        public MobAlphaTweener(Visual image, float alpha, float time)
            : base(image, alpha, time)
        { }

        public override void OnComplete()
        {
            target.KillAndErase();  // Target : MobSprite
        }
    }

    public class MobScaleTweener : ScaleTweener
    {
        public MobScaleTweener(Visual visual, PointF scale, float time)
            : base(visual, scale, time)
        { }

        public override void OnComplete()
        {
            target.KillAndErase();     // Target : MobSprite
            parent.Erase(this);        // this -> MobScaleTweener
        }

        public override void UpdateValues(float progress)
        {
            base.UpdateValues(progress);

            Visual visual = (Visual)target;

            visual.y += 12 * Game.elapsed;
            visual.am = 1.0f - progress;
        }
    }
}