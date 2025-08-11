using BaseServices.PoolServices;
using UnityEngine;

namespace Match.GridItems
{
    public class TNTParticle : MonoBehaviour, IPoolable
    {
        [SerializeField] private ParticleSystem partilce;

        private Vector3 scale;

        private void Awake()
        {
            scale = transform.localScale;
        }

        public void Init(float scaleMultiplier)
        {
            partilce.Play();

            transform.localScale = scale * scaleMultiplier;
        }

        public void OnSpawn()
        {
            transform.localScale = scale;
        }

        public void OnDespawn()
        {
        }
    }
}
