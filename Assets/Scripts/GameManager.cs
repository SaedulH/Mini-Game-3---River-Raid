using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [field: Header("Prefabs")]

    public List<GameObject> ActiveLevels;
    public List<GameObject> ActiveCheckpoint;
    public GameObject Levels;
    public GameObject Bridge;

    public int PlayerScore = 0;
    public int Countdown;
    private float _highscore;
    public TMP_Text CountdownText;
    public TMP_Text CurrentScoreValue;
    public TMP_Text FinalScoreText;
    public TMP_Text HighScoreValue;
    public TMP_Text LevelNumberValue;

    [field: Header("Components")]

    public GameObject GameOverScreen;
    public AudioData GameOverAudio;
    public AudioData LoseLifeAudio;
    public AudioData StartAudio;

    public EnemySpawner EnemySpawner;
    public GameObject LevelParent;
    public GameObject StartSegment;
    public GameObject FakeStartSegment;
    public GameObject FakeBridge;
    public GameObject BridgeParent;
    public GameObject NextInLine;

    public Animator Lives;

    private Camera _mainCamera;

    [field: Header("Variables")]

    public int LivesCount = 3;
    public bool IsGameReady = false;
    public float DestroyThreshold = 100;
    public float SpawnThreshold = 60;
    private bool _isNextLevelLoaded = false;
    public int Checkpoint;
    public int LevelNumber = 0;
    public string LevelSeed = "Level";
    public int currentSeed = 0;
    public bool IsIntoBridge = true;
    public bool IsSplitRiver = false;

    // Start is called before the first frame update
    void Start()
    {
        _highscore = PlayerPrefs.GetFloat("RiverRaid");
        HighScoreValue.text = _highscore.ToString();
        IsGameReady = false;
        _isNextLevelLoaded = false;
        if (Instance == null) { Instance = this; }

        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        EnemySpawner = FindAnyObjectByType<EnemySpawner>();
        StartCoroutine(LoadStartSegment());
        StartCoroutine(CountdownStart(Countdown));
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsGameReady) return;

        PreloadNext();
        RemovePreviousLevel();
    }

    void DetermineLevelDifficulty()
    {
        LevelNumber += 1;
        LevelSeed += LevelNumber.ToString();
        currentSeed = LevelSeed.GetHashCode();
        EnemySpawner.AdjustFrequencies(LevelNumber);


        if (LevelNumber % 3 == 0)
        {
            IsSplitRiver = true;
        }
        else
        {
            IsSplitRiver = false;
        }
    }


    [ContextMenu("LoadStartSegment")]
    public IEnumerator LoadStartSegment()
    {
        GameObject startSegment = Instantiate(StartSegment, (10f * Vector3.back), Quaternion.identity, LevelParent.transform);
        ActiveLevels.Add(startSegment);
        NextInLine = startSegment;
        yield return new WaitForEndOfFrame();

        EnemySpawner.SetupLevelMesh(startSegment);
    }

    [ContextMenu("LoadFakeStartSegment")]
    public IEnumerator LoadFakeStartSegment(Vector3 position)
    {
        Vector3 spawnPos = position + (30f * Vector3.back);
        GameObject startSegment = Instantiate(FakeStartSegment, spawnPos, Quaternion.identity, LevelParent.transform);
        ActiveLevels.Add(startSegment);
        yield return new WaitForEndOfFrame();
    }

    [ContextMenu("LoadFakeBridge")]
    public IEnumerator LoadFakeBridge(Vector3 position)
    {
        GameObject startSegment = Instantiate(FakeBridge, position, Quaternion.identity, LevelParent.transform);
        ActiveLevels.Add(startSegment);
        yield return new WaitForEndOfFrame();
    }

    public IEnumerator LoadCheckpoint()
    {
        Debug.Log("Loading next level");
        LoadBridge();
        yield return new WaitForSeconds(0.25f);
        LoadNextlevel();
    }

    void LoadNextlevel()
    {
        DetermineLevelDifficulty();

        float length = NextInLine.GetComponent<Renderer>().bounds.size.z;
        float position = NextInLine.transform.position.z + length;
        GameObject nextlevel = Instantiate(Levels, new Vector3(0f, 0f, position), Quaternion.identity, LevelParent.transform);

        ActiveLevels.Add(nextlevel);
        nextlevel.name = LevelNumber.ToString();
        NextInLine = nextlevel;
    }

    [ContextMenu("loadBridge")]
    public void LoadBridge()
    {
        float length = NextInLine.GetComponent<Renderer>().bounds.size.z;
        float position = NextInLine.transform.position.z + length;
        GameObject bridgePart = Instantiate(Bridge, new Vector3(0f, 0f, position), Quaternion.identity, BridgeParent.transform);
        if (ActiveCheckpoint.Count > 0)
        {
            Destroy(ActiveCheckpoint[0]);
            ActiveCheckpoint.Remove(ActiveCheckpoint[0]);
        }
        ActiveCheckpoint.Add(bridgePart);
    }

    void PreloadNext()
    {
        if (ActiveLevels.Count == 1 && !_isNextLevelLoaded)
        {
            float length = ActiveLevels[0].GetComponent<Renderer>().bounds.size.z;
            if ((_mainCamera.transform.position.z + SpawnThreshold) > (ActiveLevels[0].transform.position.z + length))
            {
                _isNextLevelLoaded = true;
                StartCoroutine(LoadCheckpoint());
            }
        }
    }

    public void RemovePreviousLevel()
    {
        if (ActiveLevels.Count > 0)
        {
            for (int i = ActiveLevels.Count - 1; i >= 0; i--)
            {
                float length;
                if (ActiveLevels[i].GetComponent<Renderer>() != null)
                {
                    length = ActiveLevels[i].GetComponent<Renderer>().bounds.size.z;
                }
                else
                {
                    length = ActiveLevels[i].GetComponentInChildren<Renderer>().bounds.size.z;
                }
                if ((_mainCamera.transform.position.z - DestroyThreshold) > (ActiveLevels[i].transform.position.z + length))
                {
                    Destroy(ActiveLevels[i]);
                    ActiveLevels.Remove(ActiveLevels[i]);
                    _isNextLevelLoaded = false;
                }
            }
        }
    }

    public void AddScore(int score)
    {
        PlayerScore += score;
        CurrentScoreValue.text = PlayerScore.ToString();
    }

    public void GameOver()
    {
        PlayerManager.Instance.IsPlaying = false;
        ActiveLevels = new List<GameObject>();
        ActiveCheckpoint = new List<GameObject>();
        string finalScoreString;
        if (PlayerScore > _highscore)
        {
            finalScoreString = "New High Score! : ";
            PlayerPrefs.SetFloat("RiverRaid", PlayerScore);
        }
        else
        {
            finalScoreString = "Score : ";
        }
        GameOverScreen.SetActive(true);
        AudioManager.Instance.CreateAudioBuilder()
            .WithRandomPitch()
            .WithPosition(transform.position)
            .Play(GameOverAudio);
        FinalScoreText.text = finalScoreString + PlayerScore.ToString();
        CurrentScoreValue.text = "";
        EnemySpawner.ClearAllActiveEnemies();
    }

    public void LoseLife()
    {
        PlayerManager.Instance.IsPlaying = false;
        if (LivesCount == 1)
        {
            LivesCount -= 1;
            Debug.Log("Out of Lives");
            GameOver();
        }
        else
        {
            LivesCount -= 1;
            StartCoroutine(ResetAtLastCheckpoint());
        }
        AudioManager.Instance.CreateAudioBuilder()
            .WithRandomPitch()
            .WithPosition(transform.position)
            .Play(LoseLifeAudio);
        Lives.SetInteger("Lives", LivesCount);
    }

    public void CheckPointReached()
    {
        LevelNumberValue.text = LevelNumber.ToString();
        Checkpoint = LevelNumber;
    }

    private IEnumerator ResetAtLastCheckpoint()
    {
        yield return new WaitForSeconds(1f);
        bool fakeStartSegmentRequired = true;
        bool fakeBridgeRequired = false;
        for (int i = 0; i < ActiveLevels.Count; i++)
        {
            int levelNum = int.Parse(ActiveLevels[i].name);
            if (levelNum < Checkpoint)
            {
                fakeStartSegmentRequired = false;
            }
            if(ActiveCheckpoint.Count > 0)
            {
                float distanceToCheckpoint = ActiveCheckpoint[0].transform.position.z - ActiveLevels[i].transform.position.z;
                if (distanceToCheckpoint > 100f)
                {
                    fakeBridgeRequired = true;
                }
            }

            Debug.Log("Checking Level " + ActiveLevels[i].name + " against checkpoint " + Checkpoint.ToString());
            if (ActiveLevels[i].name.Equals(Checkpoint.ToString()))
            {
                NextInLine = ActiveLevels[i];
                Vector3 resetPosition = ActiveLevels[i].transform.position;

                if (Checkpoint % 3 == 0)
                {
                    PlayerManager.Instance.ResetAtLevelStart(new Vector3(resetPosition.x, resetPosition.y, resetPosition.z - 15));
                }
                else
                {
                    PlayerManager.Instance.ResetAtLevelStart(resetPosition);
                }
                EnemySpawner.SetupLevelMesh(ActiveLevels[i]);

                break;
            }
            else
            {
                PlayerManager.Instance.ResetAtLevelStart(Vector3.zero);
                EnemySpawner.SetupLevelMesh(ActiveLevels[0]);
            }
        }

        if (fakeBridgeRequired)
        {
            StartCoroutine(LoadFakeBridge(NextInLine.transform.position));
        }
        if (fakeStartSegmentRequired)
        {
            StartCoroutine(LoadFakeStartSegment(NextInLine.transform.position));            
        }
        StartCoroutine(CountdownStart(2));

    }

    [ContextMenu("Restart Game")]
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [ContextMenu("Return to Menu")]
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    [ContextMenu("Quit Game")]
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public IEnumerator CountdownStart(int seconds)
    {
        int counter = seconds;
        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
            CountdownText.text = counter.ToString();
            if (counter == 0)
            {
                CountdownText.text = "";
            }
        }
        AudioManager.Instance.CreateAudioBuilder()
            .WithRandomPitch()
            .WithPosition(transform.position)
            .Play(StartAudio);
        PlayerManager.Instance.IsPlaying = true;
        IsGameReady = true;
    }
}
