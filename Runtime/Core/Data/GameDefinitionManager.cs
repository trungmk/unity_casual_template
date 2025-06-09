using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Core
{
    /// <summary>
    /// Manages game definition data loaded from assets.
    /// </summary>
    public class GameDefinitionManager : IGameDefinitionManager
    {
        private readonly Dictionary<Type, IGameDefinitionWrapper> _gameDefinitionWrapperDict = new Dictionary<Type, IGameDefinitionWrapper>();

        private const string GAME_DEFINITION_LABEL = "GameDefinition";

        public event Action OnInitCompleted;

        private readonly IAssetManager _assetManager;
        private bool _isInitialized = false;

        /// <summary>
        /// Constructor for GameDefinitionManager with dependency injection.
        /// </summary>
        /// <param name="assetManager">Asset manager for loading game definitions</param>
        [Inject]
        public GameDefinitionManager(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        /// <summary>
        /// Initializes the manager and loads all game definitions.
        /// </summary>
        public void Init()
        {
            if (_assetManager == null)
            {
                Debug.LogError("[GameDefinitionManager] AssetManager is null, cannot Init!");
                return;
            }

            if (_isInitialized)
                return;

            _isInitialized = true;
            _gameDefinitionWrapperDict.Clear();
            _ = LoadGameDefinitionsAsync();
        }

        /// <summary>
        /// Initialize with custom values - not implement yet.
        /// </summary>
        /// <param name="values">Custom initialization values</param>
        public virtual void Init(IDictionary<string, object> values)
        {
            // This could be extended later to load specific definitions based on values
            //Init();
        }

        public List<T> GetDefinitions<T>() where T : IGameDefinition
        {
            Type key = typeof(T);

            if (!_gameDefinitionWrapperDict.TryGetValue(key, out var wrapper))
            {
                Debug.LogWarning($"Definition wrapper for type {key} not found.");
                return new List<T>();
            }

            return wrapper.GetDefinitions<T>() ?? new List<T>();
        }

        public bool HasDefinitions<T>() where T : IGameDefinition
        {
            return _gameDefinitionWrapperDict.ContainsKey(typeof(T));
        }

        private async Task LoadGameDefinitionsAsync()
        {
            try
            {
                IList<Object> objs = await _assetManager.LoadAssetsAsyncByLabel<Object>(GAME_DEFINITION_LABEL);

                if (objs == null || objs.Count == 0)
                {
                    Debug.LogWarning($"No game definitions found with label: {GAME_DEFINITION_LABEL}");
                    //OnInitCompleted?.Invoke();

                    if (OnInitCompleted != null)
                    {
                        OnInitCompleted();
                    }
                    return;
                }

                int loadedDefinitions = 0;

                for (int i = 0; i < objs.Count; i++)
                {
                    if (objs[i] is IGameDefinitionWrapper wrapper)
                    {
                        try
                        {
                            wrapper.Init();
                            Type type = wrapper.GetGameDefinitionType();

                            if (_gameDefinitionWrapperDict.ContainsKey(type))
                            {
                                Debug.LogWarning($"Duplicate definition type found: {type}. Skipping.");
                                continue;
                            }

                            _gameDefinitionWrapperDict.Add(type, wrapper);
                            loadedDefinitions++;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error initializing definition wrapper: {ex.Message}");
                        }
                    }
                }

                Debug.Log($"Loaded {loadedDefinitions} game definition types from {objs.Count} assets.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load game definitions: {ex.Message}");
            }

            //OnInitCompleted?.Invoke();
            if (OnInitCompleted != null)
            {
                OnInitCompleted();
            }
        }
    }
}
