using System;
using UnityEngine;

namespace Code.Audio
{
  /// <summary>
  /// ScriptableObject containing all audio clips.
  /// Create via Assets → Create → Audio → Audio Library
  /// </summary>
  [CreateAssetMenu(fileName = "AudioLibrary", menuName = "Audio/Audio Library")]
  public class AudioLibrary : ScriptableObject
  {
    [Header("Music")]
    [SerializeField] private MusicEntry[] _musicClips;

    [Header("SFX")]
    [SerializeField] private SFXEntry[] _sfxClips;

    public AudioClip GetMusicClip(MusicType type)
    {
      if (_musicClips == null) return null;

      foreach (var entry in _musicClips)
      {
        if (entry.Type == type)
          return entry.Clip;
      }
      return null;
    }

    public AudioClip GetSFXClip(SFXType type)
    {
      if (_sfxClips == null) return null;

      foreach (var entry in _sfxClips)
      {
        if (entry.Type == type)
          return entry.Clip;
      }
      return null;
    }

    public float GetSFXVolume(SFXType type)
    {
      if (_sfxClips == null) return 1f;

      foreach (var entry in _sfxClips)
      {
        if (entry.Type == type)
          return entry.VolumeMultiplier;
      }
      return 1f;
    }

    [Serializable]
    public class MusicEntry
    {
      public MusicType Type;
      public AudioClip Clip;
    }

    [Serializable]
    public class SFXEntry
    {
      public SFXType Type;
      public AudioClip Clip;
      [Range(0f, 2f)] public float VolumeMultiplier = 1f;
    }
  }
}
