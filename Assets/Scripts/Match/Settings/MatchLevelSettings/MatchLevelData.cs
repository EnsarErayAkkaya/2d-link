using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace Match.Settings
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "MatchLevelData", menuName = "Match/Levels/MatchLevelData", order = 1)]
    public class MatchLevelData : ScriptableObject
    {
        public string Id;
        public Vector2Int size;

        [InfoBox("Level starting items are used to fill the grid at the start of the level. default is random")]
        public List<ListWrapper<MatchItemType>> levelStartingItems;

        [InfoBox("Grid shape controls shape of the grid. Default shape is a filled rectangle according to size. You can create empty cells with setting shape false")]
        public List<ListWrapper<bool>> levelGridShape;

        [Header("Level Targets")]
        public int moveCount;
        public int targetScore;
    }

    [System.Serializable]
    public class ListWrapper<T>
    {
        public List<T> row;
    }

    [System.Serializable]
    public class MatchItemType
    {
        [Dropdown("GetMatchItemNames")]
        public string item;

        private string[] GetMatchItemNames()
        {
            return MatchConstants.GetAllMatchItemNames();
        }
    }
}
