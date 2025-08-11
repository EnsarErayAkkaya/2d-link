using BaseServices.PoolServices;
using DG.Tweening;
using Match.Grid;
using Match.Settings;
using System;
using UnityEngine;

namespace Match.GridItems
{
    public class TNTBoosterItem : BoosterItem
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private int radius;

        [SerializeField] private BoosterConfig boosterConfig;
        [SerializeField] private BoosterData boosterData;

        [SerializeField] private TNTParticle particle;

        private int overrideRadius = 1;
        private Vector3 spriteRendererOriginalScale;

        public override BaseMatchItemData MatchItemData => boosterData;
        public override BaseMatchItemConfig MatchItemConfig => boosterConfig;

        private void Start()
        {
            spriteRendererOriginalScale = spriteRenderer.transform.localScale;
        }

        public void Init(Vector3Int coord, int overrideRadius = -1)
        {
            if (overrideRadius != -1)
            {
                this.overrideRadius = overrideRadius;
            }
            else
            {
                this.overrideRadius = radius;
            }

            base.Init(coord);
        }

        public override Sequence PlayBooster()
        {
            var seq = DOTween.Sequence();

            IGridController gridController = MatchGameService.GridController;

            string damageId = Guid.NewGuid().ToString();

            void RemoveItem(Vector3Int coord)
            {
                if (coord == GridCoordinate) return;

                gridController.DamageGridItem(coord, damageId, true);
            }

            TNTParticle instance = PoolService.Instance.Spawn(particle);
            instance.transform.position = transform.position;
            instance.Init(((float)overrideRadius) / ((float)radius));

            seq.AppendCallback(() =>
            {
                spriteRenderer.gameObject.SetActive(false);
                for (int i = -overrideRadius; i <= overrideRadius; i++)
                {
                    for (int j = -overrideRadius; j <= overrideRadius; j++)
                    {
                        if (i == 0 && j == 0) continue;

                        Vector3Int coord = GridCoordinate + new Vector3Int(i, j);

                        RemoveItem(coord);
                    }
                }
            });

            seq.AppendInterval(.5f);
            seq.AppendCallback(() =>
            {
                PoolService.Instance.Despawn(instance, 0.7f);

                gridController.RemoveGridItem(this.GridCoordinate);
            });

            return seq;
        }

        public Sequence PlayBigExplosionAnimation()
        {
            spriteRenderer.transform.localPosition = new Vector3(0, 0, -1);

            Sequence sequence = DOTween.Sequence();

            sequence.Append(spriteRenderer.transform.DOScale(2f, .4f));

            sequence.Append(spriteRenderer.transform.DOScaleY(1.3f, .3f));

            sequence.Join(transform.DOLocalRotate(new Vector3(0, 0, -20), .08f, RotateMode.Fast));
            sequence.Append(transform.DOLocalRotate(new Vector3(0, 0, 15), .12f, RotateMode.Fast));
            sequence.Append(transform.DOLocalRotate(new Vector3(0, 0, -10), .1f, RotateMode.Fast));
            sequence.Append(transform.DOLocalRotate(new Vector3(0, 0, 5), .1f, RotateMode.Fast));
            sequence.Append(transform.DOLocalRotate(new Vector3(0, 0, 0), .06f, RotateMode.Fast));

            return sequence;
        }

        public override void OnSpawn()
        {
            spriteRenderer.gameObject.SetActive(true);
            isActivated = false;
            overrideRadius = radius;
        }

        public override void OnDespawn()
        {
            spriteRenderer.transform.localScale = spriteRendererOriginalScale;
        }
    }
}