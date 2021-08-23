using watabou.utils;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.mobs;
using spdd.sprites;
using spdd.effects;
using spdd.messages;

namespace spdd.items.stones
{
    public class StoneOfAggression : Runestone
    {
        public StoneOfAggression()
        {
            image = ItemSpriteSheet.STONE_AGGRESSION;
        }

        protected override void Activate(int cell)
        {
            var ch = Actor.FindChar(cell);

            if (ch != null)
            {
                if (ch.alignment == Character.Alignment.ENEMY)
                {
                    Buff.Prolong<Aggression>(ch, Aggression.DURATION / 5f);
                }
                else
                {
                    Buff.Prolong<Aggression>(ch, Aggression.DURATION);
                }
                CellEmitter.Center(cell).Start(Speck.Factory(Speck.SCREAM), 0.3f, 3);
                Sample.Instance.Play(Assets.Sounds.READ);
            }
            else
            {
                //Item.onThrow
                Heap heap = Dungeon.level.Drop(this, cell);
                if (!heap.IsEmpty())
                {
                    heap.sprite.Drop(cell);
                }
            }
        }

        [SPDStatic]
        public class Aggression : FlavourBuff
        {
            public const float DURATION = 20f;

            public Aggression()
            {
                type = BuffType.NEGATIVE;
                announced = true;
            }

            public override void StoreInBundle(Bundle bundle)
            {
                base.StoreInBundle(bundle);
            }

            public override void RestoreFromBundle(Bundle bundle)
            {
                base.RestoreFromBundle(bundle);
            }

            public override void Detach()
            {
                //if our target is an enemy, reset the aggro of any enemies targeting it
                if (target.IsAlive())
                {
                    if (target.alignment == Character.Alignment.ENEMY)
                    {
                        foreach (Mob m in Dungeon.level.mobs)
                        {
                            if (m.alignment == Character.Alignment.ENEMY && m.IsTargeting(target))
                            {
                                m.Aggro(null);
                            }
                        }
                    }
                }
                base.Detach();
            }

            public override string ToString()
            {
                return Messages.Get(this, "name");
            }
        }
    }
}