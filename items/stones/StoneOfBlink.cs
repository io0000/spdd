using spdd.actors;
using spdd.sprites;
using spdd.mechanics;
using spdd.items.scrolls;

namespace spdd.items.stones
{
    public class StoneOfBlink : Runestone
    {
        public StoneOfBlink()
        {
            image = ItemSpriteSheet.STONE_BLINK;
        }

        private static Ballistic throwPath;

        // [FIXED] Item class의 ThrowPos와 동일해서 override할 이유가 없음
        //public override int ThrowPos(Hero user, int dst)
        //{
        //    throwPath = new Ballistic(user.pos, dst, Ballistic.PROJECTILE);
        //    return throwPath.collisionPos;
        //}

        public override void OnThrow(int cell)
        {
            if (Actor.FindChar(cell) != null && throwPath.dist >= 1)
            {
                cell = throwPath.path[throwPath.dist - 1];
            }
            throwPath = null;
            base.OnThrow(cell);
        }

        protected override void Activate(int cell)
        {
            ScrollOfTeleportation.TeleportToLocation(curUser, cell);
        }
    }
}