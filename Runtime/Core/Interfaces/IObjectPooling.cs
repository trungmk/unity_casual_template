using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Core
{
    public interface IObjectPooling
    {
        Task<T> Get<T>(string addressName, PoolItemType poolItemType = PoolItemType.General, bool isActive = true) where T : Component;
        Task<GameObject> Get(string addressName, PoolItemType poolItemType = PoolItemType.General, bool isActive = true);
        void ReturnToPool(GameObject objectToReturn);
        void DestroyObject(PoolItemType itemType);
        void DestroyAllObjects();
        void ClearAllPools();
    }
} 