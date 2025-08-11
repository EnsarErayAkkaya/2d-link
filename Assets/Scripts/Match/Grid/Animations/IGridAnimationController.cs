using DG.Tweening;
using Match.GridItems;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Match.Grid
{
    public interface IGridAnimationController
    {
        public Sequence PlayFailedMoveAnimaton(Transform item1, Transform item2);
        public Sequence PlaySuccessfulMoveAnimaton(Transform item1, Transform item2);
        public Sequence PlayMergeAnimation(List<BaseGridItem> items);
        public void PlayCollectAnimation(BaseGridItem item);
    }
}
