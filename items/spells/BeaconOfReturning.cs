using System;
using watabou.utils;
using watabou.noosa;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.sprites;
using spdd.items.scrolls;
using spdd.items.scrolls.exotic;
using spdd.items.artifacts;
using spdd.scenes;
using spdd.windows;
using spdd.utils;
using spdd.plants;
using spdd.messages;

namespace spdd.items.spells
{
    public class BeaconOfReturning : Spell
    {
        public BeaconOfReturning()
        {
            image = ItemSpriteSheet.RETURN_BEACON;
        }

        public int returnDepth = -1;
        public int returnPos;

        protected override void OnCast(Hero hero)
        {
            if (returnDepth == -1)
            {
                SetBeacon(hero);
            }
            else
            {
                var wnd = new WndOptions(
                    Messages.TitleCase(Name()),
                    Messages.Get(typeof(BeaconOfReturning), "wnd_body"),
                    Messages.Get(typeof(BeaconOfReturning), "wnd_set"),
                    Messages.Get(typeof(BeaconOfReturning), "wnd_return"));

                wnd.selectAction = (index) =>
                {
                    if (index == 0)
                    {
                        SetBeacon(hero);
                    }
                    else if (index == 1)
                    {
                        ReturnBeacon(hero);
                    }
                };

                GameScene.Show(wnd);
            }
        }

        //we reset return depth when beacons are dropped to prevent
        //having two stacks of beacons with different return locations
        public override void OnThrow(int cell)
        {
            returnDepth = -1;
            base.OnThrow(cell);
        }

        public override void DoDrop(Hero hero)
        {
            returnDepth = -1;
            base.DoDrop(hero);
        }

        private void SetBeacon(Hero hero)
        {
            returnDepth = Dungeon.depth;
            returnPos = hero.pos;

            hero.Spend(1f);
            hero.Busy();

            GLog.Information(Messages.Get(this, "set"));

            hero.sprite.Operate(hero.pos);
            Sample.Instance.Play(Assets.Sounds.BEACON);
            UpdateQuickslot();
        }

        private void ReturnBeacon(Hero hero)
        {
            if (Dungeon.BossLevel())
            {
                GLog.Warning(Messages.Get(this, "preventing"));
                return;
            }

            for (int i = 0; i < PathFinder.NEIGHBORS8.Length; ++i)
            {
                var ch = Actor.FindChar(hero.pos + PathFinder.NEIGHBORS8[i]);
                if (ch != null && ch.alignment == Character.Alignment.ENEMY)
                {
                    GLog.Warning(Messages.Get(this, "creatures"));
                    return;
                }
            }

            if (returnDepth == Dungeon.depth)
            {
                ScrollOfTeleportation.Appear(hero, returnPos);
                foreach (Mob m in Dungeon.level.mobs)
                {
                    if (m.pos == hero.pos)
                    {
                        //displace mob
                        foreach (int i in PathFinder.NEIGHBORS8)
                        {
                            if (Actor.FindChar(m.pos + i) == null && Dungeon.level.passable[m.pos + i])
                            {
                                m.pos += i;
                                m.sprite.Point(m.sprite.WorldToCamera(m.pos));
                                break;
                            }
                        }
                    }
                }
                Dungeon.level.OccupyCell(hero);
                Dungeon.Observe();
                GameScene.UpdateFog();
            }
            else
            {
                var buff1 = Dungeon.hero.FindBuff<TimekeepersHourglass.TimeFreeze>();
                if (buff1 != null)
                    buff1.Detach();
                var buff2 = Dungeon.hero.FindBuff<Swiftthistle.TimeBubble>();
                if (buff2 != null)
                    buff2.Detach();

                InterlevelScene.mode = InterlevelScene.Mode.RETURN;
                InterlevelScene.returnDepth = returnDepth;
                InterlevelScene.returnPos = returnPos;
                Game.SwitchScene(typeof(InterlevelScene));
            }
            Detach(hero.belongings.backpack);
        }

        public override string Desc()
        {
            string desc = base.Desc();
            if (returnDepth != -1)
            {
                desc += "\n\n" + Messages.Get(this, "desc_set", returnDepth);
            }
            return desc;
        }

        private static ItemSprite.Glowing WHITE = new ItemSprite.Glowing(new Color(0xFF, 0xFF, 0xFF, 0xFF));

        public override ItemSprite.Glowing Glowing()
        {
            return returnDepth != -1 ? WHITE : null;
        }

        private const string DEPTH = "depth";
        private const string POS = "pos";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(DEPTH, returnDepth);
            if (returnDepth != -1)
            {
                bundle.Put(POS, returnPos);
            }
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            returnDepth = bundle.GetInt(DEPTH);
            returnPos = bundle.GetInt(POS);
        }

        public override int Value()
        {
            //prices of ingredients, divided by output quantity
            return (int)Math.Round(quantity * ((50 + 40) / 5f), MidpointRounding.AwayFromZero);
        }

        public class Recipe : items.Recipe.SimpleRecipe
        {
            public Recipe()
            {
                inputs = new Type[] { typeof(ScrollOfPassage), typeof(ArcaneCatalyst) };
                inQuantity = new int[] { 1, 1 };

                cost = 10;

                output = typeof(BeaconOfReturning);
                outQuantity = 5;
            }
        }
    }
}