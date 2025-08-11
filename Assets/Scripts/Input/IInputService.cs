using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseServices.InputServices
{
    public enum InputType
    {
        Down, Tap, Drag, Swipe, Pinch, Up
    }

    public interface IInputService
    {
        public Action<InputType, List<Vector3>> OnInputReceived{ get; set; }
    }
}