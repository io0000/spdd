using System;
using System.Collections.Generic;
using System.Linq;
using watabou.noosa.particles;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.messages;
using spdd.sprites;
using spdd.ui;

namespace spdd.plants
{
    public class Swiftthistle : Plant
    {
        public Swiftthistle()
        {
            image = 2;
            seedClass = typeof(Seed);
        }

        public override void Activate(Character ch)
        {
            if (ch == Dungeon.hero)
            {
                Buff.Affect<TimeBubble>(ch).Reset();
                if (Dungeon.hero.subClass == HeroSubClass.WARDEN)
                {
                    Buff.Affect<Haste>(ch, 1f);
                }
            }
        }

        [SPDStatic]
        public new class Seed : Plant.Seed
        {
            public Seed()
            {
                image = ItemSpriteSheet.SEED_SWIFTTHISTLE;

                plantClass = typeof(Swiftthistle);
            }
        }

        //FIXME lots of copypasta from time freeze here
        [SPDStatic]
        public class TimeBubble : Buff
        {
            public TimeBubble()
            {
                type = BuffType.POSITIVE;
                announced = true;
            }

            internal float left;
            internal List<int> presses = new List<int>();

            public override int Icon()
            {
                return BuffIndicator.SLOW;
            }

            public override float IconFadePercent()
            {
                return Math.Max(0, (6f - left) / 6f);
            }

            public void Reset()
            {
                left = 7f;
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }

            public override string Desc()
            {
                return Messages.Get(this, "desc", DispTurns(left));
            }

            public void ProcessTime(float time)
            {
                left -= time;

                if (left <= 0)
                {
                    Detach();
                }
            }

            public void SetDelayedPress(int cell)
            {
                if (!presses.Contains(cell))
                    presses.Add(cell);
            }

            internal void TriggerPresses()
            {
                foreach (int cell in presses)
                {
                    Dungeon.level.PressCell(cell);
                }

                presses = new List<int>();
            }

            public override void Detach()
            {
                base.Detach();
                TriggerPresses();
                target.Next();
            }

            public override void Fx(bool on)
            {
                Emitter.freezeEmitters = on;
                if (on)
                {
                    foreach (Mob mob in Dungeon.level.mobs.ToArray())
                    {
                        if (mob.sprite != null)
                        {
                            mob.sprite.Add(CharSprite.State.PARALYSED);
                        }
                    }
                }
                else
                {
                    foreach (Mob mob in Dungeon.level.mobs.ToArray())
                    {
                        if (mob.paralysed <= 0)
                        {
                            mob.sprite.Remove(CharSprite.State.PARALYSED);
                        }
                    }
                }
            }

            private const string PRESSES = "presses";
            private const string LEFT = "left";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);

                int[] values = new int[presses.Count];
                for (int i = 0; i < values.Length; ++i)
                    values[i] = presses[i];

                bundle.Put(PRESSES, values);

                bundle.Put(LEFT, left);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);

                int[] values = bundle.GetIntArray(PRESSES);
                foreach (int value in values)
                    presses.Add(value);

                left = bundle.GetFloat(LEFT);
            }
        }
    }
}
