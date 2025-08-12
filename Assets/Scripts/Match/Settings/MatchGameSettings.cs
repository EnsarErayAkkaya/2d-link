using NaughtyAttributes;
using Match.Grid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match.Settings
{
    [CreateAssetMenu(fileName = "MatchGameSettings", menuName = "Match/Match Game Settings", order = 0)]
    public class MatchGameSettings : ScriptableObject
    {
        [BoxGroup("Base Match Item Configs")]
        public string baseMatchItemSetId = "default";
        [BoxGroup("Base Match Item Configs")]
        public List<MatchItemSet> baseMatchItemSets;

        [BoxGroup("Border and Background")]
        public SpriteRenderer borderLinePrefab;
        [BoxGroup("Border and Background")]
        public SpriteRenderer cellBackgroundPrefab;

        [BoxGroup("Game Rules")]
        public bool canDropDiagonal;

        public BaseMatchItemConfig GetMatchItemConfig(string setId, string itemKey)
        {
            return baseMatchItemSets.FirstOrDefault(s => s.Id == setId).configs.FirstOrDefault(z => z.Id == itemKey);
        }
    }
}