namespace spdd.actors.buffs
{
    //buff whose only internal logic is to wait and detach after a time.
    public class FlavourBuff : Buff
    {
        public override bool Act()
        {
            Detach();
            return true;
        }

        //flavour buffs can all just rely on cooldown()
        protected string DispTurns()
        {
            return DispTurns(Visualcooldown());
        }
    }
}