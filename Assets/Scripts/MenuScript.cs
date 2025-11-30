using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public AudioSource PlayAudio;

    public void ToScene(string sceneName)
    {
        StartCoroutine(Play(sceneName));
    }

    private IEnumerator Play(string sceneName)
    {
        PlayAudio.Play();
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

}
