using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Match
{
    public class ScoreController
    {
        public int Score { get; private set; }

        public Action<int> OnScoreUpdated;

        public void Init()
        {
            MoveController moveController = MatchGameService.MoveController;

            moveController.OnLinkCompleted += UpdateScore;
        }

        private void UpdateScore(Stack<Vector3Int> link)
        {
            // Update the score based on the completed link
            Score += link.Count * 10;

            OnScoreUpdated?.Invoke(Score);
        }   
    }
}