using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors;
using spdd.actors.blobs;
using spdd.effects;
using spdd.effects.particles;
using spdd.items.weapon.melee;
using spdd.mechanics;
using spdd.scenes;
using spdd.sprites;
using spdd.tiles;

namespace spdd.items.wands
{
    public class WandOfDisintegration : DamageWand
    {
        public WandOfDisintegration()
        {
            image = ItemSpriteSheet.WAND_DISINTEGRATION;

            collisionProperties = Ballistic.WONT_STOP;
        }

        public override int Min(int lvl)
        {
            return 2 + lvl;
        }

        public override int Max(int lvl)
        {
            return 8 + 4 * lvl;
        }

        protected override void OnZap(Ballistic beam)
        {
            bool terrainAffected = false;

            int level = BuffedLvl();

            int maxDistance = Math.Min(Distance(), beam.dist);

            var chars = new List<Character>();

            Blob web = Dungeon.level.GetBlob(typeof(Web));

            int terrainPassed = 2, terrainBonus = 0;
            foreach (int c in beam.SubPath(1, maxDistance))
            {
                Character ch;
                if ((ch = Actor.FindChar(c)) != null)
                {
                    //we don't want to count passed terrain after the last enemy hit. That would be a lot of bonus levels.
                    //terrainPassed starts at 2, equivalent of rounding up when /3 for integer arithmetic.
                    terrainBonus += terrainPassed / 3;
                    terrainPassed = terrainPassed % 3;

                    chars.Add(ch);
                }

                if (Dungeon.level.solid[c])
                {
                    ++terrainPassed;
                    if (web != null)
                        web.Clear(c);
                }

                if (Dungeon.level.flamable[c])
                {
                    Dungeon.level.Destroy(c);
                    GameScene.UpdateMap(c);
                    terrainAffected = true;
                }

                CellEmitter.Center(c).Burst(PurpleParticle.Burst, Rnd.IntRange(1, 2));
            }

            if (terrainAffected)
                Dungeon.Observe();

            int lvl = level + (chars.Count - 1) + terrainBonus;
            foreach (var ch in chars)
            {
                ProcessSoulMark(ch, ChargesPerCast());
                ch.Damage(DamageRoll(lvl), this);
                ch.sprite.CenterEmitter().Burst(PurpleParticle.Burst, Rnd.IntRange(1, 2));
                ch.sprite.Flash();
            }
        }

        public override void OnHit(MagesStaff staff, Character attacker, Character defender, int damage)
        {
            //no direct effect, see magesStaff.reachfactor
        }

        private int Distance()
        {
            return BuffedLvl() * 2 + 6;
        }

        public override void Fx(Ballistic beam, ICallback callback)
        {
            var d = Math.Min(beam.dist, Distance());

            int cell = beam.path[d];
            curUser.sprite.parent.Add(new Beam.DeathRay(curUser.sprite.Center(), DungeonTilemap.RaisedTileCenterToWorld(cell)));
            callback.Call();
        }

        public override void StaffFx(MagesStaff.StaffParticle particle)
        {
            particle.SetColor(new Color(0x22, 0x00, 0x22, 0xFF));
            particle.am = 0.6f;
            particle.SetLifespan(1f);
            particle.acc.Set(10, -10);
            particle.SetSize(0.5f, 3f);
            particle.ShuffleXY(1f);
        }
    }
}