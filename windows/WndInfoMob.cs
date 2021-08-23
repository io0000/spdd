using System;
using watabou.noosa.ui;
using spdd.actors.mobs;
using spdd.messages;
using spdd.scenes;
using spdd.sprites;
using spdd.ui;

namespace spdd.windows
{
    public class WndInfoMob : WndTitledMessage
    {
        public WndInfoMob(Mob mob)
            : base(new MobTitle(mob), mob.Description())
        { }

        private class MobTitle : Component
        {
            internal const int GAP = 2;
            internal CharSprite image;
            internal RenderedTextBlock name;
            internal HealthBar health;
            internal BuffIndicator buffs;

            public MobTitle(Mob mob)
            {
                name = PixelScene.RenderTextBlock(Messages.TitleCase(mob.Name()), 9);
                name.Hardlight(TITLE_COLOR);
                Add(name);

                image = mob.GetSprite();
                Add(image);

                health = new HealthBar();
                health.Level(mob);
                Add(health);

                buffs = new BuffIndicator(mob);
                Add(buffs);
            }

            protected override void Layout()
            {
                image.x = 0;
                image.y = Math.Max(0, name.Height() + health.Height() - image.Height());

                name.SetPos(x + image.width + GAP, image.Height() > name.Height() ? y + (image.Height() - name.Height()) / 2 : y);

                float w = width - image.Width() - GAP;

                health.SetRect(image.Width() + GAP, name.Bottom() + GAP, w, health.Height());

                buffs.SetPos(
                    name.Right() + GAP - 1,
                    name.Bottom() - BuffIndicator.SIZE - 2);

                height = health.Bottom();
            }
        }
    }
}