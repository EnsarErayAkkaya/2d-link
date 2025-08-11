using UnityEngine;

namespace Match
{
    public class PoolableParticle : MonoBehaviour
    {
        [SerializeField] private new ParticleSystem particleSystem;

        public ParticleSystem ParticleSystem => particleSystem;
    }
}