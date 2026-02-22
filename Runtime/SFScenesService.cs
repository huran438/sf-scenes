using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SFramework.Configs.Runtime;
using SFramework.Core.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
namespace SFramework.Scenes.Runtime
{
    public sealed class SFScenesService : ISFScenesService
    {
        public event Action<string> OnSceneLoad = s => { };
        public event Action<string> OnSceneUnload = s => { };
        public event Action<string> OnSceneLoaded = s => { };
        public event Action<string> OnSceneUnloaded = s => { };

        private readonly Dictionary<string, SceneInstance> _loadedScenes = new();
        private readonly List<string> _loadingScenes = new();
        private readonly Dictionary<string, string> _availableScenes = new();
        private readonly Dictionary<Scene, SceneInstance> _sceneToSceneInstance = new();
        private readonly Dictionary<SceneInstance, Scene> _sceneInstanceToScene = new();
        private readonly Dictionary<SceneInstance, string> _sceneInstanceToSFScene = new();

        private readonly Dictionary<string, Scene> _externallyLoadedScenes = new();
        private readonly Dictionary<Scene, string> _externallyLoadedSfScenes = new();

        private readonly ISFConfigsService _configsService;

        SFScenesService(ISFConfigsService configsService)
        {
            _configsService = configsService;
        }
        
        public UniTask Init(CancellationToken cancellationToken)
        {
            var scenesConfig = _configsService.GetConfigs<SFScenesConfig>().FirstOrDefault();

            foreach (var groupContainer in scenesConfig.Children)
            {
                foreach (SFSceneNode sceneContainer in groupContainer.Children)
                {
                    var scene = $"{scenesConfig.Id}/{groupContainer.Id}/{sceneContainer.Id}";
                    _availableScenes[scene] = sceneContainer.Path;
                }
            }
            
            return UniTask.CompletedTask;
        }

        public bool IsLoading(string sfScene)
        {
            return _loadingScenes.Contains(sfScene);
        }

        public bool IsLoading()
        {
            return _loadingScenes.Count > 0;
        }

        public bool IsLoaded(string sfScene)
        {
            if (_loadedScenes.ContainsKey(sfScene)) return true;

            if (_externallyLoadedScenes.TryGetValue(sfScene, out var scene))
            {
                if (scene.IsValid() && scene.isLoaded) return true;
                _externallyLoadedSfScenes.Remove(scene);
                _externallyLoadedScenes.Remove(sfScene);
            }

            return false;
        }

        public bool TryGetScenePath(string sfScene, out string path)
        {
            return _availableScenes.TryGetValue(sfScene, out path);
        }

        public SceneInstance GetScene(string scene)
        {
            return !_loadedScenes.ContainsKey(scene) ? new SceneInstance() : _loadedScenes[scene];
        }

        public bool GetActiveScene(out SceneInstance sceneInstance)
        {
            var activeScene = SceneManager.GetActiveScene();

            if (_sceneToSceneInstance.TryGetValue(activeScene, out var value))
            {
                sceneInstance = value;
                return true;
            }

            sceneInstance = new SceneInstance();
            return false;
        }

        public bool GetActiveScene(out string sfScene)
        {
            var activeScene = SceneManager.GetActiveScene();

            if (_sceneToSceneInstance.TryGetValue(activeScene, out var sceneInstance))
            {
                sfScene = _sceneInstanceToSFScene[sceneInstance];
                return true;
            }

            if (_externallyLoadedSfScenes.TryGetValue(activeScene, out sfScene))
            {
                return true;
            }

            sfScene = string.Empty;
            return false;
        }

        public async UniTask<SceneInstance> LoadScene(string sfScene, bool setActive)
        {
            if (!_availableScenes.ContainsKey(sfScene)) return new SceneInstance();

            if (_loadedScenes.TryGetValue(sfScene, out var loadedSceneInstance))
            {
                if (setActive)
                {
                    SceneManager.SetActiveScene(loadedSceneInstance.Scene);
                }

                return loadedSceneInstance;
            }

            if (_availableScenes.TryGetValue(sfScene, out var assetReference) && !string.IsNullOrWhiteSpace(assetReference))
            {
                var alreadyLoadedScene = SceneManager.GetSceneByPath(assetReference);

                if (!alreadyLoadedScene.IsValid() || !alreadyLoadedScene.isLoaded)
                {
                    var sceneName = Path.GetFileNameWithoutExtension(assetReference);
                    if (!string.IsNullOrWhiteSpace(sceneName))
                    {
                        alreadyLoadedScene = SceneManager.GetSceneByName(sceneName);
                    }
                }

                if (alreadyLoadedScene.IsValid() && alreadyLoadedScene.isLoaded)
                {
                    _externallyLoadedScenes[sfScene] = alreadyLoadedScene;
                    _externallyLoadedSfScenes[alreadyLoadedScene] = sfScene;

                    _loadingScenes.Add(sfScene);
                    OnSceneLoad.Invoke(sfScene);
                    _loadingScenes.Remove(sfScene);

                    if (setActive)
                    {
                        SceneManager.SetActiveScene(alreadyLoadedScene);
                    }

                    OnSceneLoaded.Invoke(sfScene);
                    return new SceneInstance();
                }

                if (Application.isEditor)
                {
                    alreadyLoadedScene = await TryFindLoadedSceneByAddressablesKey(assetReference);

                    if (alreadyLoadedScene.IsValid() && alreadyLoadedScene.isLoaded)
                    {
                        _externallyLoadedScenes[sfScene] = alreadyLoadedScene;
                        _externallyLoadedSfScenes[alreadyLoadedScene] = sfScene;

                        _loadingScenes.Add(sfScene);
                        OnSceneLoad.Invoke(sfScene);
                        _loadingScenes.Remove(sfScene);

                        if (setActive)
                        {
                            SceneManager.SetActiveScene(alreadyLoadedScene);
                        }

                        OnSceneLoaded.Invoke(sfScene);
                        return new SceneInstance();
                    }
                }
            }

            _loadingScenes.Add(sfScene);
            OnSceneLoad.Invoke(sfScene);

            assetReference = _availableScenes[sfScene];
            var sceneInstance = await Addressables.LoadSceneAsync(assetReference, LoadSceneMode.Additive);
            var scene = sceneInstance.Scene;
            _loadingScenes.Remove(sfScene);
            _loadedScenes[sfScene] = sceneInstance;
            _sceneInstanceToScene[sceneInstance] = scene;
            _sceneInstanceToSFScene[sceneInstance] = sfScene;
            _sceneToSceneInstance[scene] = sceneInstance;
            if (setActive)
            {
                SceneManager.SetActiveScene(scene);
            }
            OnSceneLoaded.Invoke(sfScene);
            return sceneInstance;
        }

        private static async UniTask<Scene> TryFindLoadedSceneByAddressablesKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return default;

            var locationsHandle = Addressables.LoadResourceLocationsAsync(key);
            try
            {
                await locationsHandle.Task;
                if (locationsHandle.Status != AsyncOperationStatus.Succeeded) return default;

                var locations = locationsHandle.Result;
                if (locations == null || locations.Count == 0) return default;

                for (var i = 0; i < SceneManager.sceneCount; i++)
                {
                    var loadedScene = SceneManager.GetSceneAt(i);
                    if (!loadedScene.IsValid() || !loadedScene.isLoaded) continue;

                    var loadedPath = loadedScene.path;

                    foreach (IResourceLocation location in locations)
                    {
                        var internalId = location?.InternalId;
                        if (string.IsNullOrWhiteSpace(internalId)) continue;

                        if (!string.IsNullOrWhiteSpace(loadedPath) && string.Equals(loadedPath, internalId))
                        {
                            return loadedScene;
                        }

                        var fileName = Path.GetFileNameWithoutExtension(internalId);
                        if (!string.IsNullOrWhiteSpace(fileName) && loadedScene.name == fileName)
                        {
                            return loadedScene;
                        }
                    }
                }

                return default;
            }
            finally
            {
                if (locationsHandle.IsValid())
                {
                    Addressables.Release(locationsHandle);
                }
            }
        }

        public async UniTask UnloadScene(string sfScene)
        {
            if (!_loadedScenes.ContainsKey(sfScene))
            {
                if (_externallyLoadedScenes.TryGetValue(sfScene, out var externalScene))
                {
                    if (!externalScene.IsValid() || !externalScene.isLoaded) return;

                    _loadingScenes.Add(sfScene);
                    OnSceneUnload.Invoke(sfScene);
                    await SceneManager.UnloadSceneAsync(externalScene).ToUniTask();
                    _loadingScenes.Remove(sfScene);

                    _externallyLoadedSfScenes.Remove(externalScene);
                    _externallyLoadedScenes.Remove(sfScene);

                    OnSceneUnloaded.Invoke(sfScene);
                }

                return;
            }

            _loadingScenes.Add(sfScene);
            OnSceneUnload.Invoke(sfScene);
            var sceneInstance = _loadedScenes[sfScene];
            var scene = _sceneInstanceToScene[sceneInstance];
            await Addressables.UnloadSceneAsync(sceneInstance).ToUniTask();
            _loadingScenes.Remove(sfScene);
            _loadedScenes.Remove(sfScene);
            _sceneInstanceToScene.Remove(sceneInstance);
            _sceneInstanceToSFScene.Remove(sceneInstance);
            _sceneToSceneInstance.Remove(scene);
            OnSceneUnloaded.Invoke(sfScene);
        }

        public async UniTask UnloadScene(SceneInstance sceneInstance)
        {
            if (!_sceneInstanceToSFScene.TryGetValue(sceneInstance, out var sfScene))
            {
                if (!sceneInstance.Scene.IsValid() || !sceneInstance.Scene.isLoaded) return;
                await Addressables.UnloadSceneAsync(sceneInstance).ToUniTask();
                return;
            }

            if (!_loadingScenes.Contains(sfScene))
            {
                _loadingScenes.Add(sfScene);
            }

            OnSceneUnload.Invoke(sfScene);

            var scene = _sceneInstanceToScene.TryGetValue(sceneInstance, out var value) ? value : sceneInstance.Scene;
            await Addressables.UnloadSceneAsync(sceneInstance).ToUniTask();

            _loadingScenes.Remove(sfScene);
            _loadedScenes.Remove(sfScene);
            _sceneInstanceToScene.Remove(sceneInstance);
            _sceneInstanceToSFScene.Remove(sceneInstance);

            if (scene.IsValid())
            {
                _sceneToSceneInstance.Remove(scene);
            }

            OnSceneUnloaded.Invoke(sfScene);
        }

        public async UniTask<SceneInstance> ReloadScene(string sfScene)
        {
            var isActiveScene = false;

            if (GetActiveScene(out string activeScene))
            {
                if (sfScene == activeScene)
                {
                    isActiveScene = true;
                }
            }

            if (!_loadedScenes.ContainsKey(sfScene)) return new SceneInstance();
            _loadingScenes.Add(sfScene);
            OnSceneUnload.Invoke(sfScene);
            var sceneInstance = _loadedScenes[sfScene];
            var scene = _sceneInstanceToScene[sceneInstance];
            await Addressables.UnloadSceneAsync(sceneInstance).ToUniTask();
            _loadedScenes.Remove(sfScene);
            _sceneInstanceToSFScene.Remove(sceneInstance);
            _sceneInstanceToScene.Remove(sceneInstance);
            _sceneToSceneInstance.Remove(scene);
            OnSceneUnloaded.Invoke(sfScene);

            if (!_availableScenes.ContainsKey(sfScene)) return new SceneInstance();
            OnSceneLoad.Invoke(sfScene);
            var assetReference = _availableScenes[sfScene];
            sceneInstance = await Addressables.LoadSceneAsync(assetReference, LoadSceneMode.Additive).ToUniTask();
            scene = sceneInstance.Scene;
            _loadingScenes.Remove(sfScene);
            _loadedScenes[sfScene] = sceneInstance;
            _sceneInstanceToScene[sceneInstance] = scene;
            _sceneInstanceToSFScene[sceneInstance] = sfScene;
            _sceneToSceneInstance[scene] = sceneInstance;

            if (isActiveScene)
            {
                SceneManager.SetActiveScene(scene);
            }
            
            OnSceneLoaded.Invoke(sfScene);
            return sceneInstance;
        }

        public void Dispose()
        {
        }

    }
}