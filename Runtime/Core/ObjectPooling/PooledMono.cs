using System;
using UnityEngine;

namespace Core
{
    /// <summary>  
    /// MonoBehaviour extended for pooling, with additional state and callback.  
    /// </summary>  
    public class PooledMono : MonoBehaviour
    {
        public Action<PooledMono> OnBackToPoolEvent { get; set; }
        public string AddressName { get; private set; }
        public PoolItemType PoolItemType { get; private set; }

        public void Init(string addressName = "", PoolItemType poolItemType = PoolItemType.General)
        {
            AddressName = addressName;
            PoolItemType = poolItemType;
        }

        public void ReturnToPool()
        {
            OnBackToPoolEvent?.Invoke(this);
        }
    }
}


