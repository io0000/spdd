using watabou.input;
using watabou.noosa;
using watabou.utils;
using spdd.scenes;

namespace spdd.ui
{
    public class ActionIndicator : Tag
    {
        Image icon;

        public static IAction action;
        public static ActionIndicator instance;

        public ActionIndicator()
            : base(new Color(0xFF, 0xFF, 0x4C, 0xFF))
        {
            instance = this;

            SetSize(24, 24);
            visible = false;
        }

        public override GameAction KeyAction()
        {
            return SPDAction.TAG_ACTION;
        }

        public override void Destroy()
        {
            base.Destroy();
            instance = null;
        }

        protected override void Layout()
        {
            base.Layout();

            if (icon != null)
            {
                icon.x = x + (width - icon.Width()) / 2;
                icon.y = y + (height - icon.Height()) / 2;
                PixelScene.Align(icon);
                if (!members.Contains(icon))
                    Add(icon);
            }
        }

        private bool needsLayout;

        public override void Update()
        {
            base.Update();

            if (!Dungeon.hero.ready)
            {
                if (icon != null)
                    icon.Alpha(0.5f);
            }
            else
            {
                if (icon != null)
                    icon.Alpha(1f);
            }

            if (!visible && action != null)
            {
                visible = true;
                UpdateIcon();
                Flash();
            }
            else
            {
                visible = action != null;
            }

            if (needsLayout)
            {
                Layout();
                needsLayout = false;
            }
        }

        protected override void OnClick()
        {
            if (action != null && Dungeon.hero.ready)
                action.DoAction();
        }

        public static void SetAction(IAction action)
        {
            ActionIndicator.action = action;
            UpdateIcon();
        }

        public static void ClearAction(IAction action)
        {
            if (ActionIndicator.action == action)
                ActionIndicator.action = null;
        }

        public static void UpdateIcon()
        {
            if (instance != null)
            {
                //synchronized(instance) 
                {
                    if (instance.icon != null)
                    {
                        instance.icon.KillAndErase();
                        instance.icon = null;
                    }
                    if (action != null)
                    {
                        instance.icon = action.GetIcon();
                        instance.needsLayout = true;
                    }
                }
            }
        }

        //public interface Action
        public interface IAction
        {
            Image GetIcon();

            void DoAction();
        }
    }
}