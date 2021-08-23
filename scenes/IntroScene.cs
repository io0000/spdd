using watabou.noosa;
using spdd.windows;
using spdd.messages;

namespace spdd.scenes
{
    public class IntroScene : PixelScene
    {
        public override void Create()
        {
            base.Create();

            var text = Messages.Get(this, "text");

            Add(new IntroSceneWndStory(text));

            FadeIn();
        }
    }

    public class IntroSceneWndStory : WndStory
    {
        public IntroSceneWndStory(string text)
            : base(text)
        { }

        public override void Hide()
        {
            base.Hide();
            Game.SwitchScene(typeof(InterlevelScene));
        }
    }
}