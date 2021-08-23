using System;
using System.Collections.Generic;
using Microsoft.Collections.Extensions;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.hero;
using spdd.sprites;
using spdd.items.potions;
using spdd.items.scrolls;
using spdd.items.quest;
using spdd.scenes;
using spdd.utils;
using spdd.effects;
using spdd.effects.particles;
using spdd.messages;

namespace spdd.items.bombs
{
    public class Bomb : Item
    {
        public Bomb()
        {
            image = ItemSpriteSheet.BOMB;

            defaultAction = AC_LIGHTTHROW;
            usesTargeting = true;

            stackable = true;
        }

        public Fuse fuse;

        //FIXME using a static variable for this is kinda gross, should be a better way
        private static bool lightingFuse;

        private const string AC_LIGHTTHROW = "LIGHTTHROW";

        public override bool IsSimilar(Item item)
        {
            return base.IsSimilar(item) && this.fuse == ((Bomb)item).fuse;
        }

        public virtual bool ExplodesDestructively()
        {
            return true;
        }

        public override List<string> Actions(Hero hero)
        {
            List<string> actions = base.Actions(hero);
            actions.Add(AC_LIGHTTHROW);
            return actions;
        }

        public override void Execute(Hero hero, string action)
        {
            if (action.Equals(AC_LIGHTTHROW))
            {
                lightingFuse = true;
                action = AC_THROW;
            }
            else
            {
                lightingFuse = false;
            }

            base.Execute(hero, action);
        }

        public override void OnThrow(int cell)
        {
            if (!Dungeon.level.pit[cell] && lightingFuse)
                Actor.AddDelayed(fuse = new Fuse().Ignite(this), 2);

            if (Actor.FindChar(cell) != null && !(Actor.FindChar(cell) is Hero))
            {
                List<int> candidates = new List<int>();
                foreach (int i in PathFinder.NEIGHBORS8)
                {
                    if (Dungeon.level.passable[cell + i])
                        candidates.Add(cell + i);
                }

                int newCell = candidates.Count == 0 ? cell : Rnd.Element(candidates);
                Dungeon.level.Drop(this, newCell).sprite.Drop(cell);
            }
            else
            {
                base.OnThrow(cell);
            }
        }

        public override bool DoPickUp(Hero hero)
        {
            if (fuse != null)
            {
                GLog.Warning(Messages.Get(this, "snuff_fuse"));
                fuse = null;
            }
            return base.DoPickUp(hero);
        }

        public virtual void Explode(int cell)
        {
            //We're blowing up, so no need for a fuse anymore.
            this.fuse = null;

            Sample.Instance.Play(Assets.Sounds.BLAST);

            if (ExplodesDestructively())
            {
                List<Character> affected = new List<Character>();

                if (Dungeon.level.heroFOV[cell])
                    CellEmitter.Center(cell).Burst(BlastParticle.Factory, 30);

                bool terrainAffected = false;
                foreach (int n in PathFinder.NEIGHBORS9)
                {
                    int c = cell + n;
                    if (c >= 0 && c < Dungeon.level.Length())
                    {
                        if (Dungeon.level.heroFOV[c])
                            CellEmitter.Get(c).Burst(SmokeParticle.Factory, 4);

                        if (Dungeon.level.flamable[c])
                        {
                            Dungeon.level.Destroy(c);
                            GameScene.UpdateMap(c);
                            terrainAffected = true;
                        }

                        //destroys items / triggers bombs caught in the blast.
                        Heap heap = Dungeon.level.heaps[c];
                        if (heap != null)
                            heap.Explode();

                        var ch = Actor.FindChar(c);
                        if (ch != null)
                            affected.Add(ch);
                    }
                }

                foreach (Character ch in affected)
                {
                    //if they have already been killed by another bomb
                    if (!ch.IsAlive())
                        continue;

                    int dmg = Rnd.NormalIntRange(5 + Dungeon.depth, 10 + Dungeon.depth * 2);

                    //those not at the center of the blast take less damage
                    if (ch.pos != cell)
                        dmg = (int)Math.Round(dmg * 0.67f, MidpointRounding.AwayFromZero);

                    dmg -= ch.DrRoll();

                    if (dmg > 0)
                        ch.Damage(dmg, this);

                    if (ch == Dungeon.hero && !ch.IsAlive())
                        Dungeon.Fail(typeof(Bomb));
                }

                if (terrainAffected)
                    Dungeon.Observe();
            }
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override Item Random()
        {
            switch (Rnd.Int(4))
            {
                case 0:
                    return new DoubleBomb();
                default:
                    return this;
            }
        }

        public override ItemSprite.Glowing Glowing()
        {
            var c = new Color(0xFF, 0x00, 0x00, 0xFF);
            return fuse != null ? new ItemSprite.Glowing(c, 0.6f) : null;
        }

        public override int Value()
        {
            return 20 * quantity;
        }

        public override string Desc()
        {
            if (fuse == null)
                return base.Desc() + "\n\n" + Messages.Get(this, "desc_fuse");
            else
                return base.Desc() + "\n\n" + Messages.Get(this, "desc_burning");
        }

        private const string FUSE = "fuse";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(FUSE, fuse);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            if (bundle.Contains(FUSE))
                Actor.Add(fuse = ((Fuse)bundle.Get(FUSE)).Ignite(this));
        }

        [SPDStatic]
        public class Fuse : Actor
        {
            public Fuse()
            {
                actPriority = BLOB_PRIO + 1; //after hero, before other actors
            }

            private Bomb bomb;

            public Fuse Ignite(Bomb bomb)
            {
                this.bomb = bomb;
                return this;
            }

            public override bool Act()
            {
                //something caused our bomb to explode early, or be defused. Do nothing.
                if (bomb.fuse != this)
                {
                    Actor.Remove(this);
                    return true;
                }

                //look for our bomb, remove it from its heap, and blow it up.
                foreach (Heap heap in Dungeon.level.heaps.Values)
                {
                    if (heap.items.Contains(bomb))
                    {
                        //FIXME this is a bit hacky, might want to generalize the functionality
                        //of bombs that don't explode instantly when their fuse ends
                        if (bomb is Noisemaker)
                        {
                            ((Noisemaker)bomb).SetTrigger(heap.pos);
                        }
                        else
                        {
                            heap.Remove(bomb);

                            bomb.Explode(heap.pos);
                        }

                        Deactivate();
                        Actor.Remove(this);
                        return true;
                    }
                }

                //can't find our bomb, something must have removed it, do nothing.
                bomb.fuse = null;
                Actor.Remove(this);
                return true;
            }
        }

        [SPDStatic]
        public class DoubleBomb : Bomb
        {
            public DoubleBomb()
            {
                image = ItemSpriteSheet.DBL_BOMB;
                stackable = false;
            }

            public override bool DoPickUp(Hero hero)
            {
                Bomb bomb = new Bomb();
                bomb.Quantity(2);
                if (bomb.DoPickUp(hero))
                {
                    //isaaaaac.... (don't bother doing this when not in english)
                    if (SPDSettings.Language() == Languages.ENGLISH)
                        hero.sprite.ShowStatus(CharSprite.NEUTRAL, "1+1 free!");
                    return true;
                }
                return false;
            }
        }

        public class EnhanceBomb : Recipe
        {
            public static OrderedDictionary<Type, Type> validIngredients = new OrderedDictionary<Type, Type>();

            static EnhanceBomb()
            {
                InitIngredients();
                InitCosts();
            }

            private static void InitIngredients()
            {
                validIngredients.Add(typeof(PotionOfFrost), typeof(FrostBomb));
                validIngredients.Add(typeof(ScrollOfMirrorImage), typeof(WoollyBomb));

                validIngredients.Add(typeof(PotionOfLiquidFlame), typeof(Firebomb));
                validIngredients.Add(typeof(ScrollOfRage), typeof(Noisemaker));

                validIngredients.Add(typeof(PotionOfInvisibility), typeof(Flashbang));
                validIngredients.Add(typeof(ScrollOfRecharging), typeof(ShockBomb));

                validIngredients.Add(typeof(PotionOfHealing), typeof(RegrowthBomb));
                validIngredients.Add(typeof(ScrollOfRemoveCurse), typeof(HolyBomb));

                validIngredients.Add(typeof(GooBlob), typeof(ArcaneBomb));
                validIngredients.Add(typeof(MetalShard), typeof(ShrapnelBomb));
            }

            //private static final HashMap<Class<?extends Bomb>, Integer> bombCosts = new HashMap<>();
            private static Dictionary<Type, int> bombCosts = new Dictionary<Type, int>();

            private static void InitCosts()
            {
                bombCosts.Add(typeof(FrostBomb), 2);
                bombCosts.Add(typeof(WoollyBomb), 2);

                bombCosts.Add(typeof(Firebomb), 4);
                bombCosts.Add(typeof(Noisemaker), 4);

                bombCosts.Add(typeof(Flashbang), 6);
                bombCosts.Add(typeof(ShockBomb), 6);

                bombCosts.Add(typeof(RegrowthBomb), 8);
                bombCosts.Add(typeof(HolyBomb), 8);

                bombCosts.Add(typeof(ArcaneBomb), 10);
                bombCosts.Add(typeof(ShrapnelBomb), 10);
            }

            public override bool TestIngredients(List<Item> ingredients)
            {
                bool bomb = false;
                bool ingredient = false;

                foreach (Item i in ingredients)
                {
                    if (!i.IsIdentified())
                        return false;

                    if (i.GetType().Equals(typeof(Bomb)))
                        bomb = true;
                    else if (validIngredients.ContainsKey(i.GetType()))
                        ingredient = true;
                }

                return bomb && ingredient;
            }

            public override int Cost(List<Item> ingredients)
            {
                foreach (Item i in ingredients)
                {
                    var type = i.GetType();
                    if (validIngredients.ContainsKey(type))
                    {
                        var bombType = validIngredients[type];
                        return bombCosts[bombType];
                    }
                }
                return 0;
            }

            public override Item Brew(List<Item> ingredients)
            {
                Item result = null;

                foreach (Item i in ingredients)
                {
                    i.Quantity(i.Quantity() - 1);

                    var type = i.GetType();

                    if (validIngredients.ContainsKey(type))
                    {
                        var bombType = validIngredients[type];

                        result = (Item)Reflection.NewInstance(bombType); // TODO확인 (루프를 돌면서 모두 생성이 맞나?)
                    }
                }

                return result;
            }

            public override Item SampleOutput(List<Item> ingredients)
            {
                foreach (Item i in ingredients)
                {
                    var type = i.GetType();

                    if (validIngredients.ContainsKey(type))
                    {
                        var bombType = validIngredients[type];

                        return (Item)Reflection.NewInstance(bombType);
                    }
                }

                return null;
            }
        }
    }
}