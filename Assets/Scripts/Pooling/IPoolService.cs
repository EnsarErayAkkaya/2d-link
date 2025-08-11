using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseServices.PoolServices
{
    public interface IPoolService
    {
        Pool InitializePool(GameObject prefab, int preload);
        Pool InitializePool(GameObject prefab, int preload, int capacity);

        T Spawn<T>(T prefab) where T : Component;
        T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component;
        T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component;

        GameObject Spawn(GameObject prefab);
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation);
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent);

        void Despawn(Component clone, float delay = 0.0f);
        void Despawn(GameObject clone, float delay = 0.0f);
    }
}