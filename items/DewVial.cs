using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.hero;
using spdd.effects;
using spdd.messages;
using spdd.sprites;
using spdd.utils;

namespace spdd.items
{
    public class DewVial : Item
    {
        private const int MAX_VOLUME = 20;

        private const string AC_DRINK = "DRINK";

        private const float TIME_TO_DRINK = 1f;

        private const string TXT_STATUS = "%d/%d";

        public DewVial()
        {
            image = ItemSpriteSheet.VIAL;

            defaultAction = AC_DRINK;

            unique = true;
        }

        private int volume;

        private const string VOLUME = "volume";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(VOLUME, volume);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            volume = bundle.GetInt(VOLUME);
        }

        public override List<string> Actions(Hero hero)
        {
            var actions = base.Actions(hero);
            if (volume > 0)
                actions.Add(AC_DRINK);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_DRINK))
            {
                if (volume > 0)
                {
                    float missingHealthPercent = 1f - (hero.HP / (float)hero.HT);

                    //trimming off 0.01 drops helps with floating point errors
                    int dropsNeeded = (int)Math.Ceiling((missingHealthPercent / 0.05f) - 0.01f);
                    dropsNeeded = (int)GameMath.Gate(1, dropsNeeded, volume);

                    //20 drops for a full heal normally
                    int heal = (int)Math.Round(hero.HT * 0.05f * dropsNeeded, MidpointRounding.AwayFromZero);

                    int effect = Math.Min(hero.HT - hero.HP, heal);
                    if (effect > 0)
                    {
                        hero.HP += effect;
                        hero.sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 1 + dropsNeeded / 5);
                        hero.sprite.ShowStatus(CharSprite.POSITIVE, Messages.Get(this, "value", effect));
                    }

                    volume -= dropsNeeded;

                    hero.Spend(TIME_TO_DRINK);
                    hero.Busy();

                    Sample.Instance.Play(Assets.Sounds.DRINK);
                    hero.sprite.Operate(hero.pos);

                    UpdateQuickslot();
                }
                else
                {
                    GLog.Warning(Messages.Get(this, "empty"));
                }
            }
        }

        public void Empty()
        {
            volume = 0;
            UpdateQuickslot();
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public bool IsFull()
        {
            return volume >= MAX_VOLUME;
        }

        public void CollectDew(Dewdrop dew)
        {
            GLog.Information(Messages.Get(this, "collected"));
            volume += dew.quantity;
            if (volume >= MAX_VOLUME)
            {
                volume = MAX_VOLUME;
                GLog.Positive(Messages.Get(this, "full"));
            }

            UpdateQuickslot();
        }

        public void Fill()
        {
            volume = MAX_VOLUME;
            UpdateQuickslot();
        }

        public override string Status()
        {
            return Messages.Format(TXT_STATUS, volume, MAX_VOLUME);
        }
    }
}