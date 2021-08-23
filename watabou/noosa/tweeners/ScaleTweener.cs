using watabou.utils;

namespace watabou.noosa.tweeners
{
    public class ScaleTweener : Tweener
    {
        protected Visual visual;

        protected PointF start;
        protected PointF end;

        public ScaleTweener(Visual visual, PointF scale, float time)
            : base(visual, time)
        {
            this.visual = visual;
            start = visual.scale;
            end = scale;
        }

        public override void UpdateValues(float progress)
        {
            visual.scale = PointF.Inter(start, end, progress);
        }
    }
}