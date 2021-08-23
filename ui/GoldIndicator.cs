using watabou.utils;
using watabou.noosa;
using watabou.noosa.ui;
using spdd.scenes;

namespace spdd.ui
{
    public class GoldIndicator : Component
    {
        private const float TIME = 2f;

        private int lastValue;

        private BitmapText tf;

        private float time;

        protected override void CreateChildren()
        {
            tf = new BitmapText(PixelScene.pixelFont);
            tf.Hardlight(new Color(0xFF, 0xFF, 0x00, 0xFF));
            Add(tf);

            visible = false;
        }

        protected override void Layout()
        {
            tf.x = x + (width - tf.Width()) / 2;
            tf.y = Bottom() - tf.Height();
        }

        public override void Update()
        {
            base.Update();

            if (visible)
            {
                time -= Game.elapsed;
                if (time > 0)
                {
                    tf.Alpha(time > TIME / 2 ? 1f : time * 2 / TIME);
                }
                else
                {
                    visible = false;
                }
            }

            if (Dungeon.gold != lastValue)
            {
                lastValue = Dungeon.gold;

                tf.Text(lastValue.ToString());
                tf.Measure();

                visible = true;
                time = TIME;

                Layout();
            }
        }
    }
}