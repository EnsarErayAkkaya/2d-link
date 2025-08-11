using Match.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match.GridItems
{
    public class MatchItem : BaseGridItem
    {
        [SerializeField] private BaseMatchItemData matchItemData;
        [SerializeField] private BaseMatchItemConfig matchItemConfig;

        public override BaseMatchItemData MatchItemData => matchItemData;
        public override BaseMatchItemConfig MatchItemConfig => matchItemConfig;
    }
}