using Match.Settings;
using UnityEngine;

namespace Match.GridItems
{
    public class ObstacleItem : BaseGridItem
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private ObstacleData obstacleItemData;
        [SerializeField] private ObstacleConfig obstacleItemConfig;

        private string lastDamageId;
        private int currentHealth;

        public override BaseMatchItemData MatchItemData => obstacleItemData;
        public override BaseMatchItemConfig MatchItemConfig => obstacleItemConfig;

        private void Start()
        {
            currentHealth = obstacleItemData.health;
        }

        public virtual bool TakeDamage(string damageId)
        {
            if (damageId != lastDamageId)
            {
                --currentHealth;

                if (currentHealth <= 0)
                {
                    return true;
                }
                else if (obstacleItemData.healthSprites.Length > 0 && currentHealth < obstacleItemData.healthSprites.Length)
                {
                    spriteRenderer.sprite = obstacleItemData.healthSprites[currentHealth];
                }
            }

            return false;
        }
    }
}