using BaseServices.PoolServices;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Match.GridItems;
using Match.Settings;
using System.Collections.Generic;
using UnityEngine;

namespace Match.Grid
{
    struct ColumnDropData
    {
        public int emptyCellCount;
        public int dropppedItemCount;
    }

    public class GridController : MonoBehaviour, IGridController
    {
        [SerializeField] private UnityEngine.Grid grid;
        [SerializeField] private ParticleSystem gridItemRemoveParticle;

        private Dictionary<Vector3Int, BaseGridItem> gridItems = new();
        private Dictionary<Vector3Int, bool> gridShape = new();

        private MatchLevelData levelData;

        private BaseGridItem selectedItem = null;
        private List<BaseGridItem> forceAppliedItems = new();

        #region PUBLIC_GETTERS
        public Dictionary<Vector3Int, BaseGridItem> GridItems => gridItems;
        public Dictionary<Vector3Int, bool> GridShape => gridShape;
        public Vector2 CellSize => grid.cellSize;

        public UnityEngine.Grid Grid => grid;

        #endregion

        public void Init()
        {
            //PoolService.Instance.InitializePool(gridItemRemoveParticle.gameObject, 15, 10);

            levelData = MatchGameService.MatchLevelData;

            GenerateGrid();

            RequestGridCalculation();

            MatchGameService.MoveController.OnLinkCompleted += OnLinkCompleted;
        }

        private void OnLinkCompleted(Stack<Vector3Int> link)
        {
            ClearSelected();
            ClearForceFocus();

            foreach (var coord in link)
            {
                RemoveGridItem(coord);
            }

            RequestGridCalculation();
        }

        public void SwapItems(Vector3Int startCoordinate, Vector3Int swipeCoordinate)
        {
            // swap items
            BaseGridItem temp = gridItems[startCoordinate];
            gridItems[startCoordinate] = gridItems[swipeCoordinate];
            gridItems[swipeCoordinate] = temp;

            // set new coordinates
            gridItems[startCoordinate].UpdateGridCoordinate(startCoordinate);
            gridItems[swipeCoordinate].UpdateGridCoordinate(swipeCoordinate);
        }

        public void GenerateGrid()
        {
            for (int j = 0; j < levelData.size.x; j++) // row
            {
                for (int i = 0; i < levelData.size.y; i++) // column
                {
                    Vector3Int coordinate = new Vector3Int(i, j, 0);

                    if (levelData.levelGridShape.Count > j && levelData.levelGridShape[j].row.Count > i)
                    {
                        if (levelData.levelGridShape[j].row[i])
                        {
                            gridShape.Add(coordinate, true);

                            if (levelData.levelStartingItems.Count > j && levelData.levelStartingItems[j].row.Count > i)
                            {
                                string itemKey = levelData.levelStartingItems[j].row[i].item;
                                BaseMatchItemConfig config = MatchGameService.Instance.GetMatchItemConfig(itemKey);

                                SetUpGridItem(CreateGridItem(config, coordinate));
                            }
                            else
                            {
                                BaseMatchItemConfig randomConfig = MatchGameService.Instance.GetMatchItemConfig(MatchConstants.randomItemKey);

                                SetUpGridItem(CreateGridItem(randomConfig, coordinate));
                            }
                        }
                        else
                        {
                            gridShape.Add(coordinate, false);
                        }
                    }
                    else
                    {
                        gridShape.Add(coordinate, true);

                        BaseMatchItemConfig randomConfig = MatchGameService.Instance.GetMatchItemConfig(MatchConstants.randomItemKey);

                        SetUpGridItem(CreateGridItem(randomConfig, coordinate));
                    }
                }
            }

            transform.position = new Vector3(levelData.size.x * grid.cellSize.x * -.5f, levelData.size.y * grid.cellSize.y * -.5f);
        }

        public void RequestGridCalculation()
        {
            CalculateGrid();
        }

        private async void CalculateGrid()
        {
            // DROP & CREATE NEW
            bool needDropIteration = true;

            List<UniTask> diagonalDropSequences = new List<UniTask>();
            List<UniTask> verticalDropSequences = new List<UniTask>();

            while (needDropIteration)
            {
                needDropIteration = false;

                verticalDropSequences = DropItemsVertically(out Dictionary<int, ColumnDropData> columnDropDatas);
                verticalDropSequences.AddRange(GenerateAllColumnItems(columnDropDatas));

                await UniTask.WhenAll(verticalDropSequences);

                if (IsDiagonalDropEnabled())
                {
                    diagonalDropSequences.Clear();

                    diagonalDropSequences.AddRange(DropItemsDiagonally());

                    if (diagonalDropSequences.Count > 0)
                    {
                        await UniTask.WhenAll(diagonalDropSequences);
                        needDropIteration = true;
                    }
                }
            }

            MatchGameService.MoveController.SetCanInteract(true);
        }

        private List<UniTask> DropItemsVertically(out Dictionary<int, ColumnDropData> columnsDropData)
        {
            List<UniTask> dropSequences = new List<UniTask>();
            columnsDropData = new Dictionary<int, ColumnDropData>();

            Vector3Int coordinate;
            Queue<Vector3Int> dropableCoordinates = new Queue<Vector3Int>();
            bool dropActive;
            bool isItemExist;
            int droppedItemCount;
            int emptyCellCount;

            for (int i = 0; i < levelData.size.x; i++) // for every column
            {
                dropActive = false;
                dropableCoordinates.Clear();
                droppedItemCount = 0;
                emptyCellCount = 0;

                for (int j = 0; j < levelData.size.y; j++) // for every row
                {
                    coordinate = new Vector3Int(i, j);

                    if (gridShape[coordinate]) // if coordinate is useable
                    {
                        isItemExist = gridItems.TryGetValue(coordinate, out BaseGridItem gridItem);

                        if (!dropActive)
                        {
                            if (!isItemExist)
                            {
                                dropActive = true;
                                dropableCoordinates.Enqueue(coordinate);
                                emptyCellCount++;
                            }
                        }
                        else if (isItemExist)
                        {
                            if (gridItem.MatchItemData.canMove && dropableCoordinates.Count > 0)
                            {
                                Vector3Int dropCoord = dropableCoordinates.Dequeue();
                                dropableCoordinates.Enqueue(coordinate);

                                gridItems.Remove(coordinate);
                                gridItems.Add(dropCoord, gridItem);
                                // PLAY DROP ANIMATION

                                gridItem.UpdateGridCoordinate(dropCoord);
                                var seq = gridItem.UpdatePosition(grid.GetCellCenterLocal(dropCoord), droppedItemCount);

                                if (seq != null)
                                {
                                    dropSequences.Add(seq.Play().AsyncWaitForCompletion().AsUniTask());
                                }

                                ++droppedItemCount;
                            }
                            else
                            {
                                dropableCoordinates.Clear();
                                dropActive = false;
                                droppedItemCount = 0;
                                emptyCellCount = 0;
                            }
                        }
                        else
                        {

                            dropableCoordinates.Enqueue(coordinate);
                            emptyCellCount++;
                        }
                    }
                }

                columnsDropData.Add(i, new ColumnDropData() { emptyCellCount = emptyCellCount, dropppedItemCount = droppedItemCount });
                //dropSequences.AddRange(GenerateColumnItems(i, emptyCellCount, droppedItemCount));
            }

            return dropSequences;
        }

        private List<UniTask> GenerateAllColumnItems(Dictionary<int, ColumnDropData> columnDropDatas)
        {
            List<UniTask> dropSequences = new List<UniTask>();

            Dictionary<int, Queue<BaseMatchItemConfig>> columnDropConfig = new();

            // decide which items to generate
            foreach (var dropData in columnDropDatas)
            {
                columnDropConfig.Add(dropData.Key, new Queue<BaseMatchItemConfig>());

                if (dropData.Value.emptyCellCount > 0)
                {
                    for (int i = 0; i < dropData.Value.emptyCellCount; i++)
                    {
                        BaseMatchItemConfig config = MatchGameService.Instance.GetRandomMatchItemConfig();

                        columnDropConfig[dropData.Key].Enqueue(config);
                    }
                }
            }

            foreach (var dropConfigs in columnDropConfig)
            {
                dropSequences.AddRange(GenerateColumnItems(dropConfigs.Key, dropConfigs.Value, columnDropDatas[dropConfigs.Key].dropppedItemCount));
            }

            return dropSequences;
        }

        private List<UniTask> GenerateColumnItems(int columnIndex, Queue<BaseMatchItemConfig> dropItemConfigs, int delayOffset)
        {
            List<UniTask> dropSequences = new List<UniTask>();

            int itemsCount = dropItemConfigs.Count;

            int k = 0;

            while (dropItemConfigs.Count > 0)
            {
                Vector3Int coordinate = new Vector3Int(columnIndex, levelData.size.y - itemsCount + k);

                Vector3 cellPos = grid.GetCellCenterLocal(coordinate);

                BaseMatchItemConfig config = dropItemConfigs.Dequeue();
                BaseGridItem item = CreateGridItem(config, coordinate);

                item.transform.SetParent(transform);

                item.transform.localPosition = cellPos + new Vector3(0, CellSize.y * levelData.size.y);

                var seq = item.UpdatePosition(cellPos, delayOffset + k);

                if (seq != null)
                {
                    dropSequences.Add(seq.Play().AsyncWaitForCompletion().AsUniTask());
                }

                gridItems.Add(item.GridCoordinate, item);

                k++;
            }

            return dropSequences;
        }

        private BaseGridItem CreateGridItem(BaseMatchItemConfig config, Vector3Int coordinate)
        {
            BaseGridItem item = PoolService.Instance.Spawn(config.prefab);
            item.Init(coordinate);
            return item;
        }

        public void SetUpGridItem(BaseGridItem item)
        {
            item.transform.SetParent(transform);
            item.transform.localPosition = grid.GetCellCenterLocal(item.GridCoordinate);

            gridItems.Add(item.GridCoordinate, item);
        }

        private List<UniTask> DropItemsDiagonally()
        {
            List<UniTask> dropSequences = new List<UniTask>();

            if (!MatchGameService.MatchGameSettings.canDropDiagonal) return dropSequences;

            Vector3Int coordinate;

            BaseGridItem gridItem = null;

            Dictionary<Vector3Int, byte> diagonallyDroppedCoordinates = new(); // just diagonlly dropped items

            for (int i = 0; i < levelData.size.x; ++i) // for every column
            {
                for (int j = 0; j < levelData.size.y; ++j) // for every row
                {
                    coordinate = new Vector3Int(i, j);

                    // if there is no item on coordinate and there is items on upper diagonals, there has diagonal drop
                    if (gridShape[coordinate] && !gridItems.ContainsKey(coordinate) && !diagonallyDroppedCoordinates.ContainsKey(coordinate))
                    {
                        gridItem = null;

                        Vector3Int leftDiagonalCoord = coordinate + new Vector3Int(-1, 1); // upper left diagonal
                        Vector3Int rightDiagonalCoord = coordinate + new Vector3Int(1, 1); // upper right diagonal

                        // item exist in upper left
                        if (gridItems.TryGetValue(leftDiagonalCoord, out BaseGridItem gridItemLeft) && gridItemLeft.MatchItemData.canMove)
                        {
                            gridItems.Remove(leftDiagonalCoord);
                            gridItems.Add(coordinate, gridItemLeft);
                            diagonallyDroppedCoordinates.Add(leftDiagonalCoord, 0); // add coordinate to just droppeds, if it can be everytihng must be fileld vertically
                            gridItem = gridItemLeft;
                        }
                        else if (gridItems.TryGetValue(rightDiagonalCoord, out BaseGridItem gridItemRight) && gridItemRight.MatchItemData.canMove)
                        {
                            gridItems.Remove(rightDiagonalCoord);
                            gridItems.Add(coordinate, gridItemRight);
                            diagonallyDroppedCoordinates.Add(rightDiagonalCoord, 0);
                            gridItem = gridItemRight;
                        }

                        // animate item
                        if (gridItem != null)
                        {
                            gridItem.UpdateGridCoordinate(coordinate);
                            var seq = gridItem.UpdatePosition(grid.GetCellCenterLocal(coordinate), 0);

                            if (seq != null)
                            {
                                dropSequences.Add(seq.Play().AsyncWaitForCompletion().AsUniTask());
                            }

                            break;
                        }
                    }
                }
            }

            return dropSequences;
        }

        private bool IsDiagonalDropEnabled()
        {
            return MatchGameService.MatchGameSettings.canDropDiagonal;
        }

        public void DamageGridItem(Vector3Int coord, string damageId, bool playParticle = true)
        {
            if (TryGetGridItem(coord, out BaseGridItem item))
            {
                //MatchGameService.AnimationController.PlayCollectAnimation(GridItems[coord]);
                RemoveGridItem(coord, playParticle);
            }

        }

        public void RemoveGridItem(Vector3Int coord, bool playParticle = true)
        {
            if (TryGetGridItem(coord, out BaseGridItem item))
            {
                PoolService.Instance.Despawn(item);
                gridItems.Remove(coord);

                if (item is MatchItem && playParticle && gridItemRemoveParticle != null)
                {
                    // play remove particle
                    var particle = PoolService.Instance.Spawn(gridItemRemoveParticle);
                    particle.transform.position = item.transform.position;

                    PoolService.Instance.Despawn(gridItemRemoveParticle, particle.main.duration);
                }
            }
        }

        public bool IsBottom(Vector3Int coord)
        {
            for (int i = 0; i < levelData.size.y; i++)
            {
                Vector3Int coordinate = new Vector3Int(coord.x, i);

                if (gridShape.TryGetValue(coordinate, out bool val) && val)
                {
                    if (coordinate == coord)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public bool TryGetGridItem(Vector3Int coord, out BaseGridItem gridItem)
        {
            coord.z = 0; // ensure z is zero for 2D grid
            if (gridItems.TryGetValue(coord, out gridItem))
            {
                return true;
            }
            else
            {
                gridItem = null;
                return false;
            }
        }

        public void ApplySelected(Vector3Int selectCoord)
        {
            if (selectedItem == null || selectedItem.GridCoordinate != selectCoord)
            {
                ClearSelected();

                if (TryGetGridItem(selectCoord, out BaseGridItem selectItem))
                {
                    selectedItem = selectItem;

                    selectItem.ApplySelected();
                    SelectForceFocus(selectCoord);
                }
            }
        }

        public void ClearSelected()
        {
            if (selectedItem != null)
            {
                selectedItem.ClearSelected();
                selectedItem = null;
            }
        }

        public void SelectForceFocus(Vector3Int focusCoord)
        {
            ClearForceFocus();

            if (TryGetGridItem(focusCoord, out BaseGridItem focusItem))
            {
                foreach (var adjecentCoord in MatchConstants.CellNeighbours)
                {
                    if (TryGetGridItem(focusCoord + adjecentCoord, out BaseGridItem neighbourItem))
                    {
                        neighbourItem.ApplyForce(adjecentCoord);
                        forceAppliedItems.Add(neighbourItem);
                    }
                }

                foreach (var diagonalCoord in MatchConstants.CellDiagonalNeighbours)
                {
                    if (TryGetGridItem(focusCoord + diagonalCoord, out BaseGridItem neighbourItem))
                    {
                        neighbourItem.ApplyForce(diagonalCoord);
                        forceAppliedItems.Add(neighbourItem);
                    }
                }
            }
        }

        public void ClearForceFocus()
        {
            foreach (var item in forceAppliedItems)
            {
                item.ClearForce();
            }

            forceAppliedItems.Clear();
        }
    }
}
