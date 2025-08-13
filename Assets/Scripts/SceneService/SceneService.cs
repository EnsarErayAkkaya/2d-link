using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BaseServices.SceneServices
{
    public class SceneService : MonoBehaviour, ISceneService
    {
        [SerializeField] private SceneServiceSettings settings;

        public SceneServiceSettings Settings => settings;

        public Action<SceneConfig> OnSceneTransitionStarted { get; set; }

        public Action<SceneConfig> OnSceneTransitionCompleted { get; set; }

        public static SceneService Instance { get; private set; }

        private void Start()
        {
            Instance = this;

            Initialize();
        }

        public async void Initialize()
        {
            if (settings.gameSceneConfig != null)
            {
                await LoadGameScene();
            }
        }

        public async Task LoadGameScene()
        {
            await LoadScene(settings.gameSceneConfig);
        }

        public async Task LoadScene(SceneConfig sceneConfig, float delay = 0)
        {
            try
            {
                if (sceneConfig == null) return;

                SceneReference[] sceneReferences = GetOpenScenes();

                OnSceneTransitionStarted?.Invoke(sceneConfig);

                if (delay != 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delay));
                }

                if (sceneConfig.removeAllOtherScenes)
                {
                    List<UniTask> tasks = new List<UniTask>();
                    for (int i = 0; i < sceneReferences.Length; i++)
                    {
                        if (sceneReferences[i].BuildIndex == settings.baseSceneConfig.sceneReference.BuildIndex) continue;

                        tasks.Add(RemoveScene(sceneReferences[i]).AsUniTask());
                    }

                    await UniTask.WhenAll(tasks);
                }

                await LoadScene(sceneConfig.sceneReference, sceneConfig.loadMode);

                OnSceneTransitionCompleted?.Invoke(sceneConfig);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public async Task RemoveScene(SceneConfig sceneConfig, float delay = 0)
        {
            try
            {
                if (sceneConfig == null || !IsSceneActive(sceneConfig)) return;

                if (delay != 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delay));
                }

                await RemoveScene(sceneConfig.sceneReference);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async Task LoadScene(SceneReference sceneReference, LoadSceneMode loadMode)
        {
            await SceneManager.LoadSceneAsync(sceneReference.BuildIndex, loadMode);

        }
        private async Task RemoveScene(SceneReference sceneReference)
        {
            await SceneManager.UnloadSceneAsync(sceneReference.BuildIndex);

        }

        public SceneReference[] GetOpenScenes()
        {
            int sceneCount = SceneManager.sceneCount;

            SceneReference[] sceneReferences = new SceneReference[sceneCount];

            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                sceneReferences[i] = settings.GetSceneReferenceByBuildIndex(scene.buildIndex);
            }

            return sceneReferences;
        }

        public bool IsSceneActive(SceneConfig sceneConfig)
        {
            SceneReference[] sceneReferences = GetOpenScenes();

            for (int i = 0; i < sceneReferences.Length; i++)
            {
                if (sceneReferences[i] != null && (sceneConfig.sceneReference.BuildIndex == sceneReferences[i].BuildIndex))
                {
                    return true;
                }
            }
            return false;
        }
    }
}