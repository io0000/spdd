using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.hero;
using spdd.effects;
using spdd.items;
using spdd.messages;
using spdd.scenes;
using spdd.utils;

namespace spdd.levels.traps
{
    public class DisarmingTrap : Trap
    {
        public DisarmingTrap()
        {
            color = RED;
            shape = LARGE_DOT;
        }

        public override void Activate()
        {
            Heap heap = Dungeon.level.heaps[pos];

            if (heap != null)
            {
                int cell = Dungeon.level.RandomRespawnCell(null);

                if (cell != -1)
                {
                    Item item = heap.PickUp();
                    Dungeon.level.Drop(item, cell).seen = true;
                    foreach (int i in PathFinder.NEIGHBORS9)
                        Dungeon.level.visited[cell + i] = true;
                    GameScene.UpdateFog();

                    Sample.Instance.Play(Assets.Sounds.TELEPORT);
                    CellEmitter.Get(pos).Burst(Speck.Factory(Speck.LIGHT), 4);
                }
            }

            if (Dungeon.hero.pos == pos && !Dungeon.hero.flying)
            {
                Hero hero = Dungeon.hero;
                KindOfWeapon weapon = hero.belongings.weapon;

                if (weapon != null && !weapon.cursed)
                {
                    int cell;
                    int tries = 20;
                    do
                    {
                        cell = Dungeon.level.RandomRespawnCell(null);
                        if (tries-- < 0 && cell != -1)
                            break;

                        PathFinder.BuildDistanceMap(pos, Dungeon.level.passable);
                    }
                    while (cell == -1 || PathFinder.distance[cell] < 10 || PathFinder.distance[cell] > 20);

                    hero.belongings.weapon = null;
                    Dungeon.quickslot.ClearItem(weapon);
                    Item.UpdateQuickslot();

                    Dungeon.level.Drop(weapon, cell).seen = true;
                    foreach (int i in PathFinder.NEIGHBORS9)
                        Dungeon.level.mapped[cell + i] = true;
                    GameScene.UpdateFog(cell, 1);

                    GLog.Warning(Messages.Get(this, "disarm"));

                    Sample.Instance.Play(Assets.Sounds.TELEPORT);
                    CellEmitter.Get(pos).Burst(Speck.Factory(Speck.LIGHT), 4);
                }
            }
        }
    }
}