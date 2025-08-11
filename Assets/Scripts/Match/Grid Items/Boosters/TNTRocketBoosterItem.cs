using DG.Tweening;
using BaseServices;
using Match.Grid;
using Match.GridItems.Particle;
using Match.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseServices.PoolServices;

namespace Match.GridItems
{
    public class TNTRocketBoosterItem : BoosterItem
    {
        [SerializeField] private BoosterConfig boosterConfig;
        [SerializeField] private BoosterData boosterData;
        [SerializeField] private float iterativeCellRemoveSpeed = .08f;
        [SerializeField] private RocketParticle particlePrefab;

        public override BaseMatchItemConfig MatchItemConfig => boosterConfig;
        public override BaseMatchItemData MatchItemData => boosterData;

        public override Sequence PlayBooster()
        {
            var seq = DOTween.Sequence();

            IGridController gridController = MatchGameService.GridController;

            MatchLevelData mld = (MatchLevelData)ResolveServices.LevelService.ActiveLevelConfig.GetLevelData();

            string damageId = System.Guid.NewGuid().ToString();

            void RemoveItem(Vector3Int coord)
            {
                if (coord == GridCoordinate) return;

                gridController.DamageGridItem(coord, damageId, true);
            }

            var seq_horizontal_1 = DOTween.Sequence();
            var seq_horizontal_2 = DOTween.Sequence();
            var seq_vertical_1 = DOTween.Sequence();
            var seq_vertical_2 = DOTween.Sequence();

            void CreateBoosterSequences(Sequence seq1, Sequence seq2, int max, int center, Vector3Int dir)
            {
                Vector3 perpendicularVec = Vector3.Cross(dir, Vector3.forward).normalized;
                Vector3Int perpendicular = new Vector3Int((int)perpendicularVec.x, (int)perpendicularVec.y, (int)perpendicularVec.z);

                for (int i = 1; i < max - center; ++i)
                {
                    Vector3Int coord = GridCoordinate + (dir * i);
                    var innerSeq = DOTween.Sequence();

                    innerSeq.AppendInterval(iterativeCellRemoveSpeed);
                    innerSeq.AppendCallback(() =>
                    {
                        RemoveItem(coord);
                        RemoveItem(coord + perpendicular);
                        RemoveItem(coord - perpendicular);
                    });

                    seq1.Append(innerSeq);
                }

                for (int i = 1; i < max - center; ++i)
                {
                    Vector3Int coord = GridCoordinate + (-dir * i);
                    var innerSeq = DOTween.Sequence();

                    innerSeq.AppendInterval(iterativeCellRemoveSpeed);
                    innerSeq.AppendCallback(() =>
                    {
                        RemoveItem(coord);
                        RemoveItem(coord + perpendicular);
                        RemoveItem(coord - perpendicular);
                    });

                    seq1.Append(innerSeq);
                }
            }

            CreateBoosterSequences(seq_horizontal_1, seq_horizontal_2, mld.size.x, GridCoordinate.x, Vector3Int.right);
            CreateBoosterSequences(seq_vertical_1, seq_vertical_2, mld.size.y, GridCoordinate.y, Vector3Int.up);

            seq.Append(seq_horizontal_1);
            seq.Join(seq_horizontal_2);
            seq.Join(seq_vertical_1);
            seq.Join(seq_vertical_2);

            // Particles
            RocketParticle particle1 = PoolService.Instance.Spawn(particlePrefab);
            RocketParticle particle2 = PoolService.Instance.Spawn(particlePrefab);
            RocketParticle particle3 = PoolService.Instance.Spawn(particlePrefab);
            RocketParticle particle4 = PoolService.Instance.Spawn(particlePrefab);

            particle1.transform.localScale = transform.position;
            particle2.transform.localScale = transform.position;
            particle3.transform.localScale = transform.position;
            particle4.transform.localScale = transform.position;

            particle1.transform.position = transform.position;
            particle2.transform.position = transform.position;
            particle3.transform.position = transform.position;
            particle4.transform.position = transform.position;

            float particleSpeed = gridController.CellSize.x / iterativeCellRemoveSpeed;

            particle1.Init(Vector3Int.up, particleSpeed, 3);
            particle2.Init(Vector3Int.down, particleSpeed, 3);
            particle3.Init(Vector3Int.right, particleSpeed, 3);
            particle4.Init(Vector3Int.left, particleSpeed, 3);

            seq.AppendCallback(() =>
            {
                gridController.RemoveGridItem(GridCoordinate, false);

                PoolService.Instance.Despawn(particle1, 1);
                PoolService.Instance.Despawn(particle2, 1);
                PoolService.Instance.Despawn(particle3, 1);
                PoolService.Instance.Despawn(particle4, 1);
            });

            return seq;
        }
    }
}