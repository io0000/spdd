namespace watabou.noosa
{
    public class Gizmo
    {
        public bool exists;
        public bool alive;
        public bool active;
        public bool visible;

        public Group parent;

        public Camera camera;

        public Gizmo()
        {
            exists = true;
            alive = true;
            active = true;
            visible = true;
        }

        public virtual void Destroy()
        {
            parent = null;
        }

        public virtual void Update()
        { }

        public virtual void Draw()
        { }

        public virtual void Kill()
        {
            alive = false;
            exists = false;
        }

        // Not exactly opposite to "kill" method
        public virtual void Revive()
        {
            alive = true;
            exists = true;
        }

        //public Camera camera() 
        public virtual Camera GetCamera()
        {
            if (camera != null)
                return camera;

            if (parent != null)
            {
                this.camera = parent.GetCamera();
                return this.camera;
            }

            return null;
        }

        public virtual bool IsVisible()
        {
            if (parent == null)
                return visible;

            return visible && parent.IsVisible();
        }

        public bool IsActive()
        {
            if (parent == null)
                return active;

            return active && parent.IsActive();
        }

        public void KillAndErase()
        {
            Kill();

            if (parent != null)
                parent.Erase(this);
        }

        public void Remove()
        {
            if (parent != null)
                parent.Remove(this);
        }
    }
}