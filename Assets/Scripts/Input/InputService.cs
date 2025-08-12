using Lean.Touch;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseServices.InputServices
{
    public class InputService : MonoBehaviour, IInputService
    {
        public Action<InputType, List<Vector3>> OnInputReceived { get; set; }

        public static InputService Instance { get; private set; }

        private void Start()
        {
            Instance = this;

            LeanTouch.OnFingerUpdate += OnFingerUpdate;
        }

        private void OnFingerUpdate(LeanFinger finger)
        {
            if (finger.IsOverGui || finger.StartedOverGui) return;

            if (finger.Up)
            {
                OnInputReceived?.Invoke(InputType.Up, new List<Vector3>() { (Vector3)finger.ScreenPosition });
            }

            if (finger.Tap)
            {
                OnInputReceived?.Invoke(InputType.Tap, new List<Vector3>() { (Vector3)finger.ScreenPosition });

            }

            if (finger.Down)
            {
                OnInputReceived?.Invoke(InputType.Down, new List<Vector3>() { (Vector3)finger.ScreenPosition });
            }

            if (finger.Swipe)
            {
                OnInputReceived?.Invoke(InputType.Swipe, new List<Vector3>() { (Vector3)finger.StartScreenPosition, finger.ScreenPosition });

            }

            // if there is only one finger active
            if (LeanTouch.Fingers.Count < 2)
            {
                // if finger moved since start
                if (finger.SwipeScaledDelta.sqrMagnitude > 0.25f)
                {
                    OnInputReceived?.Invoke(InputType.Drag, new List<Vector3>() { (Vector3)finger.StartScreenPosition, (Vector3)finger.ScreenPosition });
                }
            }
        }
    }
}