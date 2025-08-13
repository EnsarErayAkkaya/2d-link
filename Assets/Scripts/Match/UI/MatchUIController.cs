using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Match.UI
{
    public class MatchUIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI moveCountText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private GameEndScreen gameEndScreen;

        private ScoreController scoreController;

        private void Start()
        {
            scoreController = FindObjectOfType<ScoreController>();

            scoreController.OnScoreUpdated += UpdateScore;

            MatchGameService.MoveController.OnLinkCompleted += OnLinkCompleted;

            scoreText.text = $"0/{MatchGameService.MatchLevelData.targetScore}";
            moveCountText.text = (MatchGameService.MatchLevelData.moveCount - MatchGameService.MoveController.MoveMade).ToString();
        }

        private void OnLinkCompleted(Stack<Vector3Int> stack)
        {
            moveCountText.text = (MatchGameService.MatchLevelData.moveCount - MatchGameService.MoveController.MoveMade).ToString();

            if (MatchGameService.MoveController.MoveMade >= MatchGameService.MatchLevelData.moveCount)
            {
                gameEndScreen.Setup(scoreController.Score >= MatchGameService.MatchLevelData.targetScore);
                gameEndScreen.Show();
            }
        }

        private void UpdateScore(int score)
        {
            scoreText.text = $"{score}/{MatchGameService.MatchLevelData.targetScore}";
        }
    }
}