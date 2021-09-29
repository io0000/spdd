using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using watabou.utils;

namespace watabou.noosa.audio
{
    public class Sample
    {
        private static Sample instance;

        public static Sample Instance
        {
            get
            {
                if (instance == null)
                    instance = new Sample();

                return instance;
            }
        }

        protected HashMap<string, SoundEffect> ids = new HashMap<string, SoundEffect>();
        private bool enabled = true;
        private float globalVolume = 1f;

        public void Reset()
        {
            ids.Clear();

            foreach (var pair in ids)
            {
                var sound = pair.Value;
                sound.Dispose();
            }
        }

        //public void Pause()
        //{
        //    foreach (var pair in ids)
        //    {
        //        var sound = pair.Value;
        //        sound.Pause();
        //    }
        //}

        //public void Resume()
        //{
        //    foreach (var pair in ids)
        //    {
        //        var sound = pair.Value;
        //        sound.Resume();
        //    }
        //}

        public void Load(string[] assets)
        {
            foreach (var asset in assets)
            {
                if (ids.ContainsKey(asset))
                    continue;

                using (FileStream fs = new FileStream(asset, FileMode.Open))
                {
                    try
                    {
                        var sound = SoundEffect.FromStream(fs);
                        ids.Add(asset, sound);
                    }
                    catch { }
                }
            }
        }

        //public void Unload(object src)
        //{
        //    if (!Ids.ContainsKey(src))
        //        return;
        //
        //    Ids.Remove(src);
        //}

        public void Play(string id)
        {
            Play(id, 1, 1, 1);
        }

        public void Play(string id, float volume)
        {
            Play(id, volume, volume, 1);
        }

        public void Play(string id, float volume, float pitch)
        {
            Play(id, volume, volume, pitch);
        }

        public void Play(string id, float leftVolume, float rightVolume, float pitch)
        {
            /*
                float volume = Math.max(leftVolume, rightVolume);
                float pan = rightVolume - leftVolume;
                if (enabled && ids.containsKey( id )) {
                    return ids.get(id).play( globalVolume*volume, pitch, pan );
                } else {
                    return -1;
                }              
             */
            float volume = MathF.Max(leftVolume, rightVolume);
            float pan = rightVolume - leftVolume;

            if (enabled && ids.ContainsKey(id))
            {
                var sound = ids[id];

                // convert gdx style [0.5, 2] -> xna style [-1, 1]
                pitch = MathF.Log(pitch, 2.0f);

                sound.Play(globalVolume * volume, pitch, pan);
            }
        }

        private class DelayedSoundEffect
        {
            internal string id;
            internal float delay;

            internal float leftVol;
            internal float rightVol;
            internal float pitch;
        }

        private static HashSet<DelayedSoundEffect> delayedSFX = new HashSet<DelayedSoundEffect>();

        public void PlayDelayed(string id, float delay)
        {
            PlayDelayed(id, delay, 1);
        }

        public void PlayDelayed(string id, float delay, float volume)
        {
            PlayDelayed(id, delay, volume, volume, 1);
        }

        public void PlayDelayed(string id, float delay, float leftVolume, float rightVolume, float pitch)
        {
            if (delay <= 0)
            {
                Play(id, leftVolume, rightVolume, pitch);
                return;
            }

            DelayedSoundEffect sfx = new DelayedSoundEffect();
            sfx.id = id;
            sfx.delay = delay;
            sfx.leftVol = leftVolume;
            sfx.rightVol = rightVolume;
            sfx.pitch = pitch;
            delayedSFX.Add(sfx);
        }

        public void Update()
        {
            DoUpdate();

            if (delayedSFX.Count == 0)
                return;

            foreach (var sfx in delayedSFX.ToArray())
            {
                sfx.delay -= Game.elapsed;
                if (sfx.delay <= 0)
                {
                    delayedSFX.Remove(sfx);
                    Play(sfx.id, sfx.leftVol, sfx.rightVol, sfx.pitch);
                }
            }
        }

        // monogame
        public void DoUpdate()
        {
            DynamicSoundEffectInstanceManager.UpdatePlayingInstances();
            SoundEffectInstancePool.Update();
            //Microphone.UpdateMicrophones();
        }

        public void Enable(bool value)
        {
            enabled = value;
        }

        public void Volume(float value)
        {
            globalVolume = value;
        }

        //public bool IsEnabled()
        //{
        //    return enabled;
        //}
    }
}