using System.Collections;
using AudioSystem;
using UnityEngine;

public class RaidRocketScript : MonoBehaviour
{
    public float RocketSpeed = 100;
    public float TravelDistance = 0;
    public float StartPosition;
    private Collider _collider;

    public GameObject RocketModel;

    public AudioData TravelAudio;
    public ParticleSystem TravelEffect;

    public AudioData HitAudio;
    public ParticleSystem HitEffect;

    private bool _isTravelling = true;
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider>();
        _isTravelling = true;
        RocketModel.SetActive(true);
        StartPosition = transform.position.z;
        AudioManager.Instance.CreateAudioBuilder()
            .WithRandomPitch()
            .WithPosition(transform.position)
            .Play(TravelAudio);
        TravelEffect.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isTravelling) return;

        transform.Translate(RocketSpeed * Time.deltaTime * Vector3.forward);
        TravelDistance = transform.position.z - StartPosition;
        if (TravelDistance >= 25)
        {
            DestroyRocket();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if(collision.gameObject.TryGetComponent<EnemyScript>(out EnemyScript enemy))
            {
                enemy.DestroyEnemy();
                GameManager.Instance.AddScore(100);
            }
            else if (collision.gameObject.TryGetComponent<JetScript>(out JetScript jet))
            {
                jet.DestroyJet();
                GameManager.Instance.AddScore(150);
            }
        }
        else if (collision.gameObject.CompareTag("Bridge"))
        {
            if (collision.gameObject.TryGetComponent<BridgeScript>(out BridgeScript bridge))
            {
                bridge.DestroyBridge();
            }
            GameManager.Instance.AddScore(200);
        }
        DestroyRocket();
    }

    public void DestroyRocket()
    {
        _isTravelling = false;
        if(TravelEffect != null)
        {
            TravelEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        AudioManager.Instance.CreateAudioBuilder()
            .WithRandomPitch()
            .WithPosition(transform.position)
            .Play(HitAudio);
         StartCoroutine(OnDestroyEvent());
    }

    IEnumerator OnDestroyEvent()
    {
        _collider.enabled = false;
        HitEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        HitEffect.Play();
        yield return new WaitForSeconds(0.1f);
        RocketModel.SetActive(false);

        while (HitEffect.isPlaying)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}
