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
        public int moveCount;
        public List<ListWrapper<MatchItemType>> levelStartingItems;
        public List<ListWrapper<bool>> levelGridSetup;
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
