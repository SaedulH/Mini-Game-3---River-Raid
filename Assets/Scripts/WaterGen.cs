using UnityEngine;

public class WaterGen : MonoBehaviour
{

    [SerializeField] private GameObject[] water;
    [SerializeField] private GameObject waterPrefab;
    public GameObject player;

    public float choke = 0;
    public float destroyThreshold = 10f;
    public float objectLength;
    public int childsNeeded;
    
    public float waterLength;
    private bool waterLoaded = false;

    // Start is called before the first frame update
    void Start()
    {

        LoadChildObjects(waterPrefab);
        waterLoaded = true;
    }

    void LoadChildObjects(GameObject obj)
    {
        water = new GameObject[2];
        waterLength = obj.GetComponent<MeshRenderer>().bounds.size.z;

        GameObject clone = Instantiate(obj) as GameObject;
        for (int i = 0; i <= 1; i++)
        {
            GameObject c = Instantiate(clone) as GameObject;
            c.transform.SetParent(transform);
            c.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, waterLength * i);
            c.name = obj.name + i;
            water[i] = c;
        }
        Destroy(clone);
    }

    void LateUpdate()
    {
        if (waterLoaded)
        {

            for (int i = 0; i < water.Length; i++)
            {
                float zPos = water[i].transform.position.z;

                if (player.transform.position.z >= (zPos + waterLength))
                {
                    // Move the plane to the front
                    water[i].transform.position += waterLength * 2 * Vector3.forward;
                }
            }
        }
    }
}

