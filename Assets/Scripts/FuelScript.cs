using System.Collections;
using AudioSystem;
using UnityEngine;

public class FuelScript : MonoBehaviour
{
    private bool _isActive = true;
    private Collider _collider;
    public float deadzone = 20;
    public PlayerManager player;
    public GameObject FuelDepotModel;
    public AudioData DestroyAudio;
    public ParticleSystem DestroyEffect;

    void Start()
    {
        _isActive = true;
        _collider = GetComponent<Collider>();
        _collider.enabled = true;
        FuelDepotModel.SetActive(true);
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            if (_isActive && transform.position.z < (player.transform.position.z - deadzone))
            {
                OnOutOfRange();
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Rocket"))
        {
            if (collision.gameObject.TryGetComponent<RaidRocketScript>(out RaidRocketScript rocket))
            {
                rocket.DestroyRocket(); 
            }
            DestroyFuelDepot();
            GameManager.Instance.AddScore(50);
        }
    }

    public void DestroyFuelDepot()
    {
        _isActive = false;
        PlayerManager.Instance.RemoveActiveFuelDepot(this.gameObject);
        AudioManager.Instance.CreateAudioBuilder()
            .WithRandomPitch()
            .WithPosition(transform.position)
            .Play(DestroyAudio);
        StartCoroutine(OnDestroyEvent());
    }

    public void OnOutOfRange()
    {
        _isActive = false;
        PlayerManager.Instance.RemoveActiveFuelDepot(this.gameObject);
        Destroy(this.gameObject);
    }

    public IEnumerator OnDestroyEvent()
    {
        _collider.enabled = false;
        DestroyEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        DestroyEffect.Play();
        yield return new WaitForSeconds(0.1f);
        FuelDepotModel.SetActive(false);

        while (DestroyEffect.isPlaying)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}
