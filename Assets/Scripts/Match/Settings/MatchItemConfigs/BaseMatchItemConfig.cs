using NaughtyAttributes;
using Match.GridItems;
using UnityEngine;

namespace Match.Settings
{
    [CreateAssetMenu(fileName = "BaseMatchItemConfig", menuName = "Game/Match/Items/Base Match Item Config", order = 0)]
    public class BaseMatchItemConfig : ScriptableObject
    {
        [Dropdown("GetMatchItemNames")]
        public string Id;
        public BaseGridItem prefab;
        public Sprite icon;
        public bool canMerge;

        private string[] GetMatchItemNames()
        {
            return MatchConstants.GetAllMatchItemNames();
        }
    }
}