using watabou.utils;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.stones
{
    public abstract class Runestone : Item
    {
        public Runestone()
        {
            stackable = true;
            defaultAction = AC_THROW;
        }

        public override void OnThrow(int cell)
        {
            if (Dungeon.level.pit[cell] || !defaultAction.Equals(AC_THROW))
            {
                base.OnThrow(cell);
            }
            else
            {
                Activate(cell);
                Invisibility.Dispel();
            }
        }

        protected abstract void Activate(int cell);

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override int Value()
        {
            return 10 * quantity;
        }

        [SPDStatic]
        public class PlaceHolder : Runestone
        {
            public PlaceHolder()
            {
                image = ItemSpriteSheet.STONE_HOLDER;
            }

            protected override void Activate(int cell)
            {
                //does nothing
            }

            public override bool IsSimilar(Item item)
            {
                return item is Runestone;
            }

            public override string Info()
            {
                return "";
            }
        }
    }
}