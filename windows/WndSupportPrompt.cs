using watabou.utils;
using spdd.scenes;
using spdd.ui;
using spdd.messages;

namespace spdd.windows
{
    //public class WndSupportPrompt : Window
    //{
    //    protected const int WIDTH_P = 120;
    //    protected const int WIDTH_L = 200;
    //
    //    public WndSupportPrompt()
    //    {
    //        int width = PixelScene.Landscape() ? WIDTH_L : WIDTH_P;
    //
    //        IconTitle title = new IconTitle(Icons.SHPX.Get(), Messages.Get(typeof(WndSupportPrompt), "title"));
    //        title.SetRect(0, 0, width, 0);
    //        Add(title);
    //
    //        string message = Messages.Get(typeof(WndSupportPrompt), "intro");
    //        message += "\n\n" + Messages.Get(typeof(SupporterScene), "patreon_msg");
    //        if (Messages.Lang() != Languages.ENGLISH)
    //        {
    //            message += "\n" + Messages.Get(typeof(SupporterScene), "patreon_english");
    //        }
    //        message += "\n- Evan";
    //
    //        RenderedTextBlock text = PixelScene.RenderTextBlock(6);
    //        text.Text(message, width);
    //        text.SetPos(title.Left(), title.Bottom() + 4);
    //        Add(text);
    //
    //        var link = new ActionRedButton(Messages.Get(typeof(SupporterScene), "supporter_link"));
    //        link.action = () =>
    //        {
    //            string linkUrl = "https://www.patreon.com/ShatteredPixel";
    //            //tracking codes, so that the website knows where this pageview came from
    //            linkUrl += "?utm_source=shatteredpd";
    //            linkUrl += "&utm_medium=supporter_prompt";
    //            linkUrl += "&utm_campaign=ingame_link";
    //            DeviceCompat.OpenURI(linkUrl);
    //            SPDSettings.SupportNagged(true);
    //            CallBaseHide();
    //        };
    //        link.SetRect(0, text.Bottom() + 4, width, 18);
    //        Add(link);
    //
    //        var close = new ActionRedButton(Messages.Get(this, "close"));
    //        close.action = () =>
    //        {
    //            SPDSettings.SupportNagged(true);
    //            CallBaseHide();
    //        };
    //        close.SetRect(0, link.Bottom() + 2, width, 18);
    //        Add(close);
    //
    //        Resize(width, (int)close.Bottom());
    //    }
    //
    //    public override void Hide()
    //    {
    //        //do nothing, have to close via the close button
    //    }
    //
    //    public void CallBaseHide()
    //    {
    //        base.Hide();
    //    }
    //}
}