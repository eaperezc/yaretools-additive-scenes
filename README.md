# Additive Scenes

Lightweight runtime + editor tools for additive multi-scene Unity workflows.

## What's in it

- **`SceneLoader`** (runtime MonoBehaviour) — additively loads/unloads a list of scenes by name with progress tracking. Custom inspector includes a **Refresh from Build Settings** button to populate the list.
- **Scene Loader window** (Editor) — `Window > Scene Loader`. Auto-discovers enabled scenes from Build Settings and lets you Load / Unload / Ping each one, plus Load All / Unload All. Works in edit mode and play mode.

## Install

In Unity Package Manager → **Add package from git URL…**:

```
https://github.com/<your-user>/unity-additive-scenes.git
```

Requires Unity 2021.3+.

## Usage

### Runtime

Add a `SceneLoader` component to a GameObject in your bootstrap scene. Click **Refresh from Build Settings** in the inspector to populate the scene list, or fill it manually.

```csharp
using YareTools;

public class Boot : MonoBehaviour
{
    [SerializeField] SceneLoader loader;

    void Start()
    {
        loader.LoadAll(onComplete: () => Debug.Log("All loaded"));
    }
}
```

`SceneLoader` exposes `LoadAll`, `UnloadAll`, `Load(name)`, `Unload(name)`, plus `IsLoading`, `Progress`, and `CurrentSceneName` for hooking up loading screens.

### Editor

Open `Window > Scene Loader`. Each scene enabled in Build Settings gets a row with Load/Unload/Ping. Use **Refresh** to re-scan Build Settings.

In edit mode, the first scene opened uses `OpenSceneMode.Single` (so you don't end up with stale scenes); subsequent loads are additive. The window refuses to close the last open scene.

## License

MIT
