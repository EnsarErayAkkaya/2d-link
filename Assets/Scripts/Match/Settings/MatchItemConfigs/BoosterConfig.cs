using UnityEngine;

namespace Match.Settings
{
    [CreateAssetMenu(fileName = "BoosterConfig", menuName = "Game/Match/Items/Boosters/Booster Config", order = 0)]
    public class BoosterConfig : BaseMatchItemConfig
    {
        [Tooltip("Lower priority work prior")]
        public int boosterPriority;
    }
}