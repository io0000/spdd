using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.food
{
    public class SmallRation : Food
    {
        public SmallRation()
        {
            image = ItemSpriteSheet.OVERPRICED;
            energy = Hunger.HUNGRY / 2f;
        }

        public override int Value()
        {
            return 10 * quantity;
        }
    }
}