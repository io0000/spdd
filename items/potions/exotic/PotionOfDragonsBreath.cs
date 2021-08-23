using System;
using System.Collections.Generic;
using watabou.utils;
using watabou.noosa.audio;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.blobs;
using spdd.effects;
using spdd.sprites;
using spdd.scenes;
using spdd.levels;
using spdd.actors.hero;
using spdd.mechanics;
using spdd.messages;

namespace spdd.items.potions.exotic
{
    public class PotionOfDragonsBreath : ExoticPotion
    {
        public PotionOfDragonsBreath()
        {
            icon = ItemSpriteSheet.Icons.POTION_DRGBREATH;

            targeter = new Targeter(this);
        }

        //need to override drink so that time isn't spent right away
        protected override void Drink(Hero hero)
        {
            curUser = hero;
            curItem = this;

            GameScene.SelectCell(targeter);
        }

        private Targeter targeter;

        class Targeter : CellSelector.IListener
        {
            PotionOfDragonsBreath podb;

            public Targeter(PotionOfDragonsBreath podb)
            {
                this.podb = podb;
            }

            public void OnSelect(int? cell)
            {
                podb.OnSelect(cell);
            }

            public string Prompt()
            {
                return Messages.Get(typeof(PotionOfDragonsBreath), "prompt");
            }
        }

        public void OnSelect(int? c)
        {
            if (c == null && !IsKnown())
            {
                SetKnown();
                Detach(curUser.belongings.backpack);
            }
            else if (c != null)
            {
                int cell = c.Value;

                SetKnown();
                Sample.Instance.Play(Assets.Sounds.DRINK);

                var opCallback = new ActionCallback();
                opCallback.action = () =>
                {
                    curItem.Detach(curUser.belongings.backpack);

                    curUser.Spend(1f);
                    curUser.sprite.Idle();
                    curUser.sprite.Zap(cell);
                    Sample.Instance.Play(Assets.Sounds.BURNING);

                    Ballistic bolt = new Ballistic(curUser.pos, cell, Ballistic.STOP_TERRAIN | Ballistic.IGNORE_DOORS);

                    int maxDist = 6;
                    int dist = Math.Min(bolt.dist, maxDist);

                    ConeAOE cone = new ConeAOE(bolt, 6, 60, Ballistic.STOP_TERRAIN | Ballistic.STOP_TARGET | Ballistic.IGNORE_DOORS);

                    //cast to cells at the tip, rather than all cells, better performance.
                    foreach (Ballistic ray in cone.rays)
                    {
                        ((MagicMissile)curUser.sprite.parent.Recycle<MagicMissile>()).Reset(
                                MagicMissile.FIRE_CONE,
                                curUser.sprite,
                                ray.path[ray.dist],
                                null);
                    }

                    var helperCallback = new ActionCallback();
                    helperCallback.action = () =>
                    {
                        var sourcePos = bolt.sourcePos;

                        List<int> adjacentCells = new List<int>();
                        foreach (int cell2 in cone.cells)
                        {
                            //ignore caster cell
                            if (cell2 == sourcePos)
                                continue;

                            //knock doors open
                            if (Dungeon.level.map[cell2] == Terrain.DOOR)
                            {
                                Level.Set(cell2, Terrain.OPEN_DOOR);
                                GameScene.UpdateMap(cell2);
                            }

                            //only ignite cells directly near caster if they are flammable
                            if (Dungeon.level.Adjacent(sourcePos, cell2) && !Dungeon.level.flamable[cell2])
                            {
                                adjacentCells.Add(cell2);
                            }
                            else
                            {
                                GameScene.Add(Blob.Seed(cell2, 5, typeof(Fire)));
                            }

                            var ch = Actor.FindChar(cell2);
                            if (ch != null)
                            {
                                Buff.Affect<Burning>(ch).Reignite(ch);
                                Buff.Affect<Cripple>(ch, 5f);
                            }
                        }

                        //ignite cells that share a side with an adjacent cell, are flammable, and are further from the source pos
                        //This prevents short-range casts not igniting barricades or bookshelves
                        foreach (int cell2 in adjacentCells)
                        {
                            foreach (int i in PathFinder.NEIGHBORS4)
                            {
                                if (Dungeon.level.TrueDistance(cell2 + i, sourcePos) > Dungeon.level.TrueDistance(cell2, sourcePos) &&
                                    Dungeon.level.flamable[cell2 + i] &&
                                    Fire.VolumeAt(cell2 + i, typeof(Fire)) == 0)
                                {
                                    GameScene.Add(Blob.Seed(cell2 + i, 5, typeof(Fire)));
                                }
                            }
                        }

                        curUser.Next();
                    };

                    MagicMissile.BoltFromChar(curUser.sprite.parent,
                            MagicMissile.FIRE_CONE,
                            curUser.sprite,
                            bolt.path[dist / 2],
                            helperCallback);
                };

                curUser.sprite.Operate(curUser.pos, opCallback);
            }
        }
    }
}