using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Core
{
    public class SlowButtonHub : MonoBehaviour
    {

        public const float defaultPauseTime = 0.35f;
        public float Timer { get; private set; }

        [Inject]
        public void Construct()
        {
            Timer = 0;
        }

        public bool CanClick()
        {
            if (Timer > 0)
                return false;
            return true;
        }

        public void OnClick(float pauseTime = defaultPauseTime)
        {
            Timer = pauseTime;
        }

        void FixedUpdate()
        {
            if (Timer > 0)
            {
                Timer -= Time.fixedDeltaTime;
            }
        }
    }
}


