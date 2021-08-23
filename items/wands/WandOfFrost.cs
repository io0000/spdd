using System;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.items.weapon.melee;
using spdd.mechanics;
using spdd.sprites;

namespace spdd.items.wands
{
    public class WandOfFrost : DamageWand
    {
        public WandOfFrost()
        {
            image = ItemSpriteSheet.WAND_FROST;
        }

        public override int Min(int lvl)
        {
            return 2 + lvl;
        }

        public override int Max(int lvl)
        {
            return 8 + 5 * lvl;
        }

        protected override void OnZap(Ballistic bolt)
        {
            var collisionPos = bolt.collisionPos;

            Heap heap = Dungeon.level.heaps[collisionPos];
            if (heap != null)
            {
                heap.Freeze();
            }

            var ch = Actor.FindChar(collisionPos);
            if (ch != null)
            {
                int damage = DamageRoll();

                if (ch.FindBuff<Frost>() != null)
                    return; //do nothing, can't affect a frozen target

                if (ch.FindBuff<Chill>() != null)
                {
                    //6.67% less damage per turn of chill remaining, to a max of 10 turns (50% dmg)
                    float chillturns = Math.Min(10, ch.FindBuff<Chill>().Cooldown());
                    damage = (int)Math.Round(damage * Math.Pow(0.9633f, chillturns), MidpointRounding.AwayFromZero);
                }
                else
                {
                    ch.sprite.Burst(new Color(0x99, 0xCC, 0xFF, 0xFF), BuffedLvl() / 2 + 2);
                }

                ProcessSoulMark(ch, ChargesPerCast());
                ch.Damage(damage, this);
                Sample.Instance.Play(Assets.Sounds.HIT_MAGIC, 1, 1.1f * Rnd.Float(0.87f, 1.15f));

                if (ch.IsAlive())
                {
                    if (Dungeon.level.water[ch.pos])
                    {
                        Buff.Affect<Chill>(ch, 4 + BuffedLvl());
                    }
                    else
                    {
                        Buff.Affect<Chill>(ch, 2 + BuffedLvl());
                    }
                }
            }
            else
            {
                Dungeon.level.PressCell(collisionPos);
            }
        }

        public override void Fx(Ballistic bolt, ICallback callback)
        {
            MagicMissile.BoltFromChar(curUser.sprite.parent,
                MagicMissile.FROST,
                curUser.sprite,
                bolt.collisionPos,
                callback);
            Sample.Instance.Play(Assets.Sounds.ZAP);
        }

        public override void OnHit(MagesStaff staff, Character attacker, Character defender, int damage)
        {
            Chill chill = defender.FindBuff<Chill>();
            if (chill != null && chill.Cooldown() >= Chill.DURATION)
            {
                //need to delay this through an actor so that the freezing isn't broken by taking damage from the staff hit.
                (new FrostBuff()).AttachTo(defender);
            }
        }

        private class FrostBuff : FlavourBuff
        {
            public FrostBuff()
            {
                actPriority = VFX_PRIO;
            }

            public override bool Act()
            {
                Buff.Affect<Frost>(target, Frost.DURATION);
                return base.Act();
            }
        }

        public override void StaffFx(MagesStaff.StaffParticle particle)
        {
            particle.SetColor(new Color(0x88, 0xCC, 0xFF, 0xFF));
            particle.am = 0.6f;
            particle.SetLifespan(2f);
            float angle = Rnd.Float(PointF.PI2);
            particle.speed.Polar(angle, 2f);
            particle.acc.Set(0f, 1f);
            particle.SetSize(0f, 1.5f);
            particle.RadiateXY(Rnd.Float(1f));
        }
    }
}