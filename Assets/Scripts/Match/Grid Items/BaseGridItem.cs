using BaseServices.PoolServices;
using DG.Tweening;
using Match.Settings;
using UnityEngine;

namespace Match.GridItems
{
    public abstract class BaseGridItem : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Vector3Int gridCoordinate;

        private string SelectedAnimId => $"selected_base_grid_item_{this.GetHashCode()}";
        private string ForceAnimId => $"force_base_grid_item_{this.GetHashCode()}";

        public SpriteRenderer SpriteRenderer => spriteRenderer;
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
            spriteRenderer.transform.localScale = Vector3.one;
            spriteRenderer.transform.localPosition = Vector3.zero;
        }

        public virtual void OnDespawn()
        {

        }

        public void ApplySelected()
        {
            DOTween.Kill(SelectedAnimId);

            spriteRenderer.transform.DOScale(1.2f, 0.35f)
                .SetEase(Ease.InOutSine)
                .SetId(SelectedAnimId);
        }

        public void ClearSelected()
        {
            DOTween.Kill(SelectedAnimId);

            spriteRenderer.transform.DOScale(1, 0.35f)
                .SetEase(Ease.InOutSine)
                .SetId(SelectedAnimId);
        }

        public void ApplyForce(Vector3 direction)
        {
            DOTween.Kill(ForceAnimId);

            spriteRenderer.transform.DOLocalMove(direction * 0.05f, 0.3f)
                .SetEase(Ease.OutBack)
                .SetId(ForceAnimId);
        }

        public void ClearForce()
        {
            DOTween.Kill(ForceAnimId);

            spriteRenderer.transform.DOLocalMove(Vector3.zero, 0.3f)
                .SetEase(Ease.InOutSine)
                .SetId(ForceAnimId);
        }
    }
}