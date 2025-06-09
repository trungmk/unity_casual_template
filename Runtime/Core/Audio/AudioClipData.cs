using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public enum AudioClipType
    {
        Ambient,
        MusicBackground,
        SoundEffect
    }

    [Serializable]
    public class AudioClipData
    {
        public string Name;
        
        public AudioClipType type;
        
        public AudioClip clip;

        [Range(0.0f, 1.0f)]
        public float volume = 1.0f;

        [Range(0.0f, 1.0f)]
        public float pitch = 1.0f;

        public float delay = 0.0f;

        public bool isLoop = false;

        public int loopTime = 1;

        public float delayPerLoop;

        public bool isStartFade = false;

        public float startFadeTime = 0.0f;

        public bool isEndFade = false;

        public float endFadeTime = 0.0f;
		
        [NonSerialized]
        [HideInInspector]
        public AudioSource audioSource;
    }
}



