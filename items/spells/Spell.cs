using System.Collections.Generic;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.utils;
using spdd.messages;

namespace spdd.items.spells
{
    public abstract class Spell : Item
    {
        public const string AC_CAST = "CAST";

        public Spell()
        {
            stackable = true;
            defaultAction = AC_CAST;
        }

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            actions.Add(AC_CAST);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_CAST))
            {
                if (curUser.FindBuff<MagicImmune>() != null)
                {
                    GLog.Warning(Messages.Get(this, "no_magic"));
                    return;
                }

                OnCast(hero);
            }
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        protected abstract void OnCast(Hero hero);
    }
}