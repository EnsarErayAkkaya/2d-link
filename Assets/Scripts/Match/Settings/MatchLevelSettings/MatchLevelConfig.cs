using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

namespace Match.Settings
{
    [CreateAssetMenu(fileName = "MatchLevelConfig", menuName = "Match/Level/Match Level Config", order = 0)]
    public class MatchLevelConfig
    {
        public MatchLevelData matchLevelData;

        public MatchLevelData GetLevelData()
        {
            return matchLevelData;
        }

        public async Task LoadLevel()
        {
            //await ResolveServices.SceneService.LoadGameScene();
        }

        public async Task UnloadLevel()
        {
            /*await UniTask.WhenAll(
                ResolveServices.SceneService.LoadMenuScene().AsUniTask(),
                ResolveServices.SceneService.RemoveScene(ResolveServices.SceneService.Settings.gameSceneConfig).AsUniTask());*/
        }
    }
}