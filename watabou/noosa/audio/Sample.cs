using System;
using System.Collections.Generic;

namespace watabou.noosa.audio
{
    public class Sample
    {
        private static Sample _instance;
        public static Sample Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Sample();

                return _instance;
            }
        }

        public const int MaxNumberOfStreams = 8;

        //private SoundPool Pool = new SoundPool(MaxNumberOfStreams, Stream.Music, 0);

        protected Dictionary<object, int> Ids = new Dictionary<object, int>();

        private bool enabled = true;
        private float globalVolume = 1f;

        public void Dispose()
        { }

        public IntPtr Handle { get; private set; }


        public void Reset()
        {
            Ids.Clear();
        }

        public void Pause()
        { }

        public void Resume()
        { }

        public void Load(params string[] assets)
        { }

        public void Unload(object src)
        {
            if (!Ids.ContainsKey(src))
                return;

            Ids.Remove(src);
        }

        public int Play(object id)
        {
            return Play(id, 1, 1, 1);
        }

        public int Play(object id, float volume)
        {
            return Play(id, volume, volume, 1);
        }

        public long Play(object id, float volume, float pitch)
        {
            return Play(id, volume, volume, pitch);
        }

        // play(int soundID, float leftVolume, float rightVolume, int priority, int loop, float rate) 
        public int Play(object id, float leftVolume, float rightVolume, float rate)
        {
            return 0;
        }

        public void Update()
        { }

        public void Enable(bool value)
        {
            enabled = value;
        }

        public bool IsEnabled()
        {
            return enabled;
        }

        public void Volume(float value)
        {
            globalVolume = value;
        }

        public void PlayDelayed(object id, float delay)
        { }
    }
}