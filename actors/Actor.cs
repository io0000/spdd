using System;
using System.Collections;
using System.Collections.Generic;
using watabou.utils;
using spdd.scenes;

namespace spdd.actors
{
    public abstract class Actor : IBundlable
    {
        public const float TICK = 1.0f;

        private float time;
        private int id;

        //default priority values for general actor categories
        //note that some specific actors pick more specific values
        //e.g. a buff acting after all normal buffs might have priority BUFF_PRIO + 1
        public const int VFX_PRIO = 100;    //visual effects take priority
        public const int HERO_PRIO = 0;     //positive is before hero, negative after
        public const int BLOB_PRIO = -10;   //blobs act after hero, before mobs
        public const int MOB_PRIO = -20;    //mobs act between buffs and blobs
        public const int BUFF_PRIO = -30;   //buffs act last in a turn
        public const int DEFAULT = -100;      //if no priority is given, act after all else

        //used to determine what order actors act in if their time is equal. Higher values act earlier.
        public int actPriority = DEFAULT;

        public abstract bool Act();

        public virtual void Spend(float time)
        {
            this.time += time;
            //if time is very close to a whole number, round to a whole number to fix errors
            float ex = Math.Abs(this.time % 1.0f);
            if (ex < .001f)
            {
                this.time = (float)Math.Round(this.time, MidpointRounding.AwayFromZero);
            }
        }

        public void SpendToWhole()
        {
            time = (float)Math.Ceiling(time);
        }

        protected void Postpone(float time)
        {
            if (this.time < now + time)
            {
                this.time = now + time;
                //if time is very close to a whole number, round to a whole number to fix errors
                float ex = Math.Abs(this.time % 1f);
                if (ex < .001f)
                    this.time = (float)Math.Round(this.time, MidpointRounding.AwayFromZero);
            }
        }

        public float Cooldown()
        {
            return time - now;
        }

        protected virtual void Deactivate()
        {
            time = float.MaxValue;
        }

        protected virtual void OnAdd()
        { }

        protected virtual void OnRemove()
        { }

        private const string TIME = "time";
        private const string ID = "id";

        public virtual void StoreInBundle(Bundle bundle)
        {
            bundle.Put(TIME, time);
            bundle.Put(ID, id);
        }

        public virtual void RestoreFromBundle(Bundle bundle)
        {
            time = bundle.GetFloat(TIME);
            id = bundle.GetInt(ID);
        }

        private static int nextID = 1;

        public int Id()
        {
            if (id > 0)
            {
                return id;
            }
            else
            {
                return (id = nextID++);
            }
        }

        // **********************
        // *** Static members ***

        // HashSet
        private static List<Actor> all = new List<Actor>();
        // HashSet
        private static List<Character> chars = new List<Character>();
        private static Actor current;
        private static SparseArray<Actor> ids = new SparseArray<Actor>();
        private static float now;

        public static float Now()
        {
            return now;
        }

        public static void Clear()
        {
            now = 0;

            all.Clear();
            chars.Clear();

            ids.Clear();
        }

        public static void FixTime()
        {
            if (all.Count == 0)
                return;

            var min = float.MaxValue;
            foreach (var a in all)
            {
                if (a.time < min)
                    min = a.time;
            }

            //Only pull everything back by whole numbers
            //So that turns always align with a whole number
            min = (int)min;
            foreach (var a in all)
                a.time -= min;

            if (Dungeon.hero != null && all.Contains(Dungeon.hero))
                Statistics.duration += min;

            now -= min;
        }

        public static void Init()
        {
            Add(Dungeon.hero);

            foreach (var mob in Dungeon.level.mobs)
                Add(mob);

            foreach (var blob in Dungeon.level.blobs.Values)
                Add(blob);

            current = null;
        }

        private const string NEXTID = "nextid";

        public static void StoreNextID(Bundle bundle)
        {
            bundle.Put(NEXTID, nextID);
        }

        public static void RestoreNextID(Bundle bundle)
        {
            nextID = bundle.GetInt(NEXTID);
        }

        public static void ResetNextID()
        {
            nextID = 1;
        }

        public virtual void Next()
        {
            if (current == this)
            {
                current = null;
            }
        }

        public static bool Processing()
        {
            return current != null;
        }

        private enum ProcessState
        {
            None,
            WaitMoveDone,
            WaitNext
        }

        private static ProcessState processState = ProcessState.None;
        private static IEnumerator processEnumerator;
        private static bool inputNext;
        private static int waitMoveId;

        public static bool ProcessStarted()
        {
            return processEnumerator != null;
        }

        public static void StartProcess()
        {
            processEnumerator = ProcessHelper();
        }

        public static void InputNext()
        {
            inputNext = true;
        }

        public static void InputMoveDone(Actor actor)
        {
            if (waitMoveId == 0)
                return;

            if (actor.Id() == waitMoveId)
                waitMoveId = 0;
        }

        public static void Process()
        {
            if (processState == ProcessState.WaitMoveDone)
            {
                if (waitMoveId != 0)
                    return;
            }
            else if (processState == ProcessState.WaitNext)
            {
                if (inputNext == false)
                    return;

                inputNext = false;
            }

            processEnumerator.MoveNext();
            processState = (ProcessState)processEnumerator.Current;
        }

        public static IEnumerator ProcessHelper()
        {
            bool doNext = false;

            while (true)
            {
                bool interrupted = false;
                current = null;
                now = float.MaxValue;

                foreach (var actor in all)
                {
                    //some actors will always go before others if time is equal.
                    if (actor.time < now ||
                        actor.time == now && (current == null || actor.actPriority > current.actPriority))
                    {
                        now = actor.time;
                        current = actor;
                    }
                }

                if (current != null)
                {
                    //Console.WriteLine("now:{0} current:{1}", now, current.ToString());  //tt

                    Actor acting = current;

                    if (acting is Character)
                    {
                        var ch = (Character)acting;
                        var spr = ch.sprite;
                        if (spr != null)
                        {
                            if (spr.isMoving)
                            {
                                waitMoveId = current.Id();

                                if (InterlevelScene.mode == InterlevelScene.Mode.FALL)
                                    interrupted = true;
                                else
                                    yield return ProcessState.WaitMoveDone;
                            }
                        }
                    }

                    if (interrupted)
                    {
                        doNext = false;
                        current = null;
                    }
                    else
                    {
                        doNext = acting.Act();
                        if (doNext && (Dungeon.hero == null || !Dungeon.hero.IsAlive()))
                        {
                            doNext = false;
                            current = null;
                        }
                    }
                }
                else
                {
                    doNext = false;
                }

                if (!doNext)
                {
                    yield return ProcessState.WaitNext;
                }
            }
        }

        public static void Add(Actor actor)
        {
            Add(actor, now);
        }

        public static void AddDelayed(Actor actor, float delay)
        {
            Add(actor, now + delay);
        }

        private static void Add(Actor actor, float time)
        {
            if (all.Contains(actor))
                return;

            ids[actor.Id()] = actor;

            all.Add(actor);
            actor.time += time;
            actor.OnAdd();

            if (actor is Character)
            {
                var ch = (Character)actor;
                if (!chars.Contains(ch))            // list라서 검사
                    chars.Add(ch);
                foreach (var buff in ch.Buffs())
                {
                    if (!all.Contains(buff))        // list라서 검사
                        all.Add(buff);
                    buff.OnAdd();
                }
            }
        }

        public static void Remove(Actor actor)
        {
            if (actor != null)
            {
                all.Remove(actor);
                if (actor is Character)
                    chars.Remove((Character)actor);
                actor.OnRemove();

                if (actor.id > 0)
                {
                    ids.Remove(actor.id);
                }
            }
        }

        public static Character FindChar(int pos)
        {
            foreach (var ch in chars)
            {
                if (ch.pos == pos)
                    return ch;
            }

            return null;
        }

        public static Actor FindById(int id)
        {
            Actor actor;
            if (ids.TryGetValue(id, out actor))
                return actor;
            else
                return null;
        }

        public static List<Actor> All()
        {
            return new List<Actor>(all);
        }

        public static List<Character> Chars()
        {
            return new List<Character>(chars);
        }
    }

    public class ActionActor : Actor
    {
        public Func<bool> action;

        public override bool Act()
        {
            return action();
        }
    }
}
