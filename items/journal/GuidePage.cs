using spdd.journal;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.journal
{
    public class GuidePage : DocumentPage
    {
        public GuidePage()
        {
            image = ItemSpriteSheet.GUIDE_PAGE;
        }

        public override Document GetDocument()
        {
            return Document.ADVENTURERS_GUIDE;
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", GetDocument().PageTitle(Page()));
        }
    }
}