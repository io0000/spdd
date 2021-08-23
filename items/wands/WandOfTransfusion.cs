using System;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.mobs;
using spdd.effects;
using spdd.effects.particles;
using spdd.items.weapon.melee;
using spdd.mechanics;
using spdd.messages;
using spdd.sprites;
using spdd.tiles;
using spdd.utils;

namespace spdd.items.wands
{
    public class WandOfTransfusion : Wand
    {
        public WandOfTransfusion()
        {
            image = ItemSpriteSheet.WAND_TRANSFUSION;

            collisionProperties = Ballistic.PROJECTILE;
        }

        private bool freeCharge;

        protected override void OnZap(Ballistic beam)
        {
            foreach (int c in beam.SubPath(0, beam.dist))
            {
                CellEmitter.Center(c).Burst(BloodParticle.Burst, 1);
            }

            int cell = beam.collisionPos;
            var ch = Actor.FindChar(cell);
            if (ch is Mob)
            {
                ProcessSoulMark(ch, ChargesPerCast());

                //this wand does different things depending on the target.

                //heals/shields an ally or a charmed enemy while damaging self
                if (ch.alignment == Character.Alignment.ALLY || ch.FindBuff<Charm>() != null)
                {
                    // 10% of max hp
                    int selfDmg = (int)Math.Round(curUser.HT * 0.10f, MidpointRounding.AwayFromZero);

                    int healing = selfDmg + 3 * base.BuffedLvl();
                    int shielding = (ch.HP + healing) - ch.HT;
                    if (shielding > 0)
                    {
                        healing -= shielding;
                        Buff.Affect<Barrier>(ch).SetShield(shielding);
                    }
                    else
                    {
                        shielding = 0;
                    }

                    ch.HP += healing;

                    ch.sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 2 + base.BuffedLvl() / 2);
                    ch.sprite.ShowStatus(CharSprite.POSITIVE, "+%dHP", healing + shielding);

                    if (!freeCharge)
                    {
                        DamageHero(selfDmg);
                    }
                    else
                    {
                        freeCharge = false;
                    }
                }
                //for enemies...
                else
                {
                    //charms living enemies
                    if (!ch.Properties().Contains(Character.Property.UNDEAD))
                    {
                        Buff.Affect<Charm>(ch, Charm.DURATION / 2f).obj = curUser.Id();
                        ch.sprite.CenterEmitter().Start(Speck.Factory(Speck.HEART), 0.2f, 3 + base.BuffedLvl() / 2);
                    }
                    //harms the undead
                    else
                    {
                        ch.Damage(Rnd.NormalIntRange(3 + base.BuffedLvl() / 2, 6 + base.BuffedLvl()), this);
                        ch.sprite.Emitter().Start(ShadowParticle.Up, 0.05f, 10 + base.BuffedLvl());
                        Sample.Instance.Play(Assets.Sounds.BURNING);
                    }

                    //and grants a self shield
                    Buff.Affect<Barrier>(curUser).SetShield(5 + 2 * base.BuffedLvl());
                }
            }
        }

        //this wand costs health too
        private void DamageHero(int damage)
        {
            curUser.Damage(damage, this);

            if (!curUser.IsAlive())
            {
                Dungeon.Fail(GetType());
                GLog.Negative(Messages.Get(this, "ondeath"));
            }
        }

        protected override int InitialCharges()
        {
            return 1;
        }

        public override void OnHit(MagesStaff staff, Character attacker, Character defender, int damage)
        {
            // lvl 0 - 10%
            // lvl 1 - 18%
            // lvl 2 - 25%
            if (Rnd.Int(BuffedLvl() + 10) >= 9)
            {
                //grants a free use of the staff
                freeCharge = true;
                GLog.Positive(Messages.Get(this, "charged"));
                attacker.sprite.Emitter().Burst(BloodParticle.Burst, 20);
            }
        }

        public override void Fx(Ballistic beam, ICallback callback)
        {
            curUser.sprite.parent.Add(
                    new Beam.HealthRay(curUser.sprite.Center(), DungeonTilemap.RaisedTileCenterToWorld(beam.collisionPos)));
            callback.Call();
        }

        public override void StaffFx(MagesStaff.StaffParticle particle)
        {
            particle.SetColor(new Color(0xCC, 0x00, 0x00, 0xFF));
            particle.am = 0.6f;
            particle.SetLifespan(1f);
            particle.speed.Polar(Rnd.Float(PointF.PI2), 2f);
            particle.SetSize(1f, 2f);
            particle.RadiateXY(0.5f);
        }

        public override string StatsDesc()
        {
            int selfDMG = (int)Math.Round(Dungeon.hero.HT * 0.10f, MidpointRounding.AwayFromZero);
            if (levelKnown)
                return Messages.Get(this, "stats_desc", selfDMG, selfDMG + 3 * BuffedLvl(), 5 + 2 * BuffedLvl(), 3 + BuffedLvl() / 2, 6 + BuffedLvl());
            else
                return Messages.Get(this, "stats_desc", selfDMG, selfDMG, 5, 3, 6);
        }

        private const string FREECHARGE = "freecharge";

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            freeCharge = bundle.GetBoolean(FREECHARGE);
        }

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(FREECHARGE, freeCharge);
        }
    }
}