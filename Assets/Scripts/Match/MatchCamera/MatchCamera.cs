using Match.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Match
{
    public class MatchCamera : MonoBehaviour
    {
        private void Start()
        {
            MatchLevelData levelData = MatchGameService.MatchLevelData;

            Camera.main.orthographicSize = levelData.size.x * MatchGameService.GridController.CellSize.x * 1.1f;
        }
    }
}