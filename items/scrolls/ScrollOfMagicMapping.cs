using watabou.noosa.audio;
using spdd.effects;
using spdd.levels;
using spdd.scenes;
using spdd.utils;
using spdd.sprites;
using spdd.messages;

namespace spdd.items.scrolls
{
    public class ScrollOfMagicMapping : Scroll
    {
        public ScrollOfMagicMapping()
        {
            icon = ItemSpriteSheet.Icons.SCROLL_MAGICMAP;
        }

        public override void DoRead()
        {
            int length = Dungeon.level.Length();
            var map = Dungeon.level.map;
            var mapped = Dungeon.level.mapped;
            var discoverable = Dungeon.level.discoverable;

            var noticed = false;

            for (var i = 0; i < length; ++i)
            {
                var terr = map[i];

                if (discoverable[i])
                {
                    mapped[i] = true;
                    if ((Terrain.flags[terr] & Terrain.SECRET) != 0)
                    {
                        Dungeon.level.Discover(i);

                        if (Dungeon.level.heroFOV[i])
                        {
                            GameScene.DiscoverTile(i, terr);
                            Discover(i);

                            noticed = true;
                        }
                    }
                }
            }

            GameScene.UpdateFog();

            GLog.Information(Messages.Get(this, "layout"));
            if (noticed)
                Sample.Instance.Play(Assets.Sounds.SECRET);

            SpellSprite.Show(curUser, SpellSprite.MAP);
            Sample.Instance.Play(Assets.Sounds.READ);

            SetKnown();

            ReadAnimation();
        }

        public override int Value()
        {
            return IsKnown() ? 40 * quantity : base.Value();
        }

        public static void Discover(int cell)
        {
            CellEmitter.Get(cell).Start(Speck.Factory(Speck.DISCOVER), 0.1f, 4);
        }
    }
}