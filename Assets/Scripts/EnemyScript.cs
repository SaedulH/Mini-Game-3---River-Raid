using System.Collections;
using AudioSystem;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public PlayerManager Player;
    private bool _isActive = true;
    private Collider _collider;
    public float EnemySpeed = 7;
    public float Deadzone = 20;
    public float TurnCooldown = 0.5f;
    public float DetectionDistance = 2.6f;
    public Transform DetectionPoint;

    public GameObject EnemyModel;
    public AudioData TravelAudio;
    public AudioEmitter TravelAudioEmitter;
    public AudioData DestroyAudio;
    public ParticleSystem DestroyEffect;

    private float _timeSinceRotated = 0f;

    void Awake()
    {
        _isActive = true;
        _collider = GetComponent<Collider>();
        _collider.enabled = true;
        EnemyModel.SetActive(true);
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
        TravelAudioEmitter = AudioManager.Instance.CreateAudioBuilder()
            .WithParent(transform)
            .WithLoop()
            .WithRandomPitch()
            .WithPosition(transform.position)
            .Play(TravelAudio);
    }

    void Update()
    {
        _timeSinceRotated += Time.deltaTime;

        if (Player.IsPlayerAlive)
        {
            // Detect wall ahead
            Vector3 dir = Vector3.right * Mathf.Sign(EnemySpeed);
            Debug.DrawRay(DetectionPoint.position, dir * DetectionDistance, Color.red);
            if (Physics.SphereCast(DetectionPoint.position, 1f ,dir, out RaycastHit hit, DetectionDistance, LayerMask.GetMask("Collision"), QueryTriggerInteraction.Ignore))
            {
                Debug.DrawRay(DetectionPoint.position, dir * DetectionDistance, Color.green);
                if (_timeSinceRotated > 0.5f)
                {
                    float newDirection = EnemySpeed > 0 ? -180f : 0f;
                    //Debug.Log("Enemy hit wall, turning " + newDirection);
                    EnemyModel.transform.rotation = Quaternion.Euler(0f, newDirection, 0f);
                    EnemySpeed *= -1;
                    _timeSinceRotated = 0f;
                }
            }

            transform.position += EnemySpeed * Time.deltaTime * Vector3.right;
            if (_isActive && transform.position.z < Player.transform.position.z - Deadzone)
            {
                OnOutOfRange();
            }
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Level") && 
    //        _isActive)
    //    {
    //        if (_timeSinceRotated > 0.5f)
    //        {
    //            float newDirection = EnemySpeed > 0 ? -180f : 0f;
    //            //Debug.Log("Enemy hit wall, turning " + newDirection);
    //            EnemyModel.transform.rotation = Quaternion.Euler(0f, newDirection, 0f);
    //            EnemySpeed *= -1;
    //            _timeSinceRotated = 0f;
    //        }
    //    }
    //}

    public void DestroyEnemy()
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
        EnemyModel.SetActive(false);

        while (DestroyEffect.isPlaying)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}
