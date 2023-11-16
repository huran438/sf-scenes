using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using SFramework.Configs.Runtime;
using UnityEngine.AddressableAssets;
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

        SFScenesService(ISFConfigsService provider)
        {
            var _repository = provider.GetRepositories<SFScenesConfig>().FirstOrDefault();

            foreach (var groupContainer in _repository.Nodes)
            {
                foreach (SFSceneNode sceneContainer in groupContainer.Nodes)
                {
                    var scene = $"{_repository.Name}/{groupContainer.Name}/{sceneContainer.Name}";
                    _availableScenes[scene] = sceneContainer.Path;
                }
            }
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
            return _loadedScenes.ContainsKey(sfScene);
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

            sfScene = string.Empty;
            return false;
        }

        public async UniTask<SceneInstance> LoadScene(string sfScene, bool setActive)
        {
            if (!_availableScenes.ContainsKey(sfScene)) return new SceneInstance();
            _loadingScenes.Add(sfScene);
            OnSceneLoad.Invoke(sfScene);
            var assetReference = _availableScenes[sfScene];
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

        public async UniTask UnloadScene(string sfScene)
        {
            if (!_loadedScenes.ContainsKey(sfScene)) return;
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