using System;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.particles;
using watabou.noosa.tweeners;
using watabou.utils;
using spdd.actors;
using spdd.effects;
using spdd.effects.particles;
using spdd.messages;
using spdd.scenes;
using spdd.tiles;
using spdd.ui;

using Matrix = watabou.glwrap.Matrix;

namespace spdd.sprites
{
    public class CharSprite : MovieClip, Tweener.IListener, MovieClip.IListener
    {
        // Color constants for floating text
        public static Color DEFAULT = new Color(0xFF, 0xFF, 0xFF, 0xFF);
        public static Color POSITIVE = new Color(0x00, 0xFF, 0x00, 0xFF);
        public static Color NEGATIVE = new Color(0xFF, 0x00, 0x00, 0xFF);
        public static Color WARNING = new Color(0xFF, 0x88, 0x00, 0xFF);
        public static Color NEUTRAL = new Color(0xFF, 0xFF, 0x00, 0xFF);

        public const float DEFAULT_MOVE_INTERVAL = 0.1f;
        private static float moveInterval = DEFAULT_MOVE_INTERVAL;
        private const float FLASH_INTERVAL = 0.05f;

        //the amount the sprite is raised from flat when viewed in a raised perspective
        protected float perspectiveRaise = 6.0f / 16.0f; //6 pixels

        //the width and height of the shadow are a percentage of sprite size
        //offset is the number of pixels the shadow is moved down or up (handy for some animations)
        protected bool renderShadow;
        protected float shadowWidth = 1.2f;
        protected float shadowHeight = 0.25f;
        protected float shadowOffset = 0.25f;

        public enum State
        {
            BURNING,
            LEVITATING,
            INVISIBLE,
            PARALYSED,
            FROZEN,
            ILLUMINATED,
            CHILLED,
            DARKENED,
            MARKED,
            HEALING,
            SHIELDED
        }

        protected Animation idle;
        protected Animation run;
        protected Animation attack;
        protected Animation operate;
        protected Animation zap;
        protected Animation die;

        protected ICallback animCallback;

        protected PosTweener motion;

        protected Emitter burning;
        protected Emitter chilled;
        protected Emitter marked;
        protected Emitter levitation;
        protected Emitter healing;

        protected IceBlock iceBlock;
        protected DarkBlock darkBlock;
        protected TorchHalo light;
        protected ShieldHalo shield;
        protected AlphaTweener invisible;

        protected EmoIcon emo;
        protected CharHealthIndicator health;

        private Tweener jumpTweener;
        private ICallback jumpCallback;

        protected float flashTime;

        protected bool sleeping;

        public Character ch;

        //used to prevent the actor associated with this sprite from acting until movement completes
        public bool isMoving;

        public CharSprite()
        {
            listener = this;
        }

        public override void Play(Animation anim)
        {
            //Shouldn't interrupt the dieing animation
            if (curAnim == null || curAnim != die)
            {
                base.Play(anim);
            }
        }

        //intended to be used for placing a character in the game world
        public virtual void Link(Character ch)
        {
            LinkVisuals(ch);

            this.ch = ch;
            ch.sprite = this;

            Place(ch.pos);
            TurnTo(ch.pos, Rnd.Int(Dungeon.level.Length()));
            renderShadow = true;

            if (ch != Dungeon.hero)
            {
                if (health == null)
                    health = new CharHealthIndicator(ch);
                else
                    health.Target(ch);
            }

            ch.UpdateSpriteState();
        }

        //used for just updating a sprite based on a given character, not linking them or placing in the game
        public virtual void LinkVisuals(Character ch)
        {
            //do nothin by default
        }

        public PointF WorldToCamera(int cell)
        {
            const int csize = DungeonTilemap.SIZE;

            float x = ((cell % Dungeon.level.Width()) + 0.5f) * csize - Width() * 0.5f;
            float y = ((cell / Dungeon.level.Width()) + 1.0f) * csize - Height() - csize * perspectiveRaise;

            x = PixelScene.Align(Camera.main, x);
            y = PixelScene.Align(Camera.main, y);

            return new PointF(x, y);
        }

        public virtual void Place(int cell)
        {
            Point(WorldToCamera(cell));
        }

        public void ShowStatus(Color color, string text, params object[] args)
        {
            if (visible)
            {
                if (args.Length > 0)
                    text = Messages.Format(text, args);

                var pos = DestinationCenter();
                float x = pos.x;
                float y = pos.y - Height() / 2.0f;

                if (ch != null)
                    FloatingText.Show(x, y, ch.pos, text, color);
                else
                    FloatingText.Show(x, y, text, color);
            }
        }

        public virtual void Idle()
        {
            Play(idle);
        }

        public virtual void Move(int from, int to)
        {
            TurnTo(from, to);

            Play(run);

            motion = new PosTweener(this, WorldToCamera(to), moveInterval);
            motion.listener = this;
            parent.Add(motion);

            isMoving = true;

            if (visible && Dungeon.level.water[from] && !ch.flying)
                GameScene.Ripple(from);
        }

        public static void SetMoveInterval(float interval)
        {
            moveInterval = interval;
        }

        //returns where the center of this sprite will be after it completes any motion in progress
        public PointF DestinationCenter()
        {
            PosTweener motion = this.motion;
            if (motion != null)
            {
                return new PointF(motion.end.x + Width() / 2f, motion.end.y + Height() / 2f);
            }
            else
            {
                return Center();
            }
        }

        public void InterruptMotion()
        {
            if (motion != null)
                motion.Stop(false);
        }

        public virtual void Attack(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(attack);
        }

        public void Attack(int cell, ICallback callback)
        {
            animCallback = callback;
            TurnTo(ch.pos, cell);
            Play(attack);
        }

        public void Operate(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(operate);
        }

        public void Operate(int cell, ICallback callback)
        {
            animCallback = callback;
            TurnTo(ch.pos, cell);
            Play(operate);
        }

        public virtual void Zap(int cell)
        {
            TurnTo(ch.pos, cell);
            Play(zap);
        }

        public void Zap(int cell, ICallback callback)
        {
            animCallback = callback;
            Zap(cell);
        }

        public virtual void TurnTo(int from, int to)
        {
            var fx = from % Dungeon.level.Width();
            var tx = to % Dungeon.level.Width();
            if (tx > fx)
                flipHorizontal = false;
            else if (tx < fx)
                flipHorizontal = true;
        }

        public virtual void Jump(int from, int to, ICallback callback)
        {
            float distance = Dungeon.level.TrueDistance(from, to);
            Jump(from, to, callback, distance * 2, distance * 0.1f);
        }

        public void Jump(int from, int to, ICallback callback, float height, float duration)
        {
            jumpCallback = callback;

            jumpTweener = new JumpTweener(this, WorldToCamera(to), height, duration);
            jumpTweener.listener = this;
            parent.Add(jumpTweener);

            TurnTo(from, to);
        }

        public virtual void Die()
        {
            sleeping = false;
            Play(die);

            HideEmo();

            if (health != null)
                health.KillAndErase();
        }

        public Emitter Emitter()
        {
            var emitter = GameScene.GetEmitter();
            emitter.Pos(this);
            return emitter;
        }

        public Emitter CenterEmitter()
        {
            var emitter = GameScene.GetEmitter();
            emitter.Pos(Center());
            return emitter;
        }

        //public Emitter BottomEmitter()
        //{
        //    var emitter = GameScene.GetEmitter();
        //    emitter.Pos(x, y + height, width, 0);
        //    return emitter;
        //}

        public void Burst(Color color, int n)
        {
            if (visible)
                Splash.At(Center(), color, n);
        }

        public virtual void BloodBurstA(PointF from, int damage)
        {
            if (!visible)
                return;

            var c = Center();
            var n = (int)Math.Min(9 * Math.Sqrt((double)damage / ch.HT), 9);
            Splash.At(c, PointF.Angle(from, c), 3.1415926f / 2, Blood(), n);
        }

        public virtual Color Blood()
        {
            return new Color(0xBB, 0x00, 0x00, 0xFF);
        }

        public void Flash()
        {
            ra = ba = ga = 1.0f;
            flashTime = FLASH_INTERVAL;
        }

        public void Add(State state)
        {
            switch (state)
            {
                case State.BURNING:
                    burning = Emitter();
                    burning.Pour(FlameParticle.Factory, 0.06f);
                    if (visible)
                        Sample.Instance.Play(Assets.Sounds.BURNING);
                    break;

                case State.LEVITATING:
                    levitation = Emitter();
                    levitation.Pour(Speck.Factory(Speck.JET), 0.02f);
                    break;

                case State.INVISIBLE:
                    if (invisible != null)
                        invisible.KillAndErase();

                    invisible = new AlphaTweener(this, 0.4f, 0.4f);
                    if (parent != null)
                        parent.Add(invisible);
                    else
                        Alpha(0.4f);
                    break;

                case State.PARALYSED:
                    paused = true;
                    break;

                case State.FROZEN:
                    iceBlock = IceBlock.Freeze(this);
                    paused = true;
                    break;

                case State.ILLUMINATED:
                    GameScene.Effect(light = new TorchHalo(this));
                    break;

                case State.CHILLED:
                    chilled = Emitter();
                    chilled.Pour(SnowParticle.Factory, 0.1f);
                    break;

                case State.DARKENED:
                    darkBlock = DarkBlock.Darken(this);
                    break;

                case State.MARKED:
                    marked = Emitter();
                    marked.Pour(ShadowParticle.Up, 0.1f);
                    break;

                case State.HEALING:
                    healing = Emitter();
                    healing.Pour(Speck.Factory(Speck.HEALING), 0.5f);
                    break;

                case State.SHIELDED:
                    shield = new ShieldHalo(this);
                    GameScene.Effect(shield);
                    break;
            }
        }

        public void Remove(State state)
        {
            switch (state)
            {
                case State.BURNING:
                    if (burning != null)
                    {
                        burning.on = false;
                        burning = null;
                    }
                    break;

                case State.LEVITATING:
                    if (levitation != null)
                    {
                        levitation.on = false;
                        levitation = null;
                    }
                    break;

                case State.INVISIBLE:
                    if (invisible != null)
                    {
                        invisible.KillAndErase();
                        invisible = null;
                    }
                    Alpha(1.0f);
                    break;

                case State.PARALYSED:
                    paused = false;
                    break;

                case State.FROZEN:
                    if (iceBlock != null)
                    {
                        iceBlock.Melt();
                        iceBlock = null;
                    }
                    paused = false;
                    break;

                case State.ILLUMINATED:
                    if (light != null)
                        light.PutOut();
                    break;

                case State.CHILLED:
                    if (chilled != null)
                    {
                        chilled.on = false;
                        chilled = null;
                    }
                    break;

                case State.DARKENED:
                    if (darkBlock != null)
                    {
                        darkBlock.Lighten();
                        darkBlock = null;
                    }
                    break;

                case State.MARKED:
                    if (marked != null)
                    {
                        marked.on = false;
                        marked = null;
                    }
                    break;

                case State.HEALING:
                    if (healing != null)
                    {
                        healing.on = false;
                        healing = null;
                    }
                    break;

                case State.SHIELDED:
                    if (shield != null)
                        shield.PutOut();
                    break;
            }
        }

        public override void Update()
        {
            base.Update();

            if (paused && listener != null)
                listener.OnComplete(curAnim);

            if (flashTime > 0.0f && (flashTime -= Game.elapsed) <= 0.0f)
                ResetColor();

            if (burning != null)
                burning.visible = visible;

            if (levitation != null)
                levitation.visible = visible;

            if (iceBlock != null)
                iceBlock.visible = visible;

            if (chilled != null)
                chilled.visible = visible;

            if (marked != null)
                marked.visible = visible;

            if (sleeping)
                ShowSleep();
            else
                HideSleep();

            if (emo != null && emo.alive)
                emo.visible = visible;
        }

        public override void ResetColor()
        {
            base.ResetColor();

            if (invisible != null)
                Alpha(0.4f);
        }

        public virtual void ShowSleep()
        {
            if (!(emo is EmoIcon.Sleep))
            {
                if (emo != null)
                    emo.KillAndErase();

                emo = new EmoIcon.Sleep(this);
                emo.visible = visible;
            }

            Idle();
        }

        public void HideSleep()
        {
            if (emo is EmoIcon.Sleep)
            {
                emo.KillAndErase();
                emo = null;
            }
        }

        public void ShowAlert()
        {
            if (!(emo is EmoIcon.Alert))
            {
                if (emo != null)
                    emo.KillAndErase();

                emo = new EmoIcon.Alert(this);
                emo.visible = visible;
            }
        }

        public void HideAlert()
        {
            if (emo is EmoIcon.Alert)
            {
                emo.KillAndErase();
                emo = null;
            }
        }

        public void ShowLost()
        {
            if (!(emo is EmoIcon.Lost))
            {
                if (emo != null)
                    emo.KillAndErase();

                emo = new EmoIcon.Lost(this);
                emo.visible = visible;
            }
        }

        public void HideLost()
        {
            if (emo is EmoIcon.Lost)
            {
                emo.KillAndErase();
                emo = null;
            }
        }

        public void HideEmo()
        {
            if (emo != null)
            {
                emo.KillAndErase();
                emo = null;
            }
        }

        public override void Kill()
        {
            base.Kill();

            HideEmo();

            foreach (var s in Enum.GetValues(typeof(State)))
                Remove((State)s);

            if (health != null)
                health.KillAndErase();
        }

        private float[] shadowMatrix = new float[16];

        protected override void UpdateMatrix()
        {
            base.UpdateMatrix();
            Matrix.Copy(matrix, shadowMatrix);
            Matrix.Translate(shadowMatrix,
                (width * (1.0f - shadowWidth)) / 2.0f,
                (height * (1.0f - shadowHeight)) + shadowOffset);

            Matrix.Scale(shadowMatrix, shadowWidth, shadowHeight);
        }

        public override void Draw()
        {
            if (texture == null) // || (!_dirty && buffer == null))
                return;

            if (renderShadow)
            {
                //if (dirty)
                //{
                //    verticesBuffer.position(0);
                //    verticesBuffer.put(vertices);
                //    if (buffer == null)
                //        buffer = new Vertexbuffer(verticesBuffer);
                //    else
                //        buffer.updateVertices(verticesBuffer);
                //    dirty = false;
                //}

                NoosaScript script = Script();

                texture.Bind();

                script.Camera(GetCamera());

                UpdateMatrix();

                script.uModel.ValueM4(shadowMatrix);
                script.Lighting(
                    0, 0, 0, am * 0.6f,
                    0, 0, 0, aa * 0.6f);

                script.DrawQuad(vertices);
            }

            base.Draw();
        }

        // Tweener.IListener
        public void OnComplete(Tweener tweener)
        {
            if (tweener == jumpTweener)
            {
                if (visible && Dungeon.level.water[ch.pos] && !ch.flying)
                    GameScene.Ripple(ch.pos);

                if (jumpCallback != null)
                    jumpCallback.Call();
            }
            else if (tweener == motion)
            {
                //synchronized(this)
                {
                    isMoving = false;

                    motion.KillAndErase();
                    motion = null;
                    ch.OnMotionComplete();

                    //notifyAll(); thread nofify all 
                    Actor.InputMoveDone(ch);
                }
            }
        }

        // interface MovieClip.Listener
        public virtual void OnComplete(Animation anim)
        {
            if (animCallback != null)
            {
                ICallback executing = animCallback;
                animCallback = null;
                executing.Call();
            }
            else
            {
                if (anim == attack)
                {
                    Idle();
                    ch.OnAttackComplete();
                }
                else if (anim == operate)
                {
                    Idle();
                    ch.OnOperateComplete();
                }
            }
        }

        class JumpTweener : Tweener
        {
            public CharSprite visual;

            public PointF start;
            public PointF end;

            public float height;

            public JumpTweener(CharSprite visual, PointF pos, float height, float time)
                : base(visual, time)
            {
                this.visual = visual;
                start = visual.Point();
                end = pos;

                this.height = height;
            }

            public override void UpdateValues(float progress)
            {
                float hVal = -height * 4.0f * progress * (1.0f - progress);
                visual.Point(PointF.Inter(start, end, progress).Offset(0.0f, hVal));
                visual.shadowOffset = 0.25f - hVal * 0.8f;
            }
        }
    }
}