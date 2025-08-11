using System;
using System.Linq;
using UnityEngine;

namespace Match.Settings
{
    [CreateAssetMenu(fileName = "MergeRules", menuName = "Game/Match/Merge Rules", order = 2)]
    internal class MergeRules : ScriptableObject
    {
        [SerializeField] private MergeRule[] rules;
        [SerializeField] private BoosterMergeRule[] boosterMergeRules;
        
        public int RuleCount => rules.Length;

        public BoosterMergeRule GetBoosterMergeRule(BoosterConfig config1, BoosterConfig config2)
        {
            return boosterMergeRules.FirstOrDefault(s => 
            (s.mergedBoosters.Any(x => x == config1)) && (s.mergedBoosters.Any(x => x == config2)));
        }

        public MergeRule GetMergeRule(int index, float angle)
        {
            MergeRule mergeRule = new MergeRule();
            
            mergeRule.resultItem = rules[index].resultItem;
            mergeRule.positions = new Vector3Int[rules[index].positions.Length];

            for (int i = 0; i < rules[index].positions.Length; i++)
            {
                mergeRule.positions[i] = GetRotatedVector(rules[index].positions[i], angle);
            }

            return mergeRule;
        }

        private Vector3Int GetRotatedVector(Vector3Int mainVector, float angle)
        {
            if (angle == 0) return mainVector;

            Vector3Int vector = new Vector3Int(mainVector.x, mainVector.y);

            int rotateCount = (int)(angle / 90);
            for (int i = 0; i < rotateCount; i++)
            {
                vector.Set(vector.y, -vector.x, 0);
            }

            return vector;
        }
    }
    [Serializable]
    public class MergeRule
    {
        public Vector3Int[] positions;
        public BaseMatchItemConfig resultItem;
    }

    [Serializable]
    public class BoosterMergeRule
    {
        public BoosterConfig[] mergedBoosters;
        public BaseMatchItemConfig resultItem;
    }
}
