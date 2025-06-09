using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class WidgetAnimation : MonoBehaviour
    {
        [SerializeField]
        private Animation _animation = null;

        [SerializeField]
        private AnimationClip _showClip = null;

        [SerializeField]
        private AnimationClip _hideClip = null;

        const string SHOW = "SHOW";

        const string HIDE = "HIDE";

        private void Awake()
        {
            _animation.AddClip(_showClip, SHOW);
            _animation.AddClip(_hideClip, HIDE);
            _animation.Play(SHOW);

            //_showClip.events[0].animationState.
        }
    }
}


