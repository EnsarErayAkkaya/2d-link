using BaseServices.PoolServices;
using DG.Tweening;
using Match.Settings;
using UnityEngine;

namespace Match.GridItems
{
    public abstract class BaseGridItem : MonoBehaviour, IPoolable
    {
        private Vector3Int oldGridCoordinate;
        private Vector3Int gridCoordinate;

        public Vector3Int LastUserInputDirection { get; set; }
        public float LastInteractedTime { get; set; }

        public Vector3Int GridCoordinate => gridCoordinate;
        public abstract BaseMatchItemData MatchItemData { get; }
        public abstract BaseMatchItemConfig MatchItemConfig { get; }

        public virtual void Init(Vector3Int gridCoord)
        {
            gridCoordinate = gridCoord;
        }

        public virtual void UpdateGridCoordinate(Vector3Int newCoordinate)
        {
            oldGridCoordinate = gridCoordinate;
            gridCoordinate = newCoordinate;            
        }

        public Sequence UpdatePosition(Vector3 targetPos, int delayMultiplier)
        {
            DOTween.Kill(this);

            var seq = DOTween.Sequence();

            float moveByValue = MatchGameService.GridController.CellSize.y * .2f;

            seq.AppendInterval(delayMultiplier * MatchItemData.dropDelay);
            seq.Append(transform.DOLocalMove(targetPos, MatchItemData.dropDuration).SetEase(MatchItemData.dropEase));

            seq.Append(transform.DOLocalMoveY(targetPos.y - moveByValue, MatchItemData.reachedWiggleDurations[0])
                .SetEase(MatchItemData.reachedWiggleEase[0]));

            seq.Append(transform.DOLocalMoveY(targetPos.y + moveByValue / 2, MatchItemData.reachedWiggleDurations[1])
                .SetEase(MatchItemData.reachedWiggleEase[1]));

            seq.AppendCallback(() => { transform.localPosition = targetPos; });

            seq.AppendCallback(() =>
            {
                OnUpdatePositionAnimationCompleted();
            });

            seq.SetId(this);

            return seq;
        }

        public virtual void OnUpdatePositionAnimationCompleted()
        {

        }

        public virtual void OnSpawn()
        {
        }

        public virtual void OnDespawn()
        {
        }
    }
}