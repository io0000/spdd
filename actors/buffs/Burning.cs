using System;
using System.Collections.Generic;
using watabou.utils;
using spdd.actors.blobs;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.effects.particles;
using spdd.items;
using spdd.items.food;
using spdd.items.scrolls;
using spdd.scenes;
using spdd.ui;
using spdd.utils;
using spdd.sprites;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class Burning : Buff, Hero.IDoom
    {
        private const float DURATION = 8.0f;

        private float left;

        //for tracking burning of hero items
        private int burnIncrement;

        private const string LEFT = "left";
        private const string BURN = "burnIncrement";

        public Burning()
        {
            type = BuffType.NEGATIVE;
            announced = true;
        }

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(LEFT, left);
            bundle.Put(BURN, burnIncrement);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            left = bundle.GetFloat(LEFT);
            burnIncrement = bundle.GetInt(BURN);
        }

        public override bool Act()
        {
            if (target.IsAlive() && !target.IsImmune(GetType()))
            {
                int damage = Rnd.NormalIntRange(1, 3 + Dungeon.depth / 4);
                Buff.Detach<Chill>(target);

                if (target is Hero)
                {
                    Hero hero = (Hero)target;
                    hero.Damage(damage, this);
                    ++burnIncrement;

                    //at 4+ turns, there is a (turns-3)/3 chance an item burns
                    if (Rnd.Int(3) < (burnIncrement - 3))
                    {
                        burnIncrement = 0;

                        List<Item> burnable = new List<Item>();
                        //does not reach inside of containers
                        foreach (Item i in hero.belongings.backpack.items)
                        {
                            if (!i.unique && (i is Scroll || i is MysteryMeat))
                            {
                                burnable.Add(i);
                            }
                        }

                        if (burnable.Count > 0)
                        {
                            Item toBurn = Rnd.Element(burnable).Detach(hero.belongings.backpack);
                            GLog.Warning(Messages.Get(this, "burnsup", Messages.Capitalize(toBurn.ToString())));

                            if (toBurn is MysteryMeat)
                            {
                                ChargrilledMeat steak = new ChargrilledMeat();
                                if (!steak.Collect(hero.belongings.backpack))
                                {
                                    Dungeon.level.Drop(steak, hero.pos).sprite.Drop();
                                }
                            }
                            Heap.BurnFX(hero.pos);
                        }
                    }
                }
                else
                {
                    target.Damage(damage, this);
                }

                if (target is Thief && ((Thief)target).item != null)
                {
                    Item item = ((Thief)target).item;

                    if (!item.unique && item is Scroll)
                    {
                        target.sprite.Emitter().Burst(ElmoParticle.Factory, 6);
                        ((Thief)target).item = null;
                    }
                    else if (item is MysteryMeat)
                    {
                        target.sprite.Emitter().Burst(ElmoParticle.Factory, 6);
                        ((Thief)target).item = new ChargrilledMeat();
                    }
                }
            }
            else
            {
                Detach();
            }

            if (Dungeon.level.flamable[target.pos] &&
                Blob.VolumeAt(target.pos, typeof(Fire)) == 0)
            {
                GameScene.Add(Blob.Seed(target.pos, 4, typeof(Fire)));
            }

            Spend(TICK);
            left -= TICK;

            if (left <= 0 ||
                (Dungeon.level.water[target.pos] && !target.flying))
            {
                Detach();
            }

            return true;
        }

        public void Reignite(Character ch)
        {
            Reignite(ch, Burning.DURATION);
        }

        public void Reignite(Character ch, float duration)
        {
            left = duration;
        }

        public override int Icon()
        {
            return BuffIndicator.FIRE;
        }

        public override float IconFadePercent()
        {
            return Math.Max(0, (DURATION - left) / DURATION);
        }

        public override void Fx(bool on)
        {
            if (on)
                target.sprite.Add(CharSprite.State.BURNING);
            else
                target.sprite.Remove(CharSprite.State.BURNING);
        }

        public override string HeroMessage()
        {
            return Messages.Get(this, "heromsg");
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", DispTurns(left));
        }

        // Hero.IDoom
        public void OnDeath()
        {
            BadgesExtensions.ValidateDeathFromFire();

            Dungeon.Fail(GetType());
            GLog.Negative(Messages.Get(this, "ondeath"));
        }
    }
}