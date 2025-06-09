using System;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Manages audio playback including music, ambient sounds, and sound effects.
    /// </summary>
    public interface IAudioManager
    {
        bool MusicEnable { get; }

        Action<bool> OnEnableMusic { get; set; }

        void PlaySound(AudioClip clip, float pitch, float volume = 1, Action finish = null);
        void PlaySound(AudioClipData info, Action finish = null);
        void PlaySound(AudioClip clip, Action finish = null);
        void PlayMusic(AudioClipType source, AudioClip clip, float volume = 1, bool loop = true);
        void FadeOut(AudioClipType source, float fade, float time, Action finish = null);
        void FadeIn(AudioClipType source, float time, Action finish = null);
        void StopMusic(AudioClipType source);
        void SetMusicEnable(bool enable);
        void SetSoundEnable(bool enable);
        bool IsMusicEnable();
        bool IsSoundEnable();
        void PauseAllMusic();
        void UnPauseAllMusic();
        string GetRandomSound();
        string GetRandomSoundForHighRarity();
        void ResumeAll();
        void PauseAll();
    }
}
