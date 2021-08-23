using System;
using watabou.utils;

namespace watabou.noosa
{
    public class MovieClip : Image
    {
        protected Animation curAnim;
        protected int curFrame;
        protected float frameTimer;
        protected bool finished;

        public bool paused;

        public IListener listener;

        public MovieClip()
        { }

        public MovieClip(object tx)
            : base(tx)
        { }

        public override void Update()
        {
            base.Update();
            if (!paused)
            {
                UpdateAnimation();
            }
        }

        public bool Looping()
        {
            return curAnim != null && curAnim.looped;
        }

        protected void UpdateAnimation()
        {
            if (curAnim != null && curAnim.delay > 0 && (curAnim.looped || !finished))
            {
                int lastFrame = curFrame;

                frameTimer += Game.elapsed;
                while (frameTimer > curAnim.delay)
                {
                    frameTimer -= curAnim.delay;
                    if (curFrame == curAnim.frames.Length - 1)
                    {
                        if (curAnim.looped)
                            curFrame = 0;

                        finished = true;
                        if (listener != null)
                        {
                            listener.OnComplete(curAnim);
                            // This check can probably be removed
                            if (curAnim == null)
                                return;
                        }

                        // [FIXED]
                        if (curAnim.looped == false)
                            return;
                    }
                    else
                    {
                        ++curFrame;
                    }
                }

                if (curFrame != lastFrame)
                    Frame(curAnim.frames[curFrame]);
            }
        }

        public virtual void Play(Animation anim)
        {
            Play(anim, false);
        }

        public virtual void Play(Animation anim, bool force)
        {
            if (!force && (curAnim != null) && (curAnim == anim) && (curAnim.looped || !finished))
            {
                return;
            }

            curAnim = anim;
            curFrame = 0;
            finished = false;

            frameTimer = 0;

            if (anim != null)
                Frame(anim.frames[curFrame]);
        }

        public class Animation
        {
            public float delay;
            public RectF[] frames;
            public bool looped;

            public Animation(int fps, bool looped)
            {
                this.delay = 1.0f / fps;
                this.looped = looped;
            }

            public Animation Frames(params RectF[] frames)
            {
                this.frames = frames;
                return this;
            }

            // idle.frames( frames, 14, 14, 14, 14, 14, 14, 14, 14, 15, 16, 15, 16, 15, 16 );
            public Animation Frames(TextureFilm film, params object[] frames)
            {
                this.frames = new RectF[frames.Length];
                for (int i = 0; i < frames.Length; ++i)
                {
                    this.frames[i] = film.Get(frames[i]);
                }
                return this;
            }

            public Animation Clone()
            {
                return new Animation((int)Math.Round(1 / delay, MidpointRounding.AwayFromZero), looped).Frames(frames);
            }
        } // animation

        public interface IListener
        {
            void OnComplete(MovieClip.Animation anim);
        }
    }
}