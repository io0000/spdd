using System;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors.blobs;
using spdd.actors.buffs;
using spdd.sprites;
using spdd.items;
using spdd.utils;
using spdd.levels;
using spdd.effects;
using spdd.effects.particles;
using spdd.messages;
using spdd.tiles;

namespace spdd.actors.mobs
{
    public class Pylon : Mob
    {
        public Pylon()
        {
            InitInstance();

            spriteClass = typeof(PylonSprite);

            HP = HT = 50;

            maxLvl = -2;

            properties.Add(Property.MINIBOSS);
            properties.Add(Property.INORGANIC);
            properties.Add(Property.ELECTRIC);
            properties.Add(Property.IMMOVABLE);

            state = PASSIVE;
            alignment = Alignment.NEUTRAL;
        }

        private int targetNeighbor = Rnd.Int(8);

        public override bool Act()
        {
            Spend(TICK);

            Heap heap = Dungeon.level.heaps[pos];
            if (heap != null)
            {
                int n;
                do
                {
                    n = pos + PathFinder.NEIGHBORS8[Rnd.Int(8)];
                } 
                while (!Dungeon.level.passable[n] && !Dungeon.level.avoid[n]);
                Dungeon.level.Drop(heap.PickUp(), n).sprite.Drop(pos);
            }

            if (alignment == Alignment.NEUTRAL)
            {
                return true;
            }

            int cell1 = pos + PathFinder.CIRCLE8[targetNeighbor];
            int cell2 = pos + PathFinder.CIRCLE8[(targetNeighbor + 4) % 8];

            sprite.Flash();
            if (Dungeon.level.heroFOV[pos] || Dungeon.level.heroFOV[cell1] || Dungeon.level.heroFOV[cell2])
            {
                sprite.parent.Add(new Lightning(DungeonTilemap.RaisedTileCenterToWorld(cell1),
                        DungeonTilemap.RaisedTileCenterToWorld(cell2), null));
                CellEmitter.Get(cell1).Burst(SparkParticle.Factory, 3);
                CellEmitter.Get(cell2).Burst(SparkParticle.Factory, 3);
                Sample.Instance.Play(Assets.Sounds.LIGHTNING);
            }

            ShockChar(Actor.FindChar(cell1));
            ShockChar(Actor.FindChar(cell2));

            targetNeighbor = (targetNeighbor + 1) % 8;

            return true;
        }

        private void ShockChar(Character ch)
        {
            if (ch != null && !(ch is NewDM300))
            {
                ch.sprite.Flash();
                ch.Damage(Rnd.NormalIntRange(10, 20), new Electricity());

                if (ch == Dungeon.hero && !ch.IsAlive())
                {
                    Dungeon.Fail(typeof(NewDM300));
                    GLog.Negative(Messages.Get(typeof(Electricity), "ondeath"));
                }
            }
        }

        public void Activate()
        {
            alignment = Alignment.ENEMY;
            ((PylonSprite)sprite).Activate();
        }

        public override CharSprite GetSprite()
        {
            var p = (PylonSprite)base.GetSprite();
            if (alignment != Alignment.NEUTRAL)
                p.Activate();
            return p;
        }

        public override void Notice()
        {
            //do nothing
        }

        public override string Description()
        {
            if (alignment == Alignment.NEUTRAL)
            {
                return Messages.Get(this, "desc_inactive");
            }
            else
            {
                return Messages.Get(this, "desc_active");
            }
        }

        public override bool Interact(Character c)
        {
            return true;
        }

        public override void Add(Buff buff)
        {
            //immune to all buffs/debuffs when inactive
            if (alignment != Alignment.NEUTRAL)
            {
                base.Add(buff);
            }
        }

        public override void Damage(int dmg, object src)
        {
            //immune to damage when inactive
            if (alignment == Alignment.NEUTRAL)
            {
                return;
            }
            if (dmg >= 15)
            {
                //takes 15/16/17/18/19/20 dmg at 15/17/20/24/29/36 incoming dmg
                dmg = 14 + (int)(Math.Sqrt(8 * (dmg - 14) + 1) - 1) / 2;
            }
            base.Damage(dmg, src);
        }

        public override void Die(object cause)
        {
            base.Die(cause);
            ((NewCavesBossLevel)Dungeon.level).EliminatePylon();
        }

        private const string ALIGNMENT = "alignment";
        private const string TARGET_NEIGHBOR = "target_neighbor";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(ALIGNMENT, alignment.ToString());    // enum
            bundle.Put(TARGET_NEIGHBOR, targetNeighbor);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            alignment = bundle.GetEnum<Alignment>(ALIGNMENT);
            targetNeighbor = bundle.GetInt(TARGET_NEIGHBOR);
        }

        private void InitInstance()
        {
            immunities.Add(typeof(Paralysis));
            immunities.Add(typeof(Amok));
            immunities.Add(typeof(Sleep));
            immunities.Add(typeof(ToxicGas));
            immunities.Add(typeof(Terror));
            immunities.Add(typeof(Vertigo));
        }
    }
}