using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace Match.UI
{
    public class MatchUIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI moveCountText;
        [SerializeField] private TextMeshProUGUI scoreText;

        private ScoreController scoreController;

        private void Start()
        {
            scoreController = FindObjectOfType<ScoreController>();

            scoreController.OnScoreUpdated += UpdateScore;

            MatchGameService.MoveController.OnLinkCompleted += OnLinkCompleted;

            scoreText.text = $"0/{MatchGameService.MatchLevelData.targetScore}";
        }

        private void OnLinkCompleted(Stack<Vector3Int> stack)
        {
            moveCountText.text = (MatchGameService.MatchLevelData.moveCount - MatchGameService.MoveController.MoveMade).ToString();
        }

        private void UpdateScore(int score)
        {
            scoreText.text = $"{score}/{MatchGameService.MatchLevelData.targetScore}";
        }
    }
}