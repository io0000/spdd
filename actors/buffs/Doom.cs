using spdd.ui;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Doom : Buff
    {
        public Doom()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override void Fx(bool on)
        {
            if (on) 
                target.sprite.Add(CharSprite.State.DARKENED);
            else if (target.invisible == 0) 
                target.sprite.Remove(CharSprite.State.DARKENED);
        }

        public override int Icon()
        {
            return BuffIndicator.CORRUPT;
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc");
        }
    }
}