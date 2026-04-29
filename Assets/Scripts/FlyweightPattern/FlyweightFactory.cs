using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Utilities
{
    public class FlyweightFactory : NonPersistentSingleton<FlyweightFactory>
    {
        public static Dictionary<string, PooledObjectInfo> ObjectPools = new();
        private static Dictionary<FlyweightType, GameObject> poolParents = new();
        //[SerializeField] bool collectionCheck = true;
        //[SerializeField] int defaultCapacity = 10;
        //[SerializeField] int maxPoolSize = 100;

        protected override void Awake()
        {
            base.Awake();
            SetupParentObjects();
        }

        public static async Task ResetPools()
        {
            ObjectPools.Clear();
            if(poolParents == null || poolParents.Count == 0)
            {
                SetupParentObjects();
                await Task.CompletedTask;
                return;
            }

            GameObject objectPoolParent = poolParents.Values.FirstOrDefault().transform.parent.gameObject;
            foreach (GameObject parent in poolParents.Values)
            {
                Destroy(parent);
            }
            Destroy(objectPoolParent);
            poolParents.Clear();
            SetupParentObjects();

            await Task.CompletedTask;
        }

        public static void SetupParentObjects()
        {
            GameObject poolParent = new("Object Pools");
            poolParents = new Dictionary<FlyweightType, GameObject>();

            FlyweightType[] flyweightTypes = (FlyweightType[])Enum.GetValues(typeof(FlyweightType));
            foreach (var type in flyweightTypes)
            {
                if (!poolParents.ContainsKey(type))
                {
                    GameObject typeParent = new(type.ToString());
                    typeParent.name = type.ToString();
                    typeParent.transform.parent = poolParent.transform;
                    poolParents.Add(type, typeParent);
                }
            }
        }

        public static void InitialiseObject(int spawnCount, FlyweightSettings settings, bool isUnique)
        {
            List<GameObject> spawnObjs = new();
            Vector3 offScreenPos = new(0, 0, -20f);
            for (int i = 0; i < spawnCount; i++)
            {
                GameObject obj = SpawnObject(settings, offScreenPos, Quaternion.identity);
                spawnObjs.Add(obj);
            }

            foreach (GameObject obj in spawnObjs)
            {
                ReturnToPool(obj);
            }
        }

        public static GameObject SpawnObject(FlyweightSettings settings, Vector3 spawnPosition, Quaternion spawnRotaion)
        {
            PooledObjectInfo pool;

            string objName = settings.name;
            //if the pool doesn't exist, create it
            if (ObjectPools.TryGetValue(objName, out _))
            {
                pool = ObjectPools[objName];
            }
            else
            {
                pool = new PooledObjectInfo() { LookupString = objName };
                ObjectPools.Add(objName, pool);
            }

            GameObject spawnableObj = null;

            //Check if there are any inactive objects in pool
            if (spawnableObj == null)
            {
                spawnableObj = pool.InactiveObjects.FirstOrDefault();
                if (spawnableObj == null)
                {
                    GameObject parentObject = GetParentObject(settings.Type);
                    //if there are no inactive objects, create a new one
                    spawnableObj = Instantiate(settings.Prefab, spawnPosition, spawnRotaion);
                    spawnableObj.name = objName;
                    settings.Initialise(spawnableObj);
                    if (parentObject != null)
                    {
                        spawnableObj.transform.SetParent(parentObject.transform);
                    }
                }
                else
                {
                    //if there is an inactive object, reactivate it
                    pool.InactiveObjects.Remove(spawnableObj);
                }

            }

            spawnableObj.transform.SetPositionAndRotation(spawnPosition, spawnRotaion);
            if (spawnableObj.TryGetComponent<Rigidbody>(out var rb) && rb.isKinematic)
            {
                rb.position = spawnPosition;
                rb.rotation = spawnRotaion;
            }
            spawnableObj.SetActive(true);

            if (!pool.ActiveObjects.Contains(spawnableObj))
            {
                pool.ActiveObjects.Add(spawnableObj);
            }


            return spawnableObj;
        }

        private static GameObject GetParentObject(FlyweightType type)
        {
            if (poolParents.TryGetValue(type, out GameObject parent))
            {
                return parent;
            }
            else
            {
                GameObject newParent = new(type.ToString());
                poolParents.Add(type, newParent);

                return newParent;
            }
        }

        public static void SetParentObject(FlyweightType type, GameObject obj)
        {
            if (poolParents.TryGetValue(type, out GameObject parent))
            {
                obj.transform.parent = parent.transform;
            }
            else
            {
                GameObject newParent = new(type.ToString());
                poolParents.Add(type, newParent);

                obj.transform.parent = newParent.transform;
            }
        }

        public static void ReturnToPool(GameObject obj)
        {
            if (ObjectPools.TryGetValue(obj.name, out PooledObjectInfo pool))
            {
                obj.SetActive(false);
                if (!pool.InactiveObjects.Contains(obj))
                {
                    //Debug.LogWarning($"adding {obj.name} to inactiveObjects");
                    pool.InactiveObjects.Add(obj);
                }
                if (pool.ActiveObjects.Contains(obj))
                {
                    pool.ActiveObjects.Remove(obj);
                }
            }
            else
            {
                Debug.LogWarning("Trying to release an object that's not pooled: " + obj.name);
                Destroy(obj);
            }
        }

    }
}

