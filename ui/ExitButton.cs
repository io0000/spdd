using watabou.noosa;
using spdd.scenes;

namespace spdd.ui
{
    public class ExitButton : IconButton
    {
        public ExitButton()
            : base(Icons.EXIT.Get())
        {
            width = 20;
            height = 20;
        }

        protected override void OnClick()
        {
            if (Game.Scene() is TitleScene)
                Game.instance.Finish();
            else
                ShatteredPixelDungeonDash.SwitchNoFade(typeof(TitleScene));
        }
    }
}