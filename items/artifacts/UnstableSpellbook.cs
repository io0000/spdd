using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.items.rings;
using spdd.items.scrolls;
using spdd.items.scrolls.exotic;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.scenes;
using spdd.windows;
using spdd.utils;
using spdd.effects.particles;
using spdd.messages;

namespace spdd.items.artifacts
{
    public class UnstableSpellbook : Artifact
    {
        private void InitInstance()
        {
            image = ItemSpriteSheet.ARTIFACT_SPELLBOOK;

            levelCap = 10;

            charge = (int)(GetLevel() * 0.6f) + 2;
            partialCharge = 0;
            chargeCap = (int)(GetLevel() * 0.6f) + 2;

            defaultAction = AC_READ;
        }

        public const string AC_READ = "READ";
        public const string AC_ADD = "ADD";

        private List<Type> scrolls = new List<Type>();

        protected WndBag.Mode mode = WndBag.Mode.SCROLL;

        public UnstableSpellbook()
        {
            InitInstance();

            var scrollClasses = Generator.Category.SCROLL.GetClasses();
            float[] probs = (float[])Generator.Category.SCROLL.GetDefaultProbs().Clone(); //array of primitives, clone gives deep copy.
            int i = Rnd.Chances(probs);

            while (i != -1)
            {
                scrolls.Add(scrollClasses[i]);
                probs[i] = 0;

                i = Rnd.Chances(probs);
            }
            scrolls.Remove(typeof(ScrollOfTransmutation));

            itemSelector = new ItemSelector(this);
        }

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            if (IsEquipped(hero) && charge > 0 && !cursed)
                actions.Add(AC_READ);
            if (IsEquipped(hero) && GetLevel() < levelCap && !cursed)
                actions.Add(AC_ADD);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);
            if (action.Equals(AC_READ))
            {
                if (hero.FindBuff<Blindness>() != null)
                {
                    GLog.Warning(Messages.Get(this, "blinded"));
                }
                else if (!IsEquipped(hero))
                {
                    GLog.Information(Messages.Get(typeof(Artifact), "need_to_equip"));
                }
                else if (charge <= 0)
                {
                    GLog.Information(Messages.Get(this, "no_charge"));
                }
                else if (cursed)
                {
                    GLog.Information(Messages.Get(this, "cursed"));
                }
                else
                {
                    --charge;

                    Scroll scroll;
                    do
                    {
                        scroll = (Scroll)Generator.RandomUsingDefaults(Generator.Category.SCROLL);
                    }
                    while (scroll == null ||
                            //reduce the frequency of these scrolls by half
                            ((scroll is ScrollOfIdentify ||
                                 scroll is ScrollOfRemoveCurse ||
                                 scroll is ScrollOfMagicMapping) && Rnd.Int(2) == 0) ||
                            //don't roll teleportation scrolls on boss floors
                            (scroll is ScrollOfTeleportation && Dungeon.BossLevel()) ||
                            //cannot roll transmutation
                            (scroll is ScrollOfTransmutation));

                    scroll.Anonymize();
                    curItem = scroll;
                    curUser = hero;

                    //if there are charges left and the scroll has been given to the book
                    if (charge > 0 && !scrolls.Contains(scroll.GetType()))
                    {
                        Scroll fScroll = scroll;

                        ExploitHandler handler = Buff.Affect<ExploitHandler>(hero);
                        handler.scroll = scroll;
                        Type type = ExoticScroll.regToExo[scroll.GetType()];

                        var wnd = new WndOptions(
                            Messages.Get(this, "prompt"),
                            Messages.Get(this, "read_empowered"),
                            scroll.TrueName(),
                            Messages.Get(type, "name"));

                        wnd.selectAction = (index) =>
                        {
                            handler.Detach();
                            if (index == 1)
                            {
                                Scroll scr = (Scroll)Reflection.NewInstance(type);
                                --charge;
                                scr.DoRead();
                            }
                            else
                            {
                                fScroll.DoRead();
                            }
                        };
                        wnd.skipBackPressed = true;

                        GameScene.Show(wnd);
                    }
                    else
                    {
                        scroll.DoRead();
                    }
                    UpdateQuickslot();
                }
            }
            else if (action.Equals(AC_ADD))
            {
                GameScene.SelectItem(itemSelector, mode, Messages.Get(this, "prompt"));
            }
        }

        //forces the reading of a regular scroll if the player tried to exploit by quitting the game when the menu was up
        [SPDStatic]
        public class ExploitHandler : Buff
        {
            public ExploitHandler()
            {
                actPriority = VFX_PRIO;
            }

            public Scroll scroll;

            public override bool Act()
            {
                curUser = Dungeon.hero;
                curItem = scroll;
                scroll.Anonymize();
                scroll.DoRead();
                Detach();
                return true;
            }

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put("scroll", scroll);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                scroll = (Scroll)bundle.Get("scroll");
            }
        }

        protected override ArtifactBuff PassiveBuff()
        {
            return new BookRecharge(this);
        }

        public override void Charge(Hero target)
        {
            if (charge < chargeCap)
            {
                partialCharge += 0.1f;
                if (partialCharge >= 1)
                {
                    --partialCharge;
                    ++charge;
                    UpdateQuickslot();
                }
            }
        }

        public override Item Upgrade()
        {
            chargeCap = (int)((GetLevel() + 1) * 0.6f) + 2;

            //for artifact transmutation.
            while (scrolls.Count > 0 && scrolls.Count > (levelCap - 1 - GetLevel()))
                scrolls.RemoveAt(0);

            return base.Upgrade();
        }

        public override string Desc()
        {
            string desc = base.Desc();

            if (IsEquipped(Dungeon.hero))
            {
                if (cursed)
                {
                    desc += "\n\n" + Messages.Get(this, "desc_cursed");
                }

                if (GetLevel() < levelCap && scrolls.Count > 0)
                {
                    desc += "\n\n" + Messages.Get(this, "desc_index");
                    desc += "\n" + "_" + Messages.Get(scrolls[0], "name") + "_";
                    if (scrolls.Count > 1)
                        desc += "\n" + "_" + Messages.Get(scrolls[1], "name") + "_";
                }
            }

            if (GetLevel() > 0)
            {
                desc += "\n\n" + Messages.Get(this, "desc_empowered");
            }

            return desc;
        }

        private const string SCROLLS = "scrolls";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(SCROLLS, scrolls.ToArray());
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            scrolls.Clear();

            //Collections.addAll(scrolls, bundle.GetTypeArray(SCROLLS));
            foreach (var type in bundle.GetClassArray(SCROLLS))
            {
                scrolls.Add(type);
            }
        }

        public class BookRecharge : ArtifactBuff
        {
            public BookRecharge(Artifact artifact)
                : base(artifact)
            { }

            public override bool Act()
            {
                var lockedFloor = target.FindBuff<LockedFloor>();
                if (artifact.charge < artifact.chargeCap && !artifact.cursed && (lockedFloor == null || lockedFloor.RegenOn()))
                {
                    //120 turns to charge at full, 80 turns to charge at 0/8
                    float chargeGain = 1 / (120f - (artifact.chargeCap - artifact.charge) * 5f);
                    chargeGain *= RingOfEnergy.ArtifactChargeMultiplier(target);
                    artifact.partialCharge += chargeGain;

                    if (artifact.partialCharge >= 1)
                    {
                        --artifact.partialCharge;
                        ++artifact.charge;

                        if (artifact.charge == artifact.chargeCap)
                        {
                            artifact.partialCharge = 0;
                        }
                    }
                }

                UpdateQuickslot();

                Spend(TICK);

                return true;
            }
        }

        protected ItemSelector itemSelector;

        public class ItemSelector : WndBag.IListener
        {
            UnstableSpellbook book;
            public ItemSelector(UnstableSpellbook book)
            {
                this.book = book;
            }

            public void OnSelect(Item item)
            {
                var scrolls = book.scrolls;

                if (item != null && item is Scroll && item.IsIdentified())
                {
                    Hero hero = Dungeon.hero;
                    for (int i = 0; (i <= 1 && i < scrolls.Count); ++i)
                    {
                        if (scrolls[i].Equals(item.GetType()))
                        {
                            hero.sprite.Operate(hero.pos);
                            hero.Busy();
                            hero.Spend(2f);
                            Sample.Instance.Play(Assets.Sounds.BURNING);
                            hero.sprite.Emitter().Burst(ElmoParticle.Factory, 12);

                            scrolls.RemoveAt(i);
                            item.Detach(hero.belongings.backpack);

                            book.Upgrade();
                            GLog.Information(Messages.Get(typeof(UnstableSpellbook), "infuse_scroll"));
                            return;
                        }
                    }
                    GLog.Warning(Messages.Get(typeof(UnstableSpellbook), "unable_scroll"));
                }
                else if (item is Scroll && !item.IsIdentified())
                {
                    GLog.Warning(Messages.Get(typeof(UnstableSpellbook), "unknown_scroll"));
                }
            }
        }
    }
}