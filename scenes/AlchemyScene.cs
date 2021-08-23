using System;
using System.Collections.Generic;
using watabou.gltextures;
using watabou.glwrap;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.particles;
using watabou.noosa.ui;
using watabou.utils;
using spdd.actors.hero;
using spdd.effects;
using spdd.items;
using spdd.items.artifacts;
using spdd.items.weapon.missiles.darts;
using spdd.journal;
using spdd.messages;
using spdd.sprites;
using spdd.ui;
using spdd.windows;

namespace spdd.scenes
{
    public class AlchemyScene : PixelScene
    {
        private static ItemButton[] inputs = new ItemButton[3];
        private ItemSlot output;

        private Emitter smokeEmitter;
        private Emitter bubbleEmitter;

        private Emitter lowerBubbles;
        private SkinnedBlock water;

        private RenderedTextBlock energyLeft;
        private RenderedTextBlock energyCost;

        private RedButton btnCombine;

        private const int BTN_SIZE = 28;

        public AlchemyScene()
        {
            itemSelector = new ItemSelector(this);
        }

        public override void Create()
        {
            base.Create();

            water = new AlchemySkinnedBlock(
                Camera.main.width,
                Camera.main.height,
                Dungeon.level.WaterTex());
            Add(water);

            Color[] colorArr = new Color[]
            {
                new Color(0x00, 0x00, 0x00, 0x66),
                new Color(0x00, 0x00, 0x00, 0x88),
                new Color(0x00, 0x00, 0x00, 0xAA),
                new Color(0x00, 0x00, 0x00, 0xCC),
                new Color(0x00, 0x00, 0x00, 0xFF)
            };

            Image im = new Image(TextureCache.CreateGradient(colorArr));
            im.angle = 90;
            im.x = Camera.main.width;
            im.scale.x = Camera.main.height / 5f;
            im.scale.y = Camera.main.width;
            Add(im);

            RenderedTextBlock title = PixelScene.RenderTextBlock(Messages.Get(this, "title"), 9);
            title.Hardlight(Window.TITLE_COLOR);
            title.SetPos(
                    (Camera.main.width - title.Width()) / 2f,
                    (20 - title.Height()) / 2f
            );
            Align(title);
            Add(title);

            int w = 50 + Camera.main.width / 2;
            int left = (Camera.main.width - w) / 2;

            int pos = (Camera.main.height - 100) / 2;

            RenderedTextBlock desc = PixelScene.RenderTextBlock(6);
            desc.MaxWidth(w);
            desc.Text(Messages.Get(typeof(AlchemyScene), "text"));
            desc.SetPos(left + (w - desc.Width()) / 2, pos);
            Add(desc);

            pos += (int)(desc.Height() + 6);

            //synchronized (inputs) 
            {
                for (int i = 0; i < inputs.Length; ++i)
                {
                    var j = i;
                    inputs[i] = new ItemButton();
                    inputs[i].action = () =>
                    {
                        var item = inputs[j].item;
                        var slot = inputs[j].slot;

                        //super.onClick();
                        if (item != null)
                        {
                            if (!(item is AlchemistsToolkit))
                            {
                                if (!item.Collect())
                                {
                                    Dungeon.level.Drop(item, Dungeon.hero.pos);
                                }
                            }

                            item = null;
                            inputs[j].item = null;

                            slot.Item(new WndBag.Placeholder(ItemSpriteSheet.SOMETHING));
                            UpdateState();
                        }

                        WndBag bag = WndBag.LastBag(itemSelector, WndBag.Mode.ALCHEMY, Messages.Get(typeof(AlchemyScene), "select"));
                        this.AddToFront(bag);
                    };

                    inputs[i].SetRect(left + 10, pos, BTN_SIZE, BTN_SIZE);
                    Add(inputs[i]);
                    pos += BTN_SIZE + 2;
                }
            }

            btnCombine = new AlchemyRedButton("");
            ((AlchemyRedButton)btnCombine).alchemyScene = this;
            btnCombine.Enable(false);
            btnCombine.SetRect(left + (w - 30) / 2f, inputs[1].Top() + 5, 30, inputs[1].Height() - 10);
            Add(btnCombine);

            output = new AlchemyItemSlot();
            ((AlchemyItemSlot)output).scene = this;
            output.SetRect(left + w - BTN_SIZE - 10, inputs[1].Top(), BTN_SIZE, BTN_SIZE);

            ColorBlock outputBG = new ColorBlock(output.Width(), output.Height(), new Color(0x91, 0x93, 0x8C, 0x99));
            outputBG.x = output.Left();
            outputBG.y = output.Top();
            Add(outputBG);

            Add(output);
            output.visible = false;

            bubbleEmitter = new Emitter();
            smokeEmitter = new Emitter();
            bubbleEmitter.Pos(0, 0, Camera.main.width, Camera.main.height);
            smokeEmitter.Pos(outputBG.x + (BTN_SIZE - 16) / 2f, outputBG.y + (BTN_SIZE - 16) / 2f, 16, 16);
            bubbleEmitter.autoKill = false;
            smokeEmitter.autoKill = false;
            Add(bubbleEmitter);
            Add(smokeEmitter);

            pos += 10;

            lowerBubbles = new Emitter();
            lowerBubbles.Pos(0, pos, Camera.main.width, Math.Max(0, Camera.main.height - pos));
            Add(lowerBubbles);
            lowerBubbles.Pour(Speck.Factory(Speck.BUBBLE), 0.1f);

            var btnExit = new AlchemyExitButton();
            btnExit.SetPos(Camera.main.width - btnExit.Width(), 0);
            Add(btnExit);

            var btnGuide = new AlchemyIconButton(new ItemSprite(ItemSpriteSheet.ALCH_PAGE, null));
            btnGuide.scene = this;
            btnGuide.SetRect(0, 0, 20, 20);
            Add(btnGuide);

            energyLeft = PixelScene.RenderTextBlock(Messages.Get(typeof(AlchemyScene), "energy", AvailableEnergy()), 9);
            energyLeft.SetPos(
                    (Camera.main.width - energyLeft.Width()) / 2,
                    Camera.main.height - 5 - energyLeft.Height()
            );
            Add(energyLeft);

            energyCost = PixelScene.RenderTextBlock(6);
            Add(energyCost);

            FadeIn();

            try
            {
                Dungeon.SaveAll();
                BadgesExtensions.SaveGlobal();
                Journal.SaveGlobal();
            }
            catch (Exception e)
            {
                ShatteredPixelDungeonDash.ReportException(e);
            }
        }

        public class AlchemySkinnedBlock : SkinnedBlock
        {
            public AlchemySkinnedBlock(float width, float height, object tx)
                : base(width, height, tx)
            { }

            protected override NoosaScript Script()
            {
                return NoosaScriptNoLighting.Get();
            }

            public override void Draw()
            {
                //water has no alpha component, this improves performance
                Blending.Disable();
                base.Draw();
                Blending.Enable();
            }
        }

        public class AlchemyRedButton : RedButton
        {
            internal AlchemyScene alchemyScene;
            Image arrow;

            public AlchemyRedButton(string label)
                : base(label)
            { }

            protected override void CreateChildren()
            {
                base.CreateChildren();

                arrow = Icons.ARROW.Get();
                Add(arrow);
            }

            protected override void Layout()
            {
                base.Layout();
                arrow.x = x + (width - arrow.width) / 2f;
                arrow.y = y + (height - arrow.height) / 2f;
                PixelScene.Align(arrow);
            }

            public override void Enable(bool value)
            {
                base.Enable(value);
                if (value)
                {
                    arrow.Tint(1, 1, 0, 1);
                    arrow.Alpha(1f);
                    bg.Alpha(1f);
                }
                else
                {
                    arrow.SetColor(0, 0, 0);
                    arrow.Alpha(0.6f);
                    bg.Alpha(0.6f);
                }
            }

            protected override void OnClick()
            {
                base.OnClick();
                alchemyScene.Combine();
            }
        }

        public class AlchemyItemSlot : ItemSlot
        {
            internal AlchemyScene scene;

            protected override void OnClick()
            {
                base.OnClick();
                if (visible && item.TrueName() != null)
                {
                    scene.AddToFront(new WndInfoItem(item));
                }
            }
        }

        public class AlchemyExitButton : ExitButton
        {
            protected override void OnClick()
            {
                Game.SwitchScene(typeof(GameScene));
            }
        }

        public class AlchemyIconButton : IconButton
        {
            internal AlchemyScene scene;

            public AlchemyIconButton(Image icon)
                : base(icon)
            { }

            protected override void OnClick()
            {
                base.OnClick();
                scene.ClearSlots();
                scene.UpdateState();
                scene.AddToFront(new AlchemyWindow());
            }

            public class AlchemyWindow : Window
            {
                public AlchemyWindow()
                {
                    WndJournal.AlchemyTab t = new WndJournal.AlchemyTab();
                    int w, h;
                    if (Landscape())
                    {
                        w = WndJournal.WIDTH_L; h = WndJournal.HEIGHT_L;
                    }
                    else
                    {
                        w = WndJournal.WIDTH_P; h = WndJournal.HEIGHT_P;
                    }
                    Resize(w, h);
                    Add(t);
                    t.SetRect(0, 0, w, h);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            water.Offset(0, -5 * Game.elapsed);
        }

        public override void OnBackPressed()
        {
            ShatteredPixelDungeonDash.SwitchScene(typeof(GameScene));
        }

        protected ItemSelector itemSelector;

        public class ItemSelector : WndBag.IListener
        {
            AlchemyScene scene;

            public ItemSelector(AlchemyScene scene)
            {
                this.scene = scene;
            }

            public void OnSelect(Item item)
            {
                if (item != null && inputs[0] != null)
                {
                    for (int i = 0; i < inputs.Length; ++i)
                    {
                        if (inputs[i].item == null)
                        {
                            if (item is Dart)
                            {
                                inputs[i].Item(item.DetachAll(Dungeon.hero.belongings.backpack));
                            }
                            else if (item is AlchemistsToolkit)
                            {
                                scene.ClearSlots();
                                inputs[i].Item(item);
                            }
                            else
                            {
                                inputs[i].Item(item.Detach(Dungeon.hero.belongings.backpack));
                            }
                            break;
                        }
                    }
                    scene.UpdateState();
                }
            }
        }

        private List<T> FilterInput<T>() where T : Item
        {
            List<T> filtered = new List<T>();
            for (int i = 0; i < inputs.Length; ++i)
            {
                Item item = inputs[i].item;
                // if (item != null && itemClass.isInstance(item))
                if (item != null && typeof(T).IsAssignableFrom(item.GetType()))
                {
                    filtered.Add((T)item);
                }
            }
            return filtered;
        }

        private void UpdateState()
        {
            List<Item> ingredients = FilterInput<Item>();
            Recipe recipe = Recipe.FindRecipe(ingredients);

            if (recipe != null)
            {
                int cost = recipe.Cost(ingredients);

                output.Item(recipe.SampleOutput(ingredients));
                output.visible = true;

                energyCost.Text(Messages.Get(typeof(AlchemyScene), "cost", cost));
                energyCost.SetPos(
                        btnCombine.Left() + (btnCombine.Width() - energyCost.Width()) / 2,
                        btnCombine.Top() - energyCost.Height()
                );

                energyCost.visible = (cost > 0);

                if (cost <= AvailableEnergy())
                {
                    btnCombine.Enable(true);
                    energyCost.ResetColor();
                }
                else
                {
                    btnCombine.Enable(false);
                    energyCost.Hardlight(new Color(0xFF, 0x00, 0x00, 0xFF));
                }
            }
            else
            {
                btnCombine.Enable(false);
                output.visible = false;
                energyCost.visible = false;
            }
        }

        private void Combine()
        {
            List<Item> ingredients = FilterInput<Item>();
            Recipe recipe = Recipe.FindRecipe(ingredients);

            Item result = null;

            if (recipe != null)
            {
                provider.SpendEnergy(recipe.Cost(ingredients));
                energyLeft.Text(Messages.Get(typeof(AlchemyScene), "energy", AvailableEnergy()));
                energyLeft.SetPos(
                        (Camera.main.width - energyLeft.Width()) / 2,
                        Camera.main.height - 5 - energyLeft.Height()
                );

                result = recipe.Brew(ingredients);
            }

            if (result != null)
            {
                bubbleEmitter.Start(Speck.Factory(Speck.BUBBLE), 0.01f, 100);
                smokeEmitter.Burst(Speck.Factory(Speck.WOOL), 10);
                Sample.Instance.Play(Assets.Sounds.PUFF);

                output.Item(result);
                if (!(result is AlchemistsToolkit))
                {
                    if (!result.Collect())
                    {
                        Dungeon.level.Drop(result, Dungeon.hero.pos);
                    }
                }

                try
                {
                    Dungeon.SaveAll();
                }
                catch (Exception e)
                {
                    ShatteredPixelDungeonDash.ReportException(e);
                }

                //synchronized (inputs) 
                {
                    for (int i = 0; i < inputs.Length; ++i)
                    {
                        if (inputs[i] != null && inputs[i].item != null)
                        {
                            if (inputs[i].item.Quantity() <= 0 || inputs[i].item is AlchemistsToolkit)
                            {
                                inputs[i].slot.Item(new WndBag.Placeholder(ItemSpriteSheet.SOMETHING));
                                inputs[i].item = null;
                            }
                            else
                            {
                                inputs[i].slot.Item(inputs[i].item);
                            }
                        }
                    }
                }

                btnCombine.Enable(false);
            }
        }

        public void Populate(List<Item> toFind, Belongings inventory)
        {
            ClearSlots();

            int curslot = 0;
            foreach (Item finding in toFind)
            {
                int needed = finding.Quantity();
                List<Item> found = inventory.GetAllSimilar(finding);
                while (found.Count > 0 && needed > 0)
                {
                    Item detached;
                    if (finding is Dart)
                    {
                        detached = found[0].DetachAll(inventory.backpack);
                    }
                    else
                    {
                        detached = found[0].Detach(inventory.backpack);
                    }
                    inputs[curslot].Item(detached);
                    ++curslot;
                    needed -= detached.Quantity();
                    if (detached == found[0])
                    {
                        found.RemoveAt(0);
                    }
                }
            }
            UpdateState();
        }

        public override void Destroy()
        {
            //synchronized(inputs) 
            {
                ClearSlots();
                for (int i = 0; i < inputs.Length; ++i)
                {
                    inputs[i] = null;
                }
            }

            try
            {
                Dungeon.SaveAll();
                BadgesExtensions.SaveGlobal();
                Journal.SaveGlobal();
            }
            catch (Exception e)
            {
                ShatteredPixelDungeonDash.ReportException(e);
            }
            base.Destroy();
        }

        public void ClearSlots()
        {
            //synchronized(inputs) 
            {
                for (int i = 0; i < inputs.Length; ++i)
                {
                    if (inputs[i] != null && inputs[i].item != null)
                    {
                        if (!(inputs[i].item is AlchemistsToolkit))
                        {
                            if (!inputs[i].item.Collect())
                            {
                                Dungeon.level.Drop(inputs[i].item, Dungeon.hero.pos);
                            }
                        }
                        inputs[i].Item(null);
                        inputs[i].slot.Item(new WndBag.Placeholder(ItemSpriteSheet.SOMETHING));
                    }
                }
            }
        }

        public class ItemButton : Component
        {
            internal NinePatch bg;
            internal ItemSlot slot;

            public Item item;
            internal Action action;

            protected override void CreateChildren()
            {
                base.CreateChildren();

                bg = Chrome.Get(Chrome.Type.RED_BUTTON);
                Add(bg);

                slot = new ItemButtonItemSlot(this);
                slot.Enable(true);
                Add(slot);
            }

            private class ItemButtonItemSlot : ItemSlot
            {
                ItemButton button;
                public ItemButtonItemSlot(ItemButton button)
                {
                    this.button = button;
                }

                protected override void OnPointerDown()
                {
                    button.bg.Brightness(1.2f);
                    Sample.Instance.Play(Assets.Sounds.CLICK);
                }

                protected override void OnPointerUp()
                {
                    button.bg.ResetColor();
                }

                protected override void OnClick()
                {
                    button.OnClick();
                }
            }

            protected void OnClick()
            {
                if (action != null)
                    action();
            }

            protected override void Layout()
            {
                base.Layout();

                bg.x = x;
                bg.y = y;
                bg.Size(width, height);

                slot.SetRect(x + 2, y + 2, width - 4, height - 4);
            }

            public void Item(Item item)
            {
                slot.Item(this.item = item);
            }
        }

        private static IAlchemyProvider provider;

        public static void SetProvider(IAlchemyProvider p)
        {
            provider = p;
        }

        public static int AvailableEnergy()
        {
            return provider == null ? 0 : provider.GetEnergy();
        }

        public static bool ProviderIsToolkit()
        {
            return provider is AlchemistsToolkit.KitEnergy;
        }

        public interface IAlchemyProvider
        {
            int GetEnergy();

            void SpendEnergy(int reduction);
        }
    }
}