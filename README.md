Absolutely! Below is the **comprehensive and professional English documentation** for your Unity Framework, formatted to be both internal technical docs and onboarding-friendly for new team members.

---

# Unity Game Framework Documentation

## Table of Contents

1. [Architecture Overview](#architecture)
2. [Core Components](#components)
    - GameHub
    - BaseSystem
    - Audio System
3. [Lifecycle & State Management](#lifecycle)
4. [Extending & Applying to Real Projects](#extension)
5. [Game Interruption Handling](#interruption)
6. [REST API Integration (BestHTTP)](#rest-api)
7. [Operational Notes & Best Practices](#notes)

---

<a name="architecture"></a>
## 1. Architecture Overview

This modular Unity framework is designed for scalability and clean maintainability.  
Core systems are well-isolated and can be re-used or expanded in any game project.

- **GameHub:** The single main entry point, responsible for initializing, pausing/resuming, and coordinating all BaseSystems and SceneManager.
- **BaseSystem:** Abstract base class for game systems (audio, data, gameplay, etc.), providing unified lifecycle hooks (Initialize, OnPause, OnResume, etc.).
- **Audio System:** Manages music, ambient, SFX (with pooling, fade, playlists, events).
- **Unified lifecycle:** All systems are managed through OnLoaded, OnUnloaded, OnPause, OnResume and other standard lifecycle methods.
- **Plug-and-play REST API layer via BestHTTP**, ready for networked/game service requirements.

---

<a name="components"></a>
## 2. Core Components

### a. GameHub

- Root MonoBehaviour; **only one instance** should exist in the root scene.
- Manages targetFrameRate, sleep timeout (Android), and the system group (`_gameSystemGroup`).
- Controls pausing/resuming the whole game, and hooks into `OnApplicationPause`.

**Typical methods:**
- `EnterPoint()`: Initializes BaseSystems and SceneManager.
- `PauseGame()/ResumeGame()`: Directly pauses/unpauses all systems via BaseSystem hooks.
- `OnGamePause`: Global event other systems or UI may subscribe to.

### b. BaseSystem

- Base class (extending MonoBehaviour) for all systems.
- Provides **virtual methods** for all main lifecycle events:  
  - `Initialize()`
  - `OnLoaded()`, `OnUnloaded()`, `OnPreUnloaded()`
  - `OnPause()`, `OnResume()`
- To add new behavior, simply subclass and override the relevant methods.

### c. Audio System

- Separates out music, ambient, and SFX; pools sources as appropriate.
- ScriptableObject (`AudioAsset`) holds all audio metadata (volume, pitch, fade, etc.).
- Automatic pausing/resuming of all audio sources during game pause/resume.
- Fade in/out, playlists, randomization, and callback events are supported.

---

<a name="lifecycle"></a>
## 3. Lifecycle & State Management

- **On startup:**  
    1. GameHub sets framerate, initializes all BaseSystems in the designated group.
    2. SceneManager coordinates initial scene loading and broadcasts events to all systems.
- **On scene load/unload/pre-unload:**  
    - Triggers OnLoaded/OnUnloaded/OnPreUnloaded in every BaseSystem.
- **On pause/resume (app background/foreground, manual pause, etc.):**  
    - GameHub invokes OnPause/OnResume on *all* BaseSystems, synchronizing state across audio, gameplay, data, networking, etc.

**Goal:**  
Guaranteed, unified handling of all systems during any lifecycle transition, to prevent resource leaks or state corruption.

---

<a name="extension"></a>
## 4. Extending & Applying in Any Game

### ✓ To add custom functionality for your game, simply:

1. **Create a system inheriting from BaseSystem:**

    ```csharp
    using Core;
    public class MyGameDataSystem : BaseSystem
    {
        public override void OnPause()
        {
            // Save data, trigger save, clean up, etc.
        }
        public override void OnResume()
        {
            // Restore state, update UI, reload data if needed
        }
    }
    ```

2. **Add this system to the `_gameSystemGroup`** in your scene (or register via Dependency Injection if advanced).

3. **Let the framework handle all lifecycle events automatically!**
    - OnPause/OnResume and other methods will be called without manual setup.

4. **UI or special cases:**  
    Hook into `GameHub.OnGamePause` for popups or custom UI states as needed.

---

<a name="interruption"></a>
## 5. Handling Game Interruptions (Minimize/Switch App/Incoming Calls)

### **Unified Flow:**
- Whenever Unity's `OnApplicationPause` is triggered (minimize, OS context switch, phone call, etc.), or if you manually call PauseGame/ResumeGame:
    1. **GameHub** triggers OnPause/OnResume on all BaseSystems (including Audio, Data, Gameplay, Network, etc.).
    2. **Each system is responsible for its logic:**
        - AudioSystem: Pauses/unpauses music and SFX.
        - DataSystem: Saves checkpoint/progress to be robust against RAM kill.
        - NetworkSystem: Ends online sessions, re-establishes on resume.
        - Gameplay: Halts or freezes time, AI, as appropriate.
    3. For data integrity, saving on Pause is **highly recommended**, since mobile OS may kill the process without notice.

#### ***Best Practices:***
- Always override OnPause/OnResume in any system that manages critical state.
- Ensure all important systems are under `_gameSystemGroup` (or connected for auto-lifecycle handling).
- With coroutines/long-running effects, check whether they must be stopped completely on pause or simply resume where they left off.
- UI overlays or popups can listen to `OnGamePause` for showing and hiding accordingly.

---

<a name="rest-api"></a>
## 6. REST API Integration (BestHTTP)

### **Universal REST API Wrapper (GET/POST, async/await):**
```csharp
public class RestApiService
{
    public async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string> headers = null) {...}
    public async Task<T> PostAsync<T>(string endpoint, object body, Dictionary<string, string> headers = null) {...}
}
```
**How to use:**
```csharp
var api = new RestApiService("https://api-endpoint.com/api/");
// GET profile
UserProfile profile = await api.GetAsync<UserProfile>("user/profile");
// POST login
LoginResponse login = await api.PostAsync<LoginResponse>("auth/login", new { username="u", password="p" });
```
- Can be easily extended for PUT, DELETE, etc.
- Supports JSON (use LitJson, Unity's JsonUtility, or Newtonsoft.Json).
- Clean async/await for robust error handling and integration with game state.

---

<a name="notes"></a>
## 7. Operational Notes & Best Practices

- **Always save critical game/data state on OnPause**, as mobile OS may terminate the app at any time when backgrounded.
- **Verify all important components are children of `_gameSystemGroup`** (for correct lifecycle handling).
- **Mind long-running coroutines/network actions:**  
   On resume, check for timeouts, re-authenticate or reconnect as necessary.
- **Organize Audio and Data Managers** using pooling or singleton architectures for optimal performance.
- **Document every custom system** and its role for maintainability and ease of onboarding.

---

## **Summary**

- **Robust, modular startup and lifecycle system:** All systems are neatly initialized and paused/resumed in sync.
- **Easy extension:** Custom systems are simply added via BaseSystem inheritance and scene/DI registration.
- **Bulletproof interruption handling:** Audio, Data, and other systems operate safely through all state transitions.
- **Ready-to-package:** Structure is suitable for packaging as a Unity package for instant re-use.
- **REST API support built-in:** Async, extensible, plugs into game systems with minimal boilerplate.

---

If you need sample templates for custom ScriptableObjects, DI/IOC integration patterns, or onboarding guides/checklists for new developers, feel free to ask!  
You can use this docs format for both internal handbooks and onboarding or public technical references.
