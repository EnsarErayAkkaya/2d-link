using BaseServices.PoolServices;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Match.GridItems;
using Match.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

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
        [SerializeField] private PoolableParticle gridItemRemoveParticle;

        private Dictionary<Vector3Int, BaseGridItem> gridItems = new();
        private Dictionary<Vector3Int, bool> gridShape = new();
        
        private BoosterController boosterController = new BoosterController();
        private ObstacleController obstacleController = new ObstacleController();
        private MatchLevelData levelData;
        private List<MergeData> lastMergeData = null;


        #region PUBLIC_GETTERS
        public Dictionary<Vector3Int, BaseGridItem> GridItems => gridItems;
        public Dictionary<Vector3Int, bool> GridShape => gridShape;
        public Vector2 CellSize => grid.cellSize;

        public ObstacleController ObstacleController => obstacleController;
        public BoosterController BoosterController => boosterController;
        public UnityEngine.Grid Grid => grid;

        #endregion

        public void Init()
        {
            PoolService.Instance.InitializePool(gridItemRemoveParticle.gameObject, 15, 10);

            levelData = (MatchLevelData)ResolveServices.LevelService.ActiveLevelConfig.GetLevelData();

            GenerateGrid();

            RequestGridCalculation();

            MatchGameService.MoveController.OnSwipeDetected += OnSwipeDetected;
        }

        private void OnSwipeDetected(Vector3 touchStartPos, Vector3Int direction)
        {
            Vector3Int startCoordinate = grid.WorldToCell(touchStartPos);
            startCoordinate.z = 0;
            Vector3Int swipeCoordinate = startCoordinate + direction;

            // if grid item exist and swipe direction item exist
            if (gridItems.ContainsKey(startCoordinate) && gridItems.ContainsKey(swipeCoordinate) && 
                gridItems[startCoordinate].MatchItemData.canMove && gridItems[swipeCoordinate].MatchItemData.canMove)
            {
                BoosterSwipeResult boosterSwipeResult = boosterController.OnBoosterSwipe(gridItems[startCoordinate], gridItems[swipeCoordinate], swipeCoordinate);

                // is this a booster swipe, it it is swipe
                if (boosterSwipeResult != BoosterSwipeResult.NoBooster)
                {
                    if (boosterSwipeResult == BoosterSwipeResult.BoosterActivated)
                    {
                        // set last Interacted times
                        gridItems[startCoordinate].LastInteractedTime = Time.time;
                        gridItems[startCoordinate].LastUserInputDirection = -direction;

                        gridItems[swipeCoordinate].LastInteractedTime = Time.time;
                        gridItems[swipeCoordinate].LastUserInputDirection = direction;

                        SwapItems(startCoordinate, swipeCoordinate);
                        MatchGameService.AnimationController
                            .PlaySuccessfulMoveAnimaton(gridItems[startCoordinate].transform, gridItems[swipeCoordinate].transform)
                            .AppendCallback(() =>
                            {
                                gridItems[startCoordinate].OnUpdatePositionAnimationCompleted();
                                gridItems[swipeCoordinate].OnUpdatePositionAnimationCompleted();

                                RequestGridCalculation();
                            });
                    }
                    else
                    {
                        // set last Interacted times
                        gridItems[startCoordinate].LastInteractedTime = Time.time;
                        gridItems[startCoordinate].LastUserInputDirection = -direction;

                        Sequence mergeBoosterSeq = boosterController.MergeBoosters(
                            (BoosterItem)gridItems[startCoordinate], 
                            (BoosterItem)gridItems[swipeCoordinate], swipeCoordinate);

                        if (mergeBoosterSeq != null)
                        {
                            mergeBoosterSeq.AppendCallback(() => RequestGridCalculation());
                        }
                        else
                        {
                            RequestGridCalculation();
                        }
                    }
                }
                else if (MatchGameService.MergeCalculator.CalculateCoordinateMergeWithConfig(startCoordinate, gridItems[swipeCoordinate].MatchItemConfig, direction) != null ||
                    MatchGameService.MergeCalculator.CalculateCoordinateMergeWithConfig(swipeCoordinate, gridItems[startCoordinate].MatchItemConfig, -direction) != null)
                {
                    // this is a valid swipe, play successful move animation

                    // set last Interacted times
                    gridItems[startCoordinate].LastInteractedTime = Time.time;
                    gridItems[startCoordinate].LastUserInputDirection = -direction;

                    gridItems[swipeCoordinate].LastInteractedTime = Time.time;
                    gridItems[swipeCoordinate].LastUserInputDirection = direction;

                    SwapItems(startCoordinate, swipeCoordinate);
                    MatchGameService.AnimationController
                        .PlaySuccessfulMoveAnimaton(gridItems[startCoordinate].transform, gridItems[swipeCoordinate].transform).AppendCallback(() =>
                        {
                            gridItems[startCoordinate].OnUpdatePositionAnimationCompleted();
                            gridItems[swipeCoordinate].OnUpdatePositionAnimationCompleted();

                            RequestGridCalculation();
                        });
                }
                else
                {
                    // not a valid swipe, play failed move animation
                    MatchGameService.AnimationController
                        .PlayFailedMoveAnimaton(gridItems[startCoordinate].transform, gridItems[swipeCoordinate].transform).OnComplete(() =>
                        {
                            MatchGameService.MoveController.SetCanInteract(true);
                        })
                        .Play();
                }
            }
            else
            {
                MatchGameService.MoveController.SetCanInteract(true);
            }
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
            for (int j = 0; j < levelData.levelStartingItems.Count; j++) // row
            {
                for (int i = 0; i < levelData.levelStartingItems[j].row.Count; i++) // column
                {
                    Vector3Int coordinate = new Vector3Int(i, j);
                    
                    if (levelData.levelGridSetup[j].row[i])
                    {
                        string itemKey = levelData.levelStartingItems[j].row[i].item;
                        BaseMatchItemConfig config = MatchGameService.Instance.GetMatchItemConfig(itemKey);

                        SetUpGridItem(CreateGridItem(config, coordinate));

                        gridShape.Add(coordinate, true);
                    }
                    else
                    {
                        gridShape.Add(coordinate, false);
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
            // MERGE & ANIMATE
            List<UniTask> mergeSequences = MergeWithAnimations();

            bool hasAnyActivatedBooster = boosterController.HasAnyActivatedBooster();

            if (hasAnyActivatedBooster)
            {
                while (hasAnyActivatedBooster)
                {
                    mergeSequences.AddRange(boosterController.ProcessBoosters(boosterController.GetAllActiveBoosterPriorities()[0]));

                    await UniTask.WhenAll(mergeSequences);

                    mergeSequences.Clear();

                    hasAnyActivatedBooster = boosterController.HasAnyActivatedBooster();
                }
            }
            else
            {
                await UniTask.WhenAll(mergeSequences);
            }

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

            // Calculate Grid again if has anything to merge
            if (HasAnythingToMerge() || boosterController.HasAnyActivatedBooster())
            {
                await UniTask.NextFrame();

                RequestGridCalculation();
            }
            else
            {
                MatchGameService.MoveController.SetCanInteract(true);
                lastMergeData = null;
            }
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
                                    dropSequences.Add(seq.Play().ToUnitask());
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

                columnsDropData.Add(i, new ColumnDropData() {emptyCellCount = emptyCellCount, dropppedItemCount = droppedItemCount });
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
                ColumnDroppableTypes droppableTypes = levelData.columnDroppableTypes.FirstOrDefault(s => s.columnIndex == dropData.Key);

                columnDropConfig.Add(dropData.Key, new Queue<BaseMatchItemConfig>());

                if (dropData.Value.emptyCellCount > 0)
                {
                    for (int i = 0; i < dropData.Value.emptyCellCount; i++)
                    {
                        BaseMatchItemConfig config = droppableTypes != null ?
                            MatchGameService.Instance.GetMatchItemConfig(droppableTypes.possibeTypes.GetRandomElement().item) :
                            MatchGameService.Instance.GetRandomMatchItemConfig();

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
                    dropSequences.Add(seq.Play().ToUniTask());
                }

                gridItems.Add(item.GridCoordinate, item);

                k++;
            }

            return dropSequences;
        }

        private BaseGridItem CreateGridItem(BaseMatchItemConfig config, Vector3Int coordinate)
        {
            BaseGridItem item = (BaseGridItem)PoolService.Instance.Spawn(config.prefab);
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
                                dropSequences.Add(seq.Play().ToUniTask());
                            }

                            break;
                        }
                    }
                }
            }

            return dropSequences;
        }

        private List<UniTask> MergeWithAnimations()
        {
            if (lastMergeData == null)
            {
                lastMergeData = MatchGameService.MergeCalculator.CalculateMergeableItems();
            }

            List<UniTask> mergeSequences = new List<UniTask>();

            // for each merge datas
            for (int mergeIndex = 0; mergeIndex < lastMergeData.Count; mergeIndex++)
            {
                string damageId = Guid.NewGuid().ToString();

                MergeData merge = lastMergeData
                    [mergeIndex];

                // get all merging transforms and select lastInteracted object for animation target
                List<BaseGridItem> mergedItems = new List<BaseGridItem>();

                float maxLastInteractedTime = 0;
                int lastInteractedIndex = -1;

                for (int i = 0; i < merge.mergingCoordinates.Count; i++)
                {
                    mergedItems.Add(gridItems[merge.mergingCoordinates[i]]);

                    if (gridItems[merge.mergingCoordinates[i]].LastInteractedTime >= maxLastInteractedTime)
                    {
                        maxLastInteractedTime = gridItems[merge.mergingCoordinates[i]].LastInteractedTime;
                        lastInteractedIndex = i;
                    }
                }

                Vector3Int targetCoordinate = merge.mergingCoordinates[lastInteractedIndex];

                // play animation if any sequence return add waiting list
                var sequence = MatchGameService.AnimationController.PlayMergeAnimation(mergedItems);

                BaseGridItem mergeResultItem = null;

                if (merge.resultItem != null)
                {
                    if (boosterController.CheckAndCreateBooster(merge, targetCoordinate, out mergeResultItem))
                    {
                        
                    }
                    else
                    {
                        mergeResultItem = CreateGridItem(merge.resultItem, targetCoordinate);
                    }
                }

                foreach (var coord in merge.mergingCoordinates)
                {
                    obstacleController.DamageAllNeighbourObstacles(coord, damageId); // find and call take damage function of all obstacles

                    RemoveGridItem(coord);
                }

                if (mergeResultItem)
                {
                    SetUpGridItem(mergeResultItem);
                }

                if (sequence != null)
                { 
                    mergeSequences.Add(sequence.Play().ToUniTask());
                }
            }

            return mergeSequences;
        }

        private bool HasAnythingToMerge()
        {
            lastMergeData = MatchGameService.MergeCalculator.CalculateMergeableItems();
            return lastMergeData.Count > 0;
        }        

        private bool IsDiagonalDropEnabled()
        {
            return MatchGameService.MatchGameSettings.canDropDiagonal;
        }

        public void DamageGridItem(Vector3Int coord, string damageId, bool playParticle = true)
        {
            if (GridItems.TryGetValue(coord, out BaseGridItem item) && item.MatchItemConfig.canDamageByBoosters)
            {
                if (item is BoosterItem)
                {
                    ((BoosterItem)item).ActivateBooster();
                }
                else if (ObstacleController.CheckAndDamageObstacle(item, damageId))
                {
                }
                else
                {
                    MatchGameService.AnimationController.PlayCollectAnimation(GridItems[coord]);
                    RemoveGridItem(coord, playParticle);
                }
            }
            
        }

        public void RemoveGridItem(Vector3Int coord, bool playParticle = true)
        {
            if (GridItems.TryGetValue(coord, out BaseGridItem item))
            {
                PoolService.Instance.Despawn(item);
                gridItems.Remove(coord);

                if (item is MatchItem && playParticle)
                {
                    // play remove particle
                    PoolableParticle particle = PoolService.Instance.Spawn(gridItemRemoveParticle);
                    particle.transform.position = item.transform.position;

                    PoolService.Instance.Despawn(gridItemRemoveParticle, particle.ParticleSystem.main.duration);
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
    }
}
