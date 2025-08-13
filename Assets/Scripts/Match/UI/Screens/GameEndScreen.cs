using BaseServices.SceneServices;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Match.UI
{
    public class GameEndScreen : BaseScreen
    {
        [Header("Game End Screen")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private TextMeshProUGUI retryButtonText;

        public void Setup(bool isWin)
        {
            if (isWin)
            {
                // Setup for win
                resultText.text = "You Win!";
                retryButtonText.text = "Continue";
            }
            else
            {
                // Setup for lose
                resultText.text = "You Lose!";
                retryButtonText.text = "Retry";
            }
        }

        public async void OnRetryButtonClicked()
        {
            await SceneService.Instance.RemoveScene(SceneService.Instance.Settings.gameSceneConfig).AsUniTask();
            await SceneService.Instance.LoadGameScene().AsUniTask();
        }
    }
}