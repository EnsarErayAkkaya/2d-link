using BaseServices.PoolServices;
using Match.Settings;
using System.Collections.Generic;
using UnityEngine;
namespace Match.Grid
{
    public class GridBackgroundGenerator : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Grid grid;

        [SerializeField] private Transform backgroundParent;

        private List<SpriteRenderer> backgroundSprites = new();

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

                        SpriteRenderer sr = PoolService.Instance.Spawn(MatchGameService.MatchGameSettings.cellBackgroundPrefab);
                        sr.transform.SetParent(backgroundParent);
                        sr.transform.localPosition = grid.GetCellCenterLocal(coordinate);

                        backgroundSprites.Add(sr);
                    }
                }
            }

            transform.position = new Vector3(levelData.size.x * grid.cellSize.x * -.5f, levelData.size.y * grid.cellSize.y * -.5f);
        }

        private void OnDestroy()
        {
            foreach (var item in backgroundSprites)
            {
                PoolService.Instance.Despawn(item);
            }
        }
    }

}