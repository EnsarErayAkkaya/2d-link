using Match.GridItems;
using UnityEngine;

namespace Match
{
    public class ObstacleController
    {
        public void DamageAllNeighbourObstacles(Vector3Int coordinate, string damageId)
        {
            foreach (var neighbourCoord in MatchConstants.CellNeighbours)
            {
                Vector3Int coord = coordinate + neighbourCoord;

                if (MatchGameService.GridController.GridItems.TryGetValue(coord, out BaseGridItem item))
                {
                    CheckAndDamageObstacle(item, damageId);
                }
            }
        }

        public bool CheckAndDamageObstacle(BaseGridItem item, string damageId)
        {
            if ((item is ObstacleItem) && ((ObstacleItem)item).TakeDamage(damageId))
            {
                // destroyed
                MatchGameService.GridController.RemoveGridItem(item.GridCoordinate);

                return true;
            }

            return false;
        }
    }
}
