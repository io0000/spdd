using spdd.ui;

namespace spdd.actors.buffs
{
    public class Rage : FlavourBuff
    {
        public override int Icon()
        {
            return BuffIndicator.RAGE;
        }

        public override string ToString()
        {
            return "Blinded with rage";
        }
    }
}