using System;
using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.effects;
using spdd.items.artifacts;
using spdd.items.scrolls;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.utils;

namespace spdd.plants
{
    public class Fadeleaf : Plant
    {
        public Fadeleaf()
        {
            image = 10;
            seedClass = typeof(Seed);
        }

        public override void Activate(Character ch)
        {
            if (ch is Hero)
            {
                ((Hero)ch).curAction = null;

                if (((Hero)ch).subClass == HeroSubClass.WARDEN)
                {
                    if (Dungeon.BossLevel())
                    {
                        GLog.Warning(Messages.Get(typeof(ScrollOfTeleportation), "no_tele"));
                        return;
                    }

                    var buff1 = Dungeon.hero.FindBuff<TimekeepersHourglass.TimeFreeze>();
                    if (buff1 != null)
                        buff1.Detach();

                    var buff2 = Dungeon.hero.FindBuff<Swiftthistle.TimeBubble>();
                    if (buff2 != null)
                        buff2.Detach();

                    InterlevelScene.mode = InterlevelScene.Mode.RETURN;
                    InterlevelScene.returnDepth = Math.Max(1, (Dungeon.depth - 1));
                    InterlevelScene.returnPos = -2;
                    Game.SwitchScene(typeof(InterlevelScene));
                }
                else
                {
                    ScrollOfTeleportation.TeleportHero((Hero)ch);
                }
            }
            else if (ch is Mob && !ch.Properties().Contains(Character.Property.IMMOVABLE))
            {
                if (!Dungeon.BossLevel())
                {
                    int count = 10;
                    int newPos;
                    do
                    {
                        newPos = Dungeon.level.RandomRespawnCell(ch);
                        if (count-- <= 0)
                            break;
                    }
                    while (newPos == -1);

                    if (newPos != -1)
                    {
                        ch.pos = newPos;
                        if (((Mob)ch).state == ((Mob)ch).HUNTING)
                            ((Mob)ch).state = ((Mob)ch).WANDERING;

                        ch.sprite.Place(ch.pos);
                        ch.sprite.visible = Dungeon.level.heroFOV[ch.pos];
                    }
                }
            }

            if (Dungeon.level.heroFOV[pos])
            {
                CellEmitter.Get(pos).Start(Speck.Factory(Speck.LIGHT), 0.2f, 3);
            }
        }

        [SPDStatic]
        public new class Seed : Plant.Seed
        {
            public Seed()
            {
                image = ItemSpriteSheet.SEED_FADELEAF;

                plantClass = typeof(Fadeleaf);
            }
        }
    }
}