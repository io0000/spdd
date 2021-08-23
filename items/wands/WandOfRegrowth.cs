using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.actors.mobs.npcs;
using spdd.effects;
using spdd.items.weapon.enchantments;
using spdd.items.weapon.melee;
using spdd.levels;
using spdd.mechanics;
using spdd.messages;
using spdd.plants;
using spdd.scenes;
using spdd.sprites;

namespace spdd.items.wands
{
    public class WandOfRegrowth : Wand
    {
        public WandOfRegrowth()
        {
            image = ItemSpriteSheet.WAND_REGROWTH;

            collisionProperties = Ballistic.STOP_TERRAIN;
        }

        private int totChrgUsed;

        ConeAOE cone;
        int target;

        public override bool TryToZap(Hero owner, int target)
        {
            if (base.TryToZap(owner, target))
            {
                this.target = target;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void OnZap(Ballistic bolt)
        {
            List<int> cells = new List<int>(cone.cells);

            int overLimit = totChrgUsed - ChargeLimit(Dungeon.hero.lvl);
            float furrowedChance = overLimit > 0 ? (overLimit / (10f + Dungeon.hero.lvl)) : 0;

            int chrgUsed = ChargesPerCast();
            int grassToPlace = (int)Math.Round((3.67f + BuffedLvl() / 3f) * chrgUsed, MidpointRounding.AwayFromZero);

            //ignore cells which can't have anything grow in them.
            for (int i = cells.Count - 1; i >= 0; --i)
            {
                int cell = cells[i];
                int terr = Dungeon.level.map[cell];
                if (!(terr == Terrain.EMPTY ||
                    terr == Terrain.EMBERS ||
                    terr == Terrain.EMPTY_DECO ||
                    terr == Terrain.GRASS ||
                    terr == Terrain.HIGH_GRASS ||
                    terr == Terrain.FURROWED_GRASS))
                {
                    cells.RemoveAt(i);
                }
                else if (Character.HasProp(Actor.FindChar(cell), Character.Property.IMMOVABLE))
                {
                    cells.RemoveAt(i);
                }
                else if (Dungeon.level.plants[cell] != null)
                {
                    cells.RemoveAt(i);
                }
                else
                {
                    if (terr != Terrain.HIGH_GRASS && terr != Terrain.FURROWED_GRASS)
                    {
                        Level.Set(cell, Terrain.GRASS);
                        GameScene.UpdateMap(cell);
                    }
                    var ch = Actor.FindChar(cell);
                    if (ch != null)
                    {
                        ProcessSoulMark(ch, ChargesPerCast());
                        Buff.Prolong<Roots>(ch, 4f * chrgUsed);
                    }
                }
            }

            Rnd.Shuffle(cells);

            if (ChargesPerCast() >= 3)
            {
                Lotus l = new Lotus();
                l.SetLevel(BuffedLvl());
                if (cells.Contains(target) && Actor.FindChar(target) == null)
                {
                    cells.Remove(target);
                    l.pos = target;
                    GameScene.Add(l);
                }
                else
                {
                    for (int i = bolt.path.Count - 1; i >= 0; --i)
                    {
                        int c = bolt.path[i];
                        if (cells.Contains(c) && Actor.FindChar(c) == null)
                        {
                            cells.Remove(c);
                            l.pos = c;
                            GameScene.Add(l);
                            break;
                        }
                    }
                }
            }

            //places grass along center of cone
            foreach (int cell in bolt.path)
            {
                if (grassToPlace > 0 && cells.Contains(cell))
                {
                    if (Rnd.Float() > furrowedChance)
                        Level.Set(cell, Terrain.HIGH_GRASS);
                    else
                        Level.Set(cell, Terrain.FURROWED_GRASS);

                    GameScene.UpdateMap(cell);
                    --grassToPlace;
                    //moves cell to the back
                    cells.Remove(cell);
                    cells.Add(cell);
                }
            }

            if (cells.Count > 0 &&
                Rnd.Float() > furrowedChance &&
                (Rnd.Int(6) < chrgUsed))
            {
                // 16%/33%/50% chance to spawn a seed pod or dewcatcher
                int cell = cells[0];
                cells.RemoveAt(0);

                Plant.Seed seed;
                if (Rnd.Int(2) == 0)
                    seed = new Seedpod.Seed();
                else
                    seed = new Dewcatcher.Seed();

                Dungeon.level.Plant(seed, cell);
            }

            if (cells.Count > 0 &&
                Rnd.Float() > furrowedChance &&
                (Rnd.Int(3) < chrgUsed))
            {
                // 33%/66%/100% chance to spawn a plant
                int cell = cells[0];
                cells.RemoveAt(0);

                Dungeon.level.Plant((Plant.Seed)Generator.RandomUsingDefaults(Generator.Category.SEED), cell);
            }

            foreach (int cell in cells)
            {
                if (grassToPlace <= 0 || bolt.path.Contains(cell))
                    break;

                if (Dungeon.level.map[cell] == Terrain.HIGH_GRASS)
                    continue;

                if (Rnd.Float() > furrowedChance)
                    Level.Set(cell, Terrain.HIGH_GRASS);
                else
                    Level.Set(cell, Terrain.FURROWED_GRASS);

                GameScene.UpdateMap(cell);
                --grassToPlace;
            }

            if (furrowedChance < 1f)
                totChrgUsed += chrgUsed;
        }

        private int ChargeLimit(int heroLvl)
        {
            if (GetLevel() >= 10)
            {
                return int.MaxValue;
            }
            else
            {
                //8 charges at base, plus:
                //2/3.33/5/7/10/14/20/30/50/110/infinite charges per hero level, based on wand level
                float lvl = BuffedLvl();
                return (int)Math.Round(8 + heroLvl * (2 + lvl) * (1f + (lvl / (10 - lvl))), MidpointRounding.AwayFromZero);
            }
        }

        public override void OnHit(MagesStaff staff, Character attacker, Character defender, int damage)
        {
            new Blooming().Proc(staff, attacker, defender, damage);
        }

        public override void Fx(Ballistic bolt, ICallback callback)
        {
            // 4/6/8 distance
            int maxDist = 2 + 2 * ChargesPerCast();
            int dist = Math.Min(bolt.dist, maxDist);

            cone = new ConeAOE(bolt,
                    maxDist,
                    20 + 10 * ChargesPerCast(),
                    collisionProperties | Ballistic.STOP_TARGET);

            //cast to cells at the tip, rather than all cells, better performance.
            foreach (Ballistic ray in cone.rays)
            {
                var missile = curUser.sprite.parent.Recycle<MagicMissile>();
                missile.Reset(MagicMissile.FOLIAGE_CONE, curUser.sprite, ray.path[ray.dist], null);
            }

            //final zap at half distance, for timing of the actual wand effect
            MagicMissile.BoltFromChar(curUser.sprite.parent,
                    MagicMissile.FOLIAGE_CONE,
                    curUser.sprite,
                    bolt.path[dist / 2],
                    callback);
            Sample.Instance.Play(Assets.Sounds.ZAP);
        }

        protected override int ChargesPerCast()
        {
            //consumes 30% of current charges, rounded up, with a minimum of one.
            return Math.Max(1, (int)Math.Ceiling(curCharges * 0.3f));
        }

        public override string StatsDesc()
        {
            return Messages.Get(this, "stats_desc", ChargesPerCast());
        }

        public override void StaffFx(MagesStaff.StaffParticle particle)
        {
            var c1 = new Color(0x00, 0x44, 0x00, 0xFF);
            var c2 = new Color(0x88, 0xCC, 0x44, 0xFF);
            particle.SetColor(ColorMath.Random(c1, c2));
            particle.am = 1f;
            particle.SetLifespan(1f);
            particle.SetSize(1f, 1.5f);
            particle.ShuffleXY(0.5f);
            float dst = Rnd.Float(11f);
            particle.x -= dst;
            particle.y += dst;
        }

        private const string TOTAL = "totChrgUsed";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(TOTAL, totChrgUsed);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            totChrgUsed = bundle.GetInt(TOTAL);
        }


        [SPDStatic]
        public class Dewcatcher : Plant
        {
            public Dewcatcher()
            {
                image = 13;
            }

            public override void Activate(Character ch)
            {
                int nDrops = Rnd.NormalIntRange(3, 6);

                List<int> candidates = new List<int>();
                foreach (int i in PathFinder.NEIGHBORS8)
                {
                    if (Dungeon.level.passable[pos + i] &&
                        pos + i != Dungeon.level.entrance &&
                        pos + i != Dungeon.level.exit)
                    {
                        candidates.Add(pos + i);
                    }
                }

                for (int i = 0; i < nDrops && candidates.Count > 0; ++i)
                {
                    int c = Rnd.Element(candidates);
                    Dungeon.level.Drop(new Dewdrop(), c).sprite.Drop(pos);
                    candidates.Remove(c);
                }
            }

            //seed is never dropped, only care about plant class
            [SPDStatic]
            public new class Seed : Plant.Seed
            {
                public Seed()
                {
                    plantClass = typeof(Dewcatcher);
                }
            }
        }

        [SPDStatic]
        public class Seedpod : Plant
        {
            public Seedpod()
            {
                image = 14;
            }

            public override void Activate(Character ch)
            {
                int nSeeds = Rnd.NormalIntRange(2, 4);

                List<int> candidates = new List<int>();
                foreach (int i in PathFinder.NEIGHBORS8)
                {
                    if (Dungeon.level.passable[pos + i] &&
                        pos + i != Dungeon.level.entrance &&
                        pos + i != Dungeon.level.exit)
                    {
                        candidates.Add(pos + i);
                    }
                }

                for (int i = 0; i < nSeeds && candidates.Count > 0; ++i)
                {
                    int c = Rnd.Element(candidates);
                    Dungeon.level.Drop(Generator.Random(Generator.Category.SEED), c).sprite.Drop(pos);
                    candidates.Remove(c);
                }
            }

            //seed is never dropped, only care about plant class
            [SPDStatic]
            public new class Seed : Plant.Seed
            {
                public Seed()
                {
                    plantClass = typeof(Seedpod);
                }
            }
        }

        [SPDStatic]
        public class Lotus : NPC
        {
            public Lotus()
            {
                alignment = Alignment.ALLY;
                properties.Add(Property.IMMOVABLE);

                spriteClass = typeof(LotusSprite);

                viewDistance = 1;
            }

            private int wandLvl;

            public void SetLevel(int lvl)
            {
                wandLvl = lvl;
                HP = HT = 25 + 3 * lvl;
            }

            public bool InRange(int pos)
            {
                return Dungeon.level.TrueDistance(this.pos, pos) <= wandLvl;
            }

            public float SeedPreservation()
            {
                return 0.40f + 0.04f * wandLvl;
            }

            public override bool CanInteract(Character c)
            {
                return false;
            }

            public override bool Act()
            {
                base.Act();

                if (--HP <= 0)
                {
                    Destroy();
                    sprite.Die();
                }

                return true;
            }

            public override void Damage(int dmg, object src)
            { }

            public override void Add(Buff buff)
            { }

            public override void Destroy()
            {
                base.Destroy();
                Dungeon.Observe();
                GameScene.UpdateFog(pos, viewDistance + 1);
            }

            public override bool IsInvulnerable(Type effect)
            {
                return true;
            }

            public override string Description()
            {
                return Messages.Get(this, "desc", wandLvl, (int)(SeedPreservation() * 100), (int)(SeedPreservation() * 100));
            }

            private const string WAND_LVL = "wand_lvl";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(WAND_LVL, wandLvl);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                wandLvl = bundle.GetInt(WAND_LVL);
            }
        }
    }
}