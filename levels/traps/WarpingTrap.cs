using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.effects;
using spdd.items;
using spdd.items.scrolls;
using spdd.messages;
using spdd.scenes;
using spdd.utils;

namespace spdd.levels.traps
{
    public class WarpingTrap : Trap
    {
        public WarpingTrap()
        {
            color = TEAL;
            shape = STARS;
        }

        public override void Activate()
        {
            CellEmitter.Get(pos).Start(Speck.Factory(Speck.LIGHT), 0.2f, 3);
            Sample.Instance.Play(Assets.Sounds.TELEPORT);

            Character ch = Actor.FindChar(pos);
            if (ch != null && !ch.flying)
            {
                if (ch is Hero)
                {
                    ScrollOfTeleportation.TeleportHero((Hero)ch);
                    BArray.SetFalse(Dungeon.level.visited);
                    BArray.SetFalse(Dungeon.level.mapped);
                    GameScene.UpdateFog();
                    Dungeon.Observe();
                }
                else
                {
                    int count = 10;
                    int pos;
                    do
                    {
                        pos = Dungeon.level.RandomRespawnCell(ch);
                        if (count-- <= 0)
                            break;
                    }
                    while (pos == -1);

                    if (pos == -1 || Dungeon.BossLevel())
                    {
                        GLog.Warning(Messages.Get(typeof(ScrollOfTeleportation), "no_tele"));
                    }
                    else
                    {
                        ch.pos = pos;
                        if (ch is Mob && ((Mob)ch).state == ((Mob)ch).HUNTING)
                        {
                            ((Mob)ch).state = ((Mob)ch).WANDERING;
                        }
                        ch.sprite.Place(ch.pos);
                        ch.sprite.visible = Dungeon.level.heroFOV[pos];
                    }
                }
            }

            Heap heap = Dungeon.level.heaps[pos];

            if (heap != null)
            {
                int cell = Dungeon.level.RandomRespawnCell(null);

                Item item = heap.PickUp();

                if (cell != -1)
                    Dungeon.level.Drop(item, cell);
            }
        }
    }
}