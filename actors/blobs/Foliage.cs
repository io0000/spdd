using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.effects;
using spdd.effects.particles;
using spdd.levels;
using spdd.scenes;
using spdd.journal;
using spdd.messages;

namespace spdd.actors.blobs
{
    // ³ª¹µÀÙ
    public class Foliage : Blob
    {
        protected override void Evolve()
        {
            var map = Dungeon.level.map;
            bool visible = false;

            int cell;

            for (int i = area.left; i < area.right; ++i)
            {
                for (int j = area.top; j < area.bottom; ++j)
                {
                    cell = i + j * Dungeon.level.Width();
                    if (cur[cell] > 0)
                    {
                        off[cell] = cur[cell];
                        volume += off[cell];

                        if (map[cell] == Terrain.EMBERS)
                        {
                            map[cell] = Terrain.GRASS;
                            GameScene.UpdateMap(cell);
                        }

                        visible = visible || Dungeon.level.heroFOV[cell];
                    }
                    else
                    {
                        off[cell] = 0;
                    }
                }
            }

            var hero = Dungeon.hero;
            if (hero.IsAlive() && hero.VisibleEnemies() == 0 && cur[hero.pos] > 0)
            {
                Shadows s = hero.FindBuff<Shadows>();
                if (s == null)
                {
                    Buff.Affect<Shadows>(hero).Prolong();
                    Sample.Instance.Play(Assets.Sounds.MELD);
                }
                else
                {
                    s.Prolong();
                }
            }

            if (visible)
                Notes.Add(Notes.Landmark.GARDEN);
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);
            emitter.Start(ShaftParticle.Factory, 0.9f, 0);
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }
    }
}