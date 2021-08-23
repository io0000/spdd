using watabou.utils;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.glwrap;
using watabou.gltextures;

namespace spdd.ui
{
    public class Archs : Component
    {
        private const float SCROLL_SPEED = 20f;

        private SkinnedBlock arcsBg;
        private SkinnedBlock arcsFg;
        private Image darkness;

        private static float offsB;
        private static float offsF;

        public bool reversed;

        protected override void CreateChildren()
        {
            arcsBg = new ArchsSkinnedBlock(1, 1, Assets.Interfaces.ARCS_BG);
            arcsBg.autoAdjust = true;
            arcsBg.OffsetTo(0, offsB);
            Add(arcsBg);

            arcsFg = new ArchsSkinnedBlock(1, 1, Assets.Interfaces.ARCS_FG);
            ((ArchsSkinnedBlock)arcsFg).drawBase = true;
            arcsBg.autoAdjust = true;
            arcsFg.OffsetTo(0, offsF);
            Add(arcsFg);

            Color[] colors = new Color[] {
                new Color(0x00, 0x00, 0x00, 0x00),
                new Color(0x00, 0x00, 0x00, 0x22),
                new Color(0x00, 0x00, 0x00, 0x55),
                new Color(0x00, 0x00, 0x00, 0x99),
                new Color(0x00, 0x00, 0x00, 0xEE)
            };

            darkness = new Image(TextureCache.CreateGradient(colors));
            darkness.angle = 90;
            Add(darkness);
        }

        private class ArchsSkinnedBlock : SkinnedBlock
        {
            internal bool drawBase;

            public ArchsSkinnedBlock(float width, float height, object tx)
                : base(width, height, tx)
            { }

            protected override NoosaScript Script()
            {
                return NoosaScriptNoLighting.Get();
            }

            public override void Draw()
            {
                if (drawBase)
                {
                    base.Draw();
                    return;
                }
                //arch bg has no alpha component, this improves performance
                Blending.Disable();
                base.Draw();
                Blending.Enable();
            }
        }

        protected override void Layout()
        {
            arcsBg.Size(width, height);
            arcsBg.Offset(arcsBg.texture.width / 4 - (width % arcsBg.texture.width) / 2, 0);

            arcsFg.Size(width, height);
            arcsFg.Offset(arcsFg.texture.width / 4 - (width % arcsFg.texture.width) / 2, 0);

            darkness.x = width;
            darkness.scale.x = height / 5f;
            darkness.scale.y = width;
        }

        public override void Update()
        {
            base.Update();

            var shift = Game.elapsed * SCROLL_SPEED;
            if (reversed)
                shift = -shift;

            arcsBg.Offset(0, shift);
            arcsFg.Offset(0, shift * 2);

            offsB = arcsBg.OffsetY();
            offsF = arcsFg.OffsetY();
        }
    }
}