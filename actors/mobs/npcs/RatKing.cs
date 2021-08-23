using spdd.actors.buffs;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.mobs.npcs
{
    public class RatKing : NPC
    {
        public RatKing()
        {
            spriteClass = typeof(RatKingSprite);

            state = SLEEPING;
        }

        public override int DefenseSkill(Character enemy)
        {
            return INFINITE_EVASION;
        }

        public override float Speed()
        {
            return 2f;
        }

        protected override Character ChooseEnemy()
        {
            return null;
        }

        public override void Damage(int dmg, object src)
        { }

        public override void Add(Buff buff)
        { }

        public override bool Reset()
        {
            return true;
        }

        //***This functionality is for when rat king may be summoned by a distortion trap

        protected override void OnAdd()
        {
            base.OnAdd();
            if (Dungeon.depth != 5)
            {
                Yell(Messages.Get(this, "confused"));
            }
        }

        public override bool Act()
        {
            if (Dungeon.depth < 5)
            {
                if (pos == Dungeon.level.exit)
                {
                    Destroy();
                    sprite.KillAndErase();
                }
                else
                {
                    target = Dungeon.level.exit;
                }
            }
            else if (Dungeon.depth > 5)
            {
                if (pos == Dungeon.level.entrance)
                {
                    Destroy();
                    sprite.KillAndErase();
                }
                else
                {
                    target = Dungeon.level.entrance;
                }
            }
            return base.Act();
        }

        //***

        public override bool Interact(Character c)
        {
            sprite.TurnTo(pos, c.pos);

            if (c != Dungeon.hero)
            {
                return base.Interact(c);
            }

            if (state == SLEEPING)
            {
                Notice();
                Yell(Messages.Get(this, "not_sleeping"));
                state = WANDERING;
            }
            else
            {
                Yell(Messages.Get(this, "what_is_it"));
            }

            return true;
        }

        public override string Description()
        {
            return ((RatKingSprite)sprite).festive ?
                    Messages.Get(this, "desc_festive")
                    : base.Description();
        }
    }
}