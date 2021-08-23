using watabou.noosa.tweeners;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.sprites;

namespace spdd.items.weapon.missiles
{
    public class HeavyBoomerang : MissileWeapon
    {
        public HeavyBoomerang()
        {
            image = ItemSpriteSheet.BOOMERANG;
            hitSound = Assets.Sounds.HIT_CRUSH;
            hitSoundPitch = 1f;

            tier = 4;
            sticky = false;
        }

        public override int Max(int lvl)
        {
            return 4 * tier +                //16 base, down from 20
                   tier * lvl;               //scaling unchanged
        }

        protected override void RangedHit(Character enemy, int cell)
        {
            DecrementDurability();
            if (durability > 0)
            {
                Buff.Append<CircleBack>(Dungeon.hero).Setup(this, cell, Dungeon.hero.pos, Dungeon.depth);
            }
        }

        protected override void RangedMiss(int cell)
        {
            parent = null;
            Buff.Append<CircleBack>(Dungeon.hero).Setup(this, cell, Dungeon.hero.pos, Dungeon.depth);
        }

        [SPDStatic]
        public class CircleBack : Buff
        {
            private MissileWeapon boomerang;
            private int thrownPos;
            private int returnPos;
            private int returnDepth;

            private int left;

            public void Setup(MissileWeapon boomerang, int thrownPos, int returnPos, int returnDepth)
            {
                this.boomerang = boomerang;
                this.thrownPos = thrownPos;
                this.returnPos = returnPos;
                this.returnDepth = returnDepth;
                left = 3;
            }

            public int ReturnPos()
            {
                return returnPos;
            }

            public MissileWeapon Cancel()
            {
                Detach();
                return boomerang;
            }

            public override bool Act()
            {
                if (returnDepth == Dungeon.depth)
                {
                    --left;
                    if (left <= 0)
                    {
                        var returnTarget = Actor.FindChar(returnPos);
                        var target = this.target;
                        MissileSprite visual = ((MissileSprite)Dungeon.hero.sprite.parent.Recycle<MissileSprite>());

                        var callback = new ActionCallback();
                        callback.action = () =>
                        {
                            if (returnTarget == target)
                            {
                                if (target is Hero && boomerang.DoPickUp((Hero)target))
                                {
                                    //grabbing the boomerang takes no time
                                    ((Hero)target).Spend(-TIME_TO_PICK_UP);
                                }
                                else
                                {
                                    Dungeon.level.Drop(boomerang, returnPos).sprite.Drop();
                                }
                            }
                            else if (returnTarget != null)
                            {
                                if (((Hero)target).Shoot(returnTarget, boomerang))
                                    boomerang.DecrementDurability();

                                if (boomerang.durability > 0)
                                    Dungeon.level.Drop(boomerang, returnPos).sprite.Drop();
                            }
                            else
                            {
                                Dungeon.level.Drop(boomerang, returnPos).sprite.Drop();
                            }

                            Next();
                        };

                        visual.Reset(thrownPos, returnPos, boomerang, callback);
                        visual.Alpha(0f);
                        float duration = Dungeon.level.TrueDistance(thrownPos, returnPos) / 20f;
                        target.sprite.parent.Add(new AlphaTweener(visual, 1f, duration));
                        Detach();
                        return false;
                    }
                }
                Spend(TICK);
                return true;
            }

            const string BOOMERANG = "boomerang";
            const string THROWN_POS = "thrown_pos";
            const string RETURN_POS = "return_pos";
            const string RETURN_DEPTH = "return_depth";

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
                bundle.Put(BOOMERANG, boomerang);
                bundle.Put(THROWN_POS, thrownPos);
                bundle.Put(RETURN_POS, returnPos);
                bundle.Put(RETURN_DEPTH, returnDepth);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
                boomerang = (MissileWeapon)bundle.Get(BOOMERANG);
                thrownPos = bundle.GetInt(THROWN_POS);
                returnPos = bundle.GetInt(RETURN_POS);
                returnDepth = bundle.GetInt(RETURN_DEPTH);
            }
        }
    }
}