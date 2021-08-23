using spdd.items.bombs;

namespace spdd.levels.traps
{
    public class ExplosiveTrap : Trap
    {
        public ExplosiveTrap()
        {
            color = ORANGE;
            shape = DIAMOND;
        }

        public override void Activate()
        {
            (new Bomb()).Explode(pos);
        }
    }
}