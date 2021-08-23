namespace watabou.noosa.tweeners
{
    public class AlphaTweener : Tweener
    {
        public Visual image;

        protected float start;
        protected float delta;

        public AlphaTweener(Visual image, float alpha, float time)
            : base(image, time)
        {
            this.image = image;
            start = image.Alpha();
            delta = alpha - start;
        }

        public override void UpdateValues(float progress)
        {
            image.Alpha(start + delta * progress);
        }
    }
}