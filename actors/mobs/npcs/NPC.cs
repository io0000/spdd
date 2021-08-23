namespace spdd.actors.mobs.npcs
{
    public class NPC : Mob
    {
        protected NPC()
        {
            HP = HT = 1;
            EXP = 0;

            alignment = Alignment.NEUTRAL;
            state = PASSIVE;
        }

        public override void Beckon(int cell)
        { }
    }
}
