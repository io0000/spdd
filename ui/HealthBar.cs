using System;
using watabou.utils;
using watabou.noosa;
using watabou.noosa.ui;
using spdd.actors;

namespace spdd.ui
{
    public class HealthBar : Component
    {
        private static Color COLOR_BG = new Color(0xCC, 0x00, 0x00, 0xFF);
        private static Color COLOR_HP = new Color(0x00, 0xEE, 0x00, 0xFF);
        private static Color COLOR_SHLD = new Color(0xBB, 0xEE, 0xBB, 0xFF);

        private const int HEIGHT = 2;

        private ColorBlock Bg;
        private ColorBlock Shld;
        private ColorBlock Hp;

        private float health;
        private float shield;

        protected override void CreateChildren()
        {
            Bg = new ColorBlock(1, 1, COLOR_BG);
            Add(Bg);

            Shld = new ColorBlock(1, 1, COLOR_SHLD);
            Add(Shld);

            Hp = new ColorBlock(1, 1, COLOR_HP);
            Add(Hp);

            height = HEIGHT;
        }

        protected override void Layout()
        {
            Bg.x = Shld.x = Hp.x = x;
            Bg.y = Shld.y = Hp.y = y;

            Bg.Size(width, height);

            //logic here rounds up to the nearest pixel
            float pixelWidth = width;
            if (GetCamera() != null)
                pixelWidth *= GetCamera().zoom;

            Shld.Size(width * (float)Math.Ceiling(shield * pixelWidth) / pixelWidth, height);
            Hp.Size(width * (float)Math.Ceiling(health * pixelWidth) / pixelWidth, height);
        }

        public void Level(float value)
        {
            Level(value, 0f);
        }

        public void Level(float health, float shield)
        {
            this.health = health;
            this.shield = shield;
            Layout();
        }

        public void Level(Character c)
        {
            float health = c.HP;
            float shield = c.Shielding();
            float max = Math.Max(health + shield, c.HT);

            Level(health / max, (health + shield) / max);
        }
    }
}