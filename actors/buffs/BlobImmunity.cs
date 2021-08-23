using System;
using spdd.ui;
using spdd.actors.blobs;
using spdd.actors.mobs;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class BlobImmunity : FlavourBuff
    {
        public BlobImmunity()
        {
            InitInstance();

            type = BuffType.POSITIVE;
        }

        public const float DURATION = 20.0f;

        public override int Icon()
        {
            return BuffIndicator.IMMUNITY;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - Visualcooldown()) / DURATION);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        private void InitInstance()
        {
            //all harmful blobs
            immunities.Add(typeof(Blizzard));
            immunities.Add(typeof(ConfusionGas));
            immunities.Add(typeof(CorrosiveGas));
            immunities.Add(typeof(Electricity));
            immunities.Add(typeof(Fire));
            immunities.Add(typeof(Freezing));
            immunities.Add(typeof(Inferno));
            immunities.Add(typeof(ParalyticGas));
            immunities.Add(typeof(Regrowth));
            immunities.Add(typeof(SmokeScreen));
            immunities.Add(typeof(StenchGas));
            immunities.Add(typeof(StormCloud));
            immunities.Add(typeof(ToxicGas));
            immunities.Add(typeof(Web));

            immunities.Add(typeof(NewTengu.FireAbility.FireBlob));
            immunities.Add(typeof(NewTengu.BombAbility.BombBlob));
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns());
        }
    }
}