using BaseServices.PoolServices;
using Match.Settings;
using UnityEngine;
namespace Match.Grid
{
    public class GridBackgroundGenerator : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Grid grid;

        [SerializeField] private Transform backgroundParent;

        private void Start()
        {
            GenerateBackground();
        }

        private void GenerateBackground()
        {
            MatchLevelData levelData = MatchGameService.MatchLevelData;

            for (int j = 0; j < levelData.levelGridSetup.Count; j++) // row
            {
                for (int i = 0; i < levelData.levelGridSetup[j].row.Count; i++) // column
                {
                    if (levelData.levelGridSetup[j].row[i])
                    {
                        Vector3Int coordinate = new Vector3Int(i, j);

                        SpriteRenderer sr = Instantiate(MatchGameService.MatchGameSettings.cellBackgroundPrefab, backgroundParent);
                        sr.transform.localPosition = grid.GetCellCenterLocal(coordinate);
                    }
                }
            }

            transform.position = new Vector3(levelData.size.x * grid.cellSize.x * -.5f, levelData.size.y * grid.cellSize.y * -.5f);
        }
    }

}