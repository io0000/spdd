using spdd.actors.hero;
using spdd.items.artifacts;
using spdd.items.rings;
using spdd.messages;
using spdd.scenes;
using spdd.ui;
using spdd.utils;
using spdd.windows;

namespace spdd.items
{
    public class KindofMisc : EquipableItem
    {
        private const float TIME_TO_EQUIP = 1f;

        public override bool DoEquip(Hero hero)
        {
            bool equipFull = false;
            if (this is Artifact &&
                hero.belongings.artifact != null &&
                hero.belongings.misc != null)
            {
                equipFull = true;
            }
            else if (this is Ring &&
                hero.belongings.misc != null &&
                hero.belongings.ring != null)
            {
                equipFull = true;
            }

            if (equipFull)
            {
                KindofMisc[] miscs = new KindofMisc[3];
                miscs[0] = hero.belongings.artifact;
                miscs[1] = hero.belongings.misc;
                miscs[2] = hero.belongings.ring;

                bool[] enabled = new bool[3];
                enabled[0] = miscs[0] != null;
                enabled[1] = miscs[1] != null;
                enabled[2] = miscs[2] != null;

                //force swapping with the same type of item if 2x of that type is already present
                if (this is Ring && hero.belongings.misc is Ring)
                {
                    enabled[0] = false; //disable artifact
                }
                else if (this is Artifact && hero.belongings.misc is Artifact)
                {
                    enabled[2] = false; //disable ring
                }

                var wnd = new KindOfMiscWndOptions(Messages.Get(typeof(KindofMisc), "unequip_title"),
                                                Messages.Get(typeof(KindofMisc), "unequip_message"),
                                                miscs[0] == null ? "---" : Messages.TitleCase(miscs[0].ToString()),
                                                miscs[1] == null ? "---" : Messages.TitleCase(miscs[1].ToString()),
                                                miscs[2] == null ? "---" : Messages.TitleCase(miscs[2].ToString()));
                wnd.ResetEnabled(enabled);
                wnd.selectAction = (index) =>
                {
                    KindofMisc equipped = miscs[index];
                    int slot = Dungeon.quickslot.GetSlot(this);
                    Detach(hero.belongings.backpack);

                    if (equipped.DoUnequip(hero, true, false))
                    {
                        //swap out equip in misc slot if needed
                        if (index == 0 && this is Ring)
                        {
                            hero.belongings.artifact = (Artifact)hero.belongings.misc;
                            hero.belongings.misc = null;
                        }
                        else if (index == 2 && this is Artifact)
                        {
                            hero.belongings.ring = (Ring)hero.belongings.misc;
                            hero.belongings.misc = null;
                        }
                        DoEquip(hero);
                    }
                    else
                    {
                        Collect();
                    }

                    if (slot != -1)
                        Dungeon.quickslot.SetSlot(slot, this);

                    Item.UpdateQuickslot();
                };

                GameScene.Show(wnd);

                return false;
            }
            else
            {
                if (this is Artifact)
                {
                    if (hero.belongings.artifact == null)
                        hero.belongings.artifact = (Artifact)this;
                    else
                        hero.belongings.misc = (Artifact)this;
                }
                else if (this is Ring)
                {
                    if (hero.belongings.ring == null)
                        hero.belongings.ring = (Ring)this;
                    else
                        hero.belongings.misc = (Ring)this;
                }

                Detach(hero.belongings.backpack);

                Activate(hero);

                cursedKnown = true;
                if (cursed)
                {
                    EquipCursed(hero);
                    //GLog.Negative(Messages.Get(this, "equip_cursed", this));
                    GLog.Negative(Messages.Get(this, "equip_cursed"));  // mod
                }

                hero.SpendAndNext(TIME_TO_EQUIP);
                return true;
            }
        }

        private class KindOfMiscWndOptions : WndOptions
        {
            internal bool[] enabled;

            public KindOfMiscWndOptions(string title, string message, params string[] options)
                : base(title, message, options)
            { }

            // step1. WndOptions()에서 Enabled()가호출됨 (Enabled내부에서 항상 true로 리턴)
            // step2. ResetEnabled(...)가 호출됨 (step1에서 '모두 true로 설정한 버튼 enable상태'를 reset)
            public void ResetEnabled(bool[] enabled)
            {
                this.enabled = enabled;

                int index = 0;
                foreach (var member in members)
                {
                    if (member != null && member is ActionRedButton)
                    {
                        ((ActionRedButton)member).Enable(Enabled(index));
                        ++index;
                    }
                }
            }

            protected override bool Enabled(int index)
            {
                if (enabled == null)    // if (WndOptions()에서 호출될 때)?
                    return true;

                return enabled[index];
            }
        }

        public override bool DoUnequip(Hero hero, bool collect, bool single)
        {
            if (base.DoUnequip(hero, collect, single))
            {
                if (hero.belongings.artifact == this)
                {
                    hero.belongings.artifact = null;
                }
                else if (hero.belongings.misc == this)
                {
                    hero.belongings.misc = null;
                }
                else if (hero.belongings.ring == this)
                {
                    hero.belongings.ring = null;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool IsEquipped(Hero hero)
        {
            return hero.belongings.artifact == this ||
                hero.belongings.misc == this ||
                hero.belongings.ring == this;
        }
    }
}