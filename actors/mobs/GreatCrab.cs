using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.mobs.npcs;
using spdd.items.food;
using spdd.sprites;
using spdd.items.wands;
using spdd.utils;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class GreatCrab : Crab
    {
        public GreatCrab()
        {
            spriteClass = typeof(GreatCrabSprite);

            HP = HT = 25;
            defenseSkill = 0; //see damage()
            baseSpeed = 1f;

            EXP = 6;

            state = WANDERING;

            properties.Add(Property.MINIBOSS);
        }

        private int moving;

        public override bool GetCloser(int target)
        {
            //this is used so that the crab remains slower, but still detects the player at the expected rate.
            ++moving;
            if (moving < 3)
            {
                return base.GetCloser(target);
            }
            else
            {
                moving = 0;
                return true;
            }
        }

        public override void Damage(int dmg, object src)
        {
            //crab blocks all attacks originating from its current enemy if it sees them.
            //All direct damage is negated, no exceptions. environmental effects go through as normal.
            if ((enemySeen && state != SLEEPING && paralysed == 0) &&
                ((src is Wand && enemy == Dungeon.hero) || (src is Character && enemy == src)))
            {
                GLog.Negative(Messages.Get(this, "noticed"));
                sprite.ShowStatus(CharSprite.NEUTRAL, Messages.Get(this, "blocked"));
                Sample.Instance.Play(Assets.Sounds.HIT_PARRY, 1, Rnd.Float(0.96f, 1.05f));
            }
            else
            {
                base.Damage(dmg, src);
            }
        }

        public override void Die(object cause)
        {
            base.Die(cause);

            Ghost.Quest.Process();

            Dungeon.level.Drop(new MysteryMeat(), pos);
            Dungeon.level.Drop(new MysteryMeat(), pos).sprite.Drop();
        }
    }
}