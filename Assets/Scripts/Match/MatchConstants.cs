using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

namespace Match
{
    public static class MatchConstants
    {
        public static readonly Vector3Int[] CellNeighbours = {
            new Vector3Int(1, 0),
            new Vector3Int(-1, 0),
            new Vector3Int(0, 1),
            new Vector3Int(0, -1)
        };

        public static string[] GetAllMatchItemNames()
        {
            string[] result = new string[BaseMatchItemNames.Length + ObstacleItemNames.Length + BoosterItemNames.Length + CollectableItemNames.Length + 1];

            int count = 0;

            for (int i = 0; i < BaseMatchItemNames.Length; i++)
            {
                result[i] = BaseMatchItemNames[i];
            }

            count += BaseMatchItemNames.Length;

            for (int i = 0; i < ObstacleItemNames.Length; i++)
            {
                result[count + i] = ObstacleItemNames[i];
            }

            count += ObstacleItemNames.Length;

            for (int i = 0; i < BoosterItemNames.Length; i++)
            {
                result[count + i] = BoosterItemNames[i];
            }

            count += BoosterItemNames.Length;

            for (int i = 0; i < CollectableItemNames.Length; i++)
            {
                result[count + i] = CollectableItemNames[i];
            }

            result[result.Length - 1] = randomItemKey;

            return result;
        }

        public static readonly string randomItemKey = "random";

        public static readonly string[] BaseMatchItemNames =
        {
            "red",
            "green",
            "blue",
            "pink",
            "yellow"
        };

        public static readonly string[] ObstacleItemNames =
        {
            "obstacle_1",
            "obstacle_2",
        };
        
        public static readonly string[] BoosterItemNames =
        {
            "rocket",
            "tnt",
            "flying",
            "rainbow",
        };

        public static readonly string[] CollectableItemNames =
        {
            "collect_item_1"
        };
    }
}