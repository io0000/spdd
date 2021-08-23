using watabou.input;
using watabou.noosa;
using watabou.utils;
using spdd.actors.mobs;
using spdd.scenes;

namespace spdd.ui
{
    public class DangerIndicator : Tag
    {
        public static Color COLOR = new Color(0xFF, 0x4C, 0x4C, 0xFF);

        private BitmapText number;
        private Image icon;

        private int enemyIndex;

        private int lastNumber = -1;

        public DangerIndicator()
            : base(new Color(0xFF, 0x4C, 0x4C, 0xFF))
        {
            SetSize(24, 16);

            visible = false;
        }

        public override GameAction KeyAction()
        {
            return SPDAction.TAG_DANGER;
        }

        protected override void CreateChildren()
        {
            base.CreateChildren();

            number = new BitmapText(PixelScene.pixelFont);
            Add(number);

            icon = Icons.SKULL.Get();
            Add(icon);
        }

        protected override void Layout()
        {
            base.Layout();

            icon.x = Right() - 10;
            icon.y = y + (height - icon.height) / 2;

            PlaceNumber();
        }

        private void PlaceNumber()
        {
            number.x = Right() - 11 - number.Width();
            number.y = y + (height - number.BaseLine()) / 2f;
            PixelScene.Align(number);
        }

        public override void Update()
        {
            if (Dungeon.hero.IsAlive())
            {
                var v = Dungeon.hero.VisibleEnemies();
                if (v != lastNumber)
                {
                    lastNumber = v;
                    visible = lastNumber > 0;
                    if (visible)
                    {
                        number.Text(lastNumber.ToString());
                        number.Measure();
                        PlaceNumber();

                        Flash();
                    }
                }
            }
            else
            {
                visible = false;
            }

            base.Update();
        }

        protected override void OnClick()
        {
            if (Dungeon.hero.VisibleEnemies() > 0)
            {
                Mob target = Dungeon.hero.VisibleEnemy(++enemyIndex);

                QuickSlotButton.Target(target);
                if (Dungeon.hero.CanAttack(target))
                    AttackIndicator.Target(target);

                if (Dungeon.hero.curAction == null)
                    Camera.main.PanTo(target.sprite.Center(), 5f);
            }
        }
    }
}