using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RiverAlignment : MonoBehaviour
{
    private Mesh _mesh;
    private Vector3[] _vertices;
    private Vector2[] _uvs;
    private int[] _triangles;

    public int[] Alignment;
    public int BridgeWidth = 10;
    public int XSize = 90;
    public int ZSize = 20;
    public float Strength = 0.3f;
    public float Depth = -3f;
    public float Smoothness = 30;

    public bool IntoBridge;
    public int StartWidth = 0;
    public int EndWidth = 0;
    public float Slope = 0;

    private LevelGenerator _level;
    private GameManager _manager;
    public Gradient Gradient;
    public MeshCollider MeshCollider;

    void Start()
    {
        _manager = GameObject.FindGameObjectWithTag("Logic").GetComponent<GameManager>();
        _level = GameObject.FindGameObjectWithTag("Level").transform.GetChild(0).GetComponent<LevelGenerator>();
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        MeshCollider = GetComponent<MeshCollider>();
        IntoBridge = _manager.IsIntoBridge;

        CreateShape();
        UpdateMesh();
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
        _triangles = new int[XSize * ZSize * 6];
        int vert = 0;
        int tris = 0;

        for (int y = 0; y < ZSize; y++)
        {
            //Plot all the triangles within each quad in a row
            for (int x = 0; x < XSize; x++)
            {
                _triangles[tris + 0] = vert + 0;
                _triangles[tris + 1] = vert + XSize + 1;
                _triangles[tris + 2] = vert + 1;
                _triangles[tris + 3] = vert + 1;
                _triangles[tris + 4] = vert + XSize + 1;
                _triangles[tris + 5] = vert + XSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    private void CreateVertices()
    {
        _vertices = new Vector3[(XSize + 1) * (ZSize + 1)];
        _uvs = new Vector2[_vertices.Length];
        float xOffset = XSize / 2;
        AlignRiver();

        //Plot all the vertices using the martices
        for (int i = 0, z = 0; z <= ZSize; z++)
        {
            for (int x = 0; x <= XSize; x++)
            {
                float y = GetYValue(x, z, XSize);
                _vertices[i] = new Vector3(x - xOffset, y, z);
                _uvs[i] = new Vector2((float)x / XSize, (float)z / ZSize);
                i++;
            }
        }
    }

    private float GetYValue(int x, int z, float xSize)
    {
        float y = 1;
        float halfWidth = (xSize / 2);

        float leftside = halfWidth - (Alignment[z]);
        float rightside = halfWidth + (Alignment[z]);

        if (x > leftside && x < rightside)
        {
            y = Depth;
        }
        else if (x == leftside || x == rightside)
        {
            y = Depth + 1;
        }
        else if (x == leftside + 1 || x == rightside + 1)
        {
            y = Depth + 2;
        }

        return y;
    }

    private void AlignRiver()
    {
        Alignment = new int[ZSize + 1];
        int halfWidth = XSize / 2;

        if (IntoBridge)
        {
            if (_level != null)
            {
                StartWidth = _level.RiverProfile[_level.RiverProfile.Length - 1];
            }
            else
            {
                StartWidth = BridgeWidth;
            }

            EndWidth = BridgeWidth;
        }
        //else if (GameManager.Instance.ForCheckpoint)
        //{
        //    _level = GameObject.FindGameObjectWithTag("Level").transform.GetChild(0).GetComponent<LevelGenerator>();
        //    StartWidth = BridgeWidth;
        //    EndWidth = _level.RiverProfile[0];
        //}
        else
        {
            _level = GameObject.FindGameObjectWithTag("Level").transform.GetChild(1).GetComponent<LevelGenerator>();
            StartWidth = BridgeWidth;
            EndWidth = _level.RiverProfile[0];
        }
        Slope = (StartWidth - EndWidth) / ZSize;
        for (int z = 0; z <= ZSize; z++)
        {
            // Calculate the interpolation factor (between 0 and 1)
            float normalised = z / (float)(ZSize);

            // Use linear interpolation to smoothly transition from startwidth to endwidth
            Alignment[z] = (int)Mathf.Round(Mathf.Lerp(StartWidth, EndWidth, normalised));
        }
    }
}
