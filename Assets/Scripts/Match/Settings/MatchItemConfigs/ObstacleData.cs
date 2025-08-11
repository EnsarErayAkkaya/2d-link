using NaughtyAttributes;
using UnityEngine;

namespace Match.Settings
{
    [CreateAssetMenu(fileName = "ObstacleData", menuName = "Game/Match/Items/Obstacles/Obstacle Data", order = 1)]
    public class ObstacleData : BaseMatchItemData
    {
        [BoxGroup("main")]
        public int health;
        [BoxGroup("main")]
        public Sprite[] healthSprites;
    }
}