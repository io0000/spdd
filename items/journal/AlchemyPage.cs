using spdd.journal;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.journal
{
    public class AlchemyPage : DocumentPage
    {
        public AlchemyPage()
        {
            image = ItemSpriteSheet.ALCH_PAGE;
        }

        public override Document GetDocument()
        {
            return Document.ALCHEMY_GUIDE;
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", GetDocument().PageTitle(Page()));
        }
    }
}
