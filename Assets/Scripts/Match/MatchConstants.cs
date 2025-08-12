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
            string[] result = new string[BaseMatchItemNames.Length + 1];

            result[0] = randomItemKey;

            int count = 1;

            for (int i = 0; i < BaseMatchItemNames.Length; i++)
            {
                result[count + i] = BaseMatchItemNames[i];
            }

            return result;
        }

        public static readonly string randomItemKey = "random";

        public static readonly string[] BaseMatchItemNames =
        {
            "red",
            "green",
            "blue",
            "yellow"
        };
    }
}