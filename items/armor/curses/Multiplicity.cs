using System.Collections.Generic;
using watabou.utils;
using spdd.actors;
using spdd.actors.hero;
using spdd.actors.buffs;
using spdd.actors.mobs;
using spdd.actors.mobs.npcs;
using spdd.items.scrolls;
using spdd.scenes;
using spdd.sprites;

namespace spdd.items.armor.curses
{
    public class Multiplicity : Armor.Glyph
    {
        private static ItemSprite.Glowing BLACK = new ItemSprite.Glowing(new Color(0x00, 0x00, 0x00, 0xFF));

        public override int Proc(Armor armor, Character attacker, Character defender, int damage)
        {
            if (Rnd.Int(20) == 0)
            {
                var spawnPoints = new List<int>();

                for (int i = 0; i < PathFinder.NEIGHBORS8.Length; ++i)
                {
                    int p = defender.pos + PathFinder.NEIGHBORS8[i];
                    if (Actor.FindChar(p) == null && (Dungeon.level.passable[p] || Dungeon.level.avoid[p]))
                    {
                        spawnPoints.Add(p);
                    }
                }

                if (spawnPoints.Count > 0)
                {
                    Mob m = null;

                    if (Rnd.Int(2) == 0 && defender is Hero)
                    {
                        m = new MirrorImage();
                        ((MirrorImage)m).Duplicate((Hero)defender);
                    }
                    else
                    {
                        //FIXME should probably have a mob property for this
                        if (!(attacker is Mob) ||
                            attacker.Properties().Contains(Character.Property.BOSS) ||
                            attacker.Properties().Contains(Character.Property.MINIBOSS) ||
                            attacker is Mimic ||
                            attacker is Statue)
                        {
                            m = Dungeon.level.CreateMob();
                        }
                        else
                        {
                            Actor.FixTime();

                            m = (Mob)Reflection.NewInstance(attacker.GetType());

                            if (m != null)
                            {
                                Bundle store = new Bundle();
                                attacker.StoreInBundle(store);
                                m.RestoreFromBundle(store);
                                m.pos = 0;
                                m.HP = m.HT;
                                if (m.FindBuff<PinCushion>() != null)
                                {
                                    m.Remove(m.FindBuff<PinCushion>());
                                }

                                //If a thief has stolen an item, that item is not duplicated.
                                if (m is Thief)
                                {
                                    ((Thief)m).item = null;
                                }
                            }
                        }
                    }

                    if (m != null)
                    {
                        if (Character.HasProp(m, Character.Property.LARGE))
                        {
                            foreach (int i in spawnPoints.ToArray())
                            {
                                if (!Dungeon.level.openSpace[i])
                                {
                                    //remove the value, not at the index
                                    spawnPoints.Remove(i);
                                }
                            }
                        }

                        if (spawnPoints.Count > 0)
                        {
                            GameScene.Add(m);
                            ScrollOfTeleportation.Appear(m, Rnd.Element(spawnPoints));
                        }
                    }
                }
            }

            return damage;
        }

        public override ItemSprite.Glowing Glowing()
        {
            return BLACK;
        }

        public override bool Curse()
        {
            return true;
        }
    }
}