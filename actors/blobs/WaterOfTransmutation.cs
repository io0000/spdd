using System;
using watabou.utils;
using spdd.effects;
using spdd.items;
using spdd.items.artifacts;
using spdd.items.potions;
using spdd.items.rings;
using spdd.items.scrolls;
using spdd.items.wands;
using spdd.items.weapon;
using spdd.items.weapon.melee;
using spdd.plants;
using spdd.actors.hero;
using spdd.journal;
using spdd.messages;

namespace spdd.actors.blobs
{
    public class WaterOfTransmutation : WellWater
    {
        protected override Item AffectItem(Item item, int pos)
        {
            if (item is MagesStaff)
            {
                item = ChangeStaff((MagesStaff)item);
            }
            else if (item is MeleeWeapon)
            {
                item = ChangeWeapon((MeleeWeapon)item);
            }
            else if (item is Scroll)
            {
                item = ChangeScroll((Scroll)item);
            }
            else if (item is Potion)
            {
                item = ChangePotion((Potion)item);
            }
            else if (item is Ring)
            {
                item = ChangeRing((Ring)item);
            }
            else if (item is Wand)
            {
                item = ChangeWand((Wand)item);
            }
            else if (item is Plant.Seed)
            {
                item = ChangeSeed((Plant.Seed)item);
            }            
            else if (item is Artifact)
            {
                item = ChangeArtifact((Artifact)item);
            }
            else
            {
                item = null;
            }

            //incase a never-seen item pops out
            if (item != null && item.IsIdentified())
                CatalogExtensions.SetSeen(item.GetType());

            return item;
        }

        protected override bool AffectHero(Hero hero)
        {
            return false;
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);
            emitter.Start(Speck.Factory(Speck.CHANGE), 0.2f, 0);
        }
        protected override Notes.Landmark Record()
        {
            return Notes.Landmark.WELL_OF_TRANSMUTATION;
        }

        private MagesStaff ChangeStaff(MagesStaff staff)
        {
            Type wandClass = staff.WandClass();

            if (wandClass == null)
                return null;

            Wand n;

            do
            {
                n = (Wand)Generator.Random(Generator.Category.WAND);
            } while (Challenges.IsItemBlocked(n) || n.GetType() == wandClass);

            n.SetLevel(0);
            n.Identify();
            staff.ImbueWand(n, null);

            return staff;
        }

        private Weapon ChangeWeapon(MeleeWeapon w)
        {
            Weapon n = null;
            Generator.Category c = Generator.wepTiers[w.tier - 1];

            do
            {
                var classes = c.GetClasses();
                var probs = c.GetProbs();
                Type t = classes[Rnd.Chances(probs)];
                n = (MeleeWeapon)Reflection.NewInstance(t);
            } while (Challenges.IsItemBlocked(n) || n.GetType() == w.GetType());

            var level = w.GetLevel();
            if (w.curseInfusionBonus) 
                --level;

            if (level > 0)
                n.Upgrade(level);
            else if (level < 0)
                n.Degrade(-level);

            n.enchantment = w.enchantment;
            n.curseInfusionBonus = w.curseInfusionBonus;
            n.levelKnown = w.levelKnown;
            n.cursedKnown = w.cursedKnown;
            n.cursed = w.cursed;
            n.augment = w.augment;

            return n;
        }

        private Ring ChangeRing(Ring r)
        {
            Ring n;
            do
            {
                n = (Ring)Generator.Random(Generator.Category.RING);
            } while (Challenges.IsItemBlocked(n) || n.GetType() == r.GetType());

            n.SetLevel(0);

            var level = r.GetLevel();
            if (level > 0)
                n.Upgrade(level);
            else if (level < 0)
                n.Degrade(-level);

            n.levelKnown = r.levelKnown;
            n.cursedKnown = r.cursedKnown;
            n.cursed = r.cursed;

            return n;
        }

        private Artifact ChangeArtifact(Artifact a)
        {
            Artifact n = Generator.RandomArtifact();

            if (n != null && !Challenges.IsItemBlocked(n))
            {
                n.cursedKnown = a.cursedKnown;
                n.cursed = a.cursed;
                n.levelKnown = a.levelKnown;
                n.TransferUpgrade(a.VisiblyUpgraded());
                return n;
            }

            return null;
        }

        private static Wand ChangeWand(Wand w)
        {
            Wand n;
            do
            {
                n = (Wand)Generator.Random(Generator.Category.WAND);
            } while (Challenges.IsItemBlocked(n) || n.GetType() == w.GetType());

            n.SetLevel(0);
            int level = w.GetLevel();
            if (w.curseInfusionBonus)
                --level;
            n.Upgrade(level);

            n.levelKnown = w.levelKnown;
            n.cursedKnown = w.cursedKnown;
            n.cursed = w.cursed;
            n.curseInfusionBonus = w.curseInfusionBonus;

            return n;
        }

        private static Plant.Seed ChangeSeed(Plant.Seed s)
        {
            Plant.Seed n;

            do
            {
                n = (Plant.Seed)Generator.Random(Generator.Category.SEED);
            } while (n.GetType() == s.GetType());

            return n;
        }

        private Scroll ChangeScroll(Scroll s)
        {
            if (s is ScrollOfUpgrade)
                return null;

            Scroll n;
            do
            {
                n = (Scroll) Generator.Random(Generator.Category.SCROLL);
            } 
            while (n.GetType() == s.GetType());

            return n;
        }

        private Potion ChangePotion(Potion p)
        {
            if (p is PotionOfStrength)
                return null;

            Potion n;
            do
            {
                n = (Potion) Generator.Random(Generator.Category.POTION);
            } 
            while (n.GetType() == p.GetType());
            
            return n;
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }
    }
}