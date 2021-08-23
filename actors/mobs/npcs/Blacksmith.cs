using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.items;
using spdd.items.armor;
using spdd.items.weapon;
using spdd.items.quest;
using spdd.items.scrolls;
using spdd.levels.rooms;
using spdd.levels.rooms.standard;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.windows;
using spdd.journal;
using spdd.messages;

namespace spdd.actors.mobs.npcs
{
    public class Blacksmith : NPC
    {
        public Blacksmith()
        {
            spriteClass = typeof(BlacksmithSprite);
            properties.Add(Property.IMMOVABLE);
        }

        public override bool Act()
        {
            if (Dungeon.level.heroFOV[pos] && !Quest.reforged)
            {
                Notes.Add(Notes.Landmark.TROLL);
            }
            return base.Act();
        }

        public override bool Interact(Character c)
        {
            sprite.TurnTo(pos, c.pos);

            if (c != Dungeon.hero)
            {
                return true;
            }

            if (!Quest.given)
            {
                var wndQuest = new WndQuestBlacksmith(this,
                    Quest.alternative ? Messages.Get(this, "blood_1") : Messages.Get(this, "gold_1"));

                GameScene.Show(wndQuest);

                Notes.Add(Notes.Landmark.TROLL);
            }
            else if (!Quest.completed)
            {
                if (Quest.alternative)
                {
                    var pick = Dungeon.hero.belongings.GetItem<Pickaxe>();
                    if (pick == null)
                    {
                        Tell(Messages.Get(this, "lost_pick"));
                    }
                    else if (!pick.bloodStained)
                    {
                        Tell(Messages.Get(this, "blood_2"));
                    }
                    else
                    {
                        if (pick.IsEquipped(Dungeon.hero))
                            pick.DoUnequip(Dungeon.hero, false);

                        pick.Detach(Dungeon.hero.belongings.backpack);
                        Tell(Messages.Get(this, "completed"));

                        Quest.completed = true;
                        Quest.reforged = false;
                    }
                }
                else
                {
                    var pick = Dungeon.hero.belongings.GetItem<Pickaxe>();
                    var gold = Dungeon.hero.belongings.GetItem<DarkGold>();

                    if (pick == null)
                    {
                        Tell(Messages.Get(this, "lost_pick"));
                    }
                    else if (gold == null || gold.Quantity() < 15)
                    {
                        Tell(Messages.Get(this, "gold_2"));
                    }
                    else
                    {
                        if (pick.IsEquipped(Dungeon.hero))
                            pick.DoUnequip(Dungeon.hero, false);

                        pick.Detach(Dungeon.hero.belongings.backpack);
                        gold.DetachAll(Dungeon.hero.belongings.backpack);
                        Tell(Messages.Get(this, "completed"));

                        Quest.completed = true;
                        Quest.reforged = false;
                    }
                }
            }
            else if (!Quest.reforged)
            {
                GameScene.Show(new WndBlacksmith(this, Dungeon.hero));
            }
            else
            {
                Tell(Messages.Get(this, "get_lost"));
            }

            return true;
        }

        private void Tell(string text)
        {
            GameScene.Show(new WndQuest(this, text));
        }

        public static string Verify(Item item1, Item item2)
        {
            if (item1 == item2 && (item1.Quantity() == 1 && item2.Quantity() == 1))
                return Messages.Get(typeof(Blacksmith), "same_item");

            if (item1.GetType() != item2.GetType())
                return Messages.Get(typeof(Blacksmith), "diff_type");

            if (!item1.IsIdentified() || !item2.IsIdentified())
                return Messages.Get(typeof(Blacksmith), "un_ided");

            if (item1.cursed || item2.cursed)
                return Messages.Get(typeof(Blacksmith), "cursed");

            if (item1.GetLevel() < 0 || item2.GetLevel() < 0)
                return Messages.Get(typeof(Blacksmith), "degraded");

            if (!item1.IsUpgradable() || !item2.IsUpgradable())
                return Messages.Get(typeof(Blacksmith), "cant_reforge");

            return null;
        }

        public static void Upgrade(Item item1, Item item2)
        {
            Item first, second;
            if (item2.GetLevel() > item1.GetLevel())
            {
                first = item2;
                second = item1;
            }
            else
            {
                first = item1;
                second = item2;
            }

            Sample.Instance.Play(Assets.Sounds.EVOKE);
            ScrollOfUpgrade.Upgrade(Dungeon.hero);
            Item.Evoke(Dungeon.hero);

            if (second.IsEquipped(Dungeon.hero))
                ((EquipableItem)second).DoUnequip(Dungeon.hero, false);
            second.Detach(Dungeon.hero.belongings.backpack);

            if (second is Armor)
            {
                var seal = ((Armor)second).CheckSeal();
                if (seal != null)
                {
                    Dungeon.level.Drop(seal, Dungeon.hero.pos);
                }
            }

            if (first.IsEquipped(Dungeon.hero))
                ((EquipableItem)first).DoUnequip(Dungeon.hero, true);

            //preserves enchant/glyphs if present
            if (first is Weapon && ((Weapon)first).HasGoodEnchant())
            {
                ((Weapon)first).Upgrade(true);
            }
            else if (first is Armor && ((Armor)first).HasGoodGlyph())
            {
                ((Armor)first).Upgrade(true);
            }
            else
            {
                first.Upgrade();
            }

            if (!Dungeon.hero.belongings.Contains(first))
            {
                if (!first.Collect())
                {
                    Dungeon.level.Drop(first, Dungeon.hero.pos);
                }
            }

            Dungeon.hero.SpendAndNext(2f);
            BadgesExtensions.ValidateItemLevelAquired(first);
            Item.UpdateQuickslot();

            Quest.reforged = true;

            Notes.Remove(Notes.Landmark.TROLL);
        }

        public override int DefenseSkill(Character enemy)
        {
            return INFINITE_EVASION;
        }

        public override void Damage(int dmg, object src)
        { }

        public override void Add(Buff buff)
        { }

        public override bool Reset()
        {
            return true;
        }

        public class Quest
        {
            private static bool spawned;

            public static bool alternative;
            public static bool given;
            public static bool completed;
            public static bool reforged;

            public static void Reset()
            {
                spawned = false;
                given = false;
                completed = false;
                reforged = false;
            }

            private const string NODE = "blacksmith";

            private const string SPAWNED = "spawned";
            private const string ALTERNATIVE = "alternative";
            private const string GIVEN = "given";
            private const string COMPLETED = "completed";
            private const string REFORGED = "reforged";

            public static void StoreInBundle(Bundle bundle)
            {
                var node = new Bundle();

                node.Put(SPAWNED, spawned);

                if (spawned)
                {
                    node.Put(ALTERNATIVE, alternative);
                    node.Put(GIVEN, given);
                    node.Put(COMPLETED, completed);
                    node.Put(REFORGED, reforged);
                }

                bundle.Put(NODE, node);
            }

            public static void RestoreFromBundle(Bundle bundle)
            {
                var node = bundle.GetBundle(NODE);

                if (!node.IsNull() && (spawned = node.GetBoolean(SPAWNED)))
                {
                    alternative = node.GetBoolean(ALTERNATIVE);
                    given = node.GetBoolean(GIVEN);
                    completed = node.GetBoolean(COMPLETED);
                    reforged = node.GetBoolean(REFORGED);
                }
                else
                {
                    Reset();
                }
            }

            public static List<Room> Spawn(List<Room> rooms)
            {
                if (!spawned && Dungeon.depth > 11 && Rnd.Int(15 - Dungeon.depth) == 0)
                {
                    rooms.Add(new BlacksmithRoom());
                    spawned = true;
                    alternative = Rnd.Int(2) == 0;

                    given = false;
                }

                return rooms;
            }
        }

        private class WndQuestBlacksmith : WndQuest
        {
            public WndQuestBlacksmith(NPC questgiver, string text)
                : base(questgiver, text)
            { }

            public override void OnBackPressed()
            {
                base.OnBackPressed();

                Quest.given = true;
                Quest.completed = false;

                var pick = new Pickaxe();

                if (pick.DoPickUp(Dungeon.hero))
                    GLog.Information(Messages.Get(Dungeon.hero, "you_now_have", pick.Name()));
                else
                    Dungeon.level.Drop(pick, Dungeon.hero.pos).sprite.Drop();
            }
        }
    }
}