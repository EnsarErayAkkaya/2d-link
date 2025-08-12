using DG.Tweening;
using Match.GridItems;
using Match.Settings;
using System.Collections.Generic;
using UnityEngine;

namespace Match.Grid
{
    public interface IGridController
    {
        Dictionary<Vector3Int, BaseGridItem> GridItems { get; }
        Dictionary<Vector3Int, bool> GridShape { get; }
        Vector2 CellSize { get; }

        UnityEngine.Grid Grid { get; }

        void SwapItems(Vector3Int startCoordinate, Vector3Int swipeCoordinate);

        void GenerateGrid();

        void RequestGridCalculation();

        void DamageGridItem(Vector3Int coord, string damageId, bool playParticle = true);

        void RemoveGridItem(Vector3Int coord, bool playParticle = true);

        void SetUpGridItem(BaseGridItem item);

        bool IsBottom(Vector3Int coord);

        bool TryGetGridItem(Vector3Int coord, out BaseGridItem gridItem);
    }
}