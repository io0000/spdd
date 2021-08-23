using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.items.armor;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.windows;

namespace spdd.items
{
    public class BrokenSeal : Item
    {
        public const string AC_AFFIX = "AFFIX";

        //only to be used from the quickslot, for tutorial purposes mostly.
        public const string AC_INFO = "INFO_WINDOW";

        public BrokenSeal()
        {
            image = ItemSpriteSheet.SEAL;

            cursedKnown = levelKnown = true;
            unique = true;
            bones = false;

            defaultAction = AC_INFO;
        }

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            actions.Add(AC_AFFIX);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);

            if (action.Equals(AC_AFFIX))
            {
                curItem = this;
                GameScene.SelectItem(armorSelector, WndBag.Mode.ARMOR, Messages.Get(this, "prompt"));
            }
            else if (action.Equals(AC_INFO))
            {
                GameScene.Show(new WndUseItem(null, this));
            }
        }

        //scroll of upgrade can be used directly once, same as upgrading armor the seal is affixed to then removing it.
        public override bool IsUpgradable()
        {
            return GetLevel() == 0;
        }

        private ArmorSelector armorSelector = new ArmorSelector();

        class ArmorSelector : WndBag.IListener
        {
            public void OnSelect(Item item)
            {
                if (item != null && item is Armor)
                {
                    Armor armor = (Armor)item;
                    if (!armor.levelKnown)
                    {
                        GLog.Warning(Messages.Get(typeof(BrokenSeal), "unknown_armor"));
                    }
                    else if (armor.cursed || armor.GetLevel() < 0)
                    {
                        GLog.Warning(Messages.Get(typeof(BrokenSeal), "degraded_armor"));
                    }
                    else
                    {
                        GLog.Positive(Messages.Get(typeof(BrokenSeal), "affix"));
                        Dungeon.hero.sprite.Operate(Dungeon.hero.pos);
                        Sample.Instance.Play(Assets.Sounds.UNLOCK);
                        armor.AffixSeal((BrokenSeal)curItem);
                        curItem.Detach(Dungeon.hero.belongings.backpack);
                    }
                }
            }
        }

        [SPDStatic]
        public class WarriorShield : ShieldBuff
        {
            private Armor armor;
            private float partialShield;

            public override bool Act()
            {
                if (Shielding() < MaxShield())
                    partialShield += 1 / 30f;

                while (partialShield >= 1)
                {
                    IncShield();
                    --partialShield;
                }

                if (Shielding() <= 0 && MaxShield() <= 0)
                    Detach();

                Spend(TICK);
                return true;
            }

            public void Supercharge(int maxShield)
            {
                if (maxShield > Shielding())
                    SetShield(maxShield);
            }

            public void SetArmor(Armor arm)
            {
                armor = arm;
            }

            public int MaxShield()
            {
                if (armor != null && armor.IsEquipped((Hero)target))
                {
                    return 1 + armor.tier + armor.GetLevel();
                }
                else
                {
                    return 0;
                }
            }

            //logic edited slightly as buff should not detach
            public override int AbsorbDamage(int dmg)
            {
                if (Shielding() >= dmg)
                {
                    DecShield(dmg);
                    dmg = 0;
                }
                else
                {
                    dmg -= Shielding();
                    SetShield(0);
                }
                return dmg;
            }
        }
    }
}