using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace Match.Settings
{
    [System.Serializable]
    public class MatchLevelData
    {
        public string Id;
        public Vector2Int size;
        public int moveCount;
        public List<LevelTarget> targets;
        public List<ColumnDroppableTypes> columnDroppableTypes;
        public List<ListWrapper<MatchItemType>> levelStartingItems;
        public List<ListWrapper<bool>> levelGridSetup;
    }

    [System.Serializable]
    public class ListWrapper<T>
    {
        public List<T> row;
    }

    [System.Serializable]
    public class LevelTarget
    {
        public MatchItemType matchItem;
        public int count;
        public int minCountOnGrid = 0; // 0 means doesn't matter
    }

    [System.Serializable]
    public class ColumnDroppableTypes
    {
        public int columnIndex;
        public List<MatchItemType> possibeTypes;
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
