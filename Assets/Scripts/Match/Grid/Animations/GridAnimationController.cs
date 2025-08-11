using DG.Tweening;
using Match.GridItems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match.Grid
{
    internal class GridAnimationController : IGridAnimationController
    {
        public Sequence PlayFailedMoveAnimaton(Transform item1, Transform item2)
        {
            Vector3 firstPos = item1.position;
            Vector3 secondPos = item2.position;

            var seq = DOTween.Sequence();

            var settings = MatchGameService.MatchGameSettings.animationSettings;

            seq.Append(item1.DOMove(secondPos, settings.failedMove_firstMoveDuration).SetEase(settings.failedMove_firstMoveEase));
            seq.Join(item2.DOMove(firstPos, settings.failedMove_firstMoveDuration).SetEase(settings.failedMove_firstMoveEase));

            seq.Append(item2.DOMove(secondPos, settings.failedMove_secondMoveDuration).SetEase(settings.failedMove_secondMoveEase));
            seq.Join(item1.DOMove(firstPos, settings.failedMove_secondMoveDuration).SetEase(settings.failedMove_secondMoveEase));

            return seq;
        }

        public Sequence PlaySuccessfulMoveAnimaton(Transform item1, Transform item2)
        {
            var seq = DOTween.Sequence();

            var settings = MatchGameService.MatchGameSettings.animationSettings;

            seq.Append(item1.DOMove(item2.position, settings.successfulMove_duration).SetEase(settings.successfulMove_ease));
            seq.Join(item2.DOMove(item1.position, settings.successfulMove_duration).SetEase(settings.successfulMove_ease));

            return seq;
        }

        public Sequence PlayMergeAnimation(List<BaseGridItem> items)
        {
            foreach (var item in items)
            {
                PlayCollectAnimation(item);
            }

            var seq = DOTween.Sequence().AppendInterval(.2f);

            return seq;
        }

        public void PlayCollectAnimation(BaseGridItem item)
        {
        }

        public void PlayRotateAnimation(BaseGridItem item1, BaseGridItem item2)
        {

        }
    }
}
