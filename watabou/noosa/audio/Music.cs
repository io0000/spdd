using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;

namespace watabou.noosa.audio
{
    public class Music
    {
        private static Music instance;

        public static Music Instance
        {
            get
            {
                if (instance == null)
                    instance = new Music();

                return instance;
            }
        }

        private string lastPlayed;
        private bool looping;
        private bool enabled = true;
        private float volume = 1f;

        public void Play(string assetName, bool looping)
        {
            if (IsPlaying() && lastPlayed != null && lastPlayed == assetName)
            {
                return;
            }

            Stop();

            lastPlayed = assetName;
            this.looping = looping;

            if (!enabled || assetName == null)
            {
                return;
            }

            try
            {
                //var path = Path.Combine(TitleContainer.Location, assetName);
                //var song = new Song(path);
                var song = new Song(assetName);

                MediaPlayer.IsRepeating = looping;
                MediaPlayer.Volume = volume;
                MediaPlayer.Play(song);
            }
            catch { }
        }

        //public void Mute()
        //{
        //    lastPlayed = null;
        //    Stop();
        //}

        //public void Pause()
        //{ }

        //public void Resume()
        //{ }

        public void Stop()
        {
            //player.stop();
            //player.dispose();
            //player = null;
            var queue = MediaPlayer.Queue;
            var previousSong = queue.Count > 0 ? queue[0] : null;
            if (previousSong != null)
                previousSong.Dispose();

            MediaPlayer.Stop();
        }

        public void Volume(float value)
        {
            volume = value;
            MediaPlayer.Volume = value;
        }

        public bool IsPlaying()
        {
            return MediaPlayer.State == MediaState.Playing;
        }

        public void Enable(bool value)
        {
            enabled = value;

            if (IsPlaying() && !value)
            {
                Stop();
            }
            else if (!IsPlaying() && value)
            {
                Play(lastPlayed, looping);
            }
        }

        public bool IsEnabled()
        {
            return enabled;
        }
    }
}