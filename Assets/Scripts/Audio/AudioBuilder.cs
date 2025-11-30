using UnityEngine;

namespace AudioSystem
{
    public class AudioBuilder
    {
        readonly AudioManager audioManager;
        Transform parent;
        Vector3 position = Vector3.zero;
        float volume;
        bool randomPitch;
        bool additivePitch;
        float newPitch;
        bool reverb;
        bool loop;

        public AudioBuilder(AudioManager audioManager)
        {
            this.audioManager = audioManager;
        }

        public AudioBuilder WithPosition(Vector3 position)
        {
            this.position = position;
            return this;
        }

        public AudioBuilder WithParent(Transform parent)
        {
            this.parent = parent;
            return this;
        }

        public AudioBuilder WithRandomPitch()
        {
            this.randomPitch = true;
            return this;
        }

        public AudioBuilder WithAdditivePitch(float pitch)
        {
            this.additivePitch = true;
            this.newPitch = pitch;
            return this;
        }

        public AudioBuilder WithReverb()
        {
            this.reverb = true;
            return this;
        }

        public AudioBuilder WithLoop()
        {
            this.loop = true;
            return this;
        }

        public AudioBuilder WithVolume(float volume)
        {
            this.volume = volume;
            return this;
        }

        public AudioEmitter Play(AudioData audioData, bool retain = false)
        {
            if (audioData == null)
            {
                //Debug.LogWarning("SoundData is null");
                return null;
            }

            if (!audioManager.CanPlaySound(audioData)) return null;

            AudioEmitter audioEmitter = audioManager.Get();
            audioEmitter.Initialize(audioData);
            audioEmitter.transform.position = position;
            if (parent != null)
            {
                audioEmitter.transform.parent = parent;
            }
            else
            {
                audioEmitter.transform.parent = audioManager.transform;
            }

            if (randomPitch)
            {
                audioEmitter.WithRandomPitch();
            } else if (additivePitch)
            {
                audioEmitter.WithAdditivePitch(newPitch);
            }

            if (loop)
            {
                audioEmitter.WithLoop();
            }

            if(reverb)
            {
                audioEmitter.WithReverb();
            }

            if(volume > 0f)
            {
                audioEmitter.WithVolume(volume);
            }

            if (audioData.frequentSound)
            {
                audioEmitter.Node = audioManager.FrequentAudioEmitters.AddLast(audioEmitter);
            }

            audioEmitter.Play(retain);

            return audioEmitter;
        }
    }
}