using System;
using System.Collections.Generic;
using watabou.noosa;
using watabou.noosa.tweeners;
using watabou.utils;
using spdd.items;
using spdd.items.weapon;
using spdd.items.weapon.melee;
using spdd.items.weapon.missiles;
using spdd.items.weapon.missiles.darts;
using spdd.tiles;

namespace spdd.sprites
{
    public class MissileSprite : ItemSprite, Tweener.IListener
    {
        private const float SPEED = 240.0f;

        private ICallback callback;

        public virtual void Reset(int from, int to, Item item, ICallback listener)
        {
            //Reset(Dungeon.level.solid[from] ? DungeonTilemap.RaisedTileCenterToWorld(from) : DungeonTilemap.RaisedTileCenterToWorld(from),
            //        Dungeon.level.solid[to] ? DungeonTilemap.RaisedTileCenterToWorld(to) : DungeonTilemap.RaisedTileCenterToWorld(to),
            //        item, listener);

            Reset(DungeonTilemap.RaisedTileCenterToWorld(from),
                  DungeonTilemap.RaisedTileCenterToWorld(to),
                  item, listener);
        }

        public void Reset(Visual from, int to, Item item, ICallback listener)
        {
            //Reset(from.Center(),
            //        Dungeon.level.solid[to] ? DungeonTilemap.RaisedTileCenterToWorld(to) : DungeonTilemap.RaisedTileCenterToWorld(to),
            //        item, listener);
            Reset(from.Center(),
                  DungeonTilemap.RaisedTileCenterToWorld(to),
                  item, listener);
        }

        public void Reset(int from, Visual to, Item item, ICallback listener)
        {
            //Reset(Dungeon.level.solid[from] ? DungeonTilemap.RaisedTileCenterToWorld(from) : DungeonTilemap.RaisedTileCenterToWorld(from),
            //        to.Center(),
            //        item, listener);

            Reset(DungeonTilemap.RaisedTileCenterToWorld(from),
                    to.Center(),
                    item, listener);
        }

        public void Reset(Visual from, Visual to, Item item, ICallback listener)
        {
            Reset(from.Center(), to.Center(), item, listener);
        }

        public void Reset(PointF from, PointF to, Item item, ICallback listener)
        {
            Revive();

            if (item == null)
                View(0, null);
            else
                View(item);

            Setup(from,
                    to,
                    item,
                    listener);
        }


        private const int DEFAULT_ANGULAR_SPEED = 720;

        private static Dictionary<Type, int> ANGULAR_SPEEDS = new Dictionary<Type, int>();

        static MissileSprite()
        {
            ANGULAR_SPEEDS.Add(typeof(Dart), 0);
            ANGULAR_SPEEDS.Add(typeof(ThrowingKnife), 0);
            ANGULAR_SPEEDS.Add(typeof(FishingSpear), 0);
            ANGULAR_SPEEDS.Add(typeof(ThrowingSpear), 0);
            ANGULAR_SPEEDS.Add(typeof(Kunai), 0);
            ANGULAR_SPEEDS.Add(typeof(Javelin), 0);
            ANGULAR_SPEEDS.Add(typeof(Trident), 0);

            ANGULAR_SPEEDS.Add(typeof(SpiritBow.SpiritArrow), 0);
            ANGULAR_SPEEDS.Add(typeof(ScorpioSprite), 0);

            //720 is default

            ANGULAR_SPEEDS.Add(typeof(HeavyBoomerang), 1440);
            ANGULAR_SPEEDS.Add(typeof(Bolas), 1440);

            ANGULAR_SPEEDS.Add(typeof(Shuriken), 2160);

            ANGULAR_SPEEDS.Add(typeof(TenguSprite.TenguShuriken), 2160);
        }

        //TODO it might be nice to have a source and destination angle, to improve thrown weapon visuals
        private void Setup(PointF from, PointF to, Item item, ICallback listener)
        {
            OriginToCenter();

            //adjust points so they work with the center of the missile sprite, not the corner
            from.x -= Width() / 2;
            to.x -= Width() / 2;
            from.y -= Height() / 2;
            to.y -= Height() / 2;

            this.callback = listener;

            Point(from);

            PointF d = PointF.Diff(to, from);
            base.speed.Set(d).Normalize().Scale(SPEED);

            angularSpeed = DEFAULT_ANGULAR_SPEED;
            foreach (var pair in ANGULAR_SPEEDS)
            {
                var cls = pair.Key;
                if (cls.IsAssignableFrom(item.GetType()))
                {
                    angularSpeed = pair.Value;
                    break;
                }
            }

            angle = 135 - (float)(Math.Atan2(d.x, d.y) / 3.1415926 * 180);

            if (d.x >= 0)
            {
                flipHorizontal = false;
                UpdateFrame();
            }
            else
            {
                angularSpeed = -angularSpeed;
                angle += 90;
                flipHorizontal = true;
                UpdateFrame();
            }

            float speed = SPEED;

            if (item is Dart && Dungeon.hero.belongings.weapon is Crossbow)
            {
                speed *= 3.0f;
            }
            else if (item is SpiritBow.SpiritArrow ||
                item is ScorpioSprite.ScorpioShot ||
                item is TenguSprite.TenguShuriken)
            {
                speed *= 1.5f;
            }

            PosTweener tweener = new PosTweener(this, to, d.Length() / speed);
            tweener.listener = this;
            parent.Add(tweener);
        }

        // Tweener.IListener
        public void OnComplete(Tweener tweener)
        {
            Kill();
            if (callback != null)
                callback.Call();
        }
    }
}