using Core;
using UnityEngine;
using UnityEngine.UI;

public class NormalizedCanvasScaler : MonoBehaviour
{
    [SerializeField]
    private CanvasScaler _canvasScaler;

    [Header("Match Width or Height Settings")]
    [SerializeField]
    private float _withVerticalScale = 0.2f;

    [SerializeField]
    private float _withoutVerticalScale = 1f;

    [Header("Match Width or Height Settings")]
    [SerializeField]
    private float _withHorizontalScale = 0.2f;

    [SerializeField]
    private float _withoutHorizontalScale = 1f;

    void Start()
    {
        if (Camera2DManager.Instance.IsVertical)
        {
            if (Camera2DManager.Instance.IsVerticalScale)
            {
                _canvasScaler.matchWidthOrHeight = _withVerticalScale;
            }
            else
            {
                _canvasScaler.matchWidthOrHeight = _withoutVerticalScale;
            }
        }
        else
        {
            if (Camera2DManager.Instance.IsHorizontal)
            {
                _canvasScaler.matchWidthOrHeight = _withHorizontalScale;
            }
            else
            {
                _canvasScaler.matchWidthOrHeight = _withoutHorizontalScale;
            }
        }
    }
}
