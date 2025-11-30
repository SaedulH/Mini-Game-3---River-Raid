using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioSource BGM;
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        BGM = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (!BGM.isPlaying)
        {
            BGM.Play();
        }
    }
}
