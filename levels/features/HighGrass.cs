using watabou.noosa.audio;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects;
using spdd.effects.particles;
using spdd.items;
using spdd.items.armor.glyphs;
using spdd.items.artifacts;
using spdd.scenes;

namespace spdd.levels.features
{
    public class HighGrass
    {
        //prevents items dropped from grass, from trampling that same grass.
        //yes this is a bit ugly, oh well.
        private static bool freezeTrample;

        public static void Trample(Level level, int pos)
        {
            if (freezeTrample)
                return;

            var ch = Actor.FindChar(pos);

            if (level.map[pos] == Terrain.FURROWED_GRASS)
            {
                if (ch is Hero && ((Hero)ch).heroClass == HeroClass.HUNTRESS)
                {
                    //Do nothing
                    freezeTrample = true;
                }
                else
                {
                    Level.Set(pos, Terrain.GRASS);
                }
            }
            else
            {
                if (ch is Hero && ((Hero)ch).heroClass == HeroClass.HUNTRESS)
                {
                    Level.Set(pos, Terrain.FURROWED_GRASS);
                    freezeTrample = true;
                }
                else
                {
                    Level.Set(pos, Terrain.GRASS);
                }

                int naturalismLevel = 0;

                if (ch != null)
                {
                    var naturalism = ch.FindBuff<SandalsOfNature.Naturalism>();
                    if (naturalism != null)
                    {
                        if (!naturalism.IsCursed())
                        {
                            naturalismLevel = naturalism.ItemLevel() + 1;
                            naturalism.Charge();
                        }
                        else
                        {
                            naturalismLevel = -1;
                        }
                    }
                }

                if (naturalismLevel >= 0)
                {
                    // Seed, scales from 1/25 to 1/5
                    if (Rnd.Int(25 - (naturalismLevel * 5)) == 0)
                    {
                        level.Drop(Generator.Random(Generator.Category.SEED), pos).sprite.Drop();
                    }

                    // Dew, scales from 1/6 to 1/3
                    if (Rnd.Int(24 - naturalismLevel * 3) <= 3)
                    {
                        level.Drop(new Dewdrop(), pos).sprite.Drop();
                    }
                }

                if (ch is Hero)
                {
                    Hero hero = (Hero)ch;

                    //Camouflage
                    //FIXME doesn't work with sad ghost
                    if (hero.belongings.armor != null && hero.belongings.armor.HasGlyph(typeof(Camouflage), hero))
                    {
                        Buff.Prolong<Invisibility>(hero, 3 + hero.belongings.armor.BuffedLvl() / 2);
                        Sample.Instance.Play(Assets.Sounds.MELD);
                    }
                }
            }

            freezeTrample = false;

            if (ShatteredPixelDungeonDash.Scene() is GameScene)
            {
                GameScene.UpdateMap(pos);

                CellEmitter.Get(pos).Burst(LeafParticle.LevelSpecific, 4);
                if (Dungeon.level.heroFOV[pos])
                {
                    Dungeon.Observe();
                }
            }
        }
    }
}