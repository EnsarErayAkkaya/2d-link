using BaseServices.SceneServices;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    public class GameEndScreen : MonoBehaviour
    {
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

        public void OnRetryButtonClicked()
        {
            SceneService.Instance.LoadGameScene().AsUniTask();
            SceneService.Instance.RemoveScene(SceneService.Instance.Settings.gameSceneConfig).AsUniTask();
        }
    }
}