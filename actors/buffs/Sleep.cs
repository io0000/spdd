namespace spdd.actors.buffs
{
    public class Sleep : FlavourBuff
    {
        public const float SWS = 1.5f;

        public override void Fx(bool on)
        {
            if (on) 
                target.sprite.Idle();
        }
    }
}