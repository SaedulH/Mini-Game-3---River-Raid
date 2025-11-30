using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using UnityEngine;

public enum ThrusterState
{
    Slow = -1,
    Normal = 0,
    Fast = 1,
    Off = 2
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public GameObject MainCamera;

    private Rigidbody _rb;
    private GameManager _manager;
    private Animator _animator;
    private Collider _collider;

    public GameObject RocketPrefab;
    public bool RocketReady = true;

    public AudioData SpeedAudio;
    public AudioEmitter SpeedAudioEmitter;

    public ThrusterState CurrentThrusterState = ThrusterState.Off;
    public ParticleSystem LeftThrusterSmoke;
    public ParticleSystem RightThrusterSmoke;

    public GameObject PlayerModel;
    public AudioData DestroyAudio;
    public ParticleSystem DestroyEffect;

    public AudioData RefuelAudio;
    public AudioEmitter RefuelAudioEmitter;

    public List<GameObject> ActiveFuelDepots;
    public float CamDistanceOffset = 7f;
    public float CamHeightOffset = 35f;
    public float VerticalSpeed = 0;
    public float HorizontalSpeed = 15;
    public float CurrentThrustInput = 0;
    public bool IsPlaying = false;
    public bool IsPlayerAlive = true;

    public float Fast = 17;
    public float Default = 11;
    public float Slow = 5;
    public float Off = 0;

    void Start()
    {
        Instance = this;
        _manager = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameManager>();
        _animator = gameObject.GetComponentInChildren<Animator>();
        _collider = gameObject.GetComponent<Collider>();
        _collider.enabled = true;
        _rb = gameObject.GetComponent<Rigidbody>();
        ActiveFuelDepots = new List<GameObject>();
        IsPlaying = false;
        SetThrusterEffects(ThrusterState.Normal);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsPlaying)
        {
            float directionInput = Mathf.Clamp(Input.GetAxis("Horizontal"), -1f, 1f);
            float direction = directionInput * HorizontalSpeed * Time.deltaTime;
            _animator.SetFloat("Direction", directionInput);
            transform.Translate(direction, 0, 0);

            GetThrusterSpeed();

            if ((Input.GetKey(KeyCode.Space) == true || Input.GetMouseButton(0) == true) && IsPlayerAlive)
            {
                if (RocketReady)
                {
                    StartCoroutine(FireRocket());
                }
            }
        }
    }

    private void GetThrusterSpeed()
    {
        CurrentThrustInput = Input.GetAxis("Vertical");
        if (CurrentThrustInput > 0 && VerticalSpeed != Fast)
        {
            SetThrusterEffects(ThrusterState.Fast);
            VerticalSpeed = Fast;
        }
        else if (CurrentThrustInput == 0 && VerticalSpeed != Default)
        {
            SetThrusterEffects(ThrusterState.Normal);
            VerticalSpeed = Default;
        }
        else if (CurrentThrustInput < 0 && VerticalSpeed != Slow)
        {
            SetThrusterEffects(ThrusterState.Slow);
            VerticalSpeed = Slow;
        }

        transform.Translate(VerticalSpeed * Time.deltaTime * Vector3.forward);
        MainCamera.transform.position = new Vector3(0, CamHeightOffset, transform.position.z + CamDistanceOffset);
    }

    private void SetThrusterEffects(ThrusterState thruster)
    {
        if (thruster == CurrentThrusterState)
        {
            return;
        }
        CurrentThrusterState = thruster;
        if (SpeedAudioEmitter == null)
        {
            SpeedAudioEmitter = AudioManager.Instance.CreateAudioBuilder()
                .WithLoop()
                .WithParent(transform)
                .WithPosition(transform.position)
                .Play(SpeedAudio, true);
        } else
        {
            SpeedAudioEmitter.Resume();
        }

            switch (thruster)
            {
                case ThrusterState.Fast:
                    SpeedAudioEmitter
                        .WithPitch(SpeedAudio.pitch + 0.2f);
                    //add particle effects here
                    break;
                case ThrusterState.Normal:
                    SpeedAudioEmitter
                        .WithPitch(SpeedAudio.pitch);
                    //add particle effects here
                    break;
                case ThrusterState.Slow:
                    SpeedAudioEmitter
                        .WithPitch(SpeedAudio.pitch - 0.2f);
                    //add particle effects here
                    break;
                case ThrusterState.Off:
                    SpeedAudioEmitter.Pause();
                    break;
            }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.CompareTag("Level") ||
            collision.gameObject.CompareTag("Bridge"))
        {
            LoseLife();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Fuel"))
        {
            AddActiveFuelDepot(other.gameObject);          
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Fuel"))
        {
            RemoveActiveFuelDepot(other.gameObject);
        }
    }

    public void AddActiveFuelDepot(GameObject gameObject)
    {
        if (!ActiveFuelDepots.Contains(gameObject))
        {
            ActiveFuelDepots.Add(gameObject);
        }

        if (ActiveFuelDepots.Count > 0 && !FuelGauge.Instance._refueling)
        {
            FuelGauge.Instance._refueling = true;
            if (RefuelAudioEmitter == null)
            {
                RefuelAudioEmitter = AudioManager.Instance.CreateAudioBuilder()
                    .WithLoop()
                    .WithPosition(transform.position)
                    .Play(RefuelAudio);
            }
        }
    }

    public void RemoveActiveFuelDepot(GameObject gameObject)
    {
        if (ActiveFuelDepots.Contains(gameObject))
        {
            ActiveFuelDepots.Remove(gameObject);
        }

        if (ActiveFuelDepots.Count == 0 && FuelGauge.Instance._refueling)
        {
            FuelGauge.Instance._refueling = false;
            if (RefuelAudioEmitter != null)
            {
                RefuelAudioEmitter.Stop();
                RefuelAudioEmitter = null;
            }
        }
    }

    public void LoseLife()
    {
        SetThrusterEffects(ThrusterState.Off);
        StartCoroutine(OnDestroyEvent());
        ActiveFuelDepots.Clear();
        if (RefuelAudioEmitter != null)
        {
            RefuelAudioEmitter.Stop();
            RefuelAudioEmitter = null;
        }
        AudioManager.Instance.CreateAudioBuilder()
                    .WithRandomPitch()
                    .WithPosition(transform.position)
                    .Play(DestroyAudio);

        _manager.LoseLife();
    }

    IEnumerator OnDestroyEvent()
    {
        _collider.enabled = false;
        DestroyEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        DestroyEffect.Play();
        yield return new WaitForSeconds(0.1f);
        PlayerModel.SetActive(false);
    }

    IEnumerator FireRocket()
    {
        RocketReady = false;
        Instantiate(RocketPrefab, transform.position + new Vector3(0, -0.5f, 3), Quaternion.identity);
        yield return new WaitForSeconds(0.4f);
        RocketReady = true;
    }

    public void ResetAtLevelStart(Vector3 position)
    {
        _rb.linearVelocity = Vector3.zero;
        transform.position = position;
        MainCamera.transform.position = new Vector3(0, CamHeightOffset, transform.position.z + CamDistanceOffset);
        FuelGauge.Instance.Reset();
        _collider.enabled = true;
        PlayerModel.SetActive(true);
        SetThrusterEffects(ThrusterState.Normal);
    }
}

