using Match.Grid;
using Match.Settings;
using System.Collections;
using UnityEngine;

namespace Match.GridItems
{
    public class CollectableItem : BaseGridItem
    {
        [SerializeField] private BaseMatchItemData matchItemData;
        [SerializeField] private BaseMatchItemConfig matchItemConfig;

        public override BaseMatchItemData MatchItemData => matchItemData;
        public override BaseMatchItemConfig MatchItemConfig => matchItemConfig;

        public override void UpdateGridCoordinate(Vector3Int newCoordinate)
        {
            base.UpdateGridCoordinate(newCoordinate);
        }

        public override void OnUpdatePositionAnimationCompleted()
        {
            if (MatchGameService.GridController.IsBottom(GridCoordinate))
            {
                MatchGameService.AnimationController.PlayCollectAnimation(MatchGameService.GridController.GridItems[GridCoordinate]);
                MatchGameService.GridController.RemoveGridItem(GridCoordinate);
            }
        }
    }
}