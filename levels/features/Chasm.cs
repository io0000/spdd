using System;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.items.artifacts;
using spdd.items.spells;
using spdd.levels.rooms.special;
using spdd.messages;
using spdd.plants;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;
using spdd.windows;

namespace spdd.levels.features
{
    public class Chasm
    {
        public static bool jumpConfirmed;

        public static void HeroJump(Hero hero)
        {
            var wnd = new WndOptions(
                Messages.Get(typeof(Chasm), "chasm"),
                Messages.Get(typeof(Chasm), "jump"),
                Messages.Get(typeof(Chasm), "yes"),
                Messages.Get(typeof(Chasm), "no"));

            wnd.selectAction = (index) =>
            {
                if (index == 0)
                {
                    jumpConfirmed = true;
                    hero.Resume();
                }
            };

            GameScene.Show(wnd);
        }

        public static void HeroFall(int pos)
        {
            jumpConfirmed = false;

            Sample.Instance.Play(Assets.Sounds.FALLING);

            var buff1 = Dungeon.hero.FindBuff<TimekeepersHourglass.TimeFreeze>();
            if (buff1 != null)
                buff1.Detach();
            var buff2 = Dungeon.hero.FindBuff<Swiftthistle.TimeBubble>();
            if (buff2 != null)
                buff2.Detach();

            if (Dungeon.hero.IsAlive())
            {
                Dungeon.hero.Interrupt();
                InterlevelScene.mode = InterlevelScene.Mode.FALL;

                if (Dungeon.level is RegularLevel)
                {
                    var room = ((RegularLevel)Dungeon.level).Room(pos);
                    InterlevelScene.fallIntoPit = room != null && room is WeakFloorRoom;
                }
                else
                {
                    InterlevelScene.fallIntoPit = false;
                }

                Game.SwitchScene(typeof(InterlevelScene));
            }
            else
            {
                Dungeon.hero.sprite.visible = false;
            }
        }

        public static void HeroLand()
        {
            var hero = Dungeon.hero;

            var b = hero.FindBuff<FeatherFall.FeatherBuff>();
            if (b != null)
            {
                //TODO visuals
                b.Detach();
                return;
            }

            Camera.main.Shake(4, 1.0f);

            Dungeon.level.OccupyCell(hero);
            Buff.Prolong<Cripple>(hero, Cripple.DURATION);

            //The lower the hero's HP, the more bleed and the less upfront damage.
            //Hero has a 50% chance to bleed out at 66% HP, and begins to risk instant-death at 25%
            Buff.Affect<FallBleed>(hero).Set((int)Math.Round(hero.HT / (6f + (6f * (hero.HP / (float)hero.HT))), MidpointRounding.AwayFromZero));
            hero.Damage(Rnd.IntRange(hero.HT / 3, hero.HT / 2), new ChasmDoom());
        }

        public class ChasmDoom : Hero.IDoom
        {
            public void OnDeath()
            {
                BadgesExtensions.ValidateDeathFromFalling();
                Dungeon.Fail(typeof(Chasm));
                GLog.Negative(Messages.Get(typeof(Chasm), "ondeath"));
            }
        }

        public static void MobFall(Mob mob)
        {
            if (mob.IsAlive())
                mob.Die(typeof(Chasm));

            ((MobSprite)mob.sprite).Fall();
        }

        [SPDStatic]
        public class Falling : Buff
        {
            public Falling()
            {
                actPriority = VFX_PRIO;
            }

            public override bool Act()
            {
                Chasm.HeroLand();
                Detach();
                return true;
            }
        }

        [SPDStatic]
        public class FallBleed : Bleeding, Hero.IDoom
        {
            public void OnDeath()
            {
                BadgesExtensions.ValidateDeathFromFalling();
            }
        }
    }
}