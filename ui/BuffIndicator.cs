using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Collections.Extensions;
using watabou.gltextures;
using watabou.glwrap;
using watabou.noosa;
using watabou.noosa.tweeners;
using watabou.noosa.ui;
using watabou.utils;
using spdd.actors;
using spdd.actors.buffs;
using spdd.scenes;
using spdd.windows;


namespace spdd.ui
{
    public class BuffIndicator : Component
    {
        //transparent icon
        public const int NONE = 63;

        //FIXME this is becoming a mess, should do a big cleaning pass on all of these
        //and think about tinting options
        public const int MIND_VISION = 0;
        public const int LEVITATION = 1;
        public const int FIRE = 2;
        public const int POISON = 3;
        public const int PARALYSIS = 4;
        public const int HUNGER = 5;
        public const int STARVATION = 6;
        public const int SLOW = 7;
        public const int OOZE = 8;
        public const int AMOK = 9;
        public const int TERROR = 10;
        public const int ROOTS = 11;
        public const int INVISIBLE = 12;
        public const int SHADOWS = 13;
        public const int WEAKNESS = 14;
        public const int FROST = 15;
        public const int BLINDNESS = 16;
        public const int COMBO = 17;
        public const int FURY = 18;
        public const int HERB_HEALING = 19;
        public const int ARMOR = 20;
        public const int HEART = 21;
        public const int LIGHT = 22;
        public const int CRIPPLE = 23;
        public const int BARKSKIN = 24;
        public const int IMMUNITY = 25;
        public const int BLEEDING = 26;
        public const int MARK = 27;
        public const int DEFERRED = 28;
        public const int DROWSY = 29;
        public const int MAGIC_SLEEP = 30;
        public const int THORNS = 31;
        public const int FORESIGHT = 32;
        public const int VERTIGO = 33;
        public const int RECHARGING = 34;
        public const int LOCKED_FLOOR = 35;
        public const int CORRUPT = 36;
        public const int BLESS = 37;
        public const int RAGE = 38;
        public const int SACRIFICE = 39;
        public const int BERSERK = 40;
        public const int MOMENTUM = 41;
        public const int PREPARATION = 42;
        public const int WELL_FED = 43;
        public const int HEALING = 44;
        public const int WEAPON = 45;
        public const int VULNERABLE = 46;
        public const int HEX = 47;
        public const int DEGRADE = 48;

        public const int SIZE = 7;

        private static BuffIndicator heroInstance;

        private Texture texture;
        private TextureFilm film;

        private OrderedDictionary<Buff, BuffIcon> buffIcons = new OrderedDictionary<Buff, BuffIcon>();
        private bool needsRefresh;
        private Character ch;

        public BuffIndicator(Character ch)
            : base()
        {
            this.ch = ch;
            if (ch == Dungeon.hero)
            {
                heroInstance = this;
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            if (this == heroInstance)
            {
                heroInstance = null;
            }
        }

        protected override void CreateChildren()
        {
            texture = TextureCache.Get(Assets.Interfaces.BUFFS_SMALL);
            film = new TextureFilm(texture, SIZE, SIZE);
        }

        public override void Update()
        {
            base.Update();
            if (needsRefresh)
            {
                needsRefresh = false;
                Layout();
            }
        }

        protected override void Layout()
        {
            List<Buff> newBuffs = new List<Buff>();
            foreach (Buff buff in ch.Buffs())
            {
                if (buff.Icon() != NONE)
                {
                    newBuffs.Add(buff);
                }
            }

            //remove any icons no longer present
            foreach (Buff buff in buffIcons.Keys.ToArray())
            {
                if (!newBuffs.Contains(buff))
                {
                    Image icon = buffIcons[buff].icon;
                    icon.origin.Set(SIZE / 2f);
                    icon.Alpha(0.6f);
                    Add(icon);
                    Add(new BuffIndicatorAlphaTweener(icon, 0, 0.6f));

                    buffIcons[buff].Destroy();
                    Remove(buffIcons[buff]);
                    buffIcons.Remove(buff);
                }
            }

            //add new icons
            foreach (Buff buff in newBuffs)
            {
                if (!buffIcons.ContainsKey(buff))
                {
                    BuffIcon icon = new BuffIcon(this, buff);
                    Add(icon);
                    buffIcons.Add(buff, icon);
                }
            }

            //layout
            int pos = 0;
            foreach (BuffIcon icon in buffIcons.Values)
            {
                icon.UpdateIcon();
                icon.SetRect(x + pos * (SIZE + 2), y, 9, 12);
                ++pos;
            }
        }

        private class BuffIndicatorAlphaTweener : AlphaTweener
        {
            public BuffIndicatorAlphaTweener(Visual image, float alpha, float time)
                : base(image, alpha, time)
            { }

            public override void UpdateValues(float progress)
            {
                base.UpdateValues(progress);
                image.scale.Set(1 + 5 * progress);
            }

            public override void OnComplete()
            {
                image.KillAndErase();
            }
        }

        private class BuffIcon : Button
        {
            BuffIndicator indicator;
            private Buff buff;

            public Image icon;
            public Image grey;

            public BuffIcon(BuffIndicator indicator, Buff buff)
            {
                this.indicator = indicator;
                this.buff = buff;

                icon = new Image(indicator.texture);
                icon.Frame(indicator.film.Get(buff.Icon()));
                Add(icon);

                grey = new Image(TextureCache.CreateSolid(new Color(0x66, 0x66, 0x66, 0xCC)));
                Add(grey);
            }

            public void UpdateIcon()
            {
                icon.Frame(indicator.film.Get(buff.Icon()));
                buff.TintIcon(icon);
                //round up to the nearest pixel if <50% faded, otherwise round down
                float fadeHeight = buff.IconFadePercent() * icon.Height();
                float zoom = (GetCamera() != null) ? GetCamera().zoom : 1;
                if (fadeHeight < icon.Height() / 2f)
                {
                    grey.scale.Set(icon.Width(), (float)Math.Ceiling(zoom * fadeHeight) / zoom);
                }
                else
                {
                    grey.scale.Set(icon.Width(), (float)Math.Floor(zoom * fadeHeight) / zoom);
                }
            }

            protected override void Layout()
            {
                base.Layout();
                grey.x = icon.x = this.x + 1;
                grey.y = icon.y = this.y + 2;
            }

            protected override void OnClick()
            {
                if (buff.Icon() != NONE)
                    GameScene.Show(new WndInfoBuff(buff));
            }
        }

        public static void RefreshHero()
        {
            if (heroInstance != null)
            {
                heroInstance.needsRefresh = true;
            }
        }
    }
}