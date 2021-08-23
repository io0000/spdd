using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.sprites;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.scenes;
using spdd.effects;
using spdd.effects.particles;

namespace spdd.items.quest
{
    public class CeremonialCandle : Item
    {
        //generated with the wandmaker quest
        public static int ritualPos;

        public CeremonialCandle()
        {
            image = ItemSpriteSheet.CANDLE;

            defaultAction = AC_THROW;

            unique = true;
            stackable = true;
        }

        public override bool IsUpgradable()
        {
            return false;
        }

        public override bool IsIdentified()
        {
            return true;
        }

        public override void DoDrop(Hero hero)
        {
            base.DoDrop(hero);
            CheckCandles();
        }

        public override void OnThrow(int cell)
        {
            base.OnThrow(cell);
            CheckCandles();
        }

        private static void CheckCandles()
        {
            var level = Dungeon.level;

            Heap heapTop = level.heaps[ritualPos - level.Width()];
            Heap heapRight = level.heaps[ritualPos + 1];
            Heap heapBottom = level.heaps[ritualPos + level.Width()];
            Heap heapLeft = level.heaps[ritualPos - 1];

            if (heapTop != null &&
                heapRight != null &&
                heapBottom != null &&
                heapLeft != null)
            {
                if (heapTop.Peek() is CeremonialCandle &&
                    heapRight.Peek() is CeremonialCandle &&
                    heapBottom.Peek() is CeremonialCandle &&
                    heapLeft.Peek() is CeremonialCandle)
                {
                    heapTop.PickUp();
                    heapRight.PickUp();
                    heapBottom.PickUp();
                    heapLeft.PickUp();

                    var elemental = new Elemental.NewbornFireElemental();
                    var ch = Actor.FindChar(ritualPos);
                    if (ch != null)
                    {
                        List<int> candidates = new List<int>();
                        foreach (int n in PathFinder.NEIGHBORS8)
                        {
                            int cell = ritualPos + n;
                            if ((level.passable[cell] || level.avoid[cell]) && Actor.FindChar(cell) == null)
                                candidates.Add(cell);
                        }

                        if (candidates.Count > 0)
                            elemental.pos = Rnd.Element(candidates);
                        else
                            elemental.pos = ritualPos;
                    }
                    else
                    {
                        elemental.pos = ritualPos;
                    }

                    elemental.state = elemental.HUNTING;
                    GameScene.Add(elemental, 1);

                    foreach (int i in PathFinder.NEIGHBORS9)
                    {
                        CellEmitter.Get(ritualPos + i).Burst(ElmoParticle.Factory, 10);
                    }
                    Sample.Instance.Play(Assets.Sounds.BURNING);
                }
            }
        }
    }
}