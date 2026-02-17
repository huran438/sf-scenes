using System;
using SFramework.Core.Runtime;
using UnityEngine.ResourceManagement.ResourceProviders;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;



// ReSharper disable once CheckNamespace
namespace SFramework.Scenes.Runtime
{
    public interface ISFScenesService : ISFService
    {
        event Action<string> OnSceneLoad;
        event Action<string> OnSceneUnload;
        event Action<string> OnSceneLoaded;
        event Action<string> OnSceneUnloaded;
        bool IsLoading(string sfScene);
        bool IsLoading();
        bool IsLoaded(string sfScene);
        bool TryGetScenePath(string sfScene, out string path);
        bool GetActiveScene(out string sfScene);
        SceneInstance GetScene(string sfScene);
        bool GetActiveScene(out SceneInstance sceneInstance);
        UniTask<SceneInstance> LoadScene(string sfScene, bool setActive);
        UniTask UnloadScene(string sfScene);
        UniTask UnloadScene(SceneInstance sceneInstance);
        UniTask<SceneInstance> ReloadScene(string sfScene);
    }
}