using NaughtyAttributes;
using UnityEngine;

namespace Match.Settings
{
    [CreateAssetMenu(fileName = "BoosterData", menuName = "Game/Match/Items/Boosters/Booster Data", order = 1)]
    public class BoosterData : BaseMatchItemData
    {
        [BoxGroup("main")]
        public bool canActivatedByTap;
    }
}