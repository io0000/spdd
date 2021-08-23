using watabou.noosa.audio;
using watabou.utils;
using spdd.sprites;
using spdd.scenes;
using spdd.journal;
using spdd.windows;
using spdd.actors.hero;

namespace spdd.items.journal
{
    public abstract class DocumentPage : Item
    {
        public DocumentPage()
        {
            image = ItemSpriteSheet.MASTERY;
        }

        // 	public abstract Document document();
        public abstract Document GetDocument();

        private string page;

        public void Page(string page)
        {
            this.page = page;
        }

        public string Page()
        {
            return page;
        }

        public override bool DoPickUp(Hero hero)
        {
            GameScene.PickUpJournal(this, hero.pos);
            GameScene.FlashJournal();
            if (GetDocument() == Document.ALCHEMY_GUIDE)
                WndJournal.last_index = 1;
            else
                WndJournal.last_index = 0;

            GetDocument().AddPage(page);
            Sample.Instance.Play(Assets.Sounds.ITEM);
            hero.SpendAndNext(TIME_TO_PICK_UP);
            return true;
        }

        private const string PAGE = "page";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(PAGE, Page());
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            page = bundle.GetString(PAGE);
        }
    }
}