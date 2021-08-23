using watabou.utils;

namespace watabou.noosa.tweeners
{
    public class PosTweener : Tweener
    {
        public Visual visual;

        public PointF start;
        public PointF end;

        public PosTweener(Visual visual, PointF pos, float time)
            : base(visual, time)
        {
            this.visual = visual;
            start = visual.Point();
            end = pos;
        }

        public override void UpdateValues(float progress)
        {
            visual.Point(PointF.Inter(start, end, progress));
        }
    }
}