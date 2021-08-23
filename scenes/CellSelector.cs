using System;
using System.Linq;
using watabou.input;
using watabou.noosa;
using watabou.utils;
using spdd.actors;
using spdd.items;
using spdd.sprites;
using spdd.tiles;

namespace spdd.scenes
{
    public class CellSelector : ScrollArea
    {
        public IListener listener;

        public bool enabled;

        private float dragThreshold;

        public CellSelector(DungeonTilemap map)
            : base(map)
        {
            camera = map.GetCamera();

            dragThreshold = PixelScene.defaultZoom * DungeonTilemap.SIZE / 2;

            mouseZoom = camera.zoom;

            keyListener = new KeyListener();
            keyListener.selector = this;

            KeyEvent.AddKeyListener(keyListener);
        }

        private float mouseZoom;

        protected override void OnScroll(ScrollEvent ev)
        {
            float diff = ev.amount / 10.0f;

            //scale zoom difference so zooming is consistent
            diff /= ((camera.zoom + 1) / camera.zoom) - 1;
            diff = Math.Min(1, diff);
            mouseZoom = GameMath.Gate(PixelScene.minZoom, mouseZoom - diff, PixelScene.maxZoom);

            Zoom((float)Math.Round(mouseZoom, MidpointRounding.AwayFromZero));
        }

        public override void OnClick(PointerEvent touch)
        {
            if (dragging)
            {
                dragging = false;
            }
            else
            {
                PointF p = Camera.main.ScreenToCamera((int)touch.current.x, (int)touch.current.y);

                //Prioritizes a mob sprite if it and a tile overlap, so long as the mob sprite isn't more than 4 pixels into a tile the mob doesn't occupy.
                //The extra check prevents large mobs from blocking the player from clicking adjacent tiles
                foreach (var mob in Dungeon.level.mobs.ToArray())
                {
                    if (mob.sprite != null && mob.sprite.OverlapsPoint(p.x, p.y))
                    {
                        PointF c = DungeonTilemap.TileCenterToWorld(mob.pos);
                        if (Math.Abs(p.x - c.x) <= 12 && Math.Abs(p.y - c.y) <= 12)
                        {
                            Select(mob.pos);
                            return;
                        }
                    }
                }

                //Does the same but for heaps
                foreach (Heap heap in Dungeon.level.heaps.Values)
                {
                    if (heap.sprite != null && heap.sprite.OverlapsPoint(p.x, p.y))
                    {
                        PointF c = DungeonTilemap.TileCenterToWorld(heap.pos);
                        if (Math.Abs(p.x - c.x) <= 12 && Math.Abs(p.y - c.y) <= 12)
                        {
                            Select(heap.pos);
                            return;
                        }
                    }
                }

                int cell = ((DungeonTilemap)target).ScreenToTile(
                    (int)touch.current.x,
                    (int)touch.current.y,
                    true);
                Select(cell);
            }
        }

        private float Zoom(float value)
        {
            value = GameMath.Gate(PixelScene.minZoom, value, PixelScene.maxZoom);
            SPDSettings.Zoom((int)(value - PixelScene.defaultZoom));

            camera.Zoom(value);

            //Resets character sprite positions with the new camera zoom
            //This is important as characters are centered on a 16x16 tile, but may have any sprite size
            //This can lead to none-whole coordinate, which need to be aligned with the zoom
            foreach (var c in Actor.Chars())
            {
                if (c.sprite != null && !c.sprite.isMoving)
                {
                    c.sprite.Point(c.sprite.WorldToCamera(c.pos));
                }
            }

            return value;
        }

        public void Select(int cell)
        {
            if (enabled && listener != null && cell != -1)
            {
                listener.OnSelect(cell);
                GameScene.Ready();
            }
            else
            {
                GameScene.Cancel();
            }
        }

        private bool pinching;
        private PointerEvent another;
        private float startZoom;
        private float startSpan;

        public override void OnPointerDown(PointerEvent ev)
        {
            if (ev != curEvent && another == null)
            {
                if (!curEvent.down)
                {
                    curEvent = ev;
                    OnPointerDown(ev);
                    return;
                }

                pinching = true;

                another = ev;
                startSpan = PointF.Distance(curEvent.current, another.current);
                startZoom = camera.zoom;

                dragging = false;
            }
            else if (ev != curEvent)
            {
                Reset();
            }
        }

        public override void OnPointerUp(PointerEvent ev)
        {
            if (pinching && (ev == curEvent || ev == another))
            {
                pinching = false;

                Zoom((float)Math.Round(camera.zoom, MidpointRounding.AwayFromZero));

                dragging = true;
                if (ev == curEvent)
                    curEvent = another;

                another = null;
                lastPos.Set(curEvent.current);
            }
        }

        private bool dragging;
        private PointF lastPos = new PointF();

        public override void OnDrag(PointerEvent ev)
        {
            if (pinching)
            {
                var curSpan = PointF.Distance(curEvent.current, another.current);
                float zoom = startZoom * curSpan / startSpan;
                camera.Zoom(GameMath.Gate(
                    PixelScene.minZoom,
                    zoom - (zoom % 0.1f),
                    PixelScene.maxZoom));
            }
            else
            {
                if (!dragging && PointF.Distance(ev.current, ev.start) > dragThreshold)
                {
                    dragging = true;
                    lastPos.Set(ev.current);
                }
                else if (dragging)
                {
                    camera.Shift(PointF.Diff(lastPos, ev.current).InvScale(camera.zoom));
                    lastPos.Set(ev.current);
                }
            }
        }

        private GameAction heldAction = SPDAction.NONE;
        private int heldTurns;

        private KeyListener keyListener;

        class KeyListener : Signal<KeyEvent>.IListener
        {
            internal CellSelector selector;

            public bool OnSignal(KeyEvent ev)
            {
                GameAction action = KeyBindings.GetActionForKey(ev);
                if (!ev.pressed)
                {
                    if (selector.heldAction != SPDAction.NONE && selector.heldAction == action)
                    {
                        selector.ResetKeyHold();
                        return true;
                    }
                    else
                    {
                        if (action == SPDAction.ZOOM_IN)
                        {
                            selector.Zoom(selector.camera.zoom + 1);
                            return true;
                        }
                        else if (action == SPDAction.ZOOM_OUT)
                        {
                            selector.Zoom(selector.camera.zoom - 1);
                            return true;
                        }
                    }
                }
                else if (selector.MoveFromAction(action))
                {
                    selector.heldAction = action;
                    return true;
                }

                return false;
            }
        }

        private bool MoveFromAction(GameAction action)
        {
            int cell = Dungeon.hero.pos;

            if (action == SPDAction.N)
                cell += -Dungeon.level.Width();
            else if (action == SPDAction.NE)
                cell += +1 - Dungeon.level.Width();
            else if (action == SPDAction.E)
                cell += +1;
            else if (action == SPDAction.SE)
                cell += +1 + Dungeon.level.Width();
            else if (action == SPDAction.S)
                cell += +Dungeon.level.Width();
            else if (action == SPDAction.SW)
                cell += -1 + Dungeon.level.Width();
            else if (action == SPDAction.W)
                cell += -1;
            else if (action == SPDAction.NW)
                cell += -1 - Dungeon.level.Width();

            if (cell != Dungeon.hero.pos)
            {
                //each step when keyboard moving takes 0.15s, 0.125s, 0.1s, 0.1s, ...
                // this is to make it easier to move 1 or 2 steps without overshooting
                CharSprite.SetMoveInterval(CharSprite.DEFAULT_MOVE_INTERVAL +
                                            Math.Max(0, 0.05f - heldTurns * 0.025f));
                Select(cell);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ProcessKeyHold()
        {
            if (heldAction != SPDAction.NONE)
            {
                enabled = true;
                ++heldTurns;
                MoveFromAction(heldAction);
            }
        }

        public void ResetKeyHold()
        {
            heldAction = SPDAction.NONE;
            heldTurns = 0;
            CharSprite.SetMoveInterval(CharSprite.DEFAULT_MOVE_INTERVAL);
        }

        public void Cancel()
        {
            if (listener != null)
                listener.OnSelect(null);

            GameScene.Ready();
        }

        public override void Reset()
        {
            base.Reset();
            another = null;
            if (pinching)
            {
                pinching = false;

                Zoom((float)Math.Round(camera.zoom, MidpointRounding.AwayFromZero));
            }
        }

        public void Enable(bool value)
        {
            if (enabled != value)
            {
                enabled = value;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            KeyEvent.RemoveKeyListener(keyListener);
        }

        public interface IListener
        {
            void OnSelect(int? target);

            string Prompt();
        }
    }
}