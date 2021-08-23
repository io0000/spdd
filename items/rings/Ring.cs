using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.sprites;
using spdd.journal;
using spdd.utils;
using spdd.actors;
using spdd.messages;

namespace spdd.items.rings
{
    public class Ring : KindofMisc
    {
        public Buff buff;

        private static Type[] rings = {
            typeof(RingOfAccuracy),
            typeof(RingOfEvasion),
            typeof(RingOfElements),
            typeof(RingOfForce),
            typeof(RingOfFuror),
            typeof(RingOfHaste),
            typeof(RingOfEnergy),
            typeof(RingOfMight),
            typeof(RingOfSharpshooting),
            typeof(RingOfTenacity),
            typeof(RingOfWealth)
        };

        static Dictionary<string, int> gems = new Dictionary<string, int>()
        {
            {"garnet", ItemSpriteSheet.RING_GARNET},
            {"ruby", ItemSpriteSheet.RING_RUBY},
            {"topaz", ItemSpriteSheet.RING_TOPAZ},
            {"emerald", ItemSpriteSheet.RING_EMERALD},
            {"onyx", ItemSpriteSheet.RING_ONYX},
            {"opal", ItemSpriteSheet.RING_OPAL},
            {"tourmaline", ItemSpriteSheet.RING_TOURMALINE},
            {"sapphire", ItemSpriteSheet.RING_SAPPHIRE},
            {"amethyst", ItemSpriteSheet.RING_AMETHYST},
            {"quartz", ItemSpriteSheet.RING_QUARTZ},
            {"agate", ItemSpriteSheet.RING_AGATE},
            {"diamond", ItemSpriteSheet.RING_DIAMOND}
        };

        private static ItemStatusHandler handler;

        private string gem;

        //rings cannot be 'used' like other equipment, so they ID purely based on exp
        private float levelsToID = 1;

        public static void InitGems()
        {
            handler = new ItemStatusHandler(rings, gems);
        }

        public static void Save(Bundle bundle)
        {
            handler.Save(bundle);
        }

        public static void SaveSelectively(Bundle bundle, List<Item> items)
        {
            handler.SaveSelectively(bundle, items);
        }

        public static void Restore(Bundle bundle)
        {
            handler = new ItemStatusHandler(rings, gems, bundle);
        }

        public Ring()
        {
            Reset();
        }

        //anonymous rings are always IDed, do not affect ID status,
        //and their sprite is replaced by a placeholder if they are not known,
        //useful for items that appear in UIs, or which are only spawned for their effects
        protected bool anonymous;

        public void Anonymize()
        {
            if (!IsKnown())
                image = ItemSpriteSheet.RING_HOLDER;
            anonymous = true;
        }

        public override void Reset()
        {
            base.Reset();
            levelsToID = 1;

            var type = GetType();
            if (handler != null && handler.Contains(type))
            {
                image = handler.Image(type);
                gem = handler.Label(type);
            }
        }

        public override void Activate(Character ch)
        {
            buff = Buff();
            buff.AttachTo(ch);
        }

        public override bool DoUnequip(Hero hero, bool collect, bool single)
        {
            if (base.DoUnequip(hero, collect, single))
            {
                hero.Remove(buff);
                buff = null;

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsKnown()
        {
            return anonymous || (handler != null && handler.IsKnown(GetType()));
        }

        public void SetKnown()
        {
            if (!anonymous)
            {
                if (!IsKnown())
                {
                    handler.Know(GetType());
                }

                if (Dungeon.hero.IsAlive())
                {
                    CatalogExtensions.SetSeen(GetType());
                }
            }
        }

        public override string Name()
        {
            return IsKnown() ? base.Name() : Messages.Get(typeof(Ring), gem);
        }

        public override string Info()
        {
            string desc = IsKnown() ? base.Desc() : Messages.Get(this, "unknown_desc");

            if (cursed && IsEquipped(Dungeon.hero))
            {
                desc += "\n\n" + Messages.Get(typeof(Ring), "cursed_worn");
            }
            else if (cursed && cursedKnown)
            {
                desc += "\n\n" + Messages.Get(typeof(Ring), "curse_known");
            }
            else if (!IsIdentified() && cursedKnown)
            {
                desc += "\n\n" + Messages.Get(typeof(Ring), "not_cursed");
            }

            if (IsKnown())
            {
                desc += "\n\n" + StatsInfo();
            }

            return desc;
        }

        public virtual string StatsInfo()
        {
            return "";
        }

        public override Item Upgrade()
        {
            base.Upgrade();

            if (Rnd.Int(3) == 0)
                cursed = false;

            return this;
        }

        public override bool IsIdentified()
        {
            return base.IsIdentified() && IsKnown();
        }

        public override Item Identify()
        {
            SetKnown();
            levelsToID = 0;
            return base.Identify();
        }

        public override Item Random()
        {
            //+0: 66.67% (2/3)
            //+1: 26.67% (4/15)
            //+2: 6.67%  (1/15)
            int n = 0;

            if (Rnd.Int(3) == 0)
            {
                ++n;
                if (Rnd.Int(5) == 0)
                    ++n;
            }
            SetLevel(n);

            //30% chance to be cursed
            if (Rnd.Float() < 0.3f)
                cursed = true;

            return this;
        }

        public static HashSet<Type> GetKnown()
        {
            return handler.Known();
        }

        public static HashSet<Type> GetUnknown()
        {
            return handler.Unknown();
        }

        //public static bool AllKnown()
        //{
        //    return handler.Known().Count == rings.Length - 2;
        //}

        public override int Value()
        {
            var price = 75;
            if (cursed && cursedKnown)
                price /= 2;

            if (levelKnown)
            {
                if (GetLevel() > 0)
                    price *= (GetLevel() + 1);
                else if (GetLevel() < 0)
                    price /= (1 - GetLevel());
            }

            if (price < 1)
                price = 1;

            return price;
        }

        public virtual RingBuff Buff()
        {
            return null;
        }

        private const string LEVELS_TO_ID = "levels_to_ID";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(LEVELS_TO_ID, levelsToID);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            levelsToID = bundle.GetFloat(LEVELS_TO_ID);
        }

        public override void OnHeroGainExp(float levelPercent, Hero hero)
        {
            if (IsIdentified() || !IsEquipped(hero))
                return;

            //becomes IDed after 1 level
            levelsToID -= levelPercent;
            if (levelsToID <= 0)
            {
                Identify();
                GLog.Positive(Messages.Get(typeof(Ring), "identify", ToString()));
                BadgesExtensions.ValidateItemLevelAquired(this);
            }
        }

        // Type - Class<?extends RingBuff>
        public static int GetBonus<T>(Character target) where T : RingBuff
        {
            int bonus = 0;
            foreach (T buff in target.Buffs<T>())
            {
                bonus += buff.Level();
            }
            return bonus;
        }

        public static int GetBuffedBonus<T>(Character target) where T : RingBuff
        {
            int bonus = 0;
            foreach (T buff in target.Buffs<T>())
            {
                bonus += buff.BuffedLvl();
            }
            return bonus;
        }

        public int SoloBonus()
        {
            if (cursed)
            {
                return Math.Min(0, GetLevel() - 2);
            }
            else
            {
                return GetLevel() + 1;
            }
        }

        public int SoloBuffedBonus()
        {
            if (cursed)
            {
                return Math.Min(0, BuffedLvl() - 2);
            }
            else
            {
                return BuffedLvl() + 1;
            }
        }

        public class RingBuff : Buff
        {
            protected readonly Ring ring;

            public RingBuff(Ring ring)
            {
                this.ring = ring;
            }

            public override bool Act()
            {
                Spend(TICK);

                return true;
            }

            public int Level()
            {
                return ring.SoloBonus();
            }

            public int BuffedLvl()
            {
                return ring.SoloBuffedBonus();
            }
        }
    }
}