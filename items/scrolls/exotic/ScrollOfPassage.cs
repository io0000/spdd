using System;
using watabou.noosa;
using spdd.sprites;
using spdd.scenes;
using spdd.utils;
using spdd.plants;
using spdd.actors.buffs;
using spdd.items.artifacts;
using spdd.messages;

namespace spdd.items.scrolls.exotic
{
    public class ScrollOfPassage : ExoticScroll
    {
        public ScrollOfPassage()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_PASSAGE;
        }

        public override void DoRead()
        {
            SetKnown();

            if (Dungeon.BossLevel())
            {
                GLog.Warning(Messages.Get(typeof(ScrollOfTeleportation), "no_tele"));
                return;
            }

            Buff buff = Dungeon.hero.FindBuff<TimekeepersHourglass.TimeFreeze>();
            if (buff != null)
                buff.Detach();
            buff = Dungeon.hero.FindBuff<Swiftthistle.TimeBubble>();
            if (buff != null)
                buff.Detach();

            InterlevelScene.mode = InterlevelScene.Mode.RETURN;
            InterlevelScene.returnDepth = Math.Max(1, (Dungeon.depth - 1 - (Dungeon.depth - 2) % 5));
            InterlevelScene.returnPos = -1;
            Game.SwitchScene(typeof(InterlevelScene));
        }
    }
}