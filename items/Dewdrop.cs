using System;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.effects;
using spdd.sprites;
using spdd.messages;
using spdd.utils;

namespace spdd.items
{
    public class Dewdrop : Item
    {
        public Dewdrop()
        {
            image = ItemSpriteSheet.DEWDROP;

            stackable = true;
            dropsDownHeap = true;
        }

        public override bool DoPickUp(Hero hero)
        {
            DewVial vial = hero.belongings.GetItem<DewVial>();

            if (vial != null && !vial.IsFull())
            {
                vial.CollectDew(this);
            }
            else
            {
                //20 drops for a full heal
                int heal = (int)Math.Round(hero.HT * 0.05f * quantity, MidpointRounding.AwayFromZero);

                int effect = Math.Min(hero.HT - hero.HP, heal);
                if (effect > 0)
                {
                    hero.HP += effect;
                    hero.sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 1);
                    hero.sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(this, "value", effect));
                }
                else
                {
                    GLog.Information(Messages.Get(this, "already_full"));
                    return false;
                }
            }

            Sample.Instance.Play(Assets.Sounds.DEWDROP);
            hero.SpendAndNext(TIME_TO_PICK_UP);

            return true;
        }

        //max of one dew in a stack
        public override Item Quantity(int value)
        {
            quantity = Math.Min(value, 1);
            return this;
        }
    }
}