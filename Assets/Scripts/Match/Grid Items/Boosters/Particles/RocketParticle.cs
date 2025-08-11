using BaseServices.PoolServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match.GridItems.Particle
{
    public class RocketParticle : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float waveRotationSpeed;
        [SerializeField] private float rotationMax;
        [SerializeField] private float offsetMax;

        private float time = 0;
        private float speed;
        private Vector3Int direction;
        private float rotation;
        private float offset;
        private float startRotation;
        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        public void Init(Vector3Int direction, float speed, float scaleMultiplier = 1)
        {
            this.speed = speed;
            this.direction = direction;

            this.startRotation = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward);
            transform.rotation = Quaternion.Euler(0, 0, startRotation);

            transform.localScale = originalScale * scaleMultiplier;
        }

        private void Update()
        {
            time += Time.deltaTime * waveRotationSpeed;

            rotation = Mathf.Sin(time) * rotationMax;
            offset = Mathf.Sin(time) * offsetMax;

            spriteRenderer.transform.localPosition = new Vector3(offset,0 ,0);

            transform.position += ((Vector3)direction * speed * Time.deltaTime);
            transform.transform.rotation = Quaternion.Euler(0, 0, startRotation + rotation);
        }

        public void OnSpawn()
        {
            speed = 0;
            time = 0;
            rotation = 0;
            startRotation = 0;
            transform.localScale = originalScale;
        }

        public void OnDespawn()
        {
            throw new System.NotImplementedException();
        }
    }
}