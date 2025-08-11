using DG.Tweening;
using Match.Settings;
using UnityEngine;

namespace Match.GridItems
{
    public abstract class BoosterItem : BaseGridItem
    {
        protected bool isActivated = false;

        public bool IsActivated => isActivated;

        public override void Init(Vector3Int gridCoord)
        {
            base.Init(gridCoord);
        }

        public virtual void ActivateBooster()
        {
            isActivated = true;
        }

        public abstract Sequence PlayBooster();
    }
}