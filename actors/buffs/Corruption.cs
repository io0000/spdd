using spdd.ui;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Corruption : Buff
    {
        public Corruption()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        private float buildToDamage;

        public override bool AttachTo(Character target)
        {
            if (base.AttachTo(target))
            {
                target.alignment = Character.Alignment.ALLY;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Act()
        {
            buildToDamage += target.HT / 200f;

            int damage = (int)buildToDamage;
            buildToDamage -= damage;

            if (damage > 0)
                target.Damage(damage, this);

            Spend(TICK);

            return true;
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