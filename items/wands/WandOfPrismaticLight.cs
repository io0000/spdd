using System;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.effects;
using spdd.effects.particles;
using spdd.items.scrolls;
using spdd.items.weapon.melee;
using spdd.levels;
using spdd.mechanics;
using spdd.scenes;
using spdd.sprites;
using spdd.tiles;

namespace spdd.items.wands
{
    public class WandOfPrismaticLight : DamageWand
    {
        public WandOfPrismaticLight()
        {
            image = ItemSpriteSheet.WAND_PRISMATIC_LIGHT;

            collisionProperties = Ballistic.MAGIC_BOLT;
        }

        public override int Min(int lvl)
        {
            return 1 + lvl;
        }

        public override int Max(int lvl)
        {
            return 5 + 3 * lvl;
        }

        protected override void OnZap(Ballistic beam)
        {
            AffectMap(beam);

            if (Dungeon.level.viewDistance < 6)
            {
                if (Dungeon.IsChallenged(Challenges.DARKNESS))
                {
                    Buff.Prolong<Light>(curUser, 2f + BuffedLvl());
                }
                else
                {
                    Buff.Prolong<Light>(curUser, 10f + BuffedLvl() * 5);
                }
            }

            var ch = Actor.FindChar(beam.collisionPos);
            if (ch != null)
            {
                ProcessSoulMark(ch, ChargesPerCast());
                AffectTarget(ch);
            }
        }

        private void AffectTarget(Character ch)
        {
            int dmg = DamageRoll();

            //three in (5+lvl) chance of failing
            if (Rnd.Int(5 + BuffedLvl()) >= 3)
            {
                Buff.Prolong<Blindness>(ch, 2f + (BuffedLvl() * 0.333f));
                ch.sprite.Emitter().Burst(Speck.Factory(Speck.LIGHT), 6);
            }

            if (ch.Properties().Contains(Character.Property.DEMONIC) || ch.Properties().Contains(Character.Property.UNDEAD))
            {
                ch.sprite.Emitter().Start(ShadowParticle.Up, 0.05f, 10 + base.BuffedLvl());
                Sample.Instance.Play(Assets.Sounds.BURNING);

                ch.Damage((int)Math.Round(dmg * 1.333f, MidpointRounding.AwayFromZero), this);
            }
            else
            {
                ch.sprite.CenterEmitter().Burst(RainbowParticle.Burst, 10 + base.BuffedLvl());

                ch.Damage(dmg, this);
            }
        }

        private void AffectMap(Ballistic beam)
        {
            bool noticed = false;
            foreach (int c in beam.SubPath(0, beam.dist))
            {
                if (!Dungeon.level.InsideMap(c))
                    continue;

                foreach (int n in PathFinder.NEIGHBORS9)
                {
                    int cell = c + n;

                    if (Dungeon.level.discoverable[cell])
                        Dungeon.level.mapped[cell] = true;

                    int terr = Dungeon.level.map[cell];
                    if ((Terrain.flags[terr] & Terrain.SECRET) != 0)
                    {
                        Dungeon.level.Discover(cell);

                        GameScene.DiscoverTile(cell, terr);
                        ScrollOfMagicMapping.Discover(cell);

                        noticed = true;
                    }
                }

                CellEmitter.Center(c).Burst(RainbowParticle.Burst, Rnd.IntRange(1, 2));
            }

            if (noticed)
                Sample.Instance.Play(Assets.Sounds.SECRET);

            GameScene.UpdateFog();
        }

        public override void Fx(Ballistic beam, ICallback callback)
        {
            curUser.sprite.parent.Add(
                new Beam.LightRay(curUser.sprite.Center(), DungeonTilemap.RaisedTileCenterToWorld(beam.collisionPos)));
            callback.Call();
        }

        public override void OnHit(MagesStaff staff, Character attacker, Character defender, int damage)
        {
            //cripples enemy
            Buff.Prolong<Cripple>(defender, 1f + staff.BuffedLvl());
        }

        public override void StaffFx(MagesStaff.StaffParticle particle)
        {
            //particle.color( Random.Int( 0x1000000 ) );
            byte r = (byte)Rnd.Int(0xFF);
            byte g = (byte)Rnd.Int(0xFF);
            byte b = (byte)Rnd.Int(0xFF);
            var c = new Color(r, g, b, 0xFF);

            particle.SetColor(c);
            particle.am = 0.5f;
            particle.SetLifespan(1f);
            particle.speed.Polar(Rnd.Float(PointF.PI2), 2f);
            particle.SetSize(1f, 2f);
            particle.RadiateXY(0.5f);
        }
    }
}