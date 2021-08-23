using watabou.input;
using watabou.noosa;
using watabou.utils;
using spdd.scenes;

namespace spdd.ui
{
    public class ResumeIndicator : Tag
    {
        private Image icon;

        public ResumeIndicator()
            : base(new Color(0xCD, 0xD5, 0xC0, 0xFF))
        {
            SetSize(24, 24);

            visible = false;
        }

        public override GameAction KeyAction()
        {
            return SPDAction.TAG_RESUME;
        }

        protected override void CreateChildren()
        {
            base.CreateChildren();

            icon = IconsExtensions.Get(Icons.ARROW);
            Add(icon);
        }

        protected override void Layout()
        {
            base.Layout();

            icon.x = x + 1 + (width - icon.width) / 2f;
            icon.y = y + (height - icon.height) / 2f;
            PixelScene.Align(icon);
        }

        protected override void OnClick()
        {
            Dungeon.hero.Resume();
        }

        public override void Update()
        {
            if (!Dungeon.hero.IsAlive())
            {
                visible = false;
            }
            else if (visible != (Dungeon.hero.lastAction != null))
            {
                visible = Dungeon.hero.lastAction != null;
                if (visible)
                    Flash();
            }
            base.Update();
        }
    }
}