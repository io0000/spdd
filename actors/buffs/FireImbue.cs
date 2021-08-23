using System;
using watabou.utils;
using spdd.ui;
using spdd.levels;
using spdd.scenes;
using spdd.effects.particles;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class FireImbue : FlavourBuff
    {
        public FireImbue()
        {
            type = BuffType.POSITIVE;
            announced = true;

            immunities.Add(typeof(Burning));
        }

        public const float DURATION = 50f;

        protected float left;

        private const string LEFT = "left";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(LEFT, left);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            left = bundle.GetFloat(LEFT);
        }

        public void Set(float duration)
        {
            this.left = duration;
        }

        public override bool Act()
        {
            if (Dungeon.level.map[target.pos] == Terrain.GRASS)
            {
                Level.Set(target.pos, Terrain.EMBERS);
                GameScene.UpdateMap(target.pos);
            }

            Spend(TICK);
            left -= TICK;
            if (left <= 0)
                Detach();

            return true;
        }

        public void Proc(Character enemy)
        {
            if (Rnd.Int(2) == 0)
                Buff.Affect<Burning>(enemy).Reignite(enemy);
            
            enemy.sprite.Emitter().Burst(FlameParticle.Factory, 2);
        }

        public override int Icon()
        {
            return BuffIndicator.FIRE;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - left + 1) / DURATION);
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns(left));
        }
    }
}