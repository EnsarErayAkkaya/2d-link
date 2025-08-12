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

        #region SERVICES & CONTROLLERS
        private MoveController moveController = new MoveController();
        #endregion

        private static MatchGameService instance;
        private Dictionary<BaseMatchItemConfig, int> matchItemCollectedCounts = new();
        private MatchLevelData matchLevelData;

        #region INTERNAL GETTERS
        internal static MatchLevelData MatchLevelData => instance.matchLevelData;
        internal static MatchGameSettings MatchGameSettings => instance.matchGameSettings;
        internal static MoveController MoveController => instance.moveController; 
        #endregion

        #region PUBLIC GETTERS
        public static IGridController GridController => instance.gridController;
        public static MatchGameService Instance => instance;

        public Action<BaseMatchItemConfig, int> OnNewMatchItemCollected { get; set; }
        public Dictionary<BaseMatchItemConfig, int> MatchItemCollectedCounts => matchItemCollectedCounts;

        #endregion


        private void Awake()
        {
            instance = this;
        }

        public async void Setup(MatchLevelData matchLevelData)
        {
            this.matchLevelData = matchLevelData;

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
            else //if (MatchConstants.BaseMatchItemNames.Any(s => s == itemKey))
            {
                return matchGameSettings.GetMatchItemConfig(matchGameSettings.baseMatchItemSetId, itemKey);
            }
        }

        public BaseMatchItemConfig GetRandomMatchItemConfig()
        {
            return matchGameSettings.GetMatchItemConfig(matchGameSettings.baseMatchItemSetId, MatchConstants.BaseMatchItemNames[UnityEngine.Random.Range(0, MatchConstants.BaseMatchItemNames.Length)]);
        }
    }
}