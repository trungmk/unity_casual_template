using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;
using Component = UnityEngine.Component;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace Core
{
    /// <summary>  
    /// Manages asset loading, instantiation, unloading, and catalog updates using the Unity Addressables system.  
    /// </summary>  
    public class AssetManager : BaseSystem, IAssetManager
    {
        private const string LOG_TAG = "[AssetManager]";
        private IObjectResolver _container;
        private readonly Dictionary<GameObject, AsyncOperationHandle> _instantiatedObjects = new Dictionary<GameObject, AsyncOperationHandle>();

        [Inject]
        public void Constructor(IObjectResolver resolver)
        {
            _container = resolver;
        }

        #region Initialize and Download  

        public async UniTask InitializeAsync()
        {
            try
            {
                await Addressables.InitializeAsync().Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to initialize Addressables: {ex.Message}");
                throw;
            }
        }

        public AsyncOperationHandle DownloadDependenciesAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($"{LOG_TAG} Cannot download dependencies for null or empty key");
                return default;
            }

            try
            {
                return Addressables.DownloadDependenciesAsync(key, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to download dependencies for {key}: {ex.Message}");
                throw;
            }
        }

        public AsyncOperationHandle DownloadDependenciesAsync(IEnumerable<string> keys)
        {
            if (keys == null)
            {
                Debug.LogError($"{LOG_TAG} Cannot download dependencies for null keys collection");
                return default;
            }

            try
            {
                return Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.None, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to download dependencies for multiple keys: {ex.Message}");
                throw;
            }
        }

        public async UniTask<List<string>> CheckForCatalogUpdatesAsync(bool autoReleaseHandle = true)
        {
            try
            {
                AsyncOperationHandle<List<string>> handle = Addressables.CheckForCatalogUpdates(autoReleaseHandle);
                List<string> catalogs = await handle.Task;

                return catalogs ?? new List<string>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to check for catalog updates: {ex.Message}");
                return new List<string>();
            }
        }

        public async UniTask<bool> UpdateCatalogsAsync(IEnumerable<string> catalogs = null, bool autoReleaseHandle = true)
        {
            try
            {
                AsyncOperationHandle<List<IResourceLocator>> handle = Addressables.UpdateCatalogs(catalogs, autoReleaseHandle);
                await handle.Task;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to update catalogs: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Asset Loading  

        public async UniTask<T> LoadAssetAsync<T>(string address) where T : Object
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError($"{LOG_TAG} Cannot load asset with null or empty address");
                return null;
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);

            try
            {
                T asset = await handle.Task;

                if (asset == null)
                {
                    Debug.LogError($"{LOG_TAG} Could not load asset from: {address}");
                }

                return asset;
            }
            catch (OperationCanceledException)
            {
                Debug.LogError($"{LOG_TAG} Asset loading canceled: {address}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to load asset from {address}: {ex.Message}");
                return null;
            }
        }

        public async UniTask<T> LoadFromTextAssetAsync<T>(string address) where T : class
        {
            try
            {
                TextAsset textAsset = await LoadAssetAsync<TextAsset>(address);
                if (textAsset == null || string.IsNullOrEmpty(textAsset.text))
                {
                    Debug.LogError($"{LOG_TAG} TextAsset at {address} was null or empty");
                    return default;
                }

                T result = await UniTask.RunOnThreadPool(() =>
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<T>(textAsset.text);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"{LOG_TAG} Failed to deserialize JSON: {ex.Message}");
                        return default;
                    }
                });

                return result;
            }
            catch (JsonException ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to deserialize JSON from {address}: {ex.Message}");
                return default;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Error loading text asset from {address}: {ex.Message}");
                return default;
            }
        }

        public async UniTask<IList<T>> LoadAssetsAsyncByLabel<T>(string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                Debug.LogError($"{LOG_TAG} Cannot load assets with null or empty label");
                return new List<T>();
            }

            IEnumerable<object> labels = new List<object> { label };
            AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(labels, null, Addressables.MergeMode.Union);

            try
            {
                return await handle.Task;
            }
            catch (OperationCanceledException)
            {
                Debug.LogError($"{LOG_TAG} Asset loading canceled for label: {label}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to load assets with label {label}: {ex.Message}");
                return new List<T>();
            }
        }

        public async UniTask<IList<Object>> LoadAssetsAsync(IEnumerable<object> addresses)
        {
            if (addresses == null)
            {
                Debug.LogError($"{LOG_TAG} Cannot load assets with null addresses collection");
                return new List<Object>();
            }

            AsyncOperationHandle<IList<Object>> handle = Addressables.LoadAssetsAsync<Object>(addresses, null, Addressables.MergeMode.Union);

            try
            {
                return await handle.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to load multiple assets: {ex.Message}");
                return new List<Object>();
            }
        }

        public async UniTask<T> LoadAssetAsync<T>(AssetReference assetReference) where T : Object
        {
            if (assetReference == null)
            {
                Debug.LogError($"{LOG_TAG} AssetReference is null");
                return default;
            }

            if (!assetReference.RuntimeKeyIsValid())
            {
                Debug.LogError($"{LOG_TAG} AssetReference contains an invalid key");
                return default;
            }

            AsyncOperationHandle<T> handle = assetReference.LoadAssetAsync<T>();

            try
            {
                T asset = await handle.Task;

                if (asset == null)
                {
                    Debug.LogError($"{LOG_TAG} Failed to load asset from AssetReference: {assetReference}");
                }

                return asset;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to load asset from reference {assetReference}: {ex.Message}");
                return default;
            }
        }

        #endregion

        #region Asset Instantiation  

        private GameObject ConfigureInstantiatedObject(GameObject prefab, AsyncOperationHandle? sourceHandle = null)
        {
            if (prefab == null)
            {
                Debug.LogError($"{LOG_TAG} Cannot configure null prefab");
                return null;
            }

            try
            {
                bool wasActive = prefab.activeSelf;
                prefab.SetActive(false);

                GameObject instantiatedObject = _container.Instantiate(prefab);
                AutoCleanupAsset autoCleanupAsset = instantiatedObject.AddComponent<AutoCleanupAsset>();
                autoCleanupAsset.Init(this);

                _container.InjectGameObject(instantiatedObject);

                if (sourceHandle.HasValue && sourceHandle.Value.IsValid())
                {
                    _instantiatedObjects[instantiatedObject] = sourceHandle.Value;
                }

                prefab.SetActive(wasActive);
                instantiatedObject.SetActive(wasActive);

                return instantiatedObject;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to configure instantiated object: {ex.Message}");
                return null;
            }
        }

        private GameObject GetPrefabAsGameObject(Object prefab)
        {
            if (prefab == null)
                return null;

            if (prefab is GameObject gameObj)
                return gameObj;

            if (prefab is Component component)
                return component.gameObject;

            Debug.LogError($"{LOG_TAG} Cannot convert {prefab.GetType()} to GameObject");
            return null;
        }

        private GameObject InstantiateFromAsset(Object asset, AsyncOperationHandle? sourceHandle = null)
        {
            if (asset == null)
            {
                Debug.LogError($"{LOG_TAG} Cannot instantiate from null asset");
                return null;
            }

            try
            {
                GameObject prefab = GetPrefabAsGameObject(asset);
                return ConfigureInstantiatedObject(prefab, sourceHandle);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to instantiate from asset: {ex.Message}");
                return null;
            }
        }

        public async UniTask<GameObject> InstantiateAsync(string address, Transform parent = null, bool worldPositionStays = true)
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError($"{LOG_TAG} Cannot instantiate with null or empty address");
                return null;
            }

            try
            {
                AsyncOperationHandle<Object> handle = Addressables.LoadAssetAsync<Object>(address);
                Object asset = await handle.Task;

                if (asset == null)
                {
                    Debug.LogError($"{LOG_TAG} Failed to load asset from address: {address}");
                    return null;
                }

                GameObject result = InstantiateFromAsset(asset, handle);
                if (result != null && parent != null)
                {
                    result.transform.SetParent(parent, worldPositionStays);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to instantiate from address {address}: {ex.Message}");
                return null;
            }
        }

        public async UniTask<T> InstantiateAsync<T>(string address, Transform parent = null, bool worldPositionStays = true) where T : Component
        {
            GameObject go = await InstantiateAsync(address, parent, worldPositionStays);
            if (go == null)
                return null;

            T component = go.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"{LOG_TAG} GameObject instantiated from {address} does not have a {typeof(T).Name} component");
            }

            return component;
        }

        public async UniTask<GameObject> InstantiateAsync(AssetReference assetReference, Transform parent = null)
        {
            if (assetReference == null)
            {
                Debug.LogError($"{LOG_TAG} AssetReference is null");
                return null;
            }

            if (!assetReference.RuntimeKeyIsValid())
            {
                Debug.LogError($"{LOG_TAG} AssetReference contains an invalid key");
                return null;
            }

            try
            {
                AsyncOperationHandle<GameObject> handle = assetReference.InstantiateAsync(parent);
                GameObject instantiatedObject = await handle.Task;

                if (instantiatedObject == null)
                {
                    Debug.LogError($"{LOG_TAG} Failed to instantiate from AssetReference: {assetReference}");
                    return null;
                }

                _instantiatedObjects[instantiatedObject] = handle;
                AutoCleanupAsset autoCleanupAsset = instantiatedObject.AddComponent<AutoCleanupAsset>();
                autoCleanupAsset.Init(this);
                _container.InjectGameObject(instantiatedObject);

                return instantiatedObject;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to instantiate from AssetReference: {ex.Message}");
                return null;
            }
        }

        public async UniTask<T> InstantiateAsync<T>(AssetReference assetReference, Transform parent = null) where T : Component
        {
            GameObject go = await InstantiateAsync(assetReference, parent);
            if (go == null)
                return null;

            T component = go.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"{LOG_TAG} GameObject instantiated from AssetReference does not have a {typeof(T).Name} component");
            }

            return component;
        }

        public async UniTask<IList<T>> InstanceAssetsAsyncByLabel<T>(string label) where T : Component
        {
            if (string.IsNullOrEmpty(label))
            {
                Debug.LogError($"{LOG_TAG} Cannot instantiate assets with null or empty label");
                return new List<T>();
            }

            try
            {
                IEnumerable<object> labels = new List<object> { label };
                AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(labels, null, Addressables.MergeMode.Union);
                IList<T> assets = await handle.Task;
                List<T> result = new List<T>();

                foreach (T asset in assets)
                {
                    GameObject prefab = asset is GameObject go ? go : (asset as Component)?.gameObject;
                    if (prefab != null)
                    {
                        GameObject instantiatedObject = ConfigureInstantiatedObject(prefab, handle);
                        if (instantiatedObject != null)
                        {
                            T compo = instantiatedObject.GetComponent<T>();
                            if (compo != null)
                            {
                                result.Add(compo);
                            }
                            else
                            {
                                Debug.LogError($"{LOG_TAG} Instantiated object does not have component of type {typeof(T).Name}");
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to instantiate assets with label {label}: {ex.Message}");
                return new List<T>();
            }
        }

        public GameObject InstantiateGameObject(Object unityObj, Transform parent, bool isActive = true, bool worldPositionStays = true)
        {
            if (unityObj == null)
            {
                Debug.LogError($"{LOG_TAG} Cannot instantiate from null object");
                return null;
            }

            try
            {
                GameObject prefab = GetPrefabAsGameObject(unityObj);
                if (prefab == null)
                {
                    Debug.LogError($"{LOG_TAG} Failed to get GameObject from {unityObj}");
                    return null;
                }

                bool wasActive = prefab.activeSelf;
                prefab.SetActive(false);

                GameObject gameObj = ConfigureInstantiatedObject(prefab);
                if (gameObj != null)
                {
                    if (parent != null)
                    {
                        gameObj.transform.SetParent(parent, worldPositionStays);
                    }

                    gameObj.SetActive(isActive);
                    prefab.SetActive(wasActive);
                    return gameObj;
                }

                prefab.SetActive(wasActive);
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to instantiate GameObject: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Asset Unloading  

        public void ReleaseAsset(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return;
            }

            Addressables.Release(address);
        }    

        public bool UnloadAsset(Object asset)
        {
            if (asset == null)
                return false;

            try
            {
                Addressables.Release(asset);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to unload asset {asset.name}: {ex.Message}");
                return false;
            }
        }

        public void ReleaseInstance(GameObject go)
        {
            if (go == null)
                return;

            try
            {
                if (_instantiatedObjects.TryGetValue(go, out AsyncOperationHandle handle) && handle.IsValid())
                {
                    Addressables.Release(handle);
                    _instantiatedObjects.Remove(go);
                }
                else
                {
                    Addressables.ReleaseInstance(go);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Failed to release instance {go.name}: {ex.Message}");
            }
        }

        public void ReleaseAllAssets()
        {
            try
            {
                foreach (var kvp in _instantiatedObjects)
                {
                    GameObject go = kvp.Key;
                    AsyncOperationHandle handle = kvp.Value;

                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                    }

                    if (go != null)
                    {
                        Object.Destroy(go);
                    }
                }
                _instantiatedObjects.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{LOG_TAG} Error releasing all assets: {ex.Message}");
            }
        }

        #endregion
    }
}