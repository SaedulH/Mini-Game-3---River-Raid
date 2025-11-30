using System.Collections;
using AudioSystem;
using UnityEngine;

public class JetScript : MonoBehaviour
{
    private bool _isActive = true;
    private bool _lookingRight = true;
    public float JetSpeed = 27;
    private Collider _collider;
    public GameObject JetModel;

    public AudioData TravelAudio;
    public AudioEmitter TravelAudioEmitter;
    public AudioData DestroyAudio;
    public ParticleSystem DestroyEffect;

    public float Deadzone = 50;
    void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.enabled = true;
        _isActive = true;
        JetModel.SetActive(true);
    }

    private void Start()
    {
        float side = transform.position.x;
        //If Jet on right side, flip it around to face left
        int direction = side > 0 ? -1 : 1;
        float rotation = direction > 0 ? 0f : -180f;
        JetModel.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        JetModel.SetActive(true);
        _lookingRight = (direction > 0);
        JetSpeed = JetSpeed * direction;
        Deadzone = Deadzone * direction;
        TravelAudioEmitter = AudioManager.Instance.CreateAudioBuilder()
            .WithParent(transform)
            .WithRandomPitch()
            .WithPosition(transform.position)
            .Play(TravelAudio);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + (Vector3.right * JetSpeed) * Time.deltaTime;

        if (_isActive && (_lookingRight && transform.position.x > Deadzone) || (!_lookingRight && transform.position.x < Deadzone))
        {
            OnOutOfRange();
        }
    }

    public void DestroyJet()
    {
        _isActive = false;
        if (TravelAudioEmitter != null)
        {
            TravelAudioEmitter.Stop();
            TravelAudioEmitter = null;
        }
        AudioManager.Instance.CreateAudioBuilder()
            .WithRandomPitch()
            .WithPosition(transform.position)
            .Play(DestroyAudio);
        StartCoroutine(OnDestroyEvent());
    }

    public void OnOutOfRange()
    {
        _isActive = false;
        EnemySpawner.Instance.RemoveFromActiveEnemies(this.gameObject);
        if (TravelAudioEmitter != null)
        {
            TravelAudioEmitter.Stop();
            TravelAudioEmitter = null;
        }
        Destroy(this.gameObject);
    }

    public IEnumerator OnDestroyEvent()
    {
        _collider.enabled = false;
        EnemySpawner.Instance.RemoveFromActiveEnemies(this.gameObject);

        DestroyEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        DestroyEffect.Play();
        yield return new WaitForSeconds(0.1f);
        JetModel.SetActive(false);

        while (DestroyEffect.isPlaying)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}
