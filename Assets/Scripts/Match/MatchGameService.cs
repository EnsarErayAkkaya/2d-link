using Cysharp.Threading.Tasks;
using Match.Grid;
using Match.GridItems;
using Match.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match
{
    public class MatchGameService : MonoBehaviour
    {
        [SerializeField] private MatchGameSettings matchGameSettings;
        [SerializeField] private GridController gridController;
        [SerializeField] private Canvas particleCanvas;

        #region SERVICES & CONTROLLERS
        private MoveController moveController = new MoveController();
        private MergeCalculator mergeCalculator = new MergeCalculator();
        private GridAnimationController animationController = new GridAnimationController();
        #endregion

        private static MatchGameService instance;
        private Dictionary<BaseMatchItemConfig, int> matchItemCollectedCounts = new();

        #region INTERNAL GETTERS
        public static MatchGameSettings MatchGameSettings => instance.matchGameSettings;
        internal static IMergeCalculator MergeCalculator => instance.mergeCalculator;
        internal static MoveController MoveController => instance.moveController; 
        internal static Canvas ParticleCanvas => instance.particleCanvas;
        #endregion

        #region PUBLIC GETTERS
        public static IGridAnimationController AnimationController => instance.animationController;
        public static IGridController GridController => instance.gridController;
        public Action<BaseMatchItemConfig, int> OnNewMatchItemCollected { get; set; }
        public static MatchGameService Instance => instance;
        public Dictionary<BaseMatchItemConfig, int> MatchItemCollectedCounts => matchItemCollectedCounts;

        #endregion

        private void Awake()
        {
            instance = this;
        }

        private async void Start()
        {
            moveController.Init();

            await UniTask.DelayFrame(1);

            gridController.Init();
        }

        public BaseMatchItemConfig GetMatchItemConfig(string itemKey)
        {
            if (itemKey == MatchConstants.randomItemKey)
            {
                return GetRandomMatchItemConfig();
            }
            else if (MatchConstants.BaseMatchItemNames.Any(s => s == itemKey))
            {
                return matchGameSettings.GetMatchItemConfig(matchGameSettings.baseMatchItemSetId, itemKey);
            }
            else if (MatchConstants.ObstacleItemNames.Any(s => s == itemKey))
            {
                return matchGameSettings.GetObstacleConfig(matchGameSettings.obstaclesSetId, itemKey);
            }
            else if (MatchConstants.BoosterItemNames.Any(s => s == itemKey))
            {
                return matchGameSettings.GetBoosterConfig(matchGameSettings.boostersSetId, itemKey);
            }
            else //if (MatchConstants.CollectableItemNames.Any(s => s == itemKey))
            {
                return matchGameSettings.GetCollectableConfig(matchGameSettings.collectablesSetId, itemKey);
            }
        }

        public BaseMatchItemConfig GetRandomMatchItemConfig()
        {
            return matchGameSettings.GetMatchItemConfig(matchGameSettings.baseMatchItemSetId, MatchConstants.BaseMatchItemNames[UnityEngine.Random.Range(0, MatchConstants.BaseMatchItemNames.Length)]);
        }

        public Vector3Int FindMostImportantLevelTargetCoordinate(bool shouldEffectedByBooster = false)
        {
            MatchLevelData ld = (MatchLevelData)null;//ResolveServices.LevelService.ActiveLevelConfig.GetLevelData();

            foreach (var levelTarget in ld.targets)
            {
                if (matchItemCollectedCounts.Any(s => s.Key.Id == levelTarget.matchItem.item))
                {
                    int collected = matchItemCollectedCounts.FirstOrDefault(s => s.Key.Id == levelTarget.matchItem.item).Value;

                    if (collected < levelTarget.count)
                    {
                        foreach (var gridItem in gridController.GridItems)
                        {
                            if (gridItem.Value.MatchItemConfig.Id == levelTarget.matchItem.item)
                            {
                                return gridItem.Key;
                            }
                        }
                    }
                }
            }

            while (true)
            {
                Vector3Int coord = new Vector3Int(UnityEngine.Random.Range(0, ld.size.x), UnityEngine.Random.Range(0, ld.size.y));

                if (gridController.GridShape.TryGetValue(coord, out bool valid) && valid && 
                    gridController.GridItems.TryGetValue(coord, out BaseGridItem item) && (item != null))
                {
                    return coord;
                }
            }
        }
    
        public void OnMatchItemCollected(BaseMatchItemConfig config)
        {
            if (matchItemCollectedCounts.ContainsKey(config))
            {
                ++matchItemCollectedCounts[config];
            }
            else
            {
                matchItemCollectedCounts.Add(config, 1);
            }

            OnNewMatchItemCollected?.Invoke(config, matchItemCollectedCounts[config]);
        }
    }
}