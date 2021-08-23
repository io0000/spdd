using System.Collections.Generic;
using watabou.noosa.audio;
using spdd.actors.hero;
using spdd.actors.mobs.npcs;
using spdd.sprites;

namespace spdd.items
{
    public class MerchantsBeacon : Item
    {
        private const string AC_USE = "USE";

        public MerchantsBeacon()
        {
            image = ItemSpriteSheet.BEACON;

            stackable = true;

            defaultAction = AC_USE;

            bones = true;
        }

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            actions.Add(AC_USE);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_USE))
            {
                Detach(hero.belongings.backpack);
                Shopkeeper.Sell();
                Sample.Instance.Play(Assets.Sounds.BEACON);
            }
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override int Value()
        {
            return 5 * quantity;
        }
    }
}