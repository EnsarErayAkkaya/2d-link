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
        [SerializeField] private MatchLevelData matchLevelData;

        #region SERVICES & CONTROLLERS
        private MoveController moveController = new MoveController();
        private ScoreController scoreController = new ScoreController();
        #endregion

        private static MatchGameService instance;

        #region INTERNAL GETTERS
        internal static MatchLevelData MatchLevelData => instance.matchLevelData;
        internal static MatchGameSettings MatchGameSettings => instance.matchGameSettings;
        internal static MoveController MoveController => instance.moveController; 
        internal static ScoreController ScoreController => instance.scoreController; 
        #endregion

        #region PUBLIC GETTERS
        public static IGridController GridController => instance.gridController;
        public static MatchGameService Instance => instance;
        #endregion


        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            Setup();
        }

        public async void Setup()
        {
            moveController.Init();
            scoreController.Init();

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