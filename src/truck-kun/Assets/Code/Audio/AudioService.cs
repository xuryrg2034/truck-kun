using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Audio
{
  #region Enums

  public enum MusicType
  {
    None,
    MainMenu,
    Gameplay,
    Hub,
    GameOver,
    Victory
  }

  public enum SFXType
  {
    // Gameplay
    Hit,
    HitStrong,
    Collision,
    Ragdoll,

    // UI
    UIClick,
    UIHover,
    UIBack,

    // Economy
    Purchase,
    CoinPickup,
    MoneyLoss,

    // Quests
    QuestComplete,
    QuestFailed,
    QuestNew,

    // Feedback
    Violation,
    Warning,
    Success,

    // Vehicle
    EngineStart,
    EngineLoop,
    Brake,
    Skid
  }

  #endregion

  #region Settings

  [Serializable]
  public class AudioSettings
  {
    [Header("Volumes")]
    [Range(0f, 1f)] public float MasterVolume = 1f;
    [Range(0f, 1f)] public float MusicVolume = 0.5f;
    [Range(0f, 1f)] public float SFXVolume = 0.7f;

    [Header("Music")]
    public float MusicFadeDuration = 1f;

    [Header("SFX Pool")]
    public int SFXPoolSize = 10;
    public int MaxSimultaneousSFX = 5;
  }

  #endregion

  #region Interface

  public interface IAudioService
  {
    // Music
    void PlayMusic(MusicType type, bool loop = true);
    void StopMusic(bool fade = true);
    void PauseMusic();
    void ResumeMusic();

    // SFX
    void PlaySFX(SFXType type);
    void PlaySFX(SFXType type, Vector3 position);
    void PlaySFX(AudioClip clip, float volume = 1f);

    // Volume
    void SetMasterVolume(float volume);
    void SetMusicVolume(float volume);
    void SetSFXVolume(float volume);
    float GetMusicVolume();
    float GetSFXVolume();

    // State
    bool IsMusicPlaying { get; }
    MusicType CurrentMusic { get; }
  }

  #endregion

  #region Implementation

  /// <summary>
  /// Full audio service with music, SFX, and pooling.
  /// Auto-creates AudioSources and manages playback.
  /// </summary>
  public class AudioService : MonoBehaviour, IAudioService, IDisposable
  {
    private static AudioService _instance;
    public static AudioService Instance => _instance;

    [Header("Settings")]
    [SerializeField] private AudioSettings _settings = new();

    [Header("Audio Clips")]
    [SerializeField] private AudioLibrary _library;

    // Music
    private AudioSource _musicSource;
    private AudioSource _musicSourceSecondary; // For crossfade
    private MusicType _currentMusic = MusicType.None;
    private Coroutine _fadeCoroutine;

    // SFX Pool
    private Queue<AudioSource> _sfxPool;
    private List<AudioSource> _activeSFX;
    private int _sfxPlayingCount;

    // State
    public bool IsMusicPlaying => _musicSource != null && _musicSource.isPlaying;
    public MusicType CurrentMusic => _currentMusic;

    #region Initialization

    private void Awake()
    {
      if (_instance != null && _instance != this)
      {
        Destroy(gameObject);
        return;
      }

      _instance = this;
      DontDestroyOnLoad(gameObject);

      Initialize();
    }

    private void Initialize()
    {
      // Create music sources
      _musicSource = CreateAudioSource("MusicSource");
      _musicSource.loop = true;
      _musicSource.priority = 0; // Highest priority

      _musicSourceSecondary = CreateAudioSource("MusicSourceSecondary");
      _musicSourceSecondary.loop = true;
      _musicSourceSecondary.priority = 0;

      // Create SFX pool
      _sfxPool = new Queue<AudioSource>(_settings.SFXPoolSize);
      _activeSFX = new List<AudioSource>(_settings.SFXPoolSize);

      for (int i = 0; i < _settings.SFXPoolSize; i++)
      {
        AudioSource source = CreateAudioSource($"SFXSource_{i}");
        source.playOnAwake = false;
        _sfxPool.Enqueue(source);
      }

      // Load library from Resources if not assigned
      if (_library == null)
      {
        _library = Resources.Load<AudioLibrary>("AudioLibrary");
        if (_library == null)
        {
          Debug.LogWarning("[AudioService] AudioLibrary not found in Resources. Create one at Assets/Resources/AudioLibrary.asset");
          _library = ScriptableObject.CreateInstance<AudioLibrary>();
        }
      }

      // Load saved volumes
      LoadVolumeSettings();

      Debug.Log("[AudioService] Initialized");
    }

    private AudioSource CreateAudioSource(string name)
    {
      GameObject go = new GameObject(name);
      go.transform.SetParent(transform);
      return go.AddComponent<AudioSource>();
    }

    #endregion

    #region Music

    public void PlayMusic(MusicType type, bool loop = true)
    {
      if (type == MusicType.None)
      {
        StopMusic();
        return;
      }

      if (_currentMusic == type && _musicSource.isPlaying)
        return;

      AudioClip clip = _library.GetMusicClip(type);
      if (clip == null)
      {
        Debug.LogWarning($"[AudioService] No clip found for music type: {type}");
        return;
      }

      // Crossfade to new track
      if (_fadeCoroutine != null)
        StopCoroutine(_fadeCoroutine);

      _fadeCoroutine = StartCoroutine(CrossfadeMusic(clip, loop));
      _currentMusic = type;

      Debug.Log($"[AudioService] Playing music: {type}");
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, bool loop)
    {
      float duration = _settings.MusicFadeDuration;
      float targetVolume = _settings.MusicVolume * _settings.MasterVolume;

      // If currently playing, crossfade
      if (_musicSource.isPlaying)
      {
        // Setup secondary source with new clip
        _musicSourceSecondary.clip = newClip;
        _musicSourceSecondary.loop = loop;
        _musicSourceSecondary.volume = 0f;
        _musicSourceSecondary.Play();

        // Fade
        float elapsed = 0f;
        float startVolume = _musicSource.volume;

        while (elapsed < duration)
        {
          elapsed += Time.unscaledDeltaTime;
          float t = elapsed / duration;

          _musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
          _musicSourceSecondary.volume = Mathf.Lerp(0f, targetVolume, t);

          yield return null;
        }

        _musicSource.Stop();

        // Swap sources
        (_musicSource, _musicSourceSecondary) = (_musicSourceSecondary, _musicSource);
      }
      else
      {
        // Simple fade in
        _musicSource.clip = newClip;
        _musicSource.loop = loop;
        _musicSource.volume = 0f;
        _musicSource.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
          elapsed += Time.unscaledDeltaTime;
          _musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
          yield return null;
        }

        _musicSource.volume = targetVolume;
      }

      _fadeCoroutine = null;
    }

    public void StopMusic(bool fade = true)
    {
      if (!_musicSource.isPlaying)
        return;

      if (_fadeCoroutine != null)
        StopCoroutine(_fadeCoroutine);

      if (fade)
      {
        _fadeCoroutine = StartCoroutine(FadeOutMusic());
      }
      else
      {
        _musicSource.Stop();
        _currentMusic = MusicType.None;
      }
    }

    private IEnumerator FadeOutMusic()
    {
      float duration = _settings.MusicFadeDuration;
      float startVolume = _musicSource.volume;
      float elapsed = 0f;

      while (elapsed < duration)
      {
        elapsed += Time.unscaledDeltaTime;
        _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
        yield return null;
      }

      _musicSource.Stop();
      _musicSource.volume = _settings.MusicVolume * _settings.MasterVolume;
      _currentMusic = MusicType.None;
      _fadeCoroutine = null;
    }

    public void PauseMusic()
    {
      _musicSource.Pause();
    }

    public void ResumeMusic()
    {
      _musicSource.UnPause();
    }

    #endregion

    #region SFX

    public void PlaySFX(SFXType type)
    {
      AudioClip clip = _library.GetSFXClip(type);
      if (clip == null)
      {
        Debug.LogWarning($"[AudioService] No clip found for SFX type: {type}");
        return;
      }

      PlaySFXInternal(clip, transform.position, _library.GetSFXVolume(type));
    }

    public void PlaySFX(SFXType type, Vector3 position)
    {
      AudioClip clip = _library.GetSFXClip(type);
      if (clip == null)
        return;

      PlaySFXInternal(clip, position, _library.GetSFXVolume(type));
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
      if (clip == null)
        return;

      PlaySFXInternal(clip, transform.position, volume);
    }

    private void PlaySFXInternal(AudioClip clip, Vector3 position, float volumeMultiplier)
    {
      // Check simultaneous limit
      if (_sfxPlayingCount >= _settings.MaxSimultaneousSFX)
        return;

      // Get source from pool
      AudioSource source = GetPooledSource();
      if (source == null)
        return;

      source.transform.position = position;
      source.clip = clip;
      source.volume = _settings.SFXVolume * _settings.MasterVolume * volumeMultiplier;
      source.spatialBlend = 0f; // 2D sound (change to 1f for 3D)
      source.Play();

      _sfxPlayingCount++;
      _activeSFX.Add(source);

      StartCoroutine(ReturnToPoolWhenDone(source, clip.length));
    }

    private AudioSource GetPooledSource()
    {
      if (_sfxPool.Count > 0)
        return _sfxPool.Dequeue();

      // Pool exhausted - find finished source
      for (int i = _activeSFX.Count - 1; i >= 0; i--)
      {
        if (!_activeSFX[i].isPlaying)
        {
          AudioSource source = _activeSFX[i];
          _activeSFX.RemoveAt(i);
          _sfxPlayingCount--;
          return source;
        }
      }

      return null;
    }

    private IEnumerator ReturnToPoolWhenDone(AudioSource source, float delay)
    {
      yield return new WaitForSeconds(delay + 0.1f);

      if (_activeSFX.Contains(source))
      {
        _activeSFX.Remove(source);
        _sfxPool.Enqueue(source);
        _sfxPlayingCount--;
      }
    }

    #endregion

    #region Volume Control

    public void SetMasterVolume(float volume)
    {
      _settings.MasterVolume = Mathf.Clamp01(volume);
      ApplyVolumeSettings();
      SaveVolumeSettings();
    }

    public void SetMusicVolume(float volume)
    {
      _settings.MusicVolume = Mathf.Clamp01(volume);
      ApplyVolumeSettings();
      SaveVolumeSettings();
    }

    public void SetSFXVolume(float volume)
    {
      _settings.SFXVolume = Mathf.Clamp01(volume);
      SaveVolumeSettings();
    }

    public float GetMusicVolume() => _settings.MusicVolume;
    public float GetSFXVolume() => _settings.SFXVolume;

    private void ApplyVolumeSettings()
    {
      if (_musicSource != null)
        _musicSource.volume = _settings.MusicVolume * _settings.MasterVolume;

      if (_musicSourceSecondary != null)
        _musicSourceSecondary.volume = _settings.MusicVolume * _settings.MasterVolume;
    }

    private void SaveVolumeSettings()
    {
      PlayerPrefs.SetFloat("Audio_Master", _settings.MasterVolume);
      PlayerPrefs.SetFloat("Audio_Music", _settings.MusicVolume);
      PlayerPrefs.SetFloat("Audio_SFX", _settings.SFXVolume);
      PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
      _settings.MasterVolume = PlayerPrefs.GetFloat("Audio_Master", 1f);
      _settings.MusicVolume = PlayerPrefs.GetFloat("Audio_Music", 0.5f);
      _settings.SFXVolume = PlayerPrefs.GetFloat("Audio_SFX", 0.7f);
      ApplyVolumeSettings();
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
      StopMusic(false);
      StopAllCoroutines();
    }

    private void OnDestroy()
    {
      if (_instance == this)
        _instance = null;
    }

    #endregion
  }

  #endregion

  #region Audio Library

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

  #endregion
}
