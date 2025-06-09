using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class TouchUpEvent : IEvent
    {
        private static readonly TouchUpEvent _instance = new TouchUpEvent();

        public Vector3 TouchedPosition { get; set; }

        public static TouchUpEvent GetInstance(Vector3 touchedPosition)
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


