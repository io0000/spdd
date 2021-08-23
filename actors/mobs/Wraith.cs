using watabou.utils;
using watabou.noosa.tweeners;
using spdd.scenes;
using spdd.sprites;
using spdd.effects.particles;

namespace spdd.actors.mobs
{
    public class Wraith : Mob
    {
        private const float SPAWN_DELAY = 2f;

        private int level;

        public Wraith()
        {
            spriteClass = typeof(WraithSprite);

            HP = HT = 1;
            EXP = 0;

            maxLvl = -2;

            flying = true;

            properties.Add(Property.UNDEAD);
        }

        private const string Level = "level";

        public override void StoreInBundle(Bundle bundle)
        {
            base.StoreInBundle(bundle);
            bundle.Put(Level, level);
        }

        public override void RestoreFromBundle(Bundle bundle)
        {
            base.RestoreFromBundle(bundle);
            level = bundle.GetInt(Level);
            AdjustStats(level);
        }

        public override int DamageRoll()
        {
            return Rnd.NormalIntRange(1 + level / 2, 2 + level);
        }

        public override int AttackSkill(Character target)
        {
            return 10 + level;
        }

        public virtual void AdjustStats(int level)
        {
            this.level = level;
            defenseSkill = AttackSkill(null) * 5;
            enemySeen = true;
        }

        public override float SpawningWeight()
        {
            return 0f;
        }

        public override bool Reset()
        {
            state = WANDERING;
            return true;
        }

        public static void SpawnAround(int pos)
        {
            foreach (var n in PathFinder.NEIGHBORS4)
            {
                SpawnAt(pos + n);
            }
        }

        public static Wraith SpawnAt(int pos)
        {
            if (!Dungeon.level.solid[pos] && Actor.FindChar(pos) == null)
            {
                var w = new Wraith();
                w.AdjustStats(Dungeon.depth);
                w.pos = pos;
                w.state = w.HUNTING;
                GameScene.Add(w, SPAWN_DELAY);

                w.sprite.Alpha(0);
                w.sprite.parent.Add(new AlphaTweener(w.sprite, 1, 0.5f));

                w.sprite.Emitter().Burst(ShadowParticle.Curse, 5);

                return w;
            }
            else
            {
                return null;
            }
        }
    }
}