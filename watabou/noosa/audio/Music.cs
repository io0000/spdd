using System;

namespace watabou.noosa.audio
{
    public class Music
    {
        private static Music _instance;
        public static Music Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Music();

                return _instance;
            }
        }

        private bool _enabled = true;


        public void Dispose()
        { }

        public IntPtr Handle { get; private set; }


        public void Play(string assetName, bool looping)
        { }

        public void Mute()
        { }

        public void Pause()
        { }

        public void Resume()
        { }

        public void Stop()
        { }

        public void Volume(float value)
        { }

        public void Enable(bool value)
        {
            _enabled = value;
        }

        public bool IsEnabled()
        {
            return _enabled;
        }
    }
}