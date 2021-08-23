using watabou.gltextures;
using watabou.glwrap;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.particles;
using watabou.utils;
using spdd.effects;
using spdd.items;
using spdd.levels;
using spdd.scenes;
using spdd.tiles;


namespace spdd.sprites
{
    public class ItemSprite : MovieClip
    {
        public const int SIZE = 16;     // texture»çÀÌÁî

        private const float DROP_INTERVAL = 0.4f;

        public Heap heap;

        private Glowing glowing;
        //FIXME: a lot of this emitter functionality isn't very well implemented.
        //right now I want to ship 0.3.0, but should refactor in the future.
        protected Emitter emitter;
        private float phase;
        private bool glowUp;

        private float dropInterval;

        //the amount the sprite is raised from flat when viewed in a raised perspective
        protected float perspectiveRaise = 5 / 16f; //5 pixels

        //the width and height of the shadow are a percentage of sprite size
        //offset is the number of pixels the shadow is moved down or up (handy for some animations)
        protected bool renderShadow;
        protected float shadowWidth = 1.0f;
        protected float shadowHeight = 0.25f;
        protected float shadowOffset = 0.5f;

        public ItemSprite()
            : this(ItemSpriteSheet.SOMETHING, null)
        { }

        public ItemSprite(Heap heap)
            : base(Assets.Sprites.ITEMS)
        {
            View(heap);
        }

        public ItemSprite(Item item)
            : base(Assets.Sprites.ITEMS)
        {
            View(item);
        }

        public ItemSprite(int image)
            : this(image, null)
        { }

        public ItemSprite(int image, Glowing glowing)
            : base(Assets.Sprites.ITEMS)
        {
            View(image, glowing);
        }

        public void OriginToCenter()
        {
            origin.Set(width / 2, height / 2);
        }

        public void Link()
        {
            Link(heap);
        }

        public void Link(Heap heap)
        {
            this.heap = heap;
            View(heap);
            renderShadow = true;
            Place(heap.pos);
        }

        public override void Revive()
        {
            base.Revive();

            speed.Set(0);
            acc.Set(0);
            dropInterval = 0;

            heap = null;
            if (emitter != null)
            {
                emitter.KillAndErase();
                emitter = null;
            }
        }

        // public void visible(boolean value)
        public void SetVisible(bool value)
        {
            this.visible = value;
            if (emitter != null && !visible)
            {
                emitter.KillAndErase();
                emitter = null;
            }
        }

        public PointF WorldToCamera(int cell)
        {
            const int csize = DungeonTilemap.SIZE;

            return new PointF(
                cell % Dungeon.level.Width() * csize + (csize - Width()) * 0.5f,
                cell / Dungeon.level.Width() * csize + (csize - Height()) - csize * perspectiveRaise);
        }

        public void Place(int p)
        {
            if (Dungeon.level != null)
            {
                Point(WorldToCamera(p));
                shadowOffset = 0.5f;
            }
        }

        public virtual void Drop()
        {
            if (heap.IsEmpty())
                return;

            if (heap.Size() == 1)
            {
                // normally this would happen for any heap, however this is not applied to heaps greater than 1 in size
                // in order to preserve an amusing visual bug/feature that used to trigger for heaps with size > 1
                // where as long as the player continually taps, the heap sails up into the air.
                Place(heap.pos);
            }

            dropInterval = DROP_INTERVAL;

            speed.Set(0, -100);
            acc.Set(0, -speed.y / DROP_INTERVAL * 2);

            if (heap != null && heap.seen && heap.Peek() is Gold)
            {
                CellEmitter.Center(heap.pos).Burst(Speck.Factory(Speck.COIN), 5);
                Sample.Instance.Play(Assets.Sounds.GOLD, 1, 1, Rnd.Float(0.9f, 1.1f));
            }
        }

        public void Drop(int from)
        {
            if (heap.pos == from)
            {
                Drop();
            }
            else
            {
                float px = x;
                float py = y;
                Drop();

                Place(from);

                speed.Offset((px - x) / DROP_INTERVAL, (py - y) / DROP_INTERVAL);
            }
        }

        public ItemSprite View(Item item)
        {
            View(item.Image(), item.Glowing());
            Emitter emitter = item.Emitter();
            if (emitter != null && parent != null)
            {
                emitter.Pos(this);
                parent.Add(emitter);
                this.emitter = emitter;
            }
            return this;
        }

        public ItemSprite View(Heap heap)
        {
            if (heap.Size() <= 0 || heap.items == null)
            {
                return View(0, null);
            }

            switch (heap.type)
            {
                case Heap.Type.HEAP:
                case Heap.Type.FOR_SALE:
                    return View(heap.Peek());
                case Heap.Type.CHEST:
                    //case Heap.Type.MIMIC:
                    return View(ItemSpriteSheet.CHEST, null);
                case Heap.Type.LOCKED_CHEST:
                    return View(ItemSpriteSheet.LOCKED_CHEST, null);
                case Heap.Type.CRYSTAL_CHEST:
                    return View(ItemSpriteSheet.CRYSTAL_CHEST, null);
                case Heap.Type.TOMB:
                    return View(ItemSpriteSheet.TOMB, null);
                case Heap.Type.SKELETON:
                    return View(ItemSpriteSheet.BONES, null);
                case Heap.Type.REMAINS:
                    return View(ItemSpriteSheet.REMAINS, null);
                default:
                    return View(0, null);
            }
        }

        public ItemSprite View(int image, Glowing glowing)
        {
            if (emitter != null)
                emitter.KillAndErase();

            emitter = null;
            Frame(image);
            Glow(glowing);
            return this;
        }

        public void Frame(int image)
        {
            Frame(ItemSpriteSheet.film.Get(image));

            float height = ItemSpriteSheet.film.Height(image);
            //adds extra raise to very short items, so they are visible
            if (height < 8.0f)
            {
                perspectiveRaise = (5 + 8 - height) / 16.0f;
            }
        }

        public void Glow(Glowing glowing)
        {
            this.glowing = glowing;
            if (glowing == null)
                ResetColor();
        }

        public override void Kill()
        {
            base.Kill();
            if (emitter != null)
                emitter.KillAndErase();
            emitter = null;
        }

        private float[] shadowMatrix = new float[16];

        protected override void UpdateMatrix()
        {
            base.UpdateMatrix();
            Matrix.Copy(matrix, shadowMatrix);
            Matrix.Translate(shadowMatrix,
                (Width() * (1.0f - shadowWidth)) / 2.0f,
                (Height() * (1.0f - shadowHeight)) + shadowOffset);

            Matrix.Scale(shadowMatrix, shadowWidth, shadowHeight);
        }

        public override void Draw()
        {
            if (texture == null) // || (!dirty && buffer == null))
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

        public override void Update()
        {
            base.Update();

            visible = (heap == null || heap.seen);

            if (emitter != null)
                emitter.visible = visible;

            if (dropInterval > 0.0f)
            {
                shadowOffset -= speed.y * Game.elapsed * 0.8f;

                if ((dropInterval -= Game.elapsed) <= 0.0f)
                {
                    speed.Set(0.0f);
                    acc.Set(0.0f);
                    shadowOffset = 0.25f;
                    Place(heap.pos);

                    if (visible)
                    {
                        var w = Dungeon.level.water[heap.pos];
                        var m = Dungeon.level.map[heap.pos];

                        if (w)
                            GameScene.Ripple(heap.pos);

                        if (w)
                        {
                            Sample.Instance.Play(Assets.Sounds.WATER, 0.8f, Rnd.Float(1f, 1.45f));
                        }
                        else if (m == Terrain.EMPTY_SP)
                        {
                            Sample.Instance.Play(Assets.Sounds.STURDY, 0.8f, Rnd.Float(1.16f, 1.25f));
                        }
                        else if (m == Terrain.GRASS ||
                            m == Terrain.EMBERS ||
                            m == Terrain.FURROWED_GRASS)
                        {
                            Sample.Instance.Play(Assets.Sounds.GRASS, 0.8f, Rnd.Float(1.16f, 1.25f));
                        }
                        else if (m == Terrain.HIGH_GRASS)
                        {
                            Sample.Instance.Play(Assets.Sounds.STEP, 0.8f, Rnd.Float(1.16f, 1.25f));
                        }
                        else
                        {
                            Sample.Instance.Play(Assets.Sounds.STEP, 0.8f, Rnd.Float(1.16f, 1.25f));
                        }
                    }
                }
            }

            if (visible && glowing != null)
            {
                if (glowUp && (phase += Game.elapsed) > glowing.period)
                {
                    glowUp = false;
                    phase = glowing.period;
                }
                else if (!glowUp && (phase -= Game.elapsed) < 0)
                {
                    glowUp = true;
                    phase = 0;
                }

                float value = phase / glowing.period * 0.6f;

                rm = gm = bm = 1.0f - value;
                ra = glowing.red * value;
                ga = glowing.green * value;
                ba = glowing.blue * value;
            }
        }

        public static Color Pick(int index, int x, int y)
        {
            Texture tx = TextureCache.Get(Assets.Sprites.ITEMS);

            var rows = tx.width / SIZE;
            var row = index / rows;
            var col = index % rows;

            return tx.GetPixel(col * SIZE + x, row * SIZE + y);
        }

        public class Glowing
        {
            public Color color;
            public float red;
            public float green;
            public float blue;
            public float period;

            public Glowing(Color color)
                : this(color, 1.0f)
            { }

            public Glowing(Color color, float period)
            {
                this.color = color;

                red = color.R / 255f;
                green = color.G / 255f;
                blue = color.B / 255f;

                this.period = period;
            }
        }
    }
}