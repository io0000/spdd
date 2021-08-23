using watabou.noosa.audio;
using spdd.actors.buffs;
using spdd.actors.hero;
using spdd.effects;
using spdd.items;
using spdd.levels;
using spdd.scenes;
using spdd.utils;
using spdd.journal;
using spdd.messages;
using spdd.tiles;

namespace spdd.actors.blobs
{
    public class WaterOfAwareness : WellWater
    {
        protected override bool AffectHero(Hero hero)
        {
            Sample.Instance.Play(Assets.Sounds.DRINK);
            emitter.parent.Add(new Identification(hero.sprite.Center()));

            hero.belongings.Observe();

            for (var i = 0; i < Dungeon.level.Length(); ++i)
            {
                var terr = Dungeon.level.map[i];

                if ((Terrain.flags[terr] & Terrain.SECRET) != 0)
                {
                    Dungeon.level.Discover(i);

                    if (Dungeon.level.heroFOV[i])
                        GameScene.DiscoverTile(i, terr);
                }
            }

            Buff.Affect<Awareness>(hero, Awareness.DURATION);
            Dungeon.Observe();

            Dungeon.hero.Interrupt();

            GLog.Positive(Messages.Get(this, "procced"));

            return true;
        }

        protected override Item AffectItem(Item item, int pos)
        {
            if (item.IsIdentified())
                return null;
            
            item.Identify();
            BadgesExtensions.ValidateItemLevelAquired(item);

            Sample.Instance.Play(Assets.Sounds.DRINK);
            emitter.parent.Add(new Identification(DungeonTilemap.TileCenterToWorld(pos)));

            return item;
        }

        protected override Notes.Landmark Record()
        {
            return Notes.Landmark.WELL_OF_AWARENESS;
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);
            emitter.Pour(Speck.Factory(Speck.QUESTION), 0.3f);
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }
    }
}