using System;
using System.Collections.Generic;
using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.effects;
using spdd.effects.particles;
using spdd.items;
using spdd.items.wands;
using spdd.levels;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;

namespace spdd.plants
{
    public abstract class Plant : IBundlable
    {
        //public string plantName = Messages.Get(this, "name");
        public string plantName;

        public int image;
        public int pos;

        public Type seedClass;  // Class<? extends Plant.Seed>

        public Plant()
        {
            plantName = Messages.Get(this, "name");
        }

        public void Trigger()
        {
            var ch = Actor.FindChar(pos);

            if (ch is Hero)
                ((Hero)ch).Interrupt();

            Wither();
            Activate(ch);
        }

        public abstract void Activate(Character ch);

        public virtual void Wither()
        {
            Dungeon.level.Uproot(pos);

            if (Dungeon.level.heroFOV[pos])
                CellEmitter.Get(pos).Burst(LeafParticle.General, 6);

            float seedChance = 0f;
            foreach (Character c in Actor.Chars())
            {
                if (c is WandOfRegrowth.Lotus)
                {
                    var l = (WandOfRegrowth.Lotus)c;
                    if (l.InRange(pos))
                        seedChance = Math.Max(seedChance, l.SeedPreservation());
                }
            }

            if (Rnd.Float() < seedChance)
            {
                if (seedClass != null && seedClass != typeof(Rotberry.Seed))
                {
                    var seed = (Plant.Seed)Reflection.NewInstance(seedClass);

                    Dungeon.level.Drop(seed, pos).sprite.Drop();
                }
            }
        }

        private const string POS = "pos";

        public void RestoreFromBundle(Bundle bundle)
        {
            pos = bundle.GetInt(POS);
        }

        public void StoreInBundle(Bundle bundle)
        {
            bundle.Put(POS, pos);
        }

        public string Desc()
        {
            return Messages.Get(this, "desc");
        }

        [SPDStatic]
        public class Seed : Item
        {
            public const string AC_PLANT = "PLANT";

            private const float TIME_TO_PLANT = 1f;

            public Seed()
            {
                stackable = true;
                defaultAction = AC_THROW;
            }

            public Type plantClass; //Class<? extends Plant>

            public override List<string> Actions(Hero hero)
            {
                var actions = base.Actions(hero);
                actions.Add(AC_PLANT);
                return actions;
            }

            public override void OnThrow(int cell)
            {
                if (Dungeon.level.map[cell] == Terrain.ALCHEMY ||
                    Dungeon.level.pit[cell] ||
                    Dungeon.level.traps[cell] != null ||
                    Dungeon.IsChallenged(Challenges.NO_HERBALISM))
                {
                    base.OnThrow(cell);
                }
                else
                {
                    Dungeon.level.Plant(this, cell);
                    if (Dungeon.hero.subClass == HeroSubClass.WARDEN)
                    {
                        foreach (int i in PathFinder.NEIGHBORS8)
                        {
                            int c = Dungeon.level.map[cell + i];
                            if (c == Terrain.EMPTY ||
                                c == Terrain.EMPTY_DECO ||
                                c == Terrain.EMBERS ||
                                c == Terrain.GRASS)
                            {
                                Level.Set(cell + i, Terrain.FURROWED_GRASS);
                                GameScene.UpdateMap(cell + i);
                                CellEmitter.Get(cell + i).Burst(LeafParticle.LevelSpecific, 4);
                            }
                        }
                    }
                }
            }

            public override void Execute(Hero hero, string action)
            {
                base.Execute(hero, action);

                if (action.Equals(AC_PLANT))
                {
                    hero.Spend(TIME_TO_PLANT);
                    hero.Busy();
                    ((Seed)Detach(hero.belongings.backpack)).OnThrow(hero.pos);

                    hero.sprite.Operate(hero.pos);
                }
            }

            public Plant Couch(int pos, Level level)
            {
                if (level != null && level.heroFOV != null && level.heroFOV[pos])
                    Sample.Instance.Play(Assets.Sounds.PLANT);

                var plant = (Plant)Reflection.NewInstance(plantClass);
                plant.pos = pos;
                return plant;
            }

            public override bool IsUpgradable()
            {
                return false;
            }

            public override bool IsIdentified()
            {
                return true;
            }

            public override int Value()
            {
                return 10 * quantity;
            }

            public override string Desc()
            {
                return Messages.Get(plantClass, "desc");
            }

            public override string Info()
            {
                return Messages.Get(typeof(Seed), "info", Desc());
            }

            [SPDStatic]
            public class PlaceHolder : Seed
            {
                public PlaceHolder()
                {
                    image = ItemSpriteSheet.SEED_HOLDER;
                }

                public override bool IsSimilar(Item item)
                {
                    return item is Plant.Seed;
                }

                public override string Info()
                {
                    return "";
                }
            }
        }
    }
}
