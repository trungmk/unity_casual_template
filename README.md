# Template Framework for Unity (Casual Games)

## Table of Contents

1. [Features](#features)
2. [Installation](#installation)
3. [Getting Started](#getting-started)
4. [Core Components](#core-components)
5. [Lifecycle & State Management](#lifecycle)
6. [Usage Examples](#usage-examples)
7. [Advanced Features](#advanced-features)
8. [Best Practices](#best-practices)
9. [API Reference](#api-reference)

---

## Features

- ✅ **Modular Architecture**: Clean separation of concerns with BaseSystem pattern
- ✅ **Lifecycle Management**: Unified initialization, pause/resume, and cleanup
- ✅ **Audio System**: Complete audio management with pooling, fade effects, and playlists
- ✅ **Scene Management**: Additive scene loading with custom scene controllers
- ✅ **Asset Management**: Addressables-based asset loading and management
- ✅ **Dependency Injection**: VContainer integration for clean dependency management
- ✅ **Object Pooling**: Efficient object reuse for performance optimization
- ✅ **UI Management**: View-based UI system with stacking and lifecycle
- ✅ **Input Handling**: Centralized input management system
- ✅ **Data Management**: Game definition and local data persistence
- ✅ **State Machine**: Flexible state management system
- ✅ **Startup Tasks**: Dependency-based initialization system
- ✅ **Game Events**: Decoupled communication system
- ✅ **Network Support**: Built-in REST API wrapper with async/await

---

## Installation

### Prerequisites

- Unity 2021.3 or later
- Git installed on your system

### Step 1: Install Required Dependencies

The package requires the following Unity packages:
- Addressables (2.3.16+)
- Newtonsoft Json (2.0.2+) (Packages/manifest.json: "com.unity.nuget.newtonsoft-json": "2.0.2")
- Unitask (https://github.com/Cysharp/UniTask)
- VContainer (dependency injection - https://vcontainer.hadashikick.jp/getting-started/installation)

### Step 2: Install Core Framework

**Option A: Via Git URL (Recommended)**

1. Open `Window` → `Package Manager`
2. Click `+` → `Add package from git URL`
3. Enter: `https://github.com/trungmk/unity_casual_template.git#master`

**Option B: Manual Installation**

1. Find folder `your_project\Packages\`, then open file `manifest.json`
2. add "com.maktrung.core" : "https://github.com/trungmk/unity_casual_template.git#master" to  `Packages/manifest.json`.
3. The package will be automatically detected by Unity

---

## Getting Started

### Basic Setup

1. **Create a Root Scene**:
   - This will be your main entry point
   - Setup whole game systems, UIManager in this scene.

2. **Add GameHub**:
   ```csharp
   // Create an empty GameObject and add GameHub component
   // GameHub manages the entire framework lifecycle
   ```

3. **Setup Scene Structure**:
   ```
   Root Scene
   ├── GameHub (GameHub component)
   ├── Global Systems (Empty GameObject)
   │   ├── AudioManager
   │   ├── AssetManager
   │   └── [Other Systems]
   └── SceneManager (CoreSceneManager component)
   └── UIManager
   ```

### Quick Start Example

```csharp
using Core;
using UnityEngine;

// Create a custom system for your game
public class MyGameSystem : BaseSystem
{
    public override void Initialize()
    {
        Debug.Log("MyGameSystem initialized!");
    }

    public override void OnLoaded()
    {
        Debug.Log("Scene loaded, system ready!");
    }

    public override void OnPause()
    {
        Debug.Log("Game paused, saving data...");
        // Save critical game state here
    }

    public override void OnResume()
    {
        Debug.Log("Game resumed!");
    }
}
```

---

## Core Components

### GameHub
The central coordinator that manages all systems and lifecycle events.

**Key Features:**
- Single entry point for the entire application
- Manages system initialization and lifecycle
- Handles application pause/resume events
- Controls target framerate and screen timeout

### BaseSystem
Abstract base class for all game systems providing unified lifecycle management.

**Lifecycle Methods:**
- `Initialize()`: Called once during system setup
- `OnLoaded()`: Called when a scene finishes loading
- `OnPreUnloaded()`: Called before scene unloading begins
- `OnUnloaded()`: Called after scene is unloaded  
- `OnPause()`: Called when game is paused
- `OnResume()`: Called when game is resumed

### AudioManager
Comprehensive audio system with pooling, fade effects, and categorized playback.

**Features:**
- Separate music and sound effect channels
- Audio source pooling for performance
- Fade in/out effects with callbacks
- Playlist support with randomization
- Automatic pause/resume handling

**Usage:**
```csharp
// Play a sound effect
audioManager.PlaySound(soundClip, pitch: 1.0f, volume: 0.8f);

// Play background music with fade
audioManager.PlayMusic(AudioClipType.MusicBackground, musicClip, volume: 0.7f);

### AssetManager
Addressables-based asset loading and management system.

**Features:**
- Async/await asset loading
- Automatic handle management
- Label-based batch loading
- Dependency injection support
- Catalog updating capabilities

**Usage:**
```csharp
// Load a single asset
var texture = await assetManager.LoadAssetAsync<Texture2D>("MyTexture");

// Instantiate with DI injection
var instance = await assetManager.InstantiateWithInjectAsync("PlayerPrefab");
```

### UIManager
View-based UI system with stacking, caching, and lifecycle management.

**Features:**
- View stack management
- Automatic caching and reuse
- Dependency injection for UI components
- Show/hide callbacks and events
- Multi-layer UI support

**Usage:**
```csharp
// Show a UI view
uiManager.Show<MainMenuPanel>()
         .OnShowCompleted(view => {
            MainMenuPanel mainMenuPanel = view as MainMenuPanel;
         });

// Show with parameters
uiManager.Show<MainMenuPanel>(playerData, itemList);

// Hide a view
uiManager.Hide<MainMenuPanel>();

```

### CoreSceneManager
Additive scene loading system with custom scene controllers.

**Features:**
- Additive scene loading
- Scene controller pattern
- Scene transition callbacks
- ScriptableObject-based scene configuration
- Automatic lifecycle event broadcasting

---

## Lifecycle & State Management

The framework provides a unified lifecycle management system:

### Startup Flow
1. **GameHub** initializes with target framerate settings
2. All **BaseSystems** in the system group are initialized
3. **CoreSceneManager** loads the initial scene
4. Scene-specific **SceneController** takes over
5. All systems receive `OnLoaded()` callback

### Scene Transitions
1. `OnPreUnloaded()` called on all systems
2. Current scene unloaded
3. `OnUnloaded()` called on all systems  
4. New scene loaded additively
5. `OnLoaded()` called on all systems

### Pause/Resume Flow
- **Application pause** (minimize, phone call, etc.)
- **Manual pause** via `GameHub.PauseGame()`
- All systems receive `OnPause()`/`OnResume()` callbacks
- Audio automatically pauses/resumes
- Perfect for saving critical state

---

## Usage Examples

### Creating a Custom Data System

```csharp
using Core;
using UnityEngine;

public class PlayerDataSystem : BaseSystem
{
    [SerializeField] private PlayerData playerData;
    
    public override void Initialize()
    {
        LoadPlayerData();
    }
    
    public override void OnPause()
    {
        // Save player data when game is paused
        SavePlayerData();
    }
    
    private void LoadPlayerData()
    {
        // Load from PlayerPrefs, file, or remote server
        playerData = LoadFromStorage();
    }
    
    private void SavePlayerData()
    {
        // Save to persistent storage
        SaveToStorage(playerData);
    }
}
```

### Setting Up Audio with Categories

```csharp
public class GameAudioController : MonoBehaviour
{
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip buttonSound;
    
    private IAudioManager audioManager;
    
    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        
        // Start background music
        audioManager.PlayMusic(AudioClipType.MusicBackground, backgroundMusic, 0.6f);
    }
    
    public void OnButtonClick()
    {
        // Play UI sound effect
        audioManager.PlaySound(buttonSound, 1.0f, 0.8f);
    }
}
```

### Creating a Scene Controller

```csharp
using Core;

public class MainMenuController : SceneController
{
    public override void Initialize()
    {
        Debug.Log("Main Menu scene initialized");
        // Setup main menu specific logic
    }
    
    public override void UpdateContext(float deltaTime)
    {
        // Update main menu logic
    }
    
    public override void ChangeGameToPause(bool isPause)
    {
        // Handle pause state for main menu
        if (isPause)
        {
            // Show pause overlay
        }
        else
        {
            // Hide pause overlay
        }
    }
}
```

### Using the Startup Task System

```csharp
[CreateAssetMenu(fileName = "InitializeAudioTask", menuName = "Startup Tasks/Initialize Audio")]
public class InitializeAudioTask : StartupTaskBase
{
    public override void Execute()
    {
        Debug.Log("Initializing audio system...");
        
        // Simulate async initialization
        StartCoroutine(InitializeAsync());
    }
    
    private IEnumerator InitializeAsync()
    {
        yield return new WaitForSeconds(1f);
        
        HasCompleted = true;
        Debug.Log("Audio system initialized!");
    }
}
```

---

## Advanced Features

### Dependency Injection with VContainer

```csharp
// Register services in LifetimeScope
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IPlayerService, PlayerService>(Lifetime.Singleton);
        builder.Register<IGameDataService, GameDataService>(Lifetime.Singleton);
    }
}

// Inject dependencies in your systems
public class GameplaySystem : BaseSystem
{
    private IPlayerService playerService;
    private IGameDataService gameDataService;
    
    [Inject]
    public void Construct(IPlayerService player, IGameDataService gameData)
    {
        playerService = player;
        gameDataService = gameData;
    }
}
```

### Game Events for Decoupled Communication

```csharp
// Define event types
public class PlayerLevelUpEvent : IGameEvent
{
    public int NewLevel { get; set; }
    public int ExperienceGained { get; set; }

    public static PlayerLevelUpEvent GetInstance()
    {
        return _instance;
    }
   
    public void Reset()
    {
        NewLevel = 0;
        ExperienceGained = 0;
    }
}

// Publishing events
public class PlayerController : MonoBehaviour
{
    [Inject]
    private IEventManager eventManager;
    
    public void LevelUp()
    {
        eventManager.Dispatch<PlayerLevelUpEvent>(new PlayerLevelUpEvent 
        { 
            NewLevel = currentLevel + 1,
            ExperienceGained = 1000
        });
    }
}

// Subscribing to events
public class UIController : MonoBehaviour
{
    [Inject]
    private IEventManager eventManager;

    private void OnEnable()
    {
         eventManager.AddListener<PlayerLevelUpEvent>(OnPlayerLevelUp);
    }

    private void OnEnable()
    {
         eventManager.RemoveListener<PlayerLevelUpEvent>(OnPlayerLevelUp);
    }
    
    private void OnPlayerLevelUp(PlayerLevelUpEvent eventData)
    {
        ShowLevelUpNotification(eventData.NewLevel);
    }
}
```

### Object Pooling

```csharp
public class ProjectileSystem : MonoBehaviour
{
    private IObjectPooling objectPooling;
    
    [Inject]
    public void Construct(IObjectPooling pooling)
    {
        objectPooling = pooling;
    }
    
    public async void FireProjectile()
    {
        // Get projectile from pool
        var projectile = await objectPooling.Get("Bullet"); 
        
        // Configure and fire
        projectile.transform.position = firePoint.position;
        projectile.GetComponent<Projectile>().Fire(target);

        // Or
        Projectile projectile = await objectPooling.Get<Projectile>("Bullet"); 
    }
}
```

---
## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

- **Email**: maktrung@gmail.com
- **Documentation**: [Wiki](https://github.com/trungmk/unity_casual_template/wiki)

---

**Made with ❤️ for the Unity community**
