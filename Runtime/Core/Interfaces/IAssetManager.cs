using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;
using Component = UnityEngine.Component;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Core
{
    /// <summary>
    /// Interface for managing asset loading, instantiation, unloading, and catalog updates using the Unity Addressables system.
    /// </summary>
    public interface IAssetManager
    {
        /// <summary>
        /// Initializes the Addressables system asynchronously.
        /// </summary>
        UniTask InitializeAsync();

        /// <summary>
        /// Downloads dependencies for a single asset key asynchronously.
        /// </summary>
        /// <param name="key">The key of the asset to download dependencies for.</param>
        /// <returns>An AsyncOperationHandle for the download operation.</returns>
        AsyncOperationHandle DownloadDependenciesAsync(string key);

        /// <summary>
        /// Downloads dependencies for multiple asset keys asynchronously.
        /// </summary>
        /// <param name="keys">The collection of keys to download dependencies for.</param>
        /// <returns>An AsyncOperationHandle for the download operation.</returns>
        AsyncOperationHandle DownloadDependenciesAsync(IEnumerable<string> keys);

        /// <summary>
        /// Checks for updates to the Addressables content catalog asynchronously.
        /// </summary>
        /// <param name="autoReleaseHandle">Whether to automatically release the operation handle after completion.</param>
        /// <returns>A list of catalog IDs that have updates available, or an empty list if no updates are found or the operation fails.</returns>
        UniTask<List<string>> CheckForCatalogUpdatesAsync(bool autoReleaseHandle = true);

        /// <summary>
        /// Updates the Addressables content catalog asynchronously.
        /// </summary>
        /// <param name="catalogs">Optional list of catalog IDs to update. If null, updates all available catalogs.</param>
        /// <param name="autoReleaseHandle">Whether to automatically release the operation handle after completion.</param>
        /// <returns>True if the catalog update was successful, false otherwise.</returns>
        UniTask<bool> UpdateCatalogsAsync(IEnumerable<string> catalogs = null, bool autoReleaseHandle = true);

        /// <summary>
        /// Loads an asset asynchronously from the Addressables system using its address.
        /// </summary>
        /// <typeparam name="T">The type of the asset to load.</typeparam>
        /// <param name="address">The address of the asset to load.</param>
        /// <returns>The loaded asset, or null if loading fails.</returns>
        UniTask<T> LoadAssetAsync<T>(string address) where T : Object;

        /// <summary>
        /// Loads a JSON text asset and deserializes it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
        /// <param name="address">The address of the text asset to load.</param>
        /// <returns>The deserialized object, or null if loading or deserialization fails.</returns>
        UniTask<T> LoadFromTextAssetAsync<T>(string address) where T : class;

        /// <summary>
        /// Loads all assets with a specific label asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of assets to load.</typeparam>
        /// <param name="label">The label to filter assets by.</param>
        /// <returns>A list of loaded assets, or an empty list if loading fails.</returns>
        UniTask<IList<T>> LoadAssetsAsyncByLabel<T>(string label);

        /// <summary>
        /// Loads all assets from a collection of addresses or labels asynchronously.
        /// </summary>
        /// <param name="addresses">The collection of addresses or labels to load assets from.</param>
        /// <returns>A list of loaded assets, or an empty list if loading fails.</returns>
        UniTask<IList<Object>> LoadAssetsAsync(IEnumerable<object> addresses);

        /// <summary>
        /// Loads an asset using an AssetReference.
        /// </summary>
        /// <typeparam name="T">The type of the asset to load.</typeparam>
        /// <param name="assetReference">The AssetReference to load the asset from.</param>
        /// <returns>The loaded asset, or null if loading fails.</returns>
        UniTask<T> LoadAssetAsync<T>(AssetReference assetReference) where T : Object;

        /// <summary>
        /// Instantiates a GameObject from an addressable asset.
        /// </summary>
        /// <param name="address">The address of the asset to instantiate.</param>
        /// <param name="parent">The parent transform for the instantiated GameObject (optional).</param>
        /// <param name="worldPositionStays">Whether the GameObject maintains its world position when parented.</param>
        /// <returns>The instantiated GameObject, or null if instantiation fails.</returns>
        UniTask<GameObject> InstantiateAsync(string address, Transform parent = null, bool worldPositionStays = true);

        /// <summary>
        /// Instantiates a GameObject from an addressable asset and returns a specific component.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <param name="address">The address of the asset to instantiate.</param>
        /// <param name="parent">The parent transform for the instantiated GameObject (optional).</param>
        /// <param name="worldPositionStays">Whether the GameObject maintains its world position when parented.</param>
        /// <returns>The component on the instantiated GameObject, or null if instantiation fails or the component is missing.</returns>
        UniTask<T> InstantiateAsync<T>(string address, Transform parent = null, bool worldPositionStays = true) where T : Component;

        /// <summary>
        /// Instantiates a GameObject using an AssetReference.
        /// </summary>
        /// <param name="assetReference">The AssetReference to instantiate the asset from.</param>
        /// <param name="parent">The parent transform for the instantiated GameObject (optional).</param>
        /// <returns>The instantiated GameObject, or null if instantiation fails.</returns>
        UniTask<GameObject> InstantiateAsync(AssetReference assetReference, Transform parent = null);

        /// <summary>
        /// Instantiates a GameObject using an AssetReference and returns a specific component.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <param name="assetReference">The AssetReference to instantiate the asset from.</param>
        /// <param name="parent">The parent transform for the instantiated GameObject (optional).</param>
        /// <returns>The component on the instantiated GameObject, or null if instantiation fails or the component is missing.</returns>
        UniTask<T> InstantiateAsync<T>(AssetReference assetReference, Transform parent = null) where T : Component;

        /// <summary>
        /// Instantiates GameObjects from all assets with a specific label asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve from each instantiated GameObject.</typeparam>
        /// <param name="label">The label to filter assets by.</param>
        /// <returns>A list of components on the instantiated GameObjects, or an empty list if instantiation fails.</returns>
        UniTask<IList<T>> InstanceAssetsAsyncByLabel<T>(string label) where T : Component;

        /// <summary>
        /// Instantiates a GameObject from a Unity Object and sets its parent.
        /// </summary>
        /// <param name="unityObj">The Unity Object to instantiate (GameObject or Component).</param>
        /// <param name="parent">The parent transform for the instantiated GameObject.</param>
        /// <param name="isActive">Whether the instantiated GameObject should be active.</param>
        /// <param name="worldPositionStays">Whether the GameObject maintains its world position when parented.</param>
        /// <returns>The instantiated GameObject, or null if instantiation fails.</returns>
        GameObject InstantiateGameObject(Object unityObj, Transform parent, bool isActive = true, bool worldPositionStays = true);

        /// <summary>
        /// Unloads an asset from memory.
        /// </summary>
        /// <param name="asset">The asset to unload.</param>
        /// <returns>True if the asset was unloaded successfully, false otherwise.</returns>
        bool UnloadAsset(Object asset);

        /// <summary>
        /// Releases an instantiated GameObject from the Addressables system.
        /// </summary>
        /// <param name="go">The GameObject to release.</param>
        void ReleaseInstance(GameObject go);

        /// <summary>
        /// Releases all tracked assets and instances.
        /// </summary>
        void ReleaseAllAssets();

        void ReleaseAsset(string address);
    }
}