using System;
using watabou.noosa;
using watabou.utils;
using spdd.ui;
using spdd.scenes;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.actors.mobs.npcs;
using spdd.items.scrolls;
using spdd.messages;

namespace spdd.actors.buffs
{
    public class PrismaticGuard : Buff
    {
        public PrismaticGuard()
        {
            type = BuffType.POSITIVE;
        }

        private float HP;

        public override bool Act()
        {
            Hero hero = (Hero)target;

            Mob closest = null;
            int v = hero.VisibleEnemies();
            for (int i = 0; i < v; ++i)
            {
                Mob mob = hero.VisibleEnemy(i);
                if (mob.IsAlive() &&
                    mob.state != mob.PASSIVE &&
                    mob.state != mob.WANDERING &&
                    mob.state != mob.SLEEPING &&
                    !hero.mindVisionEnemies.Contains(mob) &&
                    (closest == null || Dungeon.level.Distance(hero.pos, mob.pos) < Dungeon.level.Distance(hero.pos, closest.pos)))
                {
                    closest = mob;
                }
            }

            if (closest != null && Dungeon.level.Distance(hero.pos, closest.pos) < 5)
            {
                //spawn guardian
                int bestPos = -1;
                for (int i = 0; i < PathFinder.NEIGHBORS8.Length; ++i)
                {
                    int p = hero.pos + PathFinder.NEIGHBORS8[i];
                    if (Actor.FindChar(p) == null && Dungeon.level.passable[p])
                    {
                        if (bestPos == -1 || Dungeon.level.TrueDistance(p, closest.pos) < Dungeon.level.TrueDistance(bestPos, closest.pos))
                        {
                            bestPos = p;
                        }
                    }
                }
                if (bestPos != -1)
                {
                    PrismaticImage pris = new PrismaticImage();
                    pris.Duplicate(hero, (int)Math.Floor(HP));
                    pris.state = pris.HUNTING;
                    GameScene.Add(pris, 1);
                    ScrollOfTeleportation.Appear(pris, bestPos);

                    Detach();
                }
                else
                {
                    Spend(TICK);
                }
            }
            else
            {
                Spend(TICK);
            }

            var lockedFloor = target.FindBuff<LockedFloor>();
            if (HP < MaxHP() && (lockedFloor == null || lockedFloor.RegenOn()))
            {
                HP += 0.1f;
            }

            return true;
        }

        public void Set(int HP)
        {
            this.HP = HP;
        }

        public int MaxHP()
        {
            return MaxHP((Hero)target);
        }

        public static int MaxHP(Hero hero)
        {
            return 8 + (int)Math.Floor(hero.lvl * 2.5f);
        }

        public override int Icon()
        {
            return BuffIndicator.ARMOR;
        }

        public override void TintIcon(Image icon)
        {
            icon.Hardlight(1f, 1f, 2f);
        }

        public override float IconFadePercent()
        {
            return 1f - HP / (float)MaxHP();
        }

        public override string ToString()
        {
            return Messages.Get(this, "name");
        }

        public override string Desc()
        {
            return Messages.Get(this, "desc", (int)HP, MaxHP());    // %d %d
        }

        private const string HEALTH = "hp";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(HEALTH, HP);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            HP = bundle.GetFloat(HEALTH);
        }
    }
}