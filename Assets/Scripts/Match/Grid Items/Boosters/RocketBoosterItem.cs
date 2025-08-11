using DG.Tweening;
using BaseServices;
using Match.Grid;
using Match.GridItems.Particle;
using Match.Settings;
using System;
using UnityEngine;
using BaseServices.PoolServices;

namespace Match.GridItems
{
    public class RocketBoosterItem : BoosterItem
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private BoosterConfig boosterConfig;
        [SerializeField] private BoosterData boosterData;

        [SerializeField] private float iterativeCellRemoveSpeed = .08f;
        [SerializeField] private RocketParticle particlePrefab;

        private bool isHorizontal;

        public override BaseMatchItemData MatchItemData => boosterData;
        public override BaseMatchItemConfig MatchItemConfig => boosterConfig;

        public void Init(bool isHorizontal, Vector3Int coordinate)
        {
            base.Init(coordinate);
            this.isHorizontal = isHorizontal;

            spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, -90 * (isHorizontal ? 0 : 1));
        }

        public override Sequence PlayBooster()
        {
            var seq = DOTween.Sequence();

            IGridController gridController = MatchGameService.GridController;

            MatchLevelData mld = (MatchLevelData)ResolveServices.LevelService.ActiveLevelConfig.GetLevelData();

            string damageId = Guid.NewGuid().ToString();

            void RemoveItem(Vector3Int coord)
            {
                if (coord == GridCoordinate) return;

                gridController.DamageGridItem(coord, damageId, true);
            }


            spriteRenderer.gameObject.SetActive(false);

            var seq1 = DOTween.Sequence();
            var seq2 = DOTween.Sequence();

            void CreateBoosterSequences(int max, int center, Vector3Int dir)
            {
                for (int i = 1; i < max - center; ++i)
                {
                    Vector3Int coord = GridCoordinate + (dir * i);
                    var innerSeq = DOTween.Sequence();

                    innerSeq.AppendInterval(iterativeCellRemoveSpeed);
                    innerSeq.AppendCallback(() =>
                    {
                        RemoveItem(coord);
                    });

                    seq1.Append(innerSeq);
                }

                for (int i = 1; i <= center; ++i)
                {
                    Vector3Int coord = GridCoordinate + (-dir * i);

                    var innerSeq = DOTween.Sequence();

                    innerSeq.AppendInterval(iterativeCellRemoveSpeed);
                    innerSeq.AppendCallback(() =>
                    {
                        RemoveItem(coord);
                    });

                    seq2.Append(innerSeq);
                }
            }

            Vector3Int direction = isHorizontal ? Vector3Int.right : Vector3Int.up;

            CreateBoosterSequences(isHorizontal ? mld.size.x : mld.size.y, isHorizontal ? GridCoordinate.x : GridCoordinate.y, direction);

            seq.Append(seq1);
            seq.Join(seq2);

            // Particles
            RocketParticle particle1 = PoolService.Instance.Spawn(particlePrefab);
            RocketParticle particle2 = PoolService.Instance.Spawn(particlePrefab);

            particle1.transform.position = transform.position;
            particle2.transform.position = transform.position;

            float particleSpeed = gridController.CellSize.x / iterativeCellRemoveSpeed;

            particle1.Init(direction, particleSpeed);
            particle2.Init(-direction, particleSpeed);

            seq.AppendCallback(() =>
            {
                gridController.RemoveGridItem(this.GridCoordinate);

                PoolService.Instance.Despawn(particle1, 1);
                PoolService.Instance.Despawn(particle2, 1);
            });

            return seq;
        }

        public override void OnDespawn()
        {
            isActivated = false;
            spriteRenderer.gameObject.SetActive(true);
        }
    }
}