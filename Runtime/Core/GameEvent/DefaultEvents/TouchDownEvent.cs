using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class TouchDownEvent : IEvent
    {
        private static readonly TouchDownEvent _instance = new TouchDownEvent();

        public Vector3 TouchedPosition { get; set; }

        public static TouchDownEvent GetInstance(Vector3 touchedPosition)
        {
            _instance.TouchedPosition = touchedPosition;
            return _instance;
        }

        public void Reset()
        {
            _instance.TouchedPosition = Vector3.zero;
        }
    }

}

