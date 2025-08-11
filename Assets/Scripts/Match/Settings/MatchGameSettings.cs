using NaughtyAttributes;
using Match.Grid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match.Settings
{
    [CreateAssetMenu(fileName = "MatchGameSettings", menuName = "Game/Match/Match Game Settings", order = 0)]
    public class MatchGameSettings : ScriptableObject
    {
        [BoxGroup("Base Match Item Configs")]
        public string baseMatchItemSetId = "default";
        [BoxGroup("Base Match Item Configs")]
        public List<MatchItemSet> baseMatchItemSets;

        [BoxGroup("Obstacle Item Configs")]
        public string obstaclesSetId = "default";
        [BoxGroup("Obstacle Item Configs")]
        public List<MatchItemSet> obstacleSets;

        [BoxGroup("Booster Item Configs")]
        public string boostersSetId = "default";
        [BoxGroup("Booster Item Configs")]
        public List<MatchItemSet> boosterSets;

        [BoxGroup("Collectable Item Configs")]
        public string collectablesSetId = "default";
        [BoxGroup("Collectable Item Configs")]
        public List<MatchItemSet> collectableSets;

        [BoxGroup("Border and Background")]
        public SpriteRenderer borderLinePrefab;
        [BoxGroup("Border and Background")]
        public SpriteRenderer cellBackgroundPrefab;

        [BoxGroup("Game Rules")]
        [SerializeField]
        internal MergeRules mergeRules;
        [BoxGroup("Game Rules")]
        public bool canDropDiagonal;

        [BoxGroup("Animations")]
        [SerializeField]
        internal GridAnimationSettings animationSettings;

        public BaseMatchItemConfig GetMatchItemConfig(string setId, string itemKey)
        {
            return baseMatchItemSets.FirstOrDefault(s => s.Id == setId).configs.FirstOrDefault(z => z.Id == itemKey);
        }
        public BaseMatchItemConfig GetObstacleConfig(string setId, string itemKey)
        {
            return obstacleSets.FirstOrDefault(s => s.Id == setId).configs.FirstOrDefault(z => z.Id == itemKey);
        }
        public BaseMatchItemConfig GetBoosterConfig(string setId, string itemKey)
        {
            return boosterSets.FirstOrDefault(s => s.Id == setId).configs.FirstOrDefault(z => z.Id == itemKey);
        }
        public BaseMatchItemConfig GetCollectableConfig(string setId, string itemKey)
        {
            return collectableSets.FirstOrDefault(s => s.Id == setId).configs.FirstOrDefault(z => z.Id == itemKey);
        }
    }
}