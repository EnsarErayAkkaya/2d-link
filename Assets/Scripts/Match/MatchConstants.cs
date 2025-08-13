using UnityEngine;

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

        public static readonly Vector3Int[] CellDiagonalNeighbours = {
            new Vector3Int(1, 1),
            new Vector3Int(-1, -1),
            new Vector3Int(1, -1),
            new Vector3Int(-1, 1)
        };

        /// <summary>
        /// This method returns all match item names including the random item key.
        /// Inspite of an enum this allows a safer way to add new match items without breaking existing levels.
        /// </summary>
        /// <returns></returns>
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