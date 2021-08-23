using System;
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
    public class Sungrass : Plant
    {
        public Sungrass()
        {
            image = 3;
            seedClass = typeof(Seed);
        }

        public override void Activate(Character ch)
        {
            if (ch == Dungeon.hero)
            {
                if (Dungeon.hero.subClass == HeroSubClass.WARDEN)
                {
                    Buff.Affect<Healing>(ch).SetHeal(ch.HT, 0, 1);
                }
                else
                {
                    Buff.Affect<Health>(ch).Boost(ch.HT);
                }
            }

            if (Dungeon.level.heroFOV[pos])
            {
                CellEmitter.Get(pos).Start(ShaftParticle.Factory, 0.2f, 3);
            }
        }

        [SPDStatic]
        public new class Seed : Plant.Seed
        {
            public Seed()
            {
                image = ItemSpriteSheet.SEED_SUNGRASS;

                plantClass = typeof(Sungrass);

                bones = true;
            }
        }

        [SPDStatic]
        public class Health : Buff
        {
            internal const float STEP = 1f;

            internal int pos;
            internal float partialHeal;
            internal int level;

            public Health()
            {
                type = BuffType.POSITIVE;
                announced = true;
            }

            public override bool Act()
            {
                if (target.pos != pos)
                    Detach();

                //for the hero, full heal takes ~50/93/111/120 turns at levels 1/10/20/30
                partialHeal += (40 + target.HT) / 150f;

                if (partialHeal > 1)
                {
                    target.HP += (int)partialHeal;
                    level -= (int)partialHeal;
                    partialHeal -= (int)partialHeal;
                    target.sprite.Emitter().Burst(Speck.Factory(Speck.HEALING), 1);

                    if (target.HP >= target.HT)
                    {
                        target.HP = target.HT;
                        if (target is Hero)
                        {
                            ((Hero)target).resting = false;
                        }
                    }
                }

                if (level <= 0)
                    Detach();

                Spend(STEP);
                return true;
            }

            public void Boost(int amount)
            {
                level += amount;
                pos = target.pos;
            }

            public override int Icon()
            {
                return BuffIndicator.HERB_HEALING;
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
                return Messages.Get(this, "desc", level);
            }

            internal const string POS = "pos";
            internal const string PARTIAL = "partial_heal";
            internal const string LEVEL = "level";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(POS, pos);
                bundle.Put(PARTIAL, partialHeal);
                bundle.Put(LEVEL, level);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                pos = bundle.GetInt(POS);
                partialHeal = bundle.GetFloat(PARTIAL);
                level = bundle.GetInt(LEVEL);
            }
        }
    }
}