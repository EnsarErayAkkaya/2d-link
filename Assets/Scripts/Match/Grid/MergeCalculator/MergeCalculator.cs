using Match.Settings;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match.Grid
{
    public class MergeData
    {
        public int priority;
        public List<Vector3Int> mergingCoordinates;
        public BaseMatchItemConfig resultItem;

        public MergeData()
        {
            mergingCoordinates = new List<Vector3Int>();
        }
    }
    internal class MergeCalculator : IMergeCalculator
    {
        public List<MergeData> CalculateMergeableItems()
        {
            var gridItems = MatchGameService.GridController.GridItems;

            List<MergeData> mergeDatas = new List<MergeData>();

            // find all mergeable coordinates
            foreach (var item in gridItems)
            {
                MergeData mergeData = CanCoordinateMerge(item.Key, gridItems[item.Key].MatchItemConfig, Vector3Int.zero);

                if (mergeData != null)
                {
                    // find merge
                    mergeDatas.Add(mergeData);
                }
            }

            // add all mergeDatas Conflict Dictionary for seeing conflicting merges
            Dictionary<Vector3Int, List<MergeData>> mergeConflictDict = new();

            foreach (var item in mergeDatas)
            {
                foreach (var coordinate in item.mergingCoordinates)
                {
                    if (mergeConflictDict.ContainsKey(coordinate))
                    {
                        mergeConflictDict[coordinate].Add(item);
                    }
                    else
                    {
                        mergeConflictDict[coordinate] = new List<MergeData>() { item };
                    }
                }
            }

            // itarete all coordinates and remove items with higher priority 
            List<MergeData> deletedMerges = new List<MergeData>();

            List<Vector3Int> keys = mergeConflictDict.Keys.ToList();

            keys = keys.OrderByDescending(s => mergeConflictDict[s].Count).ToList();

            foreach (var key in keys)
            {
                List<MergeData> _mergeDatas = mergeConflictDict[key];

                if (_mergeDatas.Count >= 1)
                {
                    // remove already deleteds from list
                    foreach (var deleted in deletedMerges)
                    {
                        if (_mergeDatas.Contains(deleted))
                        {
                            _mergeDatas.Remove(deleted);
                        }
                    }

                    if (_mergeDatas.Count > 1)
                    {
                        // has conflict
                        int minPriority = int.MaxValue; // smaller is better
                        int selectedIndex = 0;

                        for (int i = 0; i < _mergeDatas.Count; i++)
                        {
                            if (_mergeDatas[i].priority < minPriority)
                            {
                                selectedIndex = i;
                                minPriority = _mergeDatas[i].priority;
                            }
                        }

                        for (int i = 0; i < selectedIndex; i++)
                        {
                            deletedMerges.Add(_mergeDatas[0]);
                            _mergeDatas.RemoveAt(0);
                        }

                        for (int i = 0; i < _mergeDatas.Count - 1;)
                        {
                            deletedMerges.Add(_mergeDatas[1]);
                            _mergeDatas.RemoveAt(1);
                        }
                    }
                }


            }

            List<MergeData> result = new List<MergeData>();
            foreach (var item in mergeConflictDict)
            {
                if (item.Value.Count > 0 && !result.Contains(item.Value.First()))
                {
                    result.Add(item.Value.First());
                }
            }

            return result;
        }
       
        public MergeData CalculateCoordinateMergeWithConfig(Vector3Int coordinate, BaseMatchItemConfig matchItemConfig, Vector3Int direction)
        {
            return CanCoordinateMerge(coordinate, matchItemConfig, direction);
        }

        private MergeData CanCoordinateMerge(Vector3Int coordinate, BaseMatchItemConfig matchItemConfig, Vector3Int direction)
        {
            if (!matchItemConfig.canMerge) return null;

            var gridItems = MatchGameService.GridController.GridItems;
            var gridShape = MatchGameService.GridController.GridShape;

            MergeRules mergeRules = MatchGameService.MatchGameSettings.mergeRules;

            BaseMatchItemConfig GetMatchItemConfig(Vector3Int coord)
            {
                if (coord == coordinate)
                {
                    return matchItemConfig;
                }
                else if (direction != Vector3.zero && coord == coordinate + direction)
                {
                    return gridItems[coordinate].MatchItemConfig;
                }
                else
                {
                    return gridItems[coord].MatchItemConfig;
                }
            }

            bool IsCoordinateValid(Vector3Int coord)
            {
                return gridItems.ContainsKey(coord) && gridShape[coord];
            }

            for (var index = 0; index < mergeRules.RuleCount; index++)
            {
                for (int rotationMultiplier = 0; rotationMultiplier < 4; rotationMultiplier++)
                {
                    MergeRule rule = mergeRules.GetMergeRule(index, rotationMultiplier * 90);

                    bool foundRule;

                    Vector3Int offset;
                    for (int offsetIndex = 0; offsetIndex < rule.positions.Length; offsetIndex++)
                    {
                        offset = -rule.positions[offsetIndex];

                        //if (!IsCoordinateValid(coordinate + offset) || GetMatchItemConfig(coordinate + offset) != matchItemConfig) continue; 

                        foundRule = true;

                        for (int i = 0; i < rule.positions.Length; i++)
                        {
                            if (!IsCoordinateValid(coordinate + rule.positions[i] + offset))
                            {
                                foundRule = false;
                                break;
                            }
                            else if (matchItemConfig != GetMatchItemConfig(coordinate + rule.positions[i] + offset))
                            {
                                foundRule = false;
                                break;
                            }
                        }

                        if (foundRule)
                        {
                            MergeData mergeData = new MergeData();
                            mergeData.priority = index;
                            mergeData.resultItem = rule.resultItem;

                            //mergeData.mergingCoordinates.Add(coordinate + offset);

                            for (int i = 0; i < rule.positions.Length; i++)
                            {
                                mergeData.mergingCoordinates.Add(coordinate + rule.positions[i] + offset);
                            }

                            return mergeData;
                        }
                    }                    
                }
            }

            return null;
        }

    }
}
