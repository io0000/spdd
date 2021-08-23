namespace spdd.actors.hero
{
    public class HeroAction
    {
        public int dst; // cell ∏Ò«•ºø

        public class Move : HeroAction
        {
            public Move(int dst)
            {
                base.dst = dst;
            }
        }

        public class PickUp : HeroAction
        {
            public PickUp(int dst)
            {
                base.dst = dst;
            }
        }

        public class OpenChest : HeroAction
        {
            public OpenChest(int dst)
            {
                base.dst = dst;
            }
        }

        public class Buy : HeroAction
        {
            public Buy(int dst)
            {
                base.dst = dst;
            }
        }

        public class Interact : HeroAction
        {
            public Character ch;

            public Interact(Character ch)
            {
                this.ch = ch;
            }
        }

        public class Unlock : HeroAction
        {
            public Unlock(int door)
            {
                dst = door;
            }
        }

        public class Descend : HeroAction
        {
            public Descend(int stairs)
            {
                dst = stairs;
            }
        }

        public class Ascend : HeroAction
        {
            public Ascend(int stairs)
            {
                dst = stairs;
            }
        }

        public class Alchemy : HeroAction
        {
            public Alchemy(int pot)
            {
                dst = pot;
            }
        }

        public class Attack : HeroAction
        {
            public Character target;
            public Attack(Character target)
            {
                this.target = target;
            }
        }
    }
}