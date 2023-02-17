using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFramework.Core.Runtime;
using SFramework.Repositories.Runtime;
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

        

        [SFInject]
        public void Init(ISFRepositoryProvider provider)
        {
            var _repository = provider.GetRepositories<SFScenesRepository>().FirstOrDefault();
            
            foreach (var groupContainer in _repository.Nodes)
            {
                foreach (SFSceneNode sceneContainer in groupContainer.Nodes)
                {
                    var scene = $"{groupContainer._Name}/{sceneContainer._Name}";
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

            if (_sceneToSceneInstance.ContainsKey(activeScene))
            {
                sceneInstance = _sceneToSceneInstance[activeScene];
                return true;
            }

            sceneInstance = new SceneInstance();
            return false;
        }

        public bool GetActiveScene(out string sfScene)
        {
            var activeScene = SceneManager.GetActiveScene();

            if (_sceneToSceneInstance.ContainsKey(activeScene))
            {
                var sceneInstance = _sceneToSceneInstance[activeScene];
                sfScene = _sceneInstanceToSFScene[sceneInstance];
                return true;
            }

            sfScene = string.Empty;
            return false;
        }

        public async Task<SceneInstance> LoadScene(string sfScene, bool setActive,
            Action<SceneInstance> onDone = null)
        {
            if (!_availableScenes.ContainsKey(sfScene)) return new SceneInstance();
            _loadingScenes.Add(sfScene);
            OnSceneLoad.Invoke(sfScene);
            var assetReference = _availableScenes[sfScene];
            var asyncOperationHandle = Addressables.LoadSceneAsync(assetReference, LoadSceneMode.Additive);
            await asyncOperationHandle.Task;
            var sceneInstance = asyncOperationHandle.Result;
            var scene = sceneInstance.Scene;
            _loadingScenes.Remove(sfScene);
            _loadedScenes[sfScene] = sceneInstance;
            _sceneInstanceToScene[sceneInstance] = scene;
            _sceneInstanceToSFScene[sceneInstance] = sfScene;
            _sceneToSceneInstance[scene] = sceneInstance;
            if (setActive)
                SceneManager.SetActiveScene(scene);
            onDone?.Invoke(sceneInstance);
            OnSceneLoaded.Invoke(sfScene);
            return sceneInstance;
        }

        public async Task UnloadScene(string sfScene, Action onDone = null)
        {
            if (!_loadedScenes.ContainsKey(sfScene)) return;
            _loadingScenes.Add(sfScene);
            OnSceneUnload.Invoke(sfScene);
            var sceneInstance = _loadedScenes[sfScene];
            var scene = _sceneInstanceToScene[sceneInstance];
            var asyncOperationHandle = Addressables.UnloadSceneAsync(sceneInstance);
            await asyncOperationHandle.Task;
            _loadingScenes.Remove(sfScene);
            _loadedScenes.Remove(sfScene);
            _sceneInstanceToScene.Remove(sceneInstance);
            _sceneInstanceToSFScene.Remove(sceneInstance);
            _sceneToSceneInstance.Remove(scene);
            onDone?.Invoke();
            OnSceneUnloaded.Invoke(sfScene);
        }

        public async Task<SceneInstance> ReloadScene(string sfScene, Action onUnloaded = null,
            Action<SceneInstance> onLoaded = null)
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
            var asyncOperationHandle = Addressables.UnloadSceneAsync(sceneInstance);
            await asyncOperationHandle.Task;
            _loadedScenes.Remove(sfScene);
            _sceneInstanceToSFScene.Remove(sceneInstance);
            _sceneInstanceToScene.Remove(sceneInstance);
            _sceneToSceneInstance.Remove(scene);
            onUnloaded?.Invoke();
            OnSceneUnloaded.Invoke(sfScene);

            if (!_availableScenes.ContainsKey(sfScene)) return new SceneInstance();
            OnSceneLoad.Invoke(sfScene);
            var assetReference = _availableScenes[sfScene];
            asyncOperationHandle = Addressables.LoadSceneAsync(assetReference, LoadSceneMode.Additive);
            await asyncOperationHandle.Task;
            sceneInstance = asyncOperationHandle.Result;
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

            onLoaded?.Invoke(sceneInstance);
            OnSceneLoaded.Invoke(sfScene);
            return sceneInstance;
        }

        public void Dispose()
        {
        }
    }
}