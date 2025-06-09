using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Core
{
    /// <summary>  
    /// Object pooling system for efficient reuse of game objects.  
    /// </summary>  
    public class ObjectPooling : MonoBehaviour, IObjectPooling
    {
        [SerializeField]
        private PoolItem[] _poolItems;

        private readonly Dictionary<PoolItemType, List<PooledMono>> _pooledDict = new();
        private Transform _transform;
        private bool _isInit;
        private IAssetManager _assetManager;

        [Inject]
        public void Constructor(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        private void Awake()
        {
            _transform = transform;
        }

        /// <summary>  
        /// Initializes the pool and preloads pooled objects as configured.  
        /// </summary>  
        public async void Init(Action<List<PooledMono>> callback = null)
        {
            if (_isInit) 
            {
                return;
            }
           
            _isInit = true;

            for (int i = 0; i < _poolItems.Length; i++)
            {
                PoolItem poolItem = _poolItems[i];
                for (int j = 0; j < poolItem.AmountToPool; j++)
                {
                    PooledMono pooledMono = await CreateObject(poolItem.AddressName, poolItem.PoolType);
                    AddToPooledDict(poolItem.PoolType, pooledMono);
                }
                if (poolItem.NeedModifyAfterInit && callback != null)
                    callback(GetFromPooledDict(poolItem.PoolType));
            }
        }

        /// <summary>  
        /// Gets an object from the pool by address and type, or creates one if none available.  
        /// </summary>  
        public async Task<T> Get<T>(string addressName, PoolItemType poolItemType = PoolItemType.General, bool isActive = true) where T : Component
        {
            if (_pooledDict.TryGetValue(poolItemType, out var pooledMonos))
            {
                for (int i = 0; i < pooledMonos.Count; i++)
                {
                    if (string.Equals(pooledMonos[i].AddressName, addressName))
                    {
                        var pooledMono = pooledMonos[i];
                        pooledMonos.RemoveAt(i);
                        pooledMono.gameObject.SetActive(isActive);
                        return pooledMono.GetComponent<T>();
                    }
                }
            }
            // Not found in pool -> create a new object  
            PooledMono mono = await CreateObject(addressName, poolItemType);
            if (mono != null)
            {
                mono.gameObject.SetActive(isActive);
                return mono.GetComponent<T>();
            }
            Debug.LogError($"[ObjectPooling] Could not get object: {typeof(T)} at {addressName} ({poolItemType})");
            return null;
        }

        /// <summary>  
        /// Gets a pooled GameObject by address and type.  
        /// </summary>  
        public async Task<GameObject> Get(string addressName, PoolItemType poolItemType = PoolItemType.General, bool isActive = true)
        {
            var pooled = await Get<PooledMono>(addressName, poolItemType, isActive);
            return pooled ? pooled.gameObject : null;
        }

        /// <summary>  
        /// Returns a GameObject back to its pool.  
        /// </summary>  
        public void ReturnToPool(GameObject objectToReturn)
        {
            if (objectToReturn == null || objectToReturn.transform.parent == this.transform)
                return;

            var pooledMono = objectToReturn.GetComponent<PooledMono>();
            if (pooledMono == null)
                pooledMono = objectToReturn.AddComponent<PooledMono>();

            pooledMono.ReturnToPool();
        }

        /// <summary>  
        /// Handles returning a pooled object to its list, resetting transform and active state.  
        /// </summary>  
        private void ReturnToPool(PooledMono objectToReturn)
        {
            if (objectToReturn == null) return;
            if (objectToReturn.transform.parent == _transform) return;

            objectToReturn.gameObject.SetActive(false);
            objectToReturn.transform.SetParent(_transform);
            objectToReturn.transform.localPosition = Vector3.zero;
            AddToPooledDict(objectToReturn.PoolItemType, objectToReturn);
        }

        /// <summary>  
        /// Adds a pooled object into the dictionary for reuse.  
        /// </summary>  
        private void AddToPooledDict(PoolItemType poolItemType, PooledMono pooledMono)
        {
            if (!_pooledDict.TryGetValue(poolItemType, out var pooledMonos))
            {
                pooledMonos = new List<PooledMono>();
                _pooledDict[poolItemType] = pooledMonos;
            }
            if (!pooledMonos.Contains(pooledMono))
                pooledMonos.Add(pooledMono);
        }

        /// <summary>  
        /// Gets all pooled objects of a type.  
        /// </summary>  
        private List<PooledMono> GetFromPooledDict(PoolItemType poolItemType)
        {
            if (_pooledDict.TryGetValue(poolItemType, out var pooledMonos))
                return pooledMonos;
            return new List<PooledMono>();
        }

        /// <summary>  
        /// Instantiates a new pooled object from asset manager.  
        /// </summary>  
        private async Task<PooledMono> CreateObject(string addressName, PoolItemType poolItemType)
        {
            var go = await _assetManager.InstantiateAsync(addressName, _transform);
            if (go == null)
            {
                Debug.LogError($"[ObjectPooling] Could not instantiate object at address: {addressName}");
                return null;
            }

            var pooledItem = go.GetComponent<PooledMono>() ?? go.AddComponent<PooledMono>();
            pooledItem.OnBackToPoolEvent = ReturnToPool;
            pooledItem.Init(addressName, poolItemType);
            go.SetActive(false);

            return pooledItem;
        }

        /// <summary>  
        /// Destroys one pool group by type and clears its list.  
        /// </summary>  
        public void DestroyObject(PoolItemType itemType)
        {
            if (_pooledDict.TryGetValue(itemType, out var pooledMonos))
            {
                for (int i = pooledMonos.Count - 1; i >= 0; i--)
                {
                    Destroy(pooledMonos[i].gameObject);
                    pooledMonos.RemoveAt(i);
                }
                pooledMonos.Clear();
            }
        }

        /// <summary>  
        /// Destroys all pooled objects of all types and clears the dictionary.  
        /// </summary>  
        public void DestroyAllObjects()
        {
            foreach (var kv in _pooledDict)
            {
                var pooledMonos = kv.Value;
                for (int i = pooledMonos.Count - 1; i >= 0; i--)
                {
                    Destroy(pooledMonos[i].gameObject);
                    pooledMonos.RemoveAt(i);
                }
                pooledMonos.Clear();
            }
        }

        /// <summary>  
        /// Completely clears all pools and resets the system.  
        /// </summary>  
        public void ClearAllPools()
        {
            DestroyAllObjects();
            _pooledDict.Clear();
            _isInit = false;
        }
    }
}

