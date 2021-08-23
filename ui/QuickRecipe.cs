using System;
using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.items;
using spdd.items.bombs;
using spdd.items.food;
using spdd.items.potions;
using spdd.items.potions.brews;
using spdd.items.potions.elixirs;
using spdd.items.potions.exotic;
using spdd.items.scrolls;
using spdd.items.scrolls.exotic;
using spdd.items.spells;
using spdd.items.stones;
using spdd.messages;
using spdd.plants;
using spdd.scenes;
using spdd.sprites;
using spdd.windows;

namespace spdd.ui
{
    public class QuickRecipe : Component
    {
        private List<Item> ingredients;

        private List<ItemSlot> inputs;
        private QuickRecipe.Arrow arrow;
        private ItemSlot output;

        public QuickRecipe(Recipe.SimpleRecipe r)
            : this(r, r.GetIngredients(), r.SampleOutput(null))
        {
        }

        public QuickRecipe(Recipe r, List<Item> inputs, Item output)
        {
            ingredients = inputs;
            int cost = r.Cost(inputs);
            bool hasInputs = true;
            this.inputs = new List<ItemSlot>();
            foreach (Item inItem in inputs)
            {
                Anonymize(inItem);
                ItemSlot curr;

                Action currAction = () =>
                {
                    ShatteredPixelDungeonDash.Scene().AddToFront(new WndInfoItem(inItem));
                };

                curr = new ActionItemSlot(currAction, inItem);

                List<Item> similar = Dungeon.hero.belongings.GetAllSimilar(inItem);
                int quantity = 0;
                foreach (Item sim in similar)
                {
                    //if we are looking for a specific item, it must be IDed
                    if (sim.GetType() != inItem.GetType() || sim.IsIdentified())
                        quantity += sim.Quantity();
                }

                if (quantity < inItem.Quantity())
                {
                    curr.sprite.Alpha(0.3f);
                    hasInputs = false;
                }
                curr.ShowExtraInfo(false);
                Add(curr);
                this.inputs.Add(curr);
            }

            if (cost > 0)
            {
                arrow = new Arrow(Icons.ARROW.Get(), cost);
                arrow.quickRecipe = this;
                arrow.HardlightText(new Color(0x00, 0xCC, 0xFF, 0xFF));
            }
            else
            {
                arrow = new Arrow(Icons.ARROW.Get());
                arrow.quickRecipe = this;
            }
            if (hasInputs)
            {
                arrow.icon.Tint(1, 1, 0, 1);
                if (!(ShatteredPixelDungeonDash.Scene() is AlchemyScene))
                    arrow.Enable(false);
            }
            else
            {
                arrow.icon.SetColor(0, 0, 0);
                arrow.Enable(false);
            }
            Add(arrow);

            Anonymize(output);
            Action outputAction = () =>
            {
                ShatteredPixelDungeonDash.Scene().AddToFront(new WndInfoItem(output));
            };
            this.output = new ActionItemSlot(outputAction, output);

            if (!hasInputs)
                this.output.sprite.Alpha(0.3f);

            this.output.ShowExtraInfo(false);
            Add(this.output);

            Layout();
        }

        private class ActionItemSlot : ItemSlot
        {
            public Action action;

            public ActionItemSlot(Action action, Item item)
                : base(item)
            {
                this.action = action;
            }

            protected override void OnClick()
            {
                if (action != null)
                    action();
            }
        }

        protected override void Layout()
        {
            height = 16;
            width = 0;

            foreach (ItemSlot item in inputs)
            {
                item.SetRect(x + width, y, 16, 16);
                width += 16;
            }

            arrow.SetRect(x + width, y, 14, 16);
            width += 14;

            output.SetRect(x + width, y, 16, 16);
            width += 16;
        }

        //used to ensure that un-IDed items are not spoiled
        private void Anonymize(Item item)
        {
            if (item is Potion)
            {
                ((Potion)item).Anonymize();
            }
            else if (item is Scroll)
            {
                ((Scroll)item).Anonymize();
            }
        }

        public class Arrow : IconButton
        {
            internal QuickRecipe quickRecipe;
            internal BitmapText text;

            public Arrow()
            { }

            public Arrow(Image icon)
                : base(icon)
            { }

            public Arrow(Image icon, int count)
                : base(icon)
            {
                text = new BitmapText(count.ToString(), PixelScene.pixelFont);
                text.Measure();
                Add(text);
            }

            protected override void Layout()
            {
                base.Layout();

                if (text != null)
                {
                    text.x = x;
                    text.y = y;
                    PixelScene.Align(text);
                }
            }

            protected override void OnClick()
            {
                base.OnClick();

                //find the window this is inside of and close it
                Group parent = this.parent;
                while (parent != null)
                {
                    if (parent is Window)
                    {
                        ((Window)parent).Hide();
                        break;
                    }
                    else
                    {
                        parent = parent.parent;
                    }
                }

                ((AlchemyScene)ShatteredPixelDungeonDash.Scene()).Populate(
                    quickRecipe.ingredients,
                    Dungeon.hero.belongings);
            }

            public void HardlightText(Color color)
            {
                if (text != null)
                    text.Hardlight(color);
            }
        }

        //gets recipes for a particular alchemy guide page
        //a null entry indicates a break in section
        public static List<QuickRecipe> GetRecipes(int pageIdx)
        {
            List<QuickRecipe> result = new List<QuickRecipe>();
            switch (pageIdx)
            {
                case 0:
                default:
                    result.Add(new QuickRecipe(new Potion.SeedToPotion(),
                        new List<Item>() { new Plant.Seed.PlaceHolder().Quantity(3) },
                        new QuickRecipeWndBagPlaceHolder(ItemSpriteSheet.POTION_HOLDER)));
                    return result;
                case 1:
                    Recipe r = new Scroll.ScrollToStone();
                    foreach (Type cls in Generator.Category.SCROLL.GetClasses())
                    {
                        Scroll scroll = (Scroll)Reflection.NewInstance(cls);
                        if (!scroll.IsKnown())
                            scroll.Anonymize();
                        List<Item> inList = new List<Item>() { scroll };
                        result.Add(new QuickRecipe(r, inList, r.SampleOutput(inList)));
                    }
                    return result;
                case 2:
                    result.Add(new QuickRecipe(new StewedMeat.OneMeat()));
                    result.Add(new QuickRecipe(new StewedMeat.TwoMeat()));
                    result.Add(new QuickRecipe(new StewedMeat.ThreeMeat()));
                    result.Add(null);
                    result.Add(null);
                    result.Add(new QuickRecipe(new MeatPie.Recipe(),
                            new List<Item>() { new Pasty(), new Food(), new MysteryMeat.PlaceHolder() },
                            new MeatPie()));
                    result.Add(null);
                    result.Add(null);
                    result.Add(new QuickRecipe(new Blandfruit.CookFruit(),
                            new List<Item>() { new Blandfruit(), new Plant.Seed.PlaceHolder() },
                            new QuickRecipeBlandfruit()));
                    return result;
                case 3:
                    r = new Bomb.EnhanceBomb();
                    int i = 0;
                    foreach (var cls in Bomb.EnhanceBomb.validIngredients.Keys)
                    {
                        if (i == 2)
                        {
                            result.Add(null);
                            i = 0;
                        }
                        Item item = (Item)Reflection.NewInstance(cls);
                        List<Item> inList = new List<Item>() { new Bomb(), item };
                        result.Add(new QuickRecipe(r, inList, r.SampleOutput(inList)));
                        ++i;
                    }
                    return result;
                case 4:
                    r = new ExoticPotion.PotionToExotic();
                    foreach (var cls in Generator.Category.POTION.GetClasses())
                    {
                        Potion pot = (Potion)Reflection.NewInstance(cls);
                        List<Item> inList = new List<Item> { pot, new Plant.Seed.PlaceHolder().Quantity(2) };
                        result.Add(new QuickRecipe(r, inList, r.SampleOutput(inList)));
                    }
                    return result;
                case 5:
                    r = new ExoticScroll.ScrollToExotic();
                    foreach (var cls in Generator.Category.SCROLL.GetClasses())
                    {
                        Scroll scroll = (Scroll)Reflection.NewInstance(cls);
                        List<Item> inList = new List<Item> { scroll, new Runestone.PlaceHolder().Quantity(2) };
                        result.Add(new QuickRecipe(r, inList, r.SampleOutput(inList)));
                    }
                    return result;
                case 6:
                    result.Add(new QuickRecipe(new AlchemicalCatalyst.Recipe(), new List<Item> { new Potion.PlaceHolder(), new Plant.Seed.PlaceHolder() }, new AlchemicalCatalyst()));
                    result.Add(new QuickRecipe(new AlchemicalCatalyst.Recipe(), new List<Item> { new Potion.PlaceHolder(), new Runestone.PlaceHolder() }, new AlchemicalCatalyst()));
                    result.Add(null);
                    result.Add(null);
                    result.Add(new QuickRecipe(new ArcaneCatalyst.Recipe(), new List<Item> { new Scroll.PlaceHolder(), new Runestone.PlaceHolder() }, new ArcaneCatalyst()));
                    result.Add(new QuickRecipe(new ArcaneCatalyst.Recipe(), new List<Item> { new Scroll.PlaceHolder(), new Plant.Seed.PlaceHolder() }, new ArcaneCatalyst()));
                    return result;
                case 7:
                    result.Add(new QuickRecipe(new CausticBrew.Recipe()));
                    result.Add(new QuickRecipe(new InfernalBrew.Recipe()));
                    result.Add(new QuickRecipe(new BlizzardBrew.Recipe()));
                    result.Add(new QuickRecipe(new ShockingBrew.Recipe()));
                    result.Add(null);
                    result.Add(null);
                    result.Add(new QuickRecipe(new ElixirOfHoneyedHealing.Recipe()));
                    result.Add(new QuickRecipe(new ElixirOfMight.Recipe()));
                    result.Add(new QuickRecipe(new ElixirOfAquaticRejuvenation.Recipe()));
                    result.Add(new QuickRecipe(new ElixirOfDragonsBlood.Recipe()));
                    result.Add(new QuickRecipe(new ElixirOfIcyTouch.Recipe()));
                    result.Add(new QuickRecipe(new ElixirOfToxicEssence.Recipe()));
                    result.Add(new QuickRecipe(new ElixirOfArcaneArmor.Recipe()));
                    return result;
                case 8:
                    result.Add(new QuickRecipe(new MagicalPorter.Recipe()));
                    result.Add(new QuickRecipe(new PhaseShift.Recipe()));
                    result.Add(new QuickRecipe(new WildEnergy.Recipe()));
                    result.Add(new QuickRecipe(new BeaconOfReturning.Recipe()));
                    result.Add(null);
                    result.Add(null);
                    result.Add(new QuickRecipe(new AquaBlast.Recipe()));
                    result.Add(new QuickRecipe(new FeatherFall.Recipe()));
                    result.Add(new QuickRecipe(new ReclaimTrap.Recipe()));
                    result.Add(null);
                    result.Add(null);
                    result.Add(new QuickRecipe(new CurseInfusion.Recipe()));
                    result.Add(new QuickRecipe(new MagicalInfusion.Recipe()));
                    result.Add(new QuickRecipe(new Alchemize.Recipe()));
                    result.Add(new QuickRecipe(new Recycle.Recipe()));
                    return result;
            }
        }

        private class QuickRecipeWndBagPlaceHolder : WndBag.Placeholder
        {
            public QuickRecipeWndBagPlaceHolder(int image)
                : base(image)
            { }

            public override string Name()
            {
                return Messages.Get(typeof(Potion.SeedToPotion), "name");
            }

            public override string Info()
            {
                return "";
            }
        }

        private class QuickRecipeBlandfruit : Blandfruit
        {
            public override string Name()
            {
                return Messages.Get(typeof(Blandfruit), "cooked");
            }

            public override string Info()
            {
                return "";
            }
        }
    }
}

