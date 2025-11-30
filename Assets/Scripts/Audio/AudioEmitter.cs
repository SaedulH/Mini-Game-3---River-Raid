using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace AudioSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioEmitter : MonoBehaviour
    {
        public AudioData Data { get; private set; }
        public LinkedListNode<AudioEmitter> Node { get; set; }

        AudioSource audioSource;
        public Coroutine PlayingCoroutine { get; set; }

        void Awake()
        {
            audioSource = gameObject.GetOrAdd<AudioSource>();
        }

        public void Initialize(AudioData data)
        {
            Data = data;
            audioSource.clip = data.clip;
            audioSource.outputAudioMixerGroup = data.mixerGroup;
            audioSource.loop = data.loop;
            audioSource.playOnAwake = data.playOnAwake;

            audioSource.mute = data.mute;
            audioSource.bypassEffects = data.bypassEffects;
            audioSource.bypassListenerEffects = data.bypassListenerEffects;
            audioSource.bypassReverbZones = data.bypassReverbZones;

            audioSource.priority = data.priority;
            audioSource.volume = data.volume;
            audioSource.pitch = data.pitch;
            audioSource.panStereo = data.panStereo;
            audioSource.spatialBlend = data.spatialBlend;
            audioSource.reverbZoneMix = data.reverbZoneMix;
            audioSource.dopplerLevel = data.dopplerLevel;
            audioSource.spread = data.spread;

            audioSource.minDistance = data.minDistance;
            audioSource.maxDistance = data.maxDistance;

            audioSource.ignoreListenerVolume = data.ignoreListenerVolume;
            audioSource.ignoreListenerPause = data.ignoreListenerPause;

            audioSource.rolloffMode = data.rolloffMode;
        }

        public void Play(bool retain = false)
        {
            if (PlayingCoroutine != null)
            {
                StopCoroutine(PlayingCoroutine);
            }

            audioSource.Play();

            if (!retain)
            {
                PlayingCoroutine = StartCoroutine(WaitForSoundToEnd());
            }
        }

        IEnumerator WaitForSoundToEnd()
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
            Stop();
        }

        public void Resume()
        {
            if(!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        public void Pause()
        {
            if (PlayingCoroutine != null)
            {
                StopCoroutine(PlayingCoroutine);
                PlayingCoroutine = null;
            }

            audioSource.Stop();
        }

        public void Stop()
        {
            if (PlayingCoroutine != null)
            {
                StopCoroutine(PlayingCoroutine);
                PlayingCoroutine = null;
            }

            audioSource.Stop();
            AudioManager.Instance.ReturnToPool(this);
        }

        public void WithRandomPitch(float min = -0.1f, float max = 0.1f)
        {
            audioSource.pitch += Random.Range(min, max);
        }

        public void WithAdditivePitch(float pitch)
        {
            audioSource.pitch += pitch;
        }

        public void WithPitch(float pitch)
        {
            audioSource.pitch = pitch;
        }

        public void WithLoop()
        {
            audioSource.loop = true;
        }

        public void WithReverb(float min = -0.1f, float max = 0.1f)
        {
            audioSource.reverbZoneMix += Random.Range(min, max);
        }

        public void WithVolume(float volume)
        {
            audioSource.volume = volume;
        }
    }
}