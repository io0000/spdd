using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.scenes;
using spdd.windows;
using spdd.journal;

namespace spdd.items.keys
{
    // abstract class Key extends Item 
    public class Key : Item
    {
        public const float TIME_TO_UNLOCK = 1f;

        public Key()
        {
            stackable = true;
            unique = true;
        }

        public int depth;

        public override bool IsSimilar(Item item)
        {
            return base.IsSimilar(item) && ((Key)item).depth == depth;
        }

        public override bool DoPickUp(Hero hero)
        {
            GameScene.PickUpJournal(this, hero.pos);
            WndJournal.last_index = 2;
            Notes.Add(this);
            Sample.Instance.Play(Assets.Sounds.ITEM);
            hero.SpendAndNext(TIME_TO_PICK_UP);
            GameScene.UpdateKeyDisplay();
            return true;
        }

        private const string DEPTH = "depth";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(DEPTH, depth);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            depth = bundle.GetInt(DEPTH);
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }
    }
}