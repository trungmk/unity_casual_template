using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class DragEvent :IEvent
    {
        private static readonly DragEvent _instance = new DragEvent();

        public Vector3 TouchedPosition { get; set; }

        public static DragEvent GetInstance(Vector3 touchedPosition)
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

