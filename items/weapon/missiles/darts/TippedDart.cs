using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.items.wands;
using spdd.messages;
using spdd.plants;
using spdd.scenes;
using spdd.windows;

namespace spdd.items.weapon.missiles.darts
{
    public class TippedDart : Dart
    {
        public TippedDart()
        {
            tier = 2;

            //so that slightly more than 1.5x durability is needed for 2 uses
            baseUses = 0.65f;
        }

        private const string AC_CLEAN = "CLEAN";

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            actions.Remove(AC_TIP);
            actions.Add(AC_CLEAN);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            base.Execute(hero, action);
            if (action.Equals(AC_CLEAN))
            {
                var wnd = new WndOptions(
                    Messages.Get(this, "clean_title"),
                    Messages.Get(this, "clean_desc"),
                    Messages.Get(this, "clean_all"),
                    Messages.Get(this, "clean_one"),
                    Messages.Get(this, "cancel"));

                wnd.selectAction = (index) =>
                {
                    if (index == 0)
                    {
                        DetachAll(hero.belongings.backpack);
                        new Dart().Quantity(quantity).Collect();

                        hero.Spend(1f);
                        hero.Busy();
                        hero.sprite.Operate(hero.pos);
                    }
                    else if (index == 1)
                    {
                        Detach(hero.belongings.backpack);
                        if (!new Dart().Collect())
                            Dungeon.level.Drop(new Dart(), hero.pos).sprite.Drop();

                        hero.Spend(1f);
                        hero.Busy();
                        hero.sprite.Operate(hero.pos);
                    }
                };

                GameScene.Show(wnd);
            }
        }

        //exact same damage as regular darts, despite being higher tier.

        protected override void RangedHit(Character enemy, int cell)
        {
            targetPos = cell;
            base.RangedHit(enemy, cell);

            //need to spawn a dart
            if (durability <= 0)
            {
                //attempt to stick the dart to the enemy, just drop it if we can't.
                Dart d = new Dart();
                if (enemy.IsAlive() && sticky)
                {
                    var p = Buff.Affect<PinCushion>(enemy);
                    if (p.target == enemy)
                    {
                        p.Stick(d);
                        return;
                    }
                }
                Dungeon.level.Drop(d, enemy.pos).sprite.Drop();
            }
        }

        private static int targetPos = -1;

        protected override float DurabilityPerUse()
        {
            float use = base.DurabilityPerUse();

            if (Dungeon.hero.subClass == HeroSubClass.WARDEN)
            {
                use /= 2f;
            }

            //checks both destination and source position
            float lotusPreserve = 0f;
            if (targetPos != -1)
            {
                foreach (var ch in Actor.Chars())
                {
                    if (ch is WandOfRegrowth.Lotus)
                    {
                        WandOfRegrowth.Lotus l = (WandOfRegrowth.Lotus)ch;
                        if (l.InRange(targetPos))
                        {
                            lotusPreserve = Math.Max(lotusPreserve, l.SeedPreservation());
                        }
                    }
                }
                targetPos = -1;
            }
            int p = curUser == null ? Dungeon.hero.pos : curUser.pos;
            foreach (var ch in Actor.Chars())
            {
                if (ch is WandOfRegrowth.Lotus)
                {
                    WandOfRegrowth.Lotus l = (WandOfRegrowth.Lotus)ch;
                    if (l.InRange(p))
                    {
                        lotusPreserve = Math.Max(lotusPreserve, l.SeedPreservation());
                    }
                }
            }
            use *= (1f - lotusPreserve);

            return use;
        }

        public override int Value()
        {
            //value of regular dart plus half of the seed
            return 8 * quantity;
        }

        private static Dictionary<Type, Type> types = new Dictionary<Type, Type>();

        static TippedDart()
        {
            types.Add(typeof(Blindweed.Seed),    typeof(BlindingDart));
            types.Add(typeof(Dreamfoil.Seed),    typeof(SleepDart));
            types.Add(typeof(Earthroot.Seed),    typeof(ParalyticDart));
            types.Add(typeof(Fadeleaf.Seed),     typeof(DisplacingDart));
            types.Add(typeof(Firebloom.Seed),    typeof(IncendiaryDart));
            types.Add(typeof(Icecap.Seed),       typeof(ChillingDart));
            types.Add(typeof(Rotberry.Seed),     typeof(RotDart));
            types.Add(typeof(Sorrowmoss.Seed),   typeof(PoisonDart));
            types.Add(typeof(Starflower.Seed),   typeof(HolyDart));
            types.Add(typeof(Stormvine.Seed),    typeof(ShockingDart));
            types.Add(typeof(Sungrass.Seed),     typeof(HealingDart));
            types.Add(typeof(Swiftthistle.Seed), typeof(AdrenalineDart));
        }

        public static TippedDart GetTipped(Plant.Seed s, int quantity)
        {
            var seedType = s.GetType();
            var dart = (TippedDart)Reflection.NewInstance(types[seedType]);
            dart.Quantity(quantity);
            return dart;
        }

        public static TippedDart RandomTipped(int quantity)
        {
            Plant.Seed s;
            do
            {
                s = (Plant.Seed)Generator.RandomUsingDefaults(Generator.Category.SEED);
            }
            while (!types.ContainsKey(s.GetType()));

            return GetTipped(s, quantity);
        }
    }
}