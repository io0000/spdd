using System;
using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects;
using spdd.effects.particles;
using spdd.messages;
using spdd.sprites;
using spdd.ui;

namespace spdd.plants
{
    public class Earthroot : Plant
    {
        public Earthroot()
        {
            image = 8;
            seedClass = typeof(Seed);
        }

        public override void Activate(Character ch)
        {
            if (ch == Dungeon.hero)
            {
                if (Dungeon.hero.subClass == HeroSubClass.WARDEN)
                {
                    Buff.Affect<Barkskin>(ch).Set(Dungeon.hero.lvl + 5, 5);
                }
                else
                {
                    Buff.Affect<Armor>(ch).Level(ch.HT);
                }
            }

            if (Dungeon.level.heroFOV[pos])
            {
                CellEmitter.Bottom(pos).Start(EarthParticle.Factory, 0.05f, 8);
                Camera.main.Shake(1, 0.4f);
            }
        }

        [SPDStatic]
        public new class Seed : Plant.Seed
        {
            public Seed()
            {
                image = ItemSpriteSheet.SEED_EARTHROOT;

                plantClass = typeof(Earthroot);

                bones = true;
            }
        }

        [SPDStatic]
        public class Armor : Buff
        {
            internal const float STEP = 1f;

            internal int pos;
            internal int level;

            public Armor()
            {
                type = BuffType.POSITIVE;
                announced = true;
            }

            public override bool AttachTo(Character target)
            {
                pos = target.pos;
                return base.AttachTo(target);
            }

            public override bool Act()
            {
                if (target.pos != pos)
                {
                    Detach();
                }
                Spend(STEP);
                return true;
            }

            private static int Blocking()
            {
                return (Dungeon.depth + 5) / 2;
            }

            public int Absorb(int damage)
            {
                int block = Math.Min(damage, Blocking());
                if (level <= block)
                {
                    Detach();
                    return damage - block;
                }
                else
                {
                    level -= block;
                    return damage - block;
                }
            }

            public void Level(int value)
            {
                if (level < value)
                    level = value;

                pos = target.pos;
            }

            public override int Icon()
            {
                return BuffIndicator.ARMOR;
            }

            public override float IconFadePercent()
            {
                return Math.Max(0, (target.HT - level) / (float)target.HT);
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc", Blocking(), level);
            }

            private const string POS = "pos";
            private const string LEVEL = "level";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(POS, pos);
                bundle.Put(LEVEL, level);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                pos = bundle.GetInt(POS);
                level = bundle.GetInt(LEVEL);
            }
        }
    }
}