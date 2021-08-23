using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.food
{
    public class ChargrilledMeat : Food
    {
        public ChargrilledMeat()
        {
            image = ItemSpriteSheet.STEAK;
            energy = Hunger.HUNGRY / 2f;
        }

        public override int Value()
        {
            return 8 * quantity;
        }

        public static Food Cook(MysteryMeat ingredient)
        {
            var result = new ChargrilledMeat();
            result.quantity = ingredient.Quantity();
            return result;
        }
    }
}