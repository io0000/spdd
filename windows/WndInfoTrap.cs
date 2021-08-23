using spdd.levels.traps;
using spdd.messages;
using spdd.tiles;

namespace spdd.windows
{
    public class WndInfoTrap : WndTitledMessage
    {
        public WndInfoTrap(Trap trap)
            : base(TerrainFeaturesTilemap.Tile(trap.pos, Dungeon.level.map[trap.pos]),
                  Messages.TitleCase(trap.name),
                  (!trap.active ? Messages.Get(typeof(WndInfoTrap), "inactive") + "\n\n" : "") + trap.Desc())
        { }
    }
}