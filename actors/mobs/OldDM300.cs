using watabou.utils;
using watabou.noosa.audio;
using watabou.noosa;
using spdd.sprites;
using spdd.actors.buffs;
using spdd.actors.blobs;
using spdd.scenes;
using spdd.utils;
using spdd.ui;
using spdd.effects;
using spdd.items.quest;
using spdd.items.keys;
using spdd.levels;
using spdd.effects.particles;
using spdd.messages;

namespace spdd.actors.mobs
{
    public class OldDM300 : Mob
    {
        public OldDM300()
        {
            InitInstance();

            spriteClass = typeof(DM300Sprite);

            HP = HT = 200;
            EXP = 30;
            defenseSkill = 18;

            properties.Add(Property.BOSS);
            properties.Add(Property.INORGANIC);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(20, 25);
        }

        public override int AttackSkill(Character target)
        {
            return 28;
        }

        public override int DrRoll()
        {
            return Rnd.NormalIntRange(0, 10);
        }

        public override bool Act()
        {
            GameScene.Add(Blob.Seed(pos, 30, typeof(ToxicGas)));
            return base.Act();
        }

        public override void Move(int step)
        {
            base.Move(step);

            if (Dungeon.level.map[step] == Terrain.INACTIVE_TRAP && HP < HT)
            {
                HP += Rnd.Int(1, HT - HP);
                sprite.Emitter().Burst(ElmoParticle.Factory, 5);

                if (Dungeon.level.heroFOV[step] && Dungeon.hero.IsAlive())
                {
                    GLog.Negative(Messages.Get(this, "repair"));
                }
            }

            int w = Dungeon.level.Width();
            int[] cells = {
                step-1, step+1, step-w, step+w,
                step-1-w,
                step-1+w,
                step+1-w,
                step+1+w
            };
            int cell = cells[Rnd.Int(cells.Length)];

            if (Dungeon.level.heroFOV[cell])
            {
                CellEmitter.Get(cell).Start(Speck.Factory(Speck.ROCK), 0.07f, 10);
                Camera.main.Shake(3, 0.7f);
                Sample.Instance.Play(Assets.Sounds.ROCKS);

                if (Dungeon.level.water[cell])
                {
                    GameScene.Ripple(cell);
                }
                else if (Dungeon.level.map[cell] == Terrain.EMPTY)
                {
                    Level.Set(cell, Terrain.EMPTY_DECO);
                    GameScene.UpdateMap(cell);
                }
            }

            var ch = Actor.FindChar(cell);
            if (ch != null && ch != this)
            {
                Buff.Prolong<Paralysis>(ch, 2);
            }
        }

        public override void Damage(int dmg, object src)
        {
            base.Damage(dmg, src);

            LockedFloor lockedFloor = Dungeon.hero.FindBuff<LockedFloor>();
            if (lockedFloor != null && !IsImmune(src.GetType())) 
                lockedFloor.AddTime(dmg * 1.5f);
        }

        public override void Die(object cause)
        {
            base.Die(cause);

            GameScene.BossSlain();
            Dungeon.level.Drop(new SkeletonKey(Dungeon.depth), pos).sprite.Drop(pos);

            //60% chance of 2 shards, 30% chance of 3, 10% chance for 4. Average of 2.5
            int shards = Rnd.Chances(new float[] { 0, 0, 6, 3, 1 });
            for (int i = 0; i < shards; ++i)
            {
                int ofs;
                do
                {
                    ofs = PathFinder.NEIGHBORS8[Rnd.Int(8)];
                } 
                while (!Dungeon.level.passable[pos + ofs]);
                Dungeon.level.Drop(new MetalShard(), pos + ofs).sprite.Drop(pos);
            }

            BadgesExtensions.ValidateBossSlain();

            var beacon = Dungeon.hero.belongings.GetItem<items.artifacts.LloydsBeacon>();
            if (beacon != null)
                beacon.Upgrade();

            Yell(Messages.Get(this, "defeated"));
        }

        public override void Notice()
        {
            base.Notice();
            if (!BossHealthBar.IsAssigned())
            {
                BossHealthBar.AssignBoss(this);
                Yell(Messages.Get(this, "notice"));
                foreach (var ch in Actor.Chars())
                {
                    if (ch is items.artifacts.DriedRose.GhostHero)
                        ((items.artifacts.DriedRose.GhostHero)ch).SayBoss();
                }
            }
        }

        private void InitInstance()
        {
            immunities.Add(typeof(ToxicGas));
            immunities.Add(typeof(Terror));
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            BossHealthBar.AssignBoss(this);
        }
    }
}
