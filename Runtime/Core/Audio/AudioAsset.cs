using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioAsset", menuName = "Game Audio / Create Audio Asset")]
public class AudioAsset : ScriptableObject
{
   public List<AudioClipData> Music;
   
   public List<AudioClip> MusicPlaylist;
   
   public List<AudioClipData> SoundFX;
}
