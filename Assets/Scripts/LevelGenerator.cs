using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class LevelGenerator : MonoBehaviour
{
    private Mesh _mesh;
    public MeshFilter MeshFilter;
    private Vector3[] _vertices;
    private Vector2[] _uvs;
    private float _minTerrainHeight;
    private float _maxTerrainHeight;
    private int[] _triangles;

    public int[] RiverProfile;

    public int SlotsPerValue = 5;
    public int MaxWidth = 38;
    public int MinWidth = 4;

    public float Smoothness = 7;
    public bool IsSplitLevel = false;

    public int Width = 90;
    public int Length = 260;
    public float Strength = 0.3f;
    public float Depth = -2f;

    public int AlignmentLength = 20;
    public float AlignmentSmoothness = 30;

    public GameManager Manager;
    public Gradient Gradient;
    public MeshCollider MeshCollider;

    public int Seed;

    public GameObject[] Trees;
    public GameObject LargeHouse;
    public GameObject SmallHouse;
    public int TreeChance = 50;
    public int HouseChance = 50;
    public int SpawnRangeDistance = 10;
    // Start is called before the first frame update
    void Start()
    {
        Manager = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameManager>();
        _mesh = new Mesh();
        MeshFilter = GetComponent<MeshFilter>();
        MeshFilter.mesh = _mesh;

        MeshCollider = GetComponent<MeshCollider>();

        IsSplitLevel = Manager.IsSplitRiver;
        Seed = Manager.currentSeed;
        Random.InitState(Seed);

        CreateShape();
        UpdateMesh();
        GetValidGroundSpawnLocation();
    }

    private void OnValidate()
    {
        if (_mesh != null)
        {
            CreateShape();
            UpdateMesh();
        }
    }

    void CreateShape()
    {
        CreateVertices();
        CreateTriangles();
    }

    void UpdateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uvs;
        MeshCollider.sharedMesh = _mesh;

        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        _mesh.RecalculateTangents();
    }

    public void CreateTriangles()
    {
        int fullLength = Length + (2 * AlignmentLength);
        _triangles = new int[Width * fullLength * 6];
        int vert = 0;
        int tris = 0;

        for (int y = 0; y < fullLength; y++)
        {
            //Plot all the triangles within each quad in a row
            for (int x = 0; x < Width; x++)
            {
                _triangles[tris + 0] = vert + 0;
                _triangles[tris + 1] = vert + Width + 1;
                _triangles[tris + 2] = vert + 1;
                _triangles[tris + 3] = vert + 1;
                _triangles[tris + 4] = vert + Width + 1;
                _triangles[tris + 5] = vert + Width + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    public void CreateVertices()
    {
        GenerateRiverTrend();

        float xOffset = Width / 2;
        _vertices = new Vector3[(Width + 1) * (RiverProfile.Length)];
        _uvs = new Vector2[_vertices.Length];
        //Plot all the vertices using the martices
        for (int i = 0, z = 0; z < RiverProfile.Length; z++)
        {
            for (int x = 0; x <= Width; x++)
            {
                float y = GetYValue(x, z, RiverProfile);
                _vertices[i] = new Vector3(x - xOffset, y, z);
                _uvs[i] = new Vector2((float)x / Width, (float)z / (RiverProfile.Length - 1));

                if (y > _maxTerrainHeight)
                {
                    _maxTerrainHeight = y;
                }
                if (y < _minTerrainHeight)
                {
                    _minTerrainHeight = y;
                }

                i++;
            }
        }
    }

    private float GetYValue(int x, int z, int[] riverProfile)
    {
        float y = 1;
        float halfWidth = (Width / 2f);

        float leftSide = halfWidth - riverProfile[z];
        float rightSide = halfWidth + riverProfile[z];

        leftSide = Mathf.Round(leftSide);
        rightSide = Mathf.Round(rightSide);

        if ((x > leftSide && x < rightSide) || (x == leftSide || x == rightSide))
        {
            y = Depth;
        }
        else if (x == leftSide - 1 || x == rightSide + 1)
        {
            y = Depth + 2;
        }

        y = Mathf.Floor(y);

        bool isAlignment = (z < AlignmentLength || z > (Length));
        if (IsSplitLevel && !isAlignment)
        {
            y = CreateSplit(x, z, y, leftSide, rightSide);
        }

        return y;
    }

    private void GenerateRiverTrend()
    {
        RiverProfile = new int[Length + 1 + (2 * AlignmentLength)];
        int maxSlots = Length / SlotsPerValue;
        int halfWidth = Width / 2;
        float smoothnessAdjusted = Smoothness + Random.Range(-1, 5);
        int riverIndex = 0;

        //River Generation
        for (int z = 0; z < maxSlots; z++)
        {
            float trend = Mathf.PerlinNoise((z * 0.99f) / smoothnessAdjusted, Seed);

            //Defines how many blocks from middle should be water per row
            int trendInt = (int)Mathf.Round((trend * halfWidth) + 5);
            if (trendInt > MaxWidth)
            {
                trendInt = MaxWidth;
            }
            else if (trendInt < MinWidth)
            {
                trendInt = MinWidth;
            }

            // Assign the current value to slotsPerValue consecutive slots in the river array
            for (int j = riverIndex; j < riverIndex + SlotsPerValue; j++)
            {
                RiverProfile[AlignmentLength + j] = trendInt;
            }
            // Move the riverIndex to the next available slots
            riverIndex = (z + 1) * SlotsPerValue;
        }
        int remainder = (RiverProfile.Length % SlotsPerValue);
        for (int j = RiverProfile.Length - AlignmentLength - remainder; j < RiverProfile.Length - AlignmentLength; j++)
        {
            RiverProfile[j] = RiverProfile[j - remainder - 1];
        }

        float startTarget = RiverProfile[Mathf.Min(AlignmentLength + 1, RiverProfile.Length - 1)];
        for (int z = 0; z <= AlignmentLength && z < RiverProfile.Length; z++)
        {
            float t = z / (float)AlignmentLength;
            float smooth = Mathf.SmoothStep(0, 1, t); // smoother gradient
            RiverProfile[z] = Mathf.RoundToInt(Mathf.Lerp(10f, startTarget, smooth));
        }

        // --- Alignment End ---
        float endStart = RiverProfile[Mathf.Max(Length + AlignmentLength - 1, 0)];
        for (int i = 0; i <= AlignmentLength && RiverProfile.Length - 1 - i >= 0; i++)
        {
            float t = i / (float)AlignmentLength;
            float smooth = Mathf.SmoothStep(0, 1, t);
            int z = RiverProfile.Length - 1 - i;
            RiverProfile[z] = Mathf.RoundToInt(Mathf.Lerp(10f, endStart, smooth));
        }

        //int startWidth = RiverProfile[Length + AlignmentLength];
        //int endWidth = RiverProfile[AlignmentLength + 1];
        //for (int z = 0; z <= AlignmentLength; z++)
        //{
        //    float normalised = z / (float)(AlignmentLength);
        //    float smooth = Mathf.SmoothStep(0, 1, normalised);

        //    //Align Start
        //    RiverProfile[z] = Mathf.RoundToInt(Mathf.Lerp(10, endWidth, smooth));

        //    //Align End
        //    RiverProfile[Length + AlignmentLength + z] = Mathf.RoundToInt(Mathf.Lerp(startWidth, 10, smooth));
        //}
    }

    private float CreateSplit(int x, int z, float y, float leftSide, float rightSide)
    {
        float splitWidth = 15;
        if (RiverProfile[z] >= 20)
        {
            if (x > leftSide + splitWidth && x < rightSide - splitWidth)
            {
                y = 1;

                if (z == 0)
                {
                    y = Depth;
                }
            }
        }
        return y;
    }

    public void GetValidGroundSpawnLocation()
    {
        Vector3[] _newVertices = new Vector3[_vertices.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            _newVertices[i] = MeshFilter.transform.TransformPoint(_vertices[i]);
        }

        int spawnCount = 0;
        float groundLevel = Depth + 2;
        int currentSpawnRangeDistance = 0;
        Debug.Log($"newVertices size: {_newVertices.Length}, ground level: {groundLevel}");
        //every 50units, spawn enemies from 25units to 75units away(50units)
        for (int i = 0; i < _newVertices.Length; i += 4)
        {
            if (_newVertices[i].y > groundLevel)
            {
                bool spawnedHere = false;
                Vector3 position = new(_newVertices[i].x, _newVertices[i].y, _newVertices[i].z);
                if (5 == Random.Range(0, HouseChance) && currentSpawnRangeDistance <= 0 && IsGroundUnderneath(position))
                {
                    int houseType = Random.Range(0, 2);
                    GameObject houseToSpawn = (houseType == 0) ? LargeHouse : SmallHouse;
                    Instantiate(houseToSpawn, position, Quaternion.identity, transform);
                    spawnedHere = true;
                    spawnCount++;
                    currentSpawnRangeDistance = SpawnRangeDistance;
                }
                else
                {
                    currentSpawnRangeDistance--;
                }

                if (!spawnedHere)
                {
                    int treeIndex = Random.Range(0, Trees.Length);

                    if (5 == Random.Range(0, TreeChance) && currentSpawnRangeDistance <= 0 && IsGroundUnderneath(position))
                    {
                        Instantiate(Trees[treeIndex], position, Quaternion.identity, transform);
                        spawnCount++;
                        currentSpawnRangeDistance = SpawnRangeDistance;
                    }
                    else
                    {
                        currentSpawnRangeDistance--;
                    }
                }
            }
        }
        Debug.Log($"Spawned {spawnCount} Trees and Houses");
    }

    public bool IsGroundUnderneath(Vector3 position)
    {
        Vector3 castPosition = new(position.x, position.y + 5f, position.z);
        Physics.Raycast(castPosition, Vector3.down, out RaycastHit hitInfo, 5.1f, LayerMask.GetMask("Collision"), QueryTriggerInteraction.Ignore);
        if (!hitInfo.collider.CompareTag("Bridge"))
        {
            return true;
        }

        return false;
    }

}
