using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.items.food;
using spdd.ui;
using spdd.items;
using spdd.items.potions;
using spdd.utils;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Frost : FlavourBuff
    {
        public const float DURATION = 10.0f;

        public Frost()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override bool AttachTo(Character target)
        {
            if (!base.AttachTo(target))
                return false;

            ++target.paralysed;
            Buff.Detach<Burning>(target);
            Buff.Detach<Chill>(target);

            if (target is Hero)
            {
                var hero = (Hero)target;
                List<Item> freezable = new List<Item>();
                //does not reach inside of containers
                foreach (Item i in hero.belongings.backpack.items)
                {
                    if (!i.unique && (i is Potion || i is MysteryMeat))
                    {
                        freezable.Add(i);
                    }
                }

                if (freezable.Count > 0)
                {
                    Item toFreeze = Rnd.Element(freezable).Detach(hero.belongings.backpack);
                    GLog.Warning(Messages.Get(this, "freezes", toFreeze.ToString()));

                    if (toFreeze is Potion)
                    {
                        ((Potion)toFreeze).Shatter(hero.pos);
                    }
                    else if (toFreeze is MysteryMeat)
                    {
                        var carpaccio = new FrozenCarpaccio();
                        if (!carpaccio.Collect(hero.belongings.backpack))
                            Dungeon.level.Drop(carpaccio, hero.pos).sprite.Drop();
                    }
                }
            }
            else if (target is Thief)
            {
                Item item = ((Thief)target).item;

                if (item is Potion && !item.unique)
                {
                    ((Potion)((Thief)target).item).Shatter(target.pos);
                    ((Thief)target).item = null;
                }
                else if (item is MysteryMeat)
                {
                    ((Thief)target).item = new FrozenCarpaccio();
                }
            }

            return true;
        }

        public override void Detach()
        {
            base.Detach();
            if (target.paralysed > 0)
                --target.paralysed;
            if (Dungeon.level.water[target.pos])
                Buff.Prolong<Chill>(target, Chill.DURATION / 2.0f);
        }

        public override int Icon()
        {
            return BuffIndicator.FROST;
        }

        public override void TintIcon(Image icon)
        {
            icon.Hardlight(0.0f, 0.75f, 1.0f);
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
        }

        public override void Fx(bool on)
        {
            if (on)
                target.sprite.Add(CharSprite.State.FROZEN);
            else
                target.sprite.Remove(CharSprite.State.FROZEN);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns());
        }
    }
}