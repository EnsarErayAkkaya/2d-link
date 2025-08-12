using Match.Settings;
using UnityEngine;
namespace Match.Grid
{
    public class GridBackgroundGenerator : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Grid grid;

        [SerializeField] private Transform backgroundParent;
        [SerializeField] private Transform bordersParent;

        private void Start()
        {
            GenerateBackground();
        }

        private void GenerateBackground()
        {
            MatchLevelData levelData = null;//(MatchLevelData)ResolveServices.LevelService.ActiveLevelConfig.GetLevelData();

            for (int j = 0; j < levelData.levelGridSetup.Count; j++) // row
            {
                for (int i = 0; i < levelData.levelGridSetup[j].row.Count; i++) // column
                {
                    if (levelData.levelGridSetup[j].row[i])
                    {
                        Vector3Int coordinate = new Vector3Int(i, j);

                        SpriteRenderer sr = Instantiate(MatchGameService.MatchGameSettings.cellBackgroundPrefab, backgroundParent);
                        sr.transform.position = grid.GetCellCenterLocal(coordinate);

                        GenerateCellBorders(coordinate, levelData);
                    }
                }
            }
        }
        private void GenerateCellBorders(Vector3Int coordinate, MatchLevelData levelData)
        {
            foreach (var item in MatchConstants.CellNeighbours)
            {
                Vector2Int neighbour = new Vector2Int(coordinate.x + item.x, coordinate.y + item.y);

                if (neighbour.y >= 0 && levelData.levelGridSetup.Count > neighbour.y)
                {
                    if (neighbour.x >= 0 && levelData.levelGridSetup[neighbour.y].row.Count > neighbour.x)
                    {
                        if (!levelData.levelGridSetup[neighbour.y].row[neighbour.x])
                        {
                            AddBorderLine(coordinate, item);
                        }
                    }
                    else
                    {
                        AddBorderLine(coordinate, item);
                    }
                }
                else
                {
                    AddBorderLine(coordinate, item);
                }
            }
        }

        private void AddBorderLine(Vector3Int coordinate, Vector3Int direction)
        {
            SpriteRenderer sr = Instantiate(MatchGameService.MatchGameSettings.borderLinePrefab, bordersParent);
            sr.transform.position = grid.GetCellCenterLocal(coordinate) + 
                (new Vector3(direction.x * grid.cellSize.x * .5f, direction.y * grid.cellSize.y * .5f));

            float zAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            sr.transform.rotation = Quaternion.Euler(0, 0, zAngle + 90);
        }
    }

}