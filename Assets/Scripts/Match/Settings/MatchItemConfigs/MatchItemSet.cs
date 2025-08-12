using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Match.Settings
{
    [CreateAssetMenu(fileName = "MatchItemSet", menuName = "Match/Match Item Set", order = 1)]
    public class MatchItemSet : ScriptableObject
    {
        public string Id;
        public List<BaseMatchItemConfig> configs;
    }
}