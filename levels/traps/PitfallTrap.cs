using watabou.utils;
using spdd.scenes;
using spdd.effects;
using spdd.utils;
using spdd.effects.particles;
using spdd.items;
using spdd.actors;
using spdd.actors.mobs;
using spdd.actors.buffs;
using spdd.levels.features;
using spdd.messages;

namespace spdd.levels.traps
{
    public class PitfallTrap : Trap
    {
        public PitfallTrap()
        {
            color = RED;
            shape = DIAMOND;
        }

        public override void Activate()
        {
            if (Dungeon.BossLevel() || Dungeon.depth > 25)
            {
                GLog.Warning(Messages.Get(this, "no_pit"));
                return;
            }

            DelayedPit p = Buff.Affect<DelayedPit>(Dungeon.hero, 1);
            p.depth = Dungeon.depth;
            p.pos = pos;

            foreach (int i in PathFinder.NEIGHBORS9)
            {
                if (!Dungeon.level.solid[pos + i] || Dungeon.level.passable[pos + i])
                {
                    CellEmitter.Floor(pos + i).Burst(PitfallParticle.Factory4, 8);
                }
            }

            if (pos == Dungeon.hero.pos)
            {
                GLog.Negative(Messages.Get(this, "triggered_hero"));
            }
            else if (Dungeon.level.heroFOV[pos])
            {
                GLog.Negative(Messages.Get(this, "triggered"));
            }
        }

        [SPDStatic]
        public class DelayedPit : FlavourBuff
        {
            internal int pos;
            internal int depth;

            public override bool Act()
            {
                if (depth == Dungeon.depth)
                {
                    foreach (int i in PathFinder.NEIGHBORS9)
                    {
                        int cell = pos + i;

                        if (Dungeon.level.solid[pos + i] && !Dungeon.level.passable[pos + i])
                            continue;

                        CellEmitter.Floor(pos + i).Burst(PitfallParticle.Factory8, 12);

                        Heap heap = Dungeon.level.heaps[cell];

                        if (heap != null)
                        {
                            foreach (Item item in heap.items)
                                Dungeon.DropToChasm(item);

                            heap.sprite.Kill();
                            GameScene.Discard(heap);
                            Dungeon.level.heaps.Remove(cell);
                        }

                        var ch = Actor.FindChar(cell);

                        //don't trigger on flying chars, or immovable neutral chars
                        if (ch != null &&
                            !ch.flying &&
                            !(ch.alignment == Character.Alignment.NEUTRAL && Character.HasProp(ch, Character.Property.IMMOVABLE)))
                        {
                            if (ch == Dungeon.hero)
                            {
                                Chasm.HeroFall(cell);
                            }
                            else
                            {
                                Chasm.MobFall((Mob)ch);
                            }
                        }
                    }
                }

                Detach();
                return true;
            }

            private const string POS = "pos";
            private const string DEPTH = "depth";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(POS, pos);
                bundle.Put(DEPTH, depth);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                pos = bundle.GetInt(POS);
                depth = bundle.GetInt(DEPTH);
            }
        }

        //TODO these used to become chasms when disarmed, but the functionality was problematic
        //because it could block routes, perhaps some way to make this work elegantly?
    }
}