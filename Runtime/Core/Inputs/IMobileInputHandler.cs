using UnityEngine;

public interface IMobileInputHandler
{
    bool IsInputEnabled { get; set; }

    void OnTouchDown(Vector3 position);

    void OnTouchUp(Vector3 position);

    void OnDrag(Vector3 position);
}