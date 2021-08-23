using System;
using System.Collections.Generic;
using watabou.utils;

namespace watabou.noosa
{
    public class Group : Gizmo
    {
        protected List<Gizmo> members;

        // Accessing it is a little faster,
        // than calling members.getSize()
        public int length;

        public Group()
        {
            members = new List<Gizmo>();
            length = 0;
        }

        public override void Destroy()
        {
            base.Destroy();

            for (int i = 0; i < length; ++i)
            {
                Gizmo g = members[i];
                if (g != null)
                {
                    g.Destroy();
                }
            }

            if (members != null)
            {
                members.Clear();
                members = null;
            }
            length = 0;
        }

        public override void Update()
        {
            for (int i = 0; i < length; ++i)
            {
                Gizmo g = members[i];
                if (g != null && g.exists && g.active)
                {
                    g.Update();
                }
            }
        }

        public override void Draw()
        {
            for (int i = 0; i < length; ++i)
            {
                Gizmo g = members[i];
                if (g != null && g.exists && g.IsVisible())
                {
                    g.Draw();
                }
            }
        }

        public override void Kill()
        {
            // A killed group keeps all its members,
            // but they Get killed too
            for (int i = 0; i < length; ++i)
            {
                Gizmo g = members[i];
                if (g != null && g.exists)
                {
                    g.Kill();
                }
            }

            base.Kill();
        }

        //public virtual int IndexOf(Gizmo g)
        //{
        //    return Members.IndexOf(g);
        //}

        public Gizmo Add(Gizmo g)
        {
            if (g.parent == this)
                return g;

            if (g.parent != null)
                g.parent.Remove(g);

            // Trying to find an empty space for a new member
            for (var i = 0; i < length; ++i)
            {
                if (members[i] != null)
                    continue;

                members[i] = g;
                g.parent = this;
                return g;
            }

            members.Add(g);
            g.parent = this;
            ++length;
            return g;
        }

        public Gizmo AddToFront(Gizmo g)
        {
            if (g.parent == this)
                return g;

            if (g.parent != null)
                g.parent.Remove(g);

            // Trying to find an empty space for a new member
            // starts from the front and never goes over a none-null element
            for (int i = length - 1; i >= 0; --i)
            {
                if (members[i] == null)
                {
                    if (i == 0 || members[i - 1] != null)
                    {
                        members[i] = g;
                        g.parent = this;
                        return g;
                    }
                }
                else
                {
                    break;
                }
            }

            members.Add(g);
            g.parent = this;
            ++length;
            return g;
        }

        public Gizmo AddToBack(Gizmo g)
        {
            if (g.parent == this)
            {
                SendToBack(g);
                return g;
            }

            if (g.parent != null)
                g.parent.Remove(g);

            if (members[0] == null)
            {
                members[0] = g;
                g.parent = this;
                return g;
            }

            members.Insert(0, g);
            g.parent = this;
            ++length;
            return g;
        }

        public T Recycle<T>() where T : Gizmo
        {
            var g = GetFirstAvailable(typeof(T));
            if (g != null)
                return (T)g;

            g = (T)Reflection.NewInstance(typeof(T));
            if (g != null)
                return (T)Add(g);

            return null;
        }

        // Fast removal - replacing with null
        public Gizmo Erase(Gizmo g)
        {
            var index = members.IndexOf(g);
            if (index != -1)
            {
                members[index] = null;
                g.parent = null;
                return g;
            }
            else
            {
                return null;
            }
        }

        // Real removal
        public Gizmo Remove(Gizmo g)
        {
            if (members.Remove(g))
            {
                --length;
                g.parent = null;
                return g;
            }
            else
            {
                return null;
            }
        }

        //public virtual Gizmo Replace(Gizmo oldOne, Gizmo newOne)
        //{
        //    var index = Members.IndexOf(oldOne);
        //    if (index == -1)
        //        return null;
        //
        //    Members[index] = newOne;
        //    newOne.Parent = this;
        //    oldOne.Parent = null;
        //    return newOne;
        //}

        public Gizmo GetFirstAvailable(Type c)
        {
            for (int i = 0; i < length; ++i)
            {
                Gizmo g = members[i];
                if (g != null && !g.exists && ((c == null) || g.GetType() == c))
                {
                    return g;
                }
            }

            return null;
        }

        public int CountLiving()
        {
            int count = 0;

            for (int i = 0; i < length; ++i)
            {
                Gizmo g = members[i];
                if (g != null && g.exists && g.alive)
                {
                    ++count;
                }
            }

            return count;
        }

        //public int CountDead()
        //{
        //    return Members.Count(g => g != null && !g.Alive);
        //}

        //public virtual Gizmo Random()
        //{
        //    if (Members.Count > 0)
        //        return Members[(int)(new Random(1).NextDouble() * Members.Count)];
        //
        //    return null;
        //}

        public void Clear()
        {
            for (int i = 0; i < length; ++i)
            {
                Gizmo g = members[i];
                if (g != null)
                {
                    g.parent = null;
                }
            }
            members.Clear();
            length = 0;
        }

        public Gizmo BringToFront(Gizmo g)
        {
            if (!members.Contains(g))
                return null;

            members.Remove(g);
            members.Add(g);
            return g;
        }

        public Gizmo SendToBack(Gizmo g)
        {
            if (!members.Contains(g))
                return null;

            members.Remove(g);
            members.Insert(0, g);
            return g;
        }
    }
}