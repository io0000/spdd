using spdd.actors;
using spdd.actors.mobs;

namespace spdd.levels.traps
{
    public class TenguDartTrap : PoisonDartTrap
    {
        public TenguDartTrap()
        {
            canBeHidden = true;
            canBeSearched = false;
        }

        protected override int PoisonAmount()
        {
            return 8; //17 damage total
        }

        protected override bool CanTarget(Character ch)
        {
            return !(ch is NewTengu);
        }
    }
}