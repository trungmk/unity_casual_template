using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    public class AudioManager : BaseSystem, IAudioManager
    {
        private bool _musicEnable = true;

        private bool _soundEnable = true;

        private readonly List<AudioSource> _poolSources = new List<AudioSource>();

        private readonly List<AudioSource> _soundSources = new List<AudioSource>();

        private readonly Dictionary<AudioSource, float> _cachingFadeVolume = new Dictionary<AudioSource, float>();

        private readonly Dictionary<AudioSource, CoroutineHandle> _fadeCoroutine = new Dictionary<AudioSource, CoroutineHandle>();

        private readonly Dictionary<AudioClipType, AudioSource> _musicSources = new Dictionary<AudioClipType, AudioSource>();

        private readonly Dictionary<AudioSource, IEnumerator<float>> _musicPlayList = new Dictionary<AudioSource, IEnumerator<float>>();

        public readonly List<AudioSource> _pausedMusicSources = new List<AudioSource>();

        public Action<bool> OnEnableMusic { get; set; }

        public AudioSource CurrentPlayingAudioSource;

        private readonly List<string> _soundGroup = new List<string> { "Group1" };
        public bool MusicEnable => _musicEnable;
        private void Start()
        {
            AddMusic(AudioClipType.MusicBackground);
            AddMusic(AudioClipType.Ambient);
        }
        public void PlaySound(AudioClip clip, float pitch, float volume = 1, Action finish = null)
        {
            if (_soundEnable)
            {
                Timing.RunCoroutine(PlaySoundInPool(clip, pitch, volume, finish));
            }
        }

        public void FadeOut(AudioClipType source, float fade, float time, Action finish = null)
        {
            FadeOut(MusicSource(source), fade, time, finish);
        }

        public void FadeIn(AudioClipType source, float time, Action finish = null)
        {
            FadeIn(MusicSource(source), time, finish);
        }

        private void FadeOut(AudioSource source, float fade, float time, Action finish)
        {
            float volume = source.volume;
            if (_cachingFadeVolume.ContainsKey(source))
            {
                volume = _cachingFadeVolume[source];
            }
            else
            {
                _cachingFadeVolume.Add(source, volume);
            }

            if (_fadeCoroutine.ContainsKey(source))
            {
                Timing.KillCoroutines(_fadeCoroutine[source]);
                _fadeCoroutine.Remove(source);
            }

            var coroutine = Timing.RunCoroutine(Fade(source, volume, volume * fade, time, finish));
            _fadeCoroutine[source] = coroutine;
        }

        void FadeIn(AudioSource source, float time, Action finish)
        {
            float volume = source.volume;
            if (_cachingFadeVolume.ContainsKey(source))
            {
                volume = _cachingFadeVolume[source];
            }

            if (_fadeCoroutine.ContainsKey(source))
            {
                Timing.KillCoroutines(_fadeCoroutine[source]);
                _fadeCoroutine.Remove(source);
            }

            var coroutine = Timing.RunCoroutine(Fade(source, source.volume, volume, time, finish));
            _fadeCoroutine[source] = coroutine;
        }

        private IEnumerator<float> Fade(AudioSource source, float start, float end, float time, Action finish)
        {
            float totalTime = 0;
            while (totalTime < time)
            {
                yield return Timing.WaitForOneFrame;

                totalTime += Time.deltaTime;
                float volume = Mathf.Lerp(start, end, totalTime / time);
                source.volume = volume;
            }

            if (finish != null)
            {
                finish();
            }

            _fadeCoroutine.Remove(source);
        }


        private void AddMusic(AudioClipType source)
        {
            var musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            _musicSources.Add(source, musicSource);
        }

        private AudioSource MusicSource(AudioClipType source)
        {
            return _musicSources[source];
        }

        public void PlayMusic(AudioClipType source, AudioClip clip, float volume = 1, bool loop = true)
        {
            CurrentPlayingAudioSource = null;
            var audioSource = MusicSource(source);
            audioSource.loop = loop;
            audioSource.clip = clip;
            audioSource.volume = volume;

            if (_musicEnable)
            {
                StopPlayList(audioSource);
                audioSource.Play();
                CurrentPlayingAudioSource = audioSource;
            }
        }

        public void PlaySound(AudioClipData info, Action finish = null)
        {
            if (_soundEnable)
            {
                StartCoroutine(PlaySoundInPool(info.clip, finish));
            }
        }
        public void PlaySound(AudioClip clip, Action finish = null)
        {
            if (_soundEnable)
            {
                StartCoroutine(PlaySoundInPool(clip, finish));
            }
        }

        public bool IsMusicEnable()
        {
            return _musicEnable;
        }

        public void SetSoundEnable(bool enable)
        {
            _soundEnable = enable;

            if (!_soundEnable)
            {
                StopMusic(AudioClipType.Ambient);
            }
            else
            {
                foreach (var audioSource in _soundSources)
                {
                    if (!audioSource.isPlaying && audioSource.clip != null)
                    {
                        audioSource.Play();
                    }
                }
            }
        }

        public bool IsSoundEnable()
        {
            return _soundEnable;
        }

        public void PauseAllMusic()
        {
            _pausedMusicSources.Clear();
            foreach (var music in _musicSources)
            {
                if (music.Value.isPlaying)
                {
                    _pausedMusicSources.Add(music.Value);
                    music.Value.Pause();
                }
            }
        }

        public void UnPauseAllMusic()
        {
            foreach (var music in _pausedMusicSources)
            {
                music.UnPause();
            }
            _pausedMusicSources.Clear();
        }

        public void StopMusic(AudioClipType source)
        {
            var audioSource = MusicSource(source);
            StopPlayList(audioSource);
            audioSource.Stop();
        }

        public void SetMusicEnable(bool enable)
        {
            _musicEnable = enable;

            if (!_musicEnable)
            {
                StopMusic(AudioClipType.MusicBackground);
            }
            else if (_musicEnable)
            {
                foreach (var audioSource in _musicSources.Values)
                {
                    if (!audioSource.isPlaying && audioSource.clip != null)
                    {
                        audioSource.Play();
                    }
                }
            }

            if (OnEnableMusic != null)
            {
                OnEnableMusic(_musicEnable);
            }
        }

        public string GetRandomSound()
        {
            int randSound = Random.Range(0, _soundGroup.Count);
            return _soundGroup[randSound];
        }

        public string GetRandomSoundForHighRarity()
        {
            int randSound = Random.Range(0, _soundGroup.Count);
            return _soundGroup[randSound];
        }

        private void StopPlayList(AudioSource source)
        {
            if (_musicPlayList.ContainsKey(source))
            {
                var playing = _musicPlayList[source];
                Timing.RunCoroutine(playing);
                _musicPlayList.Remove(source);
            }
        }

        private void FinishPlayList(AudioSource source)
        {
            if (_musicPlayList.ContainsKey(source))
            {
                _musicPlayList.Remove(source);
            }
        }


        private IEnumerator<float> PlaySoundInPool(AudioClip clip, Action finish)
        {
            AudioSource source = GetAudioSource();
            _soundSources.Add(source);
            source.PlayOneShot(clip);

            while (source.isPlaying)
            {
                yield return Timing.WaitForOneFrame;
            }

            _soundSources.Remove(source);
            ReleaseAudioSource(source);

            if (finish != null)
            {
                finish();
            }
        }

        private IEnumerator<float> PlaySoundInPool(AudioClip clip, float pitch, float volume = 1, Action finish = null)
        {
            AudioSource source = GetAudioSource();
            _soundSources.Add(source);
            source.pitch = pitch;
            source.volume = volume;
            source.PlayOneShot(clip);

            while (source.isPlaying)
            {
                yield return Timing.WaitForOneFrame;
            }
            _soundSources.Remove(source);
            ReleaseAudioSource(source);
            if (finish != null)
            {
                finish();
            }
        }

        private AudioSource GetAudioSource()
        {
            if (_poolSources.Count != 0)
            {
                int lstIdx = _poolSources.Count - 1;
                var audioSource = _poolSources[lstIdx];
                _poolSources.RemoveAt(lstIdx);
                return audioSource;
            }
            else
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                return audioSource;
            }
        }

        private void ReleaseAudioSource(AudioSource audioSource)
        {
            _poolSources.Add(audioSource);
        }

        public override void OnPause()
        {
            PauseAll();
        }

        public override void OnResume()
        {
            ResumeAll();
        }

        public void ResumeAll()
        {
            foreach (var musicSource in _musicSources.Values)
            {
                musicSource.UnPause();
            }
            foreach (var soundSource in _soundSources)
            {
                soundSource.UnPause();
            }
        }

        public void PauseAll()
        {
            foreach (var musicSource in _musicSources.Values)
            {
                musicSource.Pause();
            }
            foreach (var soundSource in _soundSources)
            {
                soundSource.Pause();
            }
        }
    }
}

