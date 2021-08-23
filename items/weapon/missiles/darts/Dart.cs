using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.items.weapon.melee;
using spdd.messages;
using spdd.plants;
using spdd.scenes;
using spdd.sprites;
using spdd.windows;

namespace spdd.items.weapon.missiles.darts
{
    public class Dart : MissileWeapon
    {
        public Dart()
        {
            image = ItemSpriteSheet.DART;
            hitSound = Assets.Sounds.HIT_ARROW;
            hitSoundPitch = 1.3f;

            tier = 1;

            //infinite, even with penalties
            baseUses = 1000;

            itemSelector = new ItemSelector(this);
        }

        protected const string AC_TIP = "TIP";

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            actions.Add(AC_TIP);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);
            if (action.Equals(AC_TIP))
            {
                GameScene.SelectItem(itemSelector, WndBag.Mode.SEED, Messages.Get(this, "prompt"));
            }
        }

        public override int Min(int lvl)
        {
            if (bow != null)
            {
                return 4 +                    //4 base
                       bow.BuffedLvl() + lvl; //+1 per level or bow level
            }
            else
            {
                return 1 +     //1 base, down from 2
                       lvl;    //scaling unchanged
            }
        }

        public override int Max(int lvl)
        {
            if (bow != null)
            {
                return 12 +                           //12 base
                       3 * bow.BuffedLvl() + 2 * lvl; //+3 per bow level, +2 per level (default scaling +2)
            }
            else
            {
                return 2 +       //2 base, down from 5
                       2 * lvl;  //scaling unchanged
            }
        }

        private static Crossbow bow;

        private void UpdateCrossbow()
        {
            if (Dungeon.hero.belongings.weapon is Crossbow)
            {
                bow = (Crossbow)Dungeon.hero.belongings.weapon;
            }
            else
            {
                bow = null;
            }
        }

        public override bool HasEnchant(Type type, Character owner)
        {
            if (bow != null && bow.HasEnchant(type, owner))
            {
                return true;
            }
            else
            {
                return base.HasEnchant(type, owner);
            }
        }

        public override int Proc(Character attacker, Character defender, int damage)
        {
            if (bow != null && bow.enchantment != null && attacker.FindBuff<MagicImmune>() == null)
            {
                SetLevel(bow.GetLevel());
                damage = bow.enchantment.Proc(this, attacker, defender, damage);
                SetLevel(0);
            }
            return base.Proc(attacker, defender, damage);
        }

        public override void OnThrow(int cell)
        {
            UpdateCrossbow();
            base.OnThrow(cell);
        }

        public override void ThrowSound()
        {
            UpdateCrossbow();
            if (bow != null)
            {
                Sample.Instance.Play(Assets.Sounds.ATK_CROSSBOW, 1, Rnd.Float(0.87f, 1.15f));
            }
            else
            {
                base.ThrowSound();
            }
        }

        public override string Info()
        {
            UpdateCrossbow();
            return base.Info();
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override int Value()
        {
            return base.Value() / 2; //half normal value
        }

        private ItemSelector itemSelector;

        class ItemSelector : WndBag.IListener
        {
            private Dart dart;

            public ItemSelector(Dart dart)
            {
                this.dart = dart;
            }

            public void OnSelect(Item item)
            {
                dart.OnSelect(item);
            }
        }

        public void OnSelect(Item item)
        {
            if (item == null)
                return;

            int maxToTip = Math.Min(curItem.Quantity(), item.Quantity() * 2);
            int maxSeedsToUse = (maxToTip + 1) / 2;

            int singleSeedDarts;

            string[] options;

            var dartType = typeof(Dart);

            if (curItem.Quantity() == 1)
            {
                singleSeedDarts = 1;
                options = new string[]{
                        Messages.Get(dartType, "tip_one"),
                        Messages.Get(dartType, "tip_cancel")};
            }
            else
            {
                singleSeedDarts = 2;
                if (maxToTip <= 2)
                {
                    options = new string[]{
                        Messages.Get(dartType, "tip_two"),
                        Messages.Get(dartType, "tip_cancel")};
                }
                else
                {
                    options = new string[]{
                        Messages.Get(dartType, "tip_all", maxToTip, maxSeedsToUse),
                        Messages.Get(dartType, "tip_two"),
                        Messages.Get(dartType, "tip_cancel")};
                }
            }

            TippedDart tipResult = TippedDart.GetTipped((Plant.Seed)item, 1);

            var wnd = new WndOptions(
                    Messages.Get(dartType, "tip_title"),
                    Messages.Get(dartType, "tip_desc", tipResult.Name()) + "\n\n" + tipResult.Desc(),
                    options);

            wnd.selectAction = (index) =>
            {
                if (index == 0 && options.Length == 3)
                {
                    if (item.Quantity() <= maxSeedsToUse)
                    {
                        item.DetachAll(curUser.belongings.backpack);
                    }
                    else
                    {
                        item.Quantity(item.Quantity() - maxSeedsToUse);
                    }

                    if (maxToTip < curItem.Quantity())
                    {
                        curItem.Quantity(curItem.Quantity() - maxToTip);
                    }
                    else
                    {
                        curItem.DetachAll(curUser.belongings.backpack);
                    }

                    TippedDart newDart = TippedDart.GetTipped((Plant.Seed)item, maxToTip);
                    if (!newDart.Collect())
                        Dungeon.level.Drop(newDart, curUser.pos).sprite.Drop();

                    curUser.Spend(1f);
                    curUser.Busy();
                    curUser.sprite.Operate(curUser.pos);
                }
                else if ((index == 1 && options.Length == 3) || (index == 0 && options.Length == 2))
                {
                    item.Detach(curUser.belongings.backpack);

                    if (curItem.Quantity() <= singleSeedDarts)
                    {
                        curItem.DetachAll(curUser.belongings.backpack);
                    }
                    else
                    {
                        curItem.Quantity(curItem.Quantity() - singleSeedDarts);
                    }

                    TippedDart newDart = TippedDart.GetTipped((Plant.Seed)item, singleSeedDarts);
                    if (!newDart.Collect())
                        Dungeon.level.Drop(newDart, curUser.pos).sprite.Drop();

                    curUser.Spend(1f);
                    curUser.Busy();
                    curUser.sprite.Operate(curUser.pos);
                }
            };

            GameScene.Show(wnd);
        }
    }
}