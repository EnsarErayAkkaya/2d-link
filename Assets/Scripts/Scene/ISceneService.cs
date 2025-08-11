using Eflatun.SceneReference;
using System;
using System.Threading.Tasks;
namespace BaseServices.SceneServices
{
    public interface ISceneService
    {
        public Action<SceneConfig> OnSceneTransitionStarted { get; set; }
        public Action<SceneConfig> OnSceneTransitionCompleted { get; set; }

        public SceneServiceSettings Settings { get; }

        Task LoadGameScene();

        Task LoadScene(SceneConfig sceneConfig, float delay = 0);
        Task RemoveScene(SceneConfig sceneConfig, float delay = 0);

        SceneReference[] GetOpenScenes();
        bool IsSceneActive(SceneConfig sceneConfig);

    }
}