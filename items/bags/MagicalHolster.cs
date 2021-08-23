using spdd.items.wands;
using spdd.items.weapon.missiles;
using spdd.items.bombs;
using spdd.sprites;

namespace spdd.items.bags
{
    public class MagicalHolster : Bag
    {
        public MagicalHolster()
        {
            image = ItemSpriteSheet.HOLSTER;
        }

        public const float HOLSTER_SCALE_FACTOR = 0.85f;
        public const float HOLSTER_DURABILITY_FACTOR = 1.2f;

        public override bool CanHold(Item item)
        {
            if (item is Wand ||
                item is MissileWeapon ||
                item is Bomb)
            {
                return base.CanHold(item);
            }
            else
            {
                return false;
            }
        }

        public override int Capacity()
        {
            return 19;
        }

        public override bool Collect(Bag container)
        {
            if (base.Collect(container))
            {
                if (owner != null)
                {
                    foreach (Item item in items)
                    {
                        if (item is Wand)
                        {
                            ((Wand)item).Charge(owner, HOLSTER_SCALE_FACTOR);
                        }
                        else if (item is MissileWeapon)
                        {
                            ((MissileWeapon)item).holster = true;
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void OnDetach()
        {
            base.OnDetach();

            foreach (var item in items)
            {
                if (item is Wand)
                    ((Wand)item).StopCharging();
                else if (item is MissileWeapon)
                    ((MissileWeapon)item).holster = false;
            }
        }

        public override int Value()
        {
            return 60;
        }
    }
}