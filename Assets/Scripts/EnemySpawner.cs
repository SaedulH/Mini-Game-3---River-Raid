using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    public List<GameObject> AllActiveEnemies;
    public GameObject[] Enemies;
    public GameObject Jet;
    public GameObject Fuel;
    public GameManager GameManager;
    public GameObject LevelsParent;
    public GameObject Player;
    public PlayerManager PlayerManager;

    public int StartingFuelChance = 200;
    public int StartingSpawnChance = 125;

    public int FuelChance = 125;
    public int SpawnChance = 125;
    public float SpawnRange = 50;
    public float SpawnIntervals = 0;
    public float JetRate = 5;
    private float _timer = 0;
    private float _side = 50;
    public float Distance = 0;
    private float _lastPosition = -25;
    private bool _canSpawn = false;

    public int levelnum;

    private Vector3[] _vertices;
    private Vector3[] _newVertices;
    public MeshFilter MeshFilter;
    public Mesh CurrentMesh;

    public bool FirstStage = true;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        GameManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameManager>();
        Player = GameObject.FindGameObjectWithTag("Player");
        PlayerManager = Player.GetComponent<PlayerManager>();
        LevelsParent = GameObject.FindGameObjectWithTag("Level");
        SpawnChance = StartingSpawnChance;
        FuelChance = StartingFuelChance;
        _canSpawn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!_canSpawn)
        {
            return;
        }

        if (PlayerManager.IsPlayerAlive)
        {
            SpawnJet();
            ReadyToSpawn();
        }
    }

    public void SetupLevelMesh(GameObject activeLevel)
    {
        ClearAllActiveEnemies();

        if (activeLevel != null)
        {
            MeshFilter = activeLevel.GetComponent<MeshFilter>();
            CurrentMesh = MeshFilter.mesh;
            SpawnIntervals = 0;
            _canSpawn = true;
        }
    }

    public void ClearAllActiveEnemies()
    {
        if(AllActiveEnemies.Count > 0)
        {
            for (int i = AllActiveEnemies.Count - 1; i >= 0; i--)
            {
                if (AllActiveEnemies[i].TryGetComponent<EnemyScript>(out EnemyScript enemy))
                {
                    StartCoroutine(enemy.OnDestroyEvent());
                }
                else if (AllActiveEnemies[i].TryGetComponent<JetScript>(out JetScript jet))
                {
                    StartCoroutine(jet.OnDestroyEvent());
                }
                else if (AllActiveEnemies[i].TryGetComponent<FuelScript>(out FuelScript fuel))
                {
                    StartCoroutine(fuel.OnDestroyEvent());
                }
            }
        }
        AllActiveEnemies.Clear();
        _canSpawn = false;
    }

    public void ReadyToSpawn()
    {
        if (SpawnIntervals == 0)
        {
            StartCoroutine(GetValidSpawnLocation(25, SpawnRange));
            SpawnIntervals = 50;
            Distance = 0;
        }

        Distance = (Player.transform.position.z - _lastPosition);
        if (Distance >= SpawnIntervals)
        {
            _lastPosition = Player.transform.position.z;
            Distance = 0;

            StartCoroutine(GetValidSpawnLocation(SpawnRange, SpawnRange));
        }
    }

    public IEnumerator GetValidSpawnLocation(float minSpawnRange, float maxSpawnRange)
    {
        //Debug.Log($"Spawning Enemies from {minSpawnRange} to {maxSpawnRange}");

        //max spawn freq is 1/50
        if (SpawnChance < 50)
        {
            SpawnChance = 50;
        }
        MeshFilter = GameManager.ActiveLevels[0].GetComponent<MeshFilter>();
        _vertices = MeshFilter.sharedMesh.vertices;
        _newVertices = new Vector3[_vertices.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            _newVertices[i] = MeshFilter.transform.TransformPoint(_vertices[i]);
        }

        float minSpawnZ = Player.transform.position.z + minSpawnRange;
        float maxSpawnZ = Player.transform.position.z + (minSpawnRange + maxSpawnRange);

        int spawnCount = 0;
        int fuelSpawnedOnThisRow = 0;
        int enemySpawnedOnThisRow = 0;
        float rowNumber = -1;
        //every 50units, spawn enemies from 25units to 75units away(50units)
        for (int i = 0; i < _newVertices.Length; i += 4)
        {
            if (rowNumber != _newVertices[i].z / 5)
            {
                rowNumber = _newVertices[i].z;
                fuelSpawnedOnThisRow = 0;
                enemySpawnedOnThisRow = 0;
            }
            float vertexZ = _newVertices[i].z;

            if (vertexZ > minSpawnZ && vertexZ < maxSpawnZ && _newVertices[i].y == -2f)
            {
                bool spawnedHere = false;
                if (5 == Random.Range(0, SpawnChance) && enemySpawnedOnThisRow < 4)
                {
                    StartCoroutine(SpawnRandomEnemy(new Vector3(_newVertices[i].x, _newVertices[i].y + 1, _newVertices[i].z)));
                    spawnedHere = true;
                    enemySpawnedOnThisRow++;
                    spawnCount++;
                    yield return new WaitForEndOfFrame();
                }

                if (!spawnedHere)
                {
                    if (5 == Random.Range(0, FuelChance) && fuelSpawnedOnThisRow < 3)
                    {
                        Instantiate(Fuel, new Vector3(_newVertices[i].x, _newVertices[i].y + 2, _newVertices[i].z), Quaternion.identity, transform);
                        fuelSpawnedOnThisRow++;
                        yield return new WaitForEndOfFrame();
                    }
                }
            }
        }
        Debug.Log($"Spawned {spawnCount} enemies");
    }

    public void AdjustFrequencies(int levelnumber)
    {
        levelnum = levelnumber;
        SpawnChance = StartingSpawnChance - (2 * levelnum);
        FuelChance = StartingFuelChance + (4 * levelnum);
    }


    [ContextMenu("SpawnEnemy")]
    public void SpawnEnemy(GameObject enemy, Vector3 position)
    {
        GameObject enemyPrefab = Instantiate(enemy, position, Quaternion.identity, transform);
        AllActiveEnemies.Add(enemyPrefab);
    }

    [ContextMenu("SpawnRandomEnemy")]
    public IEnumerator SpawnRandomEnemy(Vector3 position)
    {
        int listLength = Enemies.Length;
        int selection = Random.Range(0, listLength);
        SpawnEnemy(Enemies[selection], position);
        yield return null;
    }

    [ContextMenu("SpawnFuelDepot")]
    public IEnumerator SpawnFuelDepot(Vector3 position)
    {
        Instantiate(Fuel, position, Quaternion.identity, transform);
        yield return null;
    }

    [ContextMenu("SpawnJet")]
    public void SpawnJet()
    {
        _timer += Time.deltaTime;
        if (_timer >= JetRate)
        {
            //Debug.Log("Spawning Jet!");

            _timer = 0;
            bool leftSide = (Random.value > 0.5);
            if (leftSide)
            {
                _side *= -1;
            }
            int distanceOffset = Random.Range(15, 50);
            float height = Player.transform.position.z + distanceOffset;
            SpawnEnemy(Jet, new Vector3(_side, 2, height));
        }
    }

    public void RemoveFromActiveEnemies(GameObject enemy)
    {
        if (AllActiveEnemies.Contains(enemy))
        {
            AllActiveEnemies.Remove(enemy);
        }
    }
}
