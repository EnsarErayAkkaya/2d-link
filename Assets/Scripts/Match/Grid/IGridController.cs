using DG.Tweening;
using Match.GridItems;
using Match.Settings;
using System.Collections.Generic;
using UnityEngine;

namespace Match.Grid
{
    public interface IGridController
    {
        public Dictionary<Vector3Int, BaseGridItem> GridItems { get; }
        public Dictionary<Vector3Int, bool> GridShape { get; }
        public Vector2 CellSize { get; }

        public ObstacleController ObstacleController { get; }
        public BoosterController BoosterController { get; }

        public UnityEngine.Grid Grid { get; }

        public void SwapItems(Vector3Int startCoordinate, Vector3Int swipeCoordinate);

        public void GenerateGrid();

        public void RequestGridCalculation();

        public void DamageGridItem(Vector3Int coord, string damageId, bool playParticle = true);

        public void RemoveGridItem(Vector3Int coord, bool playParticle = true);

        public void SetUpGridItem(BaseGridItem item);

        public bool IsBottom(Vector3Int coord);
    }
}