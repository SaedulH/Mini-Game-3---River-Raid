using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class StartSegment : MonoBehaviour
{
    private Mesh _mesh;
    public MeshFilter MeshFilter;
    private Vector3[] _vertices;
    private Vector2[] _uvs;
    private int[] _triangles;

    public int RiverWidth = 10;
    public int Width = 90;
    public int Length = 50;
    public float Depth = -2f;

    public MeshCollider MeshCollider;
    public GameObject[] Trees;
    public GameObject LargeHouse;
    public GameObject SmallHouse;
    public int TreeChance = 50;
    public int HouseChance = 50;
    public int SpawnRangeDistance = 10;
    void Start()
    {
        _mesh = new Mesh();
        MeshFilter = GetComponent<MeshFilter>();
        MeshFilter.mesh = _mesh;
        MeshCollider = GetComponent<MeshCollider>();

        CreateShape();
        UpdateMesh();
        GetValidGroundSpawnLocation();
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

        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        _mesh.RecalculateTangents();

        MeshCollider.sharedMesh = _mesh;
    }
    private void CreateTriangles()
    {
        _triangles = new int[Width * Length * 6];
        int vert = 0;
        int tris = 0;

        for (int y = 0; y < Length; y++)
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

    private void CreateVertices()
    {
        _vertices = new Vector3[(Width + 1) * (Length + 1)];
        _uvs = new Vector2[_vertices.Length];
        float xOffset = Width / 2; ;

        //Plot all the vertices using the martices
        for (int i = 0, z = 0; z <= Length; z++)
        {
            for (int x = 0; x <= Width; x++)
            {
                float y = GetYValue(x, z);
                _vertices[i] = new Vector3(x - xOffset, y, z);
                _uvs[i] = new Vector2((float)x / Width, (float)z / Length);
                i++;
            }
        }
    }

    private float GetYValue(int x, int z)
    {
        float y = 1;
        float halfWidth = (Width / 2);

        float leftSide = halfWidth - (RiverWidth);
        float rightSide = halfWidth + (RiverWidth);

        if ((x > leftSide && x < rightSide) || (x == leftSide || x == rightSide))
        {
            y = Depth;
        }
        else if (x == leftSide - 1 || x == rightSide + 1)
        {
            y = Depth + 2;
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
