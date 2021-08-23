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
    public class Ankh : Item
    {
        public const string AC_BLESS = "BLESS";

        public Ankh()
        {
            image = ItemSpriteSheet.ANKH;

            //You tell the ankh no, don't revive me, and then it comes back to revive you again in another run.
            //I'm not sure if that's enthusiasm or passive-aggression.
            bones = true;
        }

        private bool blessed;

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            DewVial vial = hero.belongings.GetItem<DewVial>();
            if (vial != null && vial.IsFull() && !blessed)
                actions.Add(AC_BLESS);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_BLESS))
            {
                DewVial vial = hero.belongings.GetItem<DewVial>();
                if (vial != null)
                {
                    blessed = true;
                    vial.Empty();
                    GLog.Positive(Messages.Get(this, "bless"));
                    hero.Spend(1f);
                    hero.Busy();

                    Sample.Instance.Play(Assets.Sounds.DRINK);
                    CellEmitter.Get(hero.pos).Start(Speck.Factory(Speck.LIGHT), 0.2f, 3);
                    hero.sprite.Operate(hero.pos);
                }
            }
        }

        public override string Desc()
        {
            if (blessed)
                return Messages.Get(this, "desc_blessed");
            else
                return base.Desc();
        }

        public bool IsBlessed()
        {
            return blessed;
        }

        public void Bless()
        {
            blessed = true;
        }

        private static ItemSprite.Glowing WHITE = new ItemSprite.Glowing(new Color(0xFF, 0xFF, 0xCC, 0xFF));

        public override ItemSprite.Glowing Glowing()
        {
            return IsBlessed() ? WHITE : null;
        }

        private const string BLESSED = "blessed";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(BLESSED, blessed);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            blessed = bundle.GetBoolean(BLESSED);
        }

        public override int Value()
        {
            return 50 * quantity;
        }
    }
}