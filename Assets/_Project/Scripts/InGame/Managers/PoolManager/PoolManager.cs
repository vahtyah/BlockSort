using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class Pool
{
    private static PoolManager manager;
    private readonly Queue<GameObject> availableObjects = new Queue<GameObject>();
    private readonly HashSet<GameObject> availableSeenObjects = new HashSet<GameObject>();
    private Transform parentTransform;

    private Pool(Transform parent)
    {
        this.parentTransform = parent;
    }

    private static Pool Register(GameObject prefab, Transform parentTransform = null, int initialSize = 0)
    {
        EnsureManagerExists();

        parentTransform ??= manager.transform;
        var pool = new Pool(parentTransform);

        for (var i = 0; i < initialSize; i++)
        {
            var obj = Object.Instantiate(prefab, parentTransform);
            obj.SetActive(false);
            pool.availableObjects.Enqueue(obj);
            pool.availableSeenObjects.Add(obj);
        }

        return manager.RegisterPool(prefab, pool);
    }

    public static GameObject Spawn(GameObject prefab)
    {
        return Spawn(prefab, Vector3.zero, Quaternion.identity);
    }

    public static GameObject Spawn(GameObject prefab, Transform parent)
    {
        return Spawn(prefab, Vector3.zero, Quaternion.identity, parent);
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation = default,
        Transform parent = null)
    {
        EnsureManagerExists();

        var pool = manager.GetPool(prefab) ?? Register(prefab, parent);

        GameObject obj;

        if (pool.availableObjects.Count > 0)
        {
            obj = pool.availableObjects.Dequeue();
            pool.availableSeenObjects.Remove(obj);
            if (parent != null) obj.transform.SetParent(parent, false);
            obj.transform.SetPositionAndRotation(position, rotation);
        }
        else
        {
            obj = Object.Instantiate(prefab, position, rotation, parent ?? pool.parentTransform);
        }

        manager.TrackObject(obj, pool);
        obj.SetActive(true);
        return obj;
    }

    private void Return(GameObject obj)
    {
        EnsureManagerExists();

        if (availableSeenObjects.Contains(obj))
            return;
        
        obj.SetActive(false);
        
        obj.transform.SetParent(parentTransform, false);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        availableObjects.Enqueue(obj);
        availableSeenObjects.Add(obj);
    }

    public static void Despawn(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("Attempted to despawn null object.");
            return;
        }
        
        EnsureManagerExists();

        var pool = manager.UntrackObject(obj);
        if (pool != null)
        {
            pool.Return(obj);
        }
        else
        {
            Debug.LogWarning($"Object {obj.name} doesn't belong to any pool.  Destroying instead.");
            Object.Destroy(obj);
        }
    }
    
    public void Clear()
    {
        while (availableObjects.Count > 0)
        {
            var obj = availableObjects.Dequeue();
            availableSeenObjects.Remove(obj);
            if (obj != null)
            {
                manager.UntrackObject(obj);
                Object.Destroy(obj);
            }
        }
    }

    private static void EnsureManagerExists()
    {
        if (manager == null)
            manager = PoolManager.Instance ?? new GameObject("PoolManager").AddComponent<PoolManager>();
    }
}

public class PoolManager : Singleton<PoolManager>
{
    private readonly Dictionary<GameObject, Pool> prefabToPool = new();
    private readonly Dictionary<GameObject, Pool> instanceToPool = new();

    public Pool GetPool(GameObject prefab) => prefabToPool.GetValueOrDefault(prefab);

    public void TrackObject(GameObject obj, Pool pool) => instanceToPool[obj] = pool;

    public Pool UntrackObject(GameObject obj) =>
        !instanceToPool.Remove(obj, out var pool) ? null : pool;

    public Pool RegisterPool(GameObject prefab, Pool pool)
    {
        prefabToPool.TryAdd(prefab, pool);
        return prefabToPool[prefab];
    }
    
    public void ClearAllPools()
    {
        foreach (var pool in prefabToPool.Values)
        {
            pool.Clear();
        }
        prefabToPool.Clear();
        instanceToPool.Clear();
    }
    
    protected override void OnDestroy()
    {
        ClearAllPools();
        base.OnDestroy();
    }
}