using BaseServices.PoolServices;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Match.Grid;
using Match.GridItems;
using Match.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match
{
    public enum BoosterSwipeResult
    {
        NoBooster = 0, BoosterActivated = 1, BoosterMerge = 2
    }
    public class BoosterController
    {
        public BoosterSwipeResult OnBoosterSwipe(BaseGridItem item1, BaseGridItem item2, Vector3Int mergeCoordinate)
        {
            if (item1 is BoosterItem || item2 is BoosterItem)
            {
                BoosterItem booster1 = item1 as BoosterItem;
                BoosterItem booster2 = item2 as BoosterItem;

                if (booster1 != null && booster2 != null)
                {
                    return BoosterSwipeResult.BoosterMerge;
                }
                else if (booster1 != null)
                {
                    if (booster1 is RainbowBoosterItem)
                    {
                        RainbowBoosterItem rainbow = (RainbowBoosterItem)booster1;
                        rainbow.SetTargetConfig(item2.MatchItemConfig);
                    }

                    booster1.ActivateBooster();
                }
                else if (booster2 != null)
                {
                    if (booster2 is RainbowBoosterItem)
                    {
                        RainbowBoosterItem rainbow = (RainbowBoosterItem)booster2;
                        rainbow.SetTargetConfig(item1.MatchItemConfig);
                    }

                    booster2.ActivateBooster();
                }

                return BoosterSwipeResult.BoosterActivated;
            }

            return BoosterSwipeResult.NoBooster;
        }

        public Sequence MergeBoosters(BoosterItem item1, BoosterItem item2, Vector3Int mergeCoordinate)
        {
            void GetLastInteractedItem(out BoosterItem lastInteractedItem, out Vector3Int lastInteractDirection)
            {
                if (item1.LastInteractedTime > item2.LastInteractedTime)
                {
                    lastInteractedItem = item1;
                    lastInteractDirection = item1.LastUserInputDirection;
                }
                else
                {
                    lastInteractedItem = item2;
                    lastInteractDirection = item2.LastUserInputDirection;
                }
            }

            GetLastInteractedItem(out BoosterItem lastInteractedItem, out Vector3Int dir);

            BoosterItem target = lastInteractedItem == item1 ? item2 : item1;
            var seq = DOTween.Sequence();

            IGridController gridController = MatchGameService.GridController;

            if (item1 is RainbowBoosterItem && item2 is RainbowBoosterItem) // both rainbow
            {
                // TODO: DESTYROY ALL ITEMS
            }
            else if (item1 is RainbowBoosterItem || item2 is RainbowBoosterItem) // one rainbow
            {
                MatchGameService.GridController.SwapItems(lastInteractedItem.GridCoordinate, target.GridCoordinate);

                MatchGameService.AnimationController.PlaySuccessfulMoveAnimaton(lastInteractedItem.transform, target.transform); 

                if (item1 is RainbowBoosterItem)
                {
                    RainbowBoosterItem rainbow = (RainbowBoosterItem)item1;
                    rainbow.SetTargetConfig(item2.MatchItemConfig);
                }
                else if (item2 is RainbowBoosterItem)
                {
                    RainbowBoosterItem rainbow = (RainbowBoosterItem)item2;
                    rainbow.SetTargetConfig(item1.MatchItemConfig);
                }

                item1.ActivateBooster();
                item2.ActivateBooster();
            }
            else if (item1 is RocketBoosterItem && item2 is RocketBoosterItem) // both rocket
            {
                BoosterMergeRule rule = MatchGameService.MatchGameSettings.mergeRules.GetBoosterMergeRule((BoosterConfig)item1.MatchItemConfig, (BoosterConfig)item2.MatchItemConfig);

                GridAnimationSettings animationSettings = MatchGameService.MatchGameSettings.animationSettings;

                seq.Append(lastInteractedItem.transform.DOMove(target.transform.position, animationSettings.successfulMove_duration)
                    .SetEase(animationSettings.successfulMove_ease));

                seq.AppendCallback(() =>
                {
                    gridController.RemoveGridItem(item1.GridCoordinate, false);
                    gridController.RemoveGridItem(item2.GridCoordinate, false);

                    BoosterItem bi = CreateBooster(rule.resultItem, mergeCoordinate);

                    bi.Init(mergeCoordinate);

                    MatchGameService.GridController.SetUpGridItem(bi);

                    bi.ActivateBooster();
                });

                return seq;
            }
            else if (item1 is TNTBoosterItem && item2 is TNTBoosterItem) // both TNT
            {
                GridAnimationSettings animationSettings = MatchGameService.MatchGameSettings.animationSettings;

                seq.Append(lastInteractedItem.transform.DOMove(target.transform.position, animationSettings.successfulMove_duration)
                    .SetEase(animationSettings.successfulMove_ease));

                seq.AppendCallback(() =>
                {
                    gridController.RemoveGridItem(lastInteractedItem.GridCoordinate, false);
                });

                seq.Append(((TNTBoosterItem)target).PlayBigExplosionAnimation());

                ((TNTBoosterItem)target).Init(mergeCoordinate, 2);

                target.ActivateBooster();

                return seq;
            }
            else if ((item1 is TNTBoosterItem && item2 is RocketBoosterItem) || 
                (item1 is RocketBoosterItem && item2 is TNTBoosterItem)) // TNT & Rocket
            {
                BoosterMergeRule rule = MatchGameService.MatchGameSettings.mergeRules.GetBoosterMergeRule((BoosterConfig)item1.MatchItemConfig, (BoosterConfig)item2.MatchItemConfig);

                GridAnimationSettings animationSettings = MatchGameService.MatchGameSettings.animationSettings;

                seq.Append(lastInteractedItem.transform.DOMove(target.transform.position, animationSettings.successfulMove_duration)
                    .SetEase(animationSettings.successfulMove_ease));

                seq.AppendCallback(() =>
                {
                    gridController.RemoveGridItem(lastInteractedItem.GridCoordinate, false);
                    gridController.RemoveGridItem(target.GridCoordinate, false);

                    TNTRocketBoosterItem bi = (TNTRocketBoosterItem)CreateBooster(rule.resultItem, mergeCoordinate);

                    bi.Init(mergeCoordinate);

                    MatchGameService.GridController.SetUpGridItem(bi);

                    bi.ActivateBooster();
                });

                return seq;
            }
            else if (item1 is FlyingBoosterItem || item2 is FlyingBoosterItem)
            {
                GridAnimationSettings animationSettings = MatchGameService.MatchGameSettings.animationSettings;

                seq.Append(lastInteractedItem.transform.DOMove(target.transform.position, animationSettings.successfulMove_duration)
                    .SetEase(animationSettings.successfulMove_ease));

                if (item1 is FlyingBoosterItem && item2 is FlyingBoosterItem) // both flyer
                {
                    seq.AppendCallback(() =>
                    {
                        item1.ActivateBooster();
                        item2.ActivateBooster();
                    });
                }
                else // one booster
                {
                    FlyingBoosterItem flyer;
                    BoosterItem other;

                    if(item1 is FlyingBoosterItem)
                    {
                        flyer = (FlyingBoosterItem)item1;
                        other = item2;
                    }
                    else
                    {
                        flyer = (FlyingBoosterItem)item2;
                        other = item1;
                    }

                    seq.AppendCallback(() =>
                    {
                        flyer.AttachBooster(other);
                        flyer.ActivateBooster();
                    });
                }

                return seq;
            }

            return null;
        }

        public List<int> GetAllActiveBoosterPriorities()
        {
            List<int> priorities = new();

            foreach (var item in MatchGameService.GridController.GridItems)
            {
                if (item.Value is BoosterItem)
                {
                    BoosterItem booster = (BoosterItem)item.Value;

                    if (booster.IsActivated)
                    {
                        int boosterPriority = ((BoosterConfig)booster.MatchItemConfig).boosterPriority;

                        if (!priorities.Any(s => s == boosterPriority))
                        {
                            priorities.Add(boosterPriority);
                        }
                    }
                }
            }

            return priorities.OrderBy(s => s).ToList();
        }

        public List<UniTask> ProcessBoosters(int priority)
        {
            List<UniTask> boosterSequences = new List<UniTask>();

            foreach (var item in MatchGameService.GridController.GridItems)
            {
                if (item.Value is BoosterItem)
                {
                    BoosterItem booster = (BoosterItem)item.Value;

                    if (booster.IsActivated)
                    {
                        int boosterPriority = ((BoosterConfig)booster.MatchItemConfig).boosterPriority;

                        if (boosterPriority == priority)
                        {
                            var sequence = booster.PlayBooster();

                            if (sequence != null)
                            {
                                boosterSequences.Add(sequence.Play().ToUniTask());   
                            }
                        }
                    }
                }
            }

            return boosterSequences;
        }

        public bool CheckAndCreateBooster(MergeData mergeData, Vector3Int coordinate, out BaseGridItem result)
        {
            if (mergeData.resultItem is BoosterConfig)
            {
                result = CreateBooster(mergeData.resultItem, coordinate);

                return true;
            }

            result = null;

            return false;
        }

        public BoosterItem CreateBooster(BaseMatchItemConfig boosterConfig, Vector3Int coordinate)
        {
            if (boosterConfig.prefab is RocketBoosterItem)
            {
                var booster = (RocketBoosterItem)PoolService.Instance.Spawn(boosterConfig.prefab);

                bool isHorizontal;

                Vector3Int swipeDirection = Vector3Int.zero;

                if (MatchGameService.GridController.GridItems.TryGetValue(coordinate, out var gridItem))
                {
                    swipeDirection = gridItem.LastUserInputDirection;
                }

                if (swipeDirection.y == 0)
                {
                    isHorizontal = true;
                }
                else if (swipeDirection.x == 0)
                {
                    isHorizontal = false;
                }
                else if(swipeDirection.x == 0 && swipeDirection.y == 0)
                {
                    isHorizontal = (UnityEngine.Random.Range(0, 2) == 0);
                }
                else
                {
                    isHorizontal = false;
                }

                booster.Init(isHorizontal, coordinate);

                return booster;
            }
            else if (boosterConfig.prefab is TNTBoosterItem)
            {
                var booster = (TNTBoosterItem)PoolService.Instance.Spawn(boosterConfig.prefab);

                booster.Init(coordinate);

                return booster;
            }
            else if (boosterConfig.prefab is FlyingBoosterItem)
            {
                var booster = (FlyingBoosterItem)PoolService.Instance.Spawn(boosterConfig.prefab);

                booster.Init(coordinate);

                return booster;
            }
            else if (boosterConfig.prefab is RainbowBoosterItem)
            {
                var booster = (RainbowBoosterItem)PoolService.Instance.Spawn(boosterConfig.prefab);

                booster.Init(coordinate);

                return booster;
            }
            else if (boosterConfig.prefab is DoubleRocketBoosterItem)
            {
                var booster = (DoubleRocketBoosterItem)PoolService.Instance.Spawn(boosterConfig.prefab);

                booster.Init(coordinate);

                return booster;
            }
            else if (boosterConfig.prefab is TNTRocketBoosterItem)
            {
                var booster = (TNTRocketBoosterItem)PoolService.Instance.Spawn(boosterConfig.prefab);

                booster.Init(coordinate);

                return booster;
            }

            return null;
        }

        public bool HasAnyActivatedBooster()
        {
            foreach (var item in MatchGameService.GridController.GridItems)
            {
                if (item.Value is BoosterItem)
                {
                    if (((BoosterItem)item.Value).IsActivated)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}