using System.Collections.Generic;
using UnityEngine;

namespace Match.Effects
{
    public class LinkLineController : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;

        private void Start()
        {
            MatchGameService.MoveController.OnLinkUpdated += UpdateLine;
        }

        private void UpdateLine(Stack<Vector3Int> stack)
        {
            if (stack == null || stack.Count == 0)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            lineRenderer.positionCount = stack.Count;
            Vector3[] positions = new Vector3[stack.Count];

            int i = 0;
            foreach (var coord in stack)
            {
                positions[i++] = MatchGameService.GridController.Grid.GetCellCenterWorld(coord);
            }

            lineRenderer.SetPositions(positions);
        }
    }
}
