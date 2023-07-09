using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    //// Singleton ////
    public static ObjectPool Instance;

    [SerializeField] private GameObject CanvasObject;
    
    //// Pool structs /////
    [System.Serializable]
    public struct Pool
    {
        // Trash tag is the name of the tag //
        [InspectorName("Prefab")]public GameObject prefab;
        [CanBeNull] public string soundName;
        public int prefabsCount;
    }
    [Space(10)]
    
    //// Pools ////
    public List<Pool> objectPools;
    private Dictionary<string, Queue<GameObject>> objectPoolsDict;


    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        Initialize();
    }

    public void Initialize(Action callback = null)
    {
        //// Initialize regular object pool ////
        objectPoolsDict = new Dictionary<string, Queue<GameObject>>();
        foreach(Pool pool in objectPools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for(int i = 0; i < pool.prefabsCount; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);               
            }
            
            objectPoolsDict.Add(pool.prefab.name, objectPool);
        }
        
        callback?.Invoke();
    }

    public GameObject SpawnFromPoolObject(string poolName, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var objectToSpawn = SpawnFromPool(poolName);
        
        if (parent != null) objectToSpawn.transform.parent = parent;
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    public GameObject SpawnFromPoolUI(string poolName)
    {
        var objectToSpawn = SpawnFromPool(poolName);

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.parent = CanvasObject.transform;
        

        return objectToSpawn;
    }
    
    private GameObject SpawnFromPool(string poolName)
    {
        if (objectPoolsDict == null || objectPoolsDict.Count == 0)
        {
            Debug.LogWarning("Object pool is empty!");
            return null;
        }
        
        if (!objectPoolsDict.ContainsKey(poolName))
        {
            Debug.LogWarning("Warning!" + poolName + "doesn`t exist");
            return null;
        }

        GameObject objectToSpawn = objectPoolsDict[poolName].Dequeue();
        
        if (objectToSpawn.activeSelf)
            objectToSpawn = Instantiate(objectToSpawn);

        objectToSpawn.SetActive(true);

        objectPoolsDict[poolName].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}