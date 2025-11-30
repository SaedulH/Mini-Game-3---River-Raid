using System.Collections;
using AudioSystem;
using UnityEngine;

public class BridgeScript : MonoBehaviour
{
    public AudioData DestroyAudio;
    public ParticleSystem DestroyEffect;
    public GameObject BridgeModel;

    private Collider _collider;

    public void Start()
    {
        _collider = GetComponent<Collider>();
    }

    public void DestroyBridge()
    {
        AudioManager.Instance.CreateAudioBuilder()
            .WithRandomPitch()
            .WithPosition(transform.position)
            .Play(DestroyAudio);
        StartCoroutine(OnDestroyEvent());
    }

    public IEnumerator OnDestroyEvent()
    {
        _collider.enabled = false;
        DestroyEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        DestroyEffect.Play();
        yield return new WaitForSeconds(0.1f);
        BridgeModel.SetActive(false);

        while (DestroyEffect.isPlaying)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}
