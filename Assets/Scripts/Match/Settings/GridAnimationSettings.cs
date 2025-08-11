using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Match.Settings
{
    [CreateAssetMenu(fileName = "GridAnimationSettings", menuName = "Game/Match/Animations/Animation Settings", order = 0)]
    internal class GridAnimationSettings : ScriptableObject
    {
        [BoxGroup("Failed Move Anim")]
        public float failedMove_firstMoveDuration;
        [BoxGroup("Failed Move Anim")]
        public float failedMove_secondMoveDuration;
        [BoxGroup("Failed Move Anim")]
        public Ease failedMove_firstMoveEase;
        [BoxGroup("Failed Move Anim")]
        public Ease failedMove_secondMoveEase;

        [BoxGroup("Successful Move Anim")]
        public float successfulMove_duration;
        [BoxGroup("Successful Move Anim")]
        public Ease successfulMove_ease;
    }
}
