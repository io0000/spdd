using System;
using watabou.input;
using watabou.noosa;
using watabou.noosa.ui;
using watabou.utils;
using spdd.scenes;

namespace spdd.ui
{
    public abstract class OptionSlider : Component
    {
        private PointerArea pointerArea;

        private RenderedTextBlock title;
        private RenderedTextBlock minTxt;
        private RenderedTextBlock maxTxt;

        //values are expressed internally as ints, but they can easily be interpreted as something else externally.
        private int minVal;
        private int maxVal;
        private int selectedVal;

        private NinePatch sliderNode;
        private NinePatch BG;
        private ColorBlock sliderBG;
        private ColorBlock[] sliderTicks;
        private float tickDist;

        public OptionSlider(string title, string minTxt, string maxTxt, int minVal, int maxVal)
        {
            //shouldn't function if this happens.
            if (minVal > maxVal)
            {
                minVal = maxVal;
                active = false;
            }

            this.title.Text(title);
            this.minTxt.Text(minTxt);
            this.maxTxt.Text(maxTxt);

            this.minVal = minVal;
            this.maxVal = maxVal;

            sliderTicks = new ColorBlock[(maxVal - minVal) + 1];
            for (int i = 0; i < sliderTicks.Length; ++i)
            {
                Add(sliderTicks[i] = new ColorBlock(1, 11, new Color(0x22, 0x22, 0x22, 0xFF)));
            }
            Add(sliderNode);
        }

        protected abstract void OnChange();

        public int GetSelectedValue()
        {
            return selectedVal;
        }

        public void SetSelectedValue(int val)
        {
            this.selectedVal = val;
            sliderNode.x = (int)(x + tickDist * (selectedVal - minVal));
            sliderNode.y = sliderBG.y - 4;
        }

        protected override void CreateChildren()
        {
            base.CreateChildren();

            Add(BG = Chrome.Get(Chrome.Type.RED_BUTTON));
            BG.Alpha(0.5f);

            Add(title = PixelScene.RenderTextBlock(9));
            Add(this.minTxt = PixelScene.RenderTextBlock(6));
            Add(this.maxTxt = PixelScene.RenderTextBlock(6));

            Add(sliderBG = new ColorBlock(1, 1, new Color(0x22, 0x22, 0x22, 0xFF)));
            sliderNode = Chrome.Get(Chrome.Type.RED_BUTTON);
            sliderNode.Size(5, 9);

            pointerArea = new OptionSliderPointerArea(this);
            Add(pointerArea);
        }

        private class OptionSliderPointerArea : PointerArea
        {
            OptionSlider silder;
            internal bool pressed;

            public OptionSliderPointerArea(OptionSlider silder)
                : base(0, 0, 0, 0)
            {
                this.silder = silder;
            }

            public override void OnPointerDown(PointerEvent ev)
            {
                pressed = true;
                PointF p = GetCamera().ScreenToCamera((int)ev.current.x, (int)ev.current.y);
                silder.sliderNode.x = GameMath.Gate(silder.sliderBG.x - 2,
                    p.x - silder.sliderNode.Width() / 2,
                    silder.sliderBG.x + silder.sliderBG.Width() - 2);
                silder.sliderNode.Brightness(1.5f);
            }

            public override void OnPointerUp(PointerEvent ev)
            {
                if (pressed)
                {
                    PointF p = GetCamera().ScreenToCamera((int)ev.current.x, (int)ev.current.y);
                    silder.sliderNode.x = GameMath.Gate(silder.sliderBG.x - 2,
                        p.x - silder.sliderNode.Width() / 2,
                        silder.sliderBG.x + silder.sliderBG.Width() - 2);
                    silder.sliderNode.ResetColor();

                    //sets the selected value
                    silder.selectedVal = silder.minVal + (int)Math.Round((silder.sliderNode.x - x) / silder.tickDist, MidpointRounding.AwayFromZero);
                    silder.sliderNode.x = x + silder.tickDist * (silder.selectedVal - silder.minVal);
                    PixelScene.Align(silder.sliderNode);
                    silder.OnChange();
                    pressed = false;
                }
            }

            public override void OnDrag(PointerEvent ev)
            {
                if (pressed)
                {
                    PointF p = GetCamera().ScreenToCamera((int)ev.current.x, (int)ev.current.y);
                    silder.sliderNode.x = GameMath.Gate(silder.sliderBG.x - 2,
                        p.x - silder.sliderNode.Width() / 2,
                        silder.sliderBG.x + silder.sliderBG.Width() - 2);
                }
            }
        }

        protected override void Layout()
        {
            title.SetPos(
                x + (width - title.Width()) / 2,
                y + 2);
            PixelScene.Align(title);
            sliderBG.y = y + Height() - 8;
            sliderBG.x = x + 2;
            sliderBG.Size(width - 5, 1);
            tickDist = sliderBG.Width() / (maxVal - minVal);
            for (int i = 0; i < sliderTicks.Length; ++i)
            {
                sliderTicks[i].y = sliderBG.y - 5;
                sliderTicks[i].x = x + 2 + (tickDist * i);
                PixelScene.Align(sliderTicks[i]);
            }

            minTxt.SetPos(
                x + 1,
                sliderBG.y - 6 - minTxt.Height());
            maxTxt.SetPos(
                x + Width() - maxTxt.Width() - 1,
                sliderBG.y - 6 - minTxt.Height());

            sliderNode.x = x + tickDist * (selectedVal - minVal);
            sliderNode.y = sliderBG.y - 4;
            PixelScene.Align(sliderNode);

            pointerArea.x = x;
            pointerArea.y = y;
            pointerArea.width = Width();
            pointerArea.height = Height();

            BG.Size(Width(), Height());
            BG.x = x;
            BG.y = y;
        }
    }
}