using UnityEngine;

public class CheckpointScript : MonoBehaviour
{
    public AudioSource CheckPointAudio;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("CheckPoint Reached");
            CheckPointAudio.Play();
            GameManager.Instance.CheckPointReached();
        }
    }
}
