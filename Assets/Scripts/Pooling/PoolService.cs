using System.Collections.Generic;
using UnityEngine;

namespace BaseServices.PoolServices
{
    public class PoolService : MonoBehaviour, IPoolService
    {
        [SerializeField] private PoolServiceSettings settings;

        // All the currently active pools in the scene
        public List<Pool> AllPools = new List<Pool>();

        // The reference between a spawned GameObject and its pool
        public Dictionary<GameObject, Pool> AllLinks = new Dictionary<GameObject, Pool>();

        private Transform poolsTransform = null;

        public PoolServiceSettings Settings => settings;
        public static PoolService Instance { get; private set; }

        private void Start()
        {
            Instance = this;

            poolsTransform = new GameObject("Pools").transform;
            settings.InitializeDefinedPools();
        }

        private void Update()
        {
            foreach (var pool in AllPools)
            {
                // Go through all marked objects
                for (var i = pool.delayedDestructions.Count - 1; i >= 0; i--)
                {
                    var markedObject = pool.delayedDestructions[i];

                    // Is it still valid?
                    if (markedObject.Clone != null)
                    {
                        // Age it
                        markedObject.Life -= Time.deltaTime;

                        // Dead?
                        if (markedObject.Life <= 0.0f)
                        {
                            RemoveDelayedDestruction(pool, i);

                            // Despawn it
                            Despawn(markedObject.Clone);
                        }
                    }
                    else
                    {
                        RemoveDelayedDestruction(pool,i);
                    }
                }
            }
        }

        private void RemoveDelayedDestruction(Pool pool, int index)
        {
            var delayedDestruction = pool.delayedDestructions[index];

            pool.delayedDestructions.RemoveAt(index);

            ClassPool<DelayedDestruction>.Despawn(delayedDestruction);
        }

        public Pool InitializePool(GameObject prefab, int preload)
        {
            return InitializePool(prefab, preload, settings.defaultPoolCapacity);
        }

        public Pool InitializePool(GameObject prefab, int preload, int capacity)
        {
            // Find the pool that handles this prefab
            var pool = AllPools.Find(p => p.Prefab == prefab);

            // Create a new pool for this prefab?
            if (pool == null)
            {
                pool = new Pool(prefab, new GameObject(prefab.name + " Pool").transform, capacity, preload);

                pool.PoolParent.transform.SetParent(this.poolsTransform);

                // Add new pool to AllPools list
                AllPools.Add(pool);
            }

            return pool;
        }

        // These methods allows you to spawn prefabs via Component with varying levels of transform data
        public T Spawn<T>(T prefab)
            where T : Component
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity, null);
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation)
            where T : Component
        {
            return Spawn(prefab, position, rotation, null);
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent)
            where T : Component
        {
            // Clone this prefabs's GameObject
            var gameObject = prefab != null ? prefab.gameObject : null;
            var clone = Spawn(gameObject, position, rotation, parent);

            // Return the same component from the clone
            return clone != null ? clone.GetComponent<T>() : null;
        }

        // These methods allows you to spawn prefabs via GameObject with varying levels of transform data
        public GameObject Spawn(GameObject prefab)
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity, null);
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Spawn(prefab, position, rotation, null);
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (prefab != null)
            {
                var pool = InitializePool(prefab, settings.defaultPoolPreload);

                // Spawn a clone from this pool
                var clone = pool.FastSpawn(position, rotation, parent);

                // Was a clone created?
                // NOTE: This will be null if the pool's capacity has been reached
                if (clone != null)
                {
                    // Associate this clone with this pool
                    AllLinks.Add(clone, pool);

                    // Return the clone
                    return clone.gameObject;
                }
            }
            else if (settings.debugLog)
            {
                Debug.LogWarning("Attempting to spawn a null prefab");
            }

            return null;
        }

        // This allows you to despawn a clone via Component, with optional delay
        public void Despawn(Component clone, float delay = 0.0f)
        {
            if (clone != null)
                Despawn(clone.gameObject, delay);
        }

        // This allows you to despawn a clone via GameObject, with optional delay
        public void Despawn(GameObject clone, float delay = 0.0f)
        {
            if (clone != null)
            {
                Pool pool;

                // Try and find the pool associated with this clone
                if (AllLinks.TryGetValue(clone, out pool) == true)
                {
                    if (delay == 0)
                    {
                        // Remove the association
                        AllLinks.Remove(clone);
                    }

                    // Despawn it
                    pool.FastDespawn(clone, delay);
                }
                else
                {
                    if (settings.debugLog)
                        Debug.LogWarning("Attempting to despawn " + clone.name + ", but failed to find pool for it! Make sure you created it using PoolService.Spawn!");

                    // Fall back to normal destroying
                    GameObject.Destroy(clone);
                }
            }
            else
            {
                //Debug.LogError("Attempting to despawn a null clone");
            }
        }
    }

    public class DelayedDestruction
    {
        public GameObject Clone;

        public float Life;
    }

    public class Pool
    {
        public GameObject Prefab;

        public int Capacity;

        // All the currently cached prefab instances
        public List<GameObject> cache = new List<GameObject>();

        // All the delayed destruction objects
        public List<DelayedDestruction> delayedDestructions = new List<DelayedDestruction>();

        // The total amount of created prefabs
        public int total;

        public Transform PoolParent;

        // Returns the total amount of spawned clones
        public int Total
        {
            get
            {
                return total;
            }
        }

        // Returns the amount of cached clones
        public int Cached
        {
            get
            {
                return cache.Count;
            }
        }

        public Pool(GameObject prefab, Transform poolParent)
        {
            this.Prefab = prefab;
            this.PoolParent = poolParent;
        }

        public Pool(GameObject prefab, Transform poolParent, int capacity)
        {
            this.Prefab = prefab;
            this.PoolParent = poolParent;
            this.Capacity = capacity;
        }

        public Pool(GameObject prefab, Transform poolParent, int capacity, int preload)
        {
            this.Prefab = prefab;
            this.PoolParent = poolParent;
            this.Capacity = capacity;

            if (Prefab != null)
            {
                for (var i = total; i < preload; i++)
                {
                    FastPreload();
                }
            }
        }

        // This will return a clone from the cache, or create a new instance
        public GameObject FastSpawn(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (Prefab != null)
            {
                // Attempt to spawn from the cache
                while (cache.Count > 0)
                {
                    // Get last cache entry
                    var index = cache.Count - 1;
                    var clone = cache[index];

                    // Remove cache entry
                    cache.RemoveAt(index);

                    if (clone != null)
                    {
                        // Update transform of clone
                        var cloneTransform = clone.transform;

                        cloneTransform.localPosition = position;

                        cloneTransform.localRotation = rotation;

                        cloneTransform.SetParent(parent, false);

                        // Activate clone
                        clone.SetActive(true);

                        // Messages?
                        SendNotification(clone, "OnSpawn");

                        return clone;
                    }
                    else if (PoolService.Instance.Settings.debugLog)
                    {
                        Debug.LogWarning("The " + PoolParent.name + " contained a null cache entry");
                    }
                }

                // Make a new clone?
                if (Capacity <= 0 || total < Capacity)
                {
                    var clone = FastClone(position, rotation, parent);

                    // Messages?
                    SendNotification(clone, "OnSpawn");

                    return clone;
                }
            }
            else
            {
                if (PoolService.Instance.Settings.debugLog)
                    Debug.LogWarning("Attempting to spawn null");
            }

            return null;
        }

        // Returns a clone of the prefab and increments the total
        // NOTE: Prefab is assumed to exist
        private GameObject FastClone(Vector3 position, Quaternion rotation, Transform parent)
        {
            var clone = GameObject.Instantiate(Prefab, position, rotation);

            total += 1;

            clone.name = Prefab.name + " " + total;

            clone.transform.SetParent(parent, false);

            return clone;
        }

        // This will despawn a clone and add it to the cache
        public void FastDespawn(GameObject clone, float delay = 0.0f)
        {
            if (clone != null)
            {
                // Delay the despawn?
                if (delay > 0.0f)
                {
                    // Make sure we only add it to the marked object list once
                    if (delayedDestructions.Exists(m => m.Clone == clone) == false)
                    {
                        var delayedDestruction = ClassPool<DelayedDestruction>.Spawn() ?? new DelayedDestruction();

                        delayedDestruction.Clone = clone;
                        delayedDestruction.Life = delay;

                        delayedDestructions.Add(delayedDestruction);
                    }
                }
                // Despawn now?
                else
                {
                    // Add it to the cache
                    cache.Add(clone);

                    // Messages?
                    SendNotification(clone, "OnDespawn");

                    // Deactivate it
                    clone.SetActive(false);

                    // Move it under this GO
                    clone.transform.SetParent(PoolParent, false);
                }
            }
            else
            {
                //Debug.LogWarning("Attempting to despawn a null clone");
            }
        }

        // This allows you to make another clone and add it to the cache
        public void FastPreload()
        {
            if (Prefab != null)
            {
                // Create clone
                var clone = FastClone(Vector3.zero, Quaternion.identity, null);

                // Add it to the cache
                cache.Add(clone);

                // Deactivate it
                clone.SetActive(false);

                // Move it under this GO
                clone.transform.SetParent(PoolParent, false);
            }
        }

        // Sends messages to clones
        // NOTE: clone is assumed to exist
        private void SendNotification(GameObject clone, string messageName)
        {
            clone.SendMessage(messageName, SendMessageOptions.DontRequireReceiver);
        }
    }
}
