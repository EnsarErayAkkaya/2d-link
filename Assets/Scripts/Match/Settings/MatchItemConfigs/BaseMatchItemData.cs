using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match.Settings
{
    [CreateAssetMenu(fileName = "BaseMatchItemData", menuName = "Match/Items/Base Match Item Data", order = 1)]
    public class BaseMatchItemData : ScriptableObject
    {
        [BoxGroup("main")]
        public bool canMove;

        [ShowIf("canMove")]
        [BoxGroup("drop")]
        public float dropDuration = .3f;
        [ShowIf("canMove")]
        [BoxGroup("drop")]
        public float dropDelay = .1f;
        [ShowIf("canMove")]
        [BoxGroup("drop")]
        public Ease dropEase = Ease.InOutSine;

        [ShowIf("canMove")]
        [BoxGroup("wiggle")]
        public float reachedWiggleMovementMultpilier = .2f;
        [ShowIf("canMove")]
        [BoxGroup("wiggle")]
        public float[] reachedWiggleDurations = { .06f, .07f, .04f };
        [ShowIf("canMove")]
        [BoxGroup("wiggle")]
        public Ease[] reachedWiggleEase = { Ease.InSine, Ease.InSine, Ease.InSine };
    }
}