using watabou.utils;
using spdd.actors.buffs;
using spdd.actors.blobs;
using spdd.items.food;
using spdd.mechanics;
using spdd.scenes;
using spdd.sprites;

namespace spdd.actors.mobs
{
    public class Spinner : Mob
    {
        public Spinner()
        {
            InitInstance();

            spriteClass = typeof(SpinnerSprite);

            HP = HT = 50;
            defenseSkill = 17;

            EXP = 9;
            maxLvl = 17;

            loot = new MysteryMeat();
            lootChance = 0.125f;

            FLEEING = new SpinnerFleeing(this);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(10, 25);
        }

        public override int AttackSkill(Character target)
        {
            return 22;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 6);
        }

        private int webCoolDown;
        private int lastEnemyPos = -1;

        private const string WEB_COOLDOWN = "web_cooldown";
        private const string LAST_ENEMY_POS = "last_enemy_pos";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(WEB_COOLDOWN, webCoolDown);
            bundle.Put(LAST_ENEMY_POS, lastEnemyPos);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            webCoolDown = bundle.GetInt(WEB_COOLDOWN);
            lastEnemyPos = bundle.GetInt(LAST_ENEMY_POS);
        }

        public override bool Act()
        {
            var lastState = state;
            bool result = base.Act();

            //if state changed from wandering to hunting, we haven't acted yet, don't update.
            if (!(lastState == WANDERING && state == HUNTING))
            {
                --webCoolDown;
                if (shotWebVisually)
                {
                    result = shotWebVisually = false;
                }
                else
                {
                    if (enemy != null && enemySeen)
                    {
                        lastEnemyPos = enemy.pos;
                    }
                    else
                    {
                        lastEnemyPos = Dungeon.hero.pos;
                    }
                }
            }

            if (state == FLEEING &&
                FindBuff<Terror>() == null &&
                enemy != null &&
                enemySeen &&
                enemy.FindBuff<Poison>() == null)
            {
                state = HUNTING;
            }
            return result;
        }

        public override int AttackProc(Character enemy, int damage)
        {
            damage = base.AttackProc(enemy, damage);

            if (Rnd.Int(2) == 0)
            {
                Buff.Affect<Poison>(enemy).Set(Rnd.Int(7, 9));
                webCoolDown = 0;
                state = FLEEING;
            }

            return damage;
        }

        private bool shotWebVisually;

        public override void Move(int step)
        {
            if (enemySeen && webCoolDown <= 0 && lastEnemyPos != -1)
            {
                if (WebPos() != -1)
                {
                    if (sprite != null && (sprite.visible || enemy.sprite.visible))
                    {
                        sprite.Zap(WebPos());
                        shotWebVisually = true;
                    }
                    else
                    {
                        ShootWeb();
                    }
                }
            }
            base.Move(step);
        }

        public int WebPos()
        {
            if (enemy == null)
                return -1;

            Ballistic b;
            //aims web in direction _enemy is moving, or between self and _enemy if they aren't moving
            if (lastEnemyPos == enemy.pos)
            {
                b = new Ballistic(enemy.pos, pos, Ballistic.WONT_STOP);
            }
            else
            {
                b = new Ballistic(lastEnemyPos, enemy.pos, Ballistic.WONT_STOP);
            }

            int collisionIndex = 0;
            for (int i = 0; i < b.path.Count; ++i)
            {
                if (b.path[i] == enemy.pos)
                {
                    collisionIndex = i;
                    break;
                }
            }

            //in case target is at the edge of the map and there are no more cells in the path
            if (b.path.Count <= collisionIndex + 1)
            {
                return -1;
            }

            int webPos = b.path[collisionIndex + 1];

            if (Dungeon.level.passable[webPos])
            {
                return webPos;
            }
            else
            {
                return -1;
            }
        }

        public void ShootWeb()
        {
            int webPos = WebPos();
            if (enemy != null && webPos != enemy.pos && webPos != -1)
            {
                int i;
                for (i = 0; i < PathFinder.CIRCLE8.Length; ++i)
                {
                    if ((enemy.pos + PathFinder.CIRCLE8[i]) == webPos)
                    {
                        break;
                    }
                }

                //spread to the tile hero was moving towards and the two adjacent ones
                int leftPos = enemy.pos + PathFinder.CIRCLE8[Left(i)];
                int rightPos = enemy.pos + PathFinder.CIRCLE8[Right(i)];

                if (Dungeon.level.passable[leftPos])
                    GameScene.Add(Blob.Seed(leftPos, 20, typeof(Web)));
                if (Dungeon.level.passable[webPos])
                    GameScene.Add(Blob.Seed(webPos, 20, typeof(Web)));
                if (Dungeon.level.passable[rightPos])
                    GameScene.Add(Blob.Seed(rightPos, 20, typeof(Web)));

                webCoolDown = 10;

                if (Dungeon.level.heroFOV[enemy.pos])
                {
                    Dungeon.hero.Interrupt();
                }
            }
            Next();
        }

        private int Left(int direction)
        {
            return direction == 0 ? 7 : direction - 1;
        }

        private int Right(int direction)
        {
            return direction == 7 ? 0 : direction + 1;
        }

        private void InitInstance()
        {
            resistances.Add(typeof(Poison));
            immunities.Add(typeof(Web));
        }

        private class SpinnerFleeing : Fleeing
        {
            public SpinnerFleeing(Mob mob)
                : base(mob)
            { }

            public override void NowhereToRun()
            {
                if (mob.FindBuff<Terror>() == null)
                    mob.state = mob.HUNTING;
                else
                    base.NowhereToRun();
            }
        }
    }
}