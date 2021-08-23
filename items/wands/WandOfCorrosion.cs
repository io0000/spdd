using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.effects;
using spdd.effects.particles;
using spdd.sprites;
using spdd.mechanics;
using spdd.scenes;
using spdd.items.weapon.melee;

namespace spdd.items.wands
{
    public class WandOfCorrosion : Wand
    {
        public WandOfCorrosion()
        {
            image = ItemSpriteSheet.WAND_CORROSION;

            collisionProperties = Ballistic.STOP_TARGET | Ballistic.STOP_TERRAIN;
        }

        protected override void OnZap(Ballistic bolt)
        {
            var collisionPos = bolt.collisionPos;

            CorrosiveGas gas = (CorrosiveGas)Blob.Seed(collisionPos, 50 + 10 * BuffedLvl(), typeof(CorrosiveGas));
            CellEmitter.Get(collisionPos).Burst(Speck.Factory(Speck.CORROSION), 10);
            gas.SetStrength(2 + BuffedLvl());
            GameScene.Add(gas);
            Sample.Instance.Play(Assets.Sounds.GAS);

            foreach (int i in PathFinder.NEIGHBORS9)
            {
                var ch = Actor.FindChar(collisionPos + i);
                if (ch != null)
                {
                    ProcessSoulMark(ch, ChargesPerCast());
                }
            }

            if (Actor.FindChar(collisionPos) == null)
            {
                Dungeon.level.PressCell(collisionPos);
            }
        }

        public override void Fx(Ballistic bolt, ICallback callback)
        {
            MagicMissile.BoltFromChar(
                    curUser.sprite.parent,
                    MagicMissile.CORROSION,
                    curUser.sprite,
                    bolt.collisionPos,
                    callback);
            Sample.Instance.Play(Assets.Sounds.ZAP);
        }

        public override void OnHit(MagesStaff staff, Character attacker, Character defender, int damage)
        {
            // lvl 0 - 33%
            // lvl 1 - 50%
            // lvl 2 - 60%
            if (Rnd.Int(BuffedLvl() + 3) >= 2)
            {
                Buff.Affect<Ooze>(defender).Set(Ooze.DURATION);
                CellEmitter.Center(defender.pos).Burst(CorrosionParticle.Splash, 5);
            }
        }

        public override void StaffFx(MagesStaff.StaffParticle particle)
        {
            var c1 = new Color(0xAA, 0xAA, 0xAA, 0xFF);
            var c2 = new Color(0xFF, 0x88, 0x00, 0xFF);
            particle.SetColor(ColorMath.Random(c1, c2));
            particle.am = 0.6f;
            particle.SetLifespan(1f);
            particle.acc.Set(0, 20);
            particle.SetSize(0.5f, 3f);
            particle.ShuffleXY(1f);
        }
    }
}