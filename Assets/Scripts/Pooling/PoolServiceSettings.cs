using System.Collections.Generic;
using UnityEngine;

namespace BaseServices.PoolServices
{
    [System.Serializable]
    internal struct PoolInitializeData
    {
        [SerializeField] internal GameObject poolItem;
        [SerializeField] internal int preload;
        [SerializeField] internal int capacity;
    }

    [CreateAssetMenu(fileName = "PoolServiceSettings", menuName = "Scriptable Objects/Pooling/Pool Settings", order = 0)]
    public class PoolServiceSettings : ScriptableObject
    {
        public int defaultPoolCapacity = 100;
        public int defaultPoolPreload = 5;
        public bool debugLog = true;

        [SerializeField] private List<PoolInitializeData> poolInitializeData;

        public void InitializeDefinedPools()
        {
            foreach (var item in poolInitializeData)
            {
                InitializePoolItem(item);
            }
        }

        private void InitializePoolItem(PoolInitializeData data)
        {
            PoolService.Instance.InitializePool(data.poolItem, data.preload, data.capacity);
        }
    }
}
