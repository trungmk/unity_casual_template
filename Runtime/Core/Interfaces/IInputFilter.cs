using UnityEngine;

namespace Core
{
    public interface IInputFilter
    {
        void OnUserPress(Vector3 target);
        void OnUserDrag(Vector3 target);
        void OnUserRelease(Vector3 target);
    }
} 