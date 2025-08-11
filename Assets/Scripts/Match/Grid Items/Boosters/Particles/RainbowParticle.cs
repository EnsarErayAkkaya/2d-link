using BaseServices.PoolServices;
using DG.Tweening;
using System;
using UnityEngine;

namespace Match.GridItems.Particle
{
    public class RainbowParticle : MonoBehaviour, IPoolable
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float targetReachDuration;
        [SerializeField] private float widthFadeDuration;

        [SerializeField] private float lineWidthStart;
        [SerializeField] private float lineWidthEnd;

        int linePositionCount;
        float[] linePositionsMultipliers;

        private void Awake()
        {
            linePositionCount = lineRenderer.positionCount;

            linePositionsMultipliers = new float[linePositionCount - 1];

            for (int i = 1; i < linePositionCount - 1; i++)
            {
                linePositionsMultipliers[i] = lineRenderer.GetPosition(i).x;
            }
        }

        public Sequence Init(Vector3 target, Action onRayReached)
        {
            var seq = DOTween.Sequence();

            float passedTimePosition = 0;

            Vector3 endPos = Vector3.zero;

            lineRenderer.SetPosition(0, Vector3.zero);

            seq.AppendCallback(() =>
            {
                lineRenderer.enabled = true;
            });

            seq.Append(DOTween.To(() => passedTimePosition, (x) => passedTimePosition = x, 1, targetReachDuration).OnUpdate(() =>
            {
                endPos = Vector3.Lerp(Vector3.zero, target-transform.position, passedTimePosition / 1.0f);

                for (int i = 1; i < linePositionCount; i++)
                {
                    lineRenderer.SetPosition(i, endPos * linePositionsMultipliers[i - 1]);
                }
            }));

            seq.AppendCallback(() =>
            {
                onRayReached();
            });

            float passedTimeWidth = 0;

            float width = lineWidthStart;

            seq.Append(DOTween.To(() => passedTimeWidth, (x) => passedTimeWidth = x, 1, widthFadeDuration).OnUpdate(() =>
            {
                width = Mathf.Lerp(lineWidthStart, lineWidthEnd, passedTimeWidth / widthFadeDuration);

                lineRenderer.widthMultiplier = width;
            }));

            return seq;
        }

        public void OnSpawn()
        {
            lineRenderer.SetPosition(1, Vector3.zero);
            lineRenderer.widthMultiplier = lineWidthStart;

            lineRenderer.enabled = false;
        }

        public void OnDespawn()
        {
            throw new NotImplementedException();
        }
    }
}