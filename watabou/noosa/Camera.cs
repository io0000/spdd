using System;
using System.Collections.Generic;
using System.Linq;
using watabou.utils;

namespace watabou.noosa
{
    public class Camera : Gizmo
    {
        private static List<Camera> all = new List<Camera>();

        protected static float invW2;
        protected static float invH2;

        public static Camera main;

        public bool fullScreen;

        public float zoom;

        public int x;
        public int y;
        public int width;
        public int height;

        public int screenWidth;
        public int screenHeight;

        public float[] matrix;

        public PointF scroll;

        private float shakeMagX = 10.0f;
        private float shakeMagY = 10.0f;
        private float shakeTime;
        private float shakeDuration = 1.0f;

        protected float shakeX;
        protected float shakeY;

        public static Camera Reset()
        {
            return Reset(CreateFullscreen(1.0f));
        }

        public static Camera Reset(Camera newCamera)
        {
            invW2 = 2.0f / Game.width;
            invH2 = 2.0f / Game.height;

            foreach (var camera in all)
                camera.Destroy();

            all.Clear();

            return main = Add(newCamera);
        }

        public static Camera Add(Camera camera)
        {
            all.Add(camera);
            return camera;
        }

        public static Camera Remove(Camera camera)
        {
            all.Remove(camera);
            return camera;
        }

        public static void UpdateAll()
        {
            //int length = all.size();
            //for (int i = 0; i < length; ++i)
            //{
            //    Camera c = all.get(i);
            //    if (c.exists && c.active)
            //    {
            //        c.update();
            //    }
            //}
            foreach (var c in all.Where(c => c.exists && c.active))
                c.Update();
        }

        public static Camera CreateFullscreen(float zoom)
        {
            var w = (int)Math.Ceiling(Game.width / zoom);
            var h = (int)Math.Ceiling(Game.height / zoom);

            var c = new Camera(
                (int)(Game.width - w * zoom) / 2,
                (int)(Game.height - h * zoom) / 2,
                w, h, zoom);

            c.fullScreen = true;
            return c;
        }

        public Camera(int x, int y, int width, int height, float zoom)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.zoom = zoom;

            screenWidth = (int)(width * zoom);
            screenHeight = (int)(height * zoom);

            scroll = new PointF();

            matrix = new float[16];
            glwrap.Matrix.SetIdentity(matrix);
        }

        public override void Destroy()
        {
            panIntensity = 0.0f;
        }

        public void Zoom(float value)
        {
            Zoom(value,
                scroll.x + width / 2,
                scroll.y + height / 2);
        }

        public void Zoom(float value, float fx, float fy)
        {
            zoom = value;
            width = (int)(screenWidth / zoom);
            height = (int)(screenHeight / zoom);

            SnapTo(fx, fy);
        }

        public void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;
            screenWidth = (int)(width * zoom);
            screenHeight = (int)(height * zoom);
        }

        private Visual followTarget;
        private PointF panTarget;
        //camera moves at a speed such that it will pan to its current target in 1/intensity seconds
        //keep in mind though that this speed is constantly decreasing, so actual pan time is higher
        private float panIntensity;

        public override void Update()
        {
            base.Update();

            if (followTarget != null)
                panTarget = followTarget.Center();

            if (panIntensity > 0.0f)
            {
                PointF panMove = new PointF();
                panMove.x = panTarget.x - (scroll.x + width / 2.0f);
                panMove.y = panTarget.y - (scroll.y + height / 2.0f);

                panMove.Scale(Math.Min(1.0f, Game.elapsed * panIntensity));

                scroll.Offset(panMove);
            }

            if ((shakeTime -= Game.elapsed) > 0.0f)
            {
                var damping = shakeTime / shakeDuration;
                shakeX = Rnd.Float(-shakeMagX, +shakeMagX) * damping;
                shakeY = Rnd.Float(-shakeMagY, +shakeMagY) * damping;
            }
            else
            {
                shakeX = 0.0f;
                shakeY = 0.0f;
            }

            UpdateMatrix();
        }

        public PointF Center()
        {
            return new PointF(width / 2, height / 2);
        }

        //public bool HitTest(float x, float y)
        //{
        //    return x >= this.x && y >= this.y && x < this.x + screenWidth && y < this.y + screenHeight;
        //}

        public void Shift(PointF point)
        {
            scroll.Offset(point);
            panIntensity = 0.0f;
        }

        public void SnapTo(float x, float y)
        {
            scroll.Set(x - width / 2, y - height / 2);
            panIntensity = 0.0f;
            followTarget = null;
        }

        public void SnapTo(PointF point)
        {
            SnapTo(point.x, point.y);
        }

        public void PanTo(PointF dst, float intensity)
        {
            panTarget = dst;
            panIntensity = intensity;
            followTarget = null;
        }

        public void PanFollow(Visual target, float intensity)
        {
            followTarget = target;
            panIntensity = intensity;
        }

        public PointF ScreenToCamera(int x, int y)
        {
            return new PointF(
                (x - this.x) / zoom + scroll.x,
                (y - this.y) / zoom + scroll.y);
        }

        public Point CameraToScreen(float x, float y)
        {
            return new Point(
                (int)((x - scroll.x) * zoom + this.x),
                (int)((y - scroll.y) * zoom + this.y));
        }

        public float ScreenWidth()
        {
            return width * zoom;
        }

        public float ScreenHeight()
        {
            return height * zoom;
        }

        protected virtual void UpdateMatrix()
        {
            /*	Matrix.setIdentity( matrix );
                Matrix.translate( matrix, -1, +1 );
                Matrix.scale( matrix, 2f / G.width, -2f / G.height );
                Matrix.translate( matrix, x, y );
                Matrix.scale( matrix, zoom, zoom );
                Matrix.translate( matrix, scroll.x, scroll.y );*/

            matrix[0] = +zoom * invW2;
            matrix[5] = -zoom * invH2;

            matrix[12] = -1 + x * invW2 - (scroll.x + shakeX) * matrix[0];
            matrix[13] = +1 - y * invH2 - (scroll.y + shakeY) * matrix[5];
        }

        public void Shake(float magnitude, float duration)
        {
            shakeMagX = shakeMagY = magnitude;
            shakeTime = shakeDuration = duration;
        }
    }
}