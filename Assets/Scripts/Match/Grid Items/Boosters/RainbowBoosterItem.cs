using BaseServices.PoolServices;
using DG.Tweening;
using Match.GridItems.Particle;
using Match.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match.GridItems
{
    public class RainbowBoosterItem : BoosterItem
    {
        [SerializeField] private BoosterConfig boosterConfig;
        [SerializeField] private BoosterData boosterData;

        [SerializeField] private Animator animator;

        [SerializeField] private RainbowParticle rainbowParticle;

        [SerializeField] private PoolableParticle destroyParticle;

        private BaseMatchItemConfig targetConfig;

        public override BaseMatchItemData MatchItemData => boosterData;
        public override BaseMatchItemConfig MatchItemConfig => boosterConfig;

        public void SetTargetConfig(BaseMatchItemConfig config)
        {
            targetConfig = config;
        }

        public override Sequence PlayBooster()
        {
            var seq = DOTween.Sequence();

            List<BaseGridItem> foundItems = new List<BaseGridItem>();

            BaseMatchItemConfig convertTarget = null;

            // if selected config base match item
            if (MatchConstants.BaseMatchItemNames.Any(s => s == targetConfig.Id))
            {
                foreach (var item in MatchGameService.GridController.GridItems)
                {
                    if (item.Value.MatchItemConfig == targetConfig)
                    {
                        foundItems.Add(item.Value);
                    }
                }
            }
            else if (MatchConstants.BoosterItemNames.Any(s => s == targetConfig.Id))
            {
                convertTarget = MatchGameService.Instance.GetRandomMatchItemConfig();

                foreach (var item in MatchGameService.GridController.GridItems)
                {
                    if (item.Value.MatchItemConfig == convertTarget)
                    {
                        foundItems.Add(item.Value);
                    }
                }
            }

            float duration = PlayRotateAnim();

            seq.AppendInterval(.5f);

            float delayDuration = Mathf.Min(duration / (foundItems.Count + 1), .1f);

            for (int i = 0; i < foundItems.Count; i++)
            {
                BaseGridItem gridItem = foundItems[i];

                if (gridItem == null) continue;

                IPoolable poolItem = PoolService.Instance.Spawn(rainbowParticle);
                poolItem.transform.position = transform.position;

                seq.Join(poolItem.gameObject.GetComponent<RainbowParticle>().Init(gridItem.transform.position, () =>
                {
                    if (convertTarget != null)
                    {
                        MatchGameService.GridController.DamageGridItem(gridItem.GridCoordinate, "", false);

                        BoosterItem booster = MatchGameService.GridController.BoosterController.CreateBooster(targetConfig, gridItem.GridCoordinate);
                        booster.ActivateBooster();
                        MatchGameService.GridController.SetUpGridItem(booster);
                    }
                    else
                    {
                        MatchGameService.AnimationController.PlayCollectAnimation(gridItem);
                        MatchGameService.GridController.DamageGridItem(gridItem.GridCoordinate, "");
                    }

                }).SetDelay(i * delayDuration));
            }

            seq.AppendCallback(() =>
            {
                MatchGameService.GridController.RemoveGridItem(GridCoordinate, false);

                // play remove particle
                var particle = PoolService.Instance.Spawn(destroyParticle);
                particle.transform.position = transform.position;

                PoolableParticle poolableParticle = particle.gameObject.GetComponent<PoolableParticle>();

                PoolService.Instance.Despawn(poolableParticle, poolableParticle.ParticleSystem.main.duration);
            });

            return seq;
        }

        private float PlayRotateAnim()
        {
            animator.SetTrigger("Rotate");

            return animator.runtimeAnimatorController.animationClips.FirstOrDefault(s => s.name == "Rotate").length;
        }

        public override void ActivateBooster()
        {
            base.ActivateBooster();

            if (targetConfig == null)
            {
                targetConfig = MatchGameService.Instance.GetRandomMatchItemConfig();
            }
        }

        public override void OnSpawn()
        {
            isActivated = false;
            targetConfig = null;
        }
    }
}