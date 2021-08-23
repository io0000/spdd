using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.effects;
using spdd.items.weapon.enchantments;
using spdd.items.weapon.melee;
using spdd.levels;
using spdd.mechanics;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;

using Fire = spdd.actors.blobs.Fire;
using Paralysis = spdd.actors.buffs.Paralysis;

namespace spdd.items.wands
{
    public class WandOfFireblast : DamageWand
    {
        public WandOfFireblast()
        {
            image = ItemSpriteSheet.WAND_FIREBOLT;

            collisionProperties = Ballistic.STOP_TERRAIN | Ballistic.IGNORE_DOORS;
        }

        //1x/2x/3x damage
        public override int Min(int lvl)
        {
            return (1 + lvl) * ChargesPerCast();
        }

        //1x/2x/3x damage
        public override int Max(int lvl)
        {
            return (6 + 2 * lvl) * ChargesPerCast();
        }

        ConeAOE cone;

        protected override void OnZap(Ballistic bolt)
        {
            var affectedChars = new List<Character>();
            var adjacentCells = new List<int>();

            foreach (int cell in cone.cells)
            {
                //ignore caster cell
                if (cell == bolt.sourcePos)
                    continue;

                //knock doors open
                if (Dungeon.level.map[cell] == Terrain.DOOR)
                {
                    Level.Set(cell, Terrain.OPEN_DOOR);
                    GameScene.UpdateMap(cell);
                }

                //only ignite cells directly near caster if they are flammable
                if (Dungeon.level.Adjacent(bolt.sourcePos, cell) && !Dungeon.level.flamable[cell])
                {
                    adjacentCells.Add(cell);
                }
                else
                {
                    GameScene.Add(Blob.Seed(cell, 1 + ChargesPerCast(), typeof(Fire)));
                }

                var ch = Actor.FindChar(cell);
                if (ch != null)
                    affectedChars.Add(ch);
            }

            //ignite cells that share a side with an adjacent cell, are flammable, and are further from the source pos
            //This prevents short-range casts not igniting barricades or bookshelves
            foreach (int cell in adjacentCells)
            {
                foreach (int i in PathFinder.NEIGHBORS4)
                {
                    if (Dungeon.level.TrueDistance(cell + i, bolt.sourcePos) > Dungeon.level.TrueDistance(cell, bolt.sourcePos) &&
                        Dungeon.level.flamable[cell + i] &&
                        Fire.VolumeAt(cell + i, typeof(Fire)) == 0)
                    {
                        GameScene.Add(Blob.Seed(cell + i, 1 + ChargesPerCast(), typeof(Fire)));
                    }
                }
            }

            foreach (var ch in affectedChars)
            {
                ProcessSoulMark(ch, ChargesPerCast());
                ch.Damage(DamageRoll(), this);
                if (ch.IsAlive())
                {
                    Buff.Affect<Burning>(ch).Reignite(ch);
                    switch (ChargesPerCast())
                    {
                        case 1:
                            break; //no effects
                        case 2:
                            Buff.Affect<Cripple>(ch, 4f);
                            break;
                        case 3:
                            Buff.Affect<Paralysis>(ch, 4f);
                            break;
                    }
                }
            }
        }

        public override void OnHit(MagesStaff staff, Character attacker, Character defender, int damage)
        {
            //acts like blazing enchantment
            new Blazing().Proc(staff, attacker, defender, damage);
        }

        public override void Fx(Ballistic bolt, ICallback callback)
        {
            //need to perform flame spread logic here so we can determine what cells to put flames in.

            // 5/7/9 distance
            int maxDist = 3 + 2 * ChargesPerCast();
            int dist = Math.Min(bolt.dist, maxDist);

            cone = new ConeAOE(bolt,
                maxDist,
                30 + 20 * ChargesPerCast(),
                collisionProperties | Ballistic.STOP_TARGET);

            //cast to cells at the tip, rather than all cells, better performance.
            foreach (Ballistic ray in cone.rays)
            {
                ((MagicMissile)curUser.sprite.parent.Recycle<MagicMissile>()).Reset(
                    MagicMissile.FIRE_CONE,
                    curUser.sprite,
                    ray.path[ray.dist],
                    null);
            }

            //final zap at half distance, for timing of the actual wand effect
            MagicMissile.BoltFromChar(curUser.sprite.parent,
                MagicMissile.FIRE_CONE,
                curUser.sprite,
                bolt.path[dist / 2],
                callback);
            Sample.Instance.Play(Assets.Sounds.ZAP);
            Sample.Instance.Play(Assets.Sounds.BURNING);
        }

        protected override int ChargesPerCast()
        {
            //consumes 30% of current charges, rounded up, with a minimum of one.
            return Math.Max(1, (int)Math.Ceiling(curCharges * 0.3f));
        }

        public override string StatsDesc()
        {
            if (levelKnown)
            {
                return Messages.Get(this, "stats_desc", ChargesPerCast(), Min(), Max());
            }
            else
            {
                return Messages.Get(this, "stats_desc", ChargesPerCast(), Min(0), Max(0));
            }
        }

        public override void StaffFx(MagesStaff.StaffParticle particle)
        {
            particle.SetColor(new Color(0xEE, 0x77, 0x22, 0xFF));
            particle.am = 0.5f;
            particle.SetLifespan(0.6f);
            particle.acc.Set(0, -40);
            particle.SetSize(0f, 3f);
            particle.ShuffleXY(1.5f);
        }
    }
}