using Match.GridItems;
using Match.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Match.Grid
{
    internal interface IMergeCalculator
    {
        public List<MergeData> CalculateMergeableItems();
        public MergeData CalculateCoordinateMergeWithConfig(Vector3Int coordinate, BaseMatchItemConfig matchItemConfig, Vector3Int direction);
    }
}
