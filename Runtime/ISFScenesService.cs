using System;
using SFramework.Core.Runtime;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Threading.Tasks;

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
        bool GetActiveScene(out string sfScene);
        SceneInstance GetScene(string sfScene);
        bool GetActiveScene(out SceneInstance sceneInstance);
        Task<SceneInstance> LoadScene(string sfScene, bool setActive, Action<SceneInstance> loadCallback = null);
        Task UnloadScene(string sfScene, Action unloadCallback = null);
        Task<SceneInstance> ReloadScene(string sfScene, Action unloadCallback = null, Action<SceneInstance> loadCallback = null);
    }
}