using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core
{
    //public class GameLifetimeScope : LifetimeScope
    //{
    //    [SerializeField] 
    //    private CoreSceneManager _sceneManager = default;

    //    [SerializeField] 
    //    private Transform _gameSystemGroup = default;

    //    [SerializeField]
    //    private UIManager _uiManager = default;

    //    [SerializeField]
    //    private AudioManager _audioManager = default;

    //    [SerializeField]
    //    private ObjectPooling _objectPooling = default;

    //    [SerializeField]
    //    private UIObjectPooling _uiObjectPooling = default;
        
    //    [SerializeField]
    //    private GameData _gameData = default;
        
    //    [SerializeField]
    //    private StartupTaskRunner _startupTaskRunner = default;
        
    //    [SerializeField]
    //    private SlowButtonHub _slowButtonHub = default;

    //    [SerializeField]
    //    private AssetManager _assetManager = default;

    //    protected override void Configure(IContainerBuilder builder)
    //    {
    //        // Register systems with interfaces
    //        builder.RegisterComponent(_sceneManager).AsSelf().AsImplementedInterfaces();
    //        builder.RegisterComponent(_uiManager).AsSelf().AsImplementedInterfaces();
    //        builder.RegisterComponent(_audioManager).AsSelf().AsImplementedInterfaces();
    //        builder.RegisterComponent(_objectPooling).As<IObjectPooling>();
    //        builder.RegisterComponent(_uiObjectPooling).As<IUIObjectPooling>();
    //        builder.RegisterComponent(_gameData).As<IGameDataManager>();
    //        builder.RegisterComponent(_startupTaskRunner).As<IStartupTaskRunner>();

    //        // Register other services
    //        builder.Register<IEventManager, EventManager>(Lifetime.Singleton);
    //        builder.Register<IInputFilter, InputFilter>(Lifetime.Singleton);

    //        // Find all BaseSystems in the game systems group and register them
    //        var baseSystems = _gameSystemGroup.GetComponentsInChildren<BaseSystem>().ToList();
    //        foreach (var system in baseSystems)
    //        {
    //            if (system != _sceneManager && 
    //                system != _uiManager && 
    //                system != _audioManager && 
    //                system != _objectPooling && 
    //                system != _uiObjectPooling && 
    //                system != _gameData &&
    //                system != _startupTaskRunner)
    //            {
    //                builder.RegisterComponent(system).AsSelf();
    //            }
    //        }

    //        // Register GameController to manage the lifecycle
    //        builder.RegisterEntryPoint<GameController>().AsSelf();
    //    }
    //}
}
