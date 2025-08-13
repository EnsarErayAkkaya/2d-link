using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Match
{
    public class ScoreController : MonoBehaviour
    {
        public int Score { get; private set; }

        public Action<int> OnScoreUpdated;

        private void Start()
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