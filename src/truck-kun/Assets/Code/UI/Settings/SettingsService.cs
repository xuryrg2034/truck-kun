using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.UI.Settings
{
  #region Settings Data

  [Serializable]
  public class SettingsData
  {
    public float MusicVolume = 0.5f;
    public float SFXVolume = 0.7f;
    public bool Fullscreen = true;
    public int ResolutionIndex = -1; // -1 = current/default

    public SettingsData Clone()
    {
      return new SettingsData
      {
        MusicVolume = MusicVolume,
        SFXVolume = SFXVolume,
        Fullscreen = Fullscreen,
        ResolutionIndex = ResolutionIndex
      };
    }
  }

  #endregion

  #region Settings Service

  public interface ISettingsService
  {
    SettingsData CurrentSettings { get; }
    Resolution[] AvailableResolutions { get; }

    void LoadSettings();
    void SaveSettings();
    void ApplySettings();
    void ApplySettings(SettingsData data);
    void ResetToDefaults();

    event Action<SettingsData> OnSettingsChanged;
  }

  public class SettingsService : ISettingsService
  {
    private const string MusicVolumeKey = "Settings_MusicVolume";
    private const string SFXVolumeKey = "Settings_SFXVolume";
    private const string FullscreenKey = "Settings_Fullscreen";
    private const string ResolutionIndexKey = "Settings_ResolutionIndex";
    private const string SettingsExistKey = "Settings_Exists";

    private static SettingsService _instance;
    public static SettingsService Instance => _instance ??= new SettingsService();

    private SettingsData _currentSettings;
    private Resolution[] _availableResolutions;

    public SettingsData CurrentSettings => _currentSettings;
    public Resolution[] AvailableResolutions => _availableResolutions;

    public event Action<SettingsData> OnSettingsChanged;

    private SettingsService()
    {
      CacheResolutions();
      LoadSettings();
    }

    private void CacheResolutions()
    {
      // Get unique resolutions (filter out duplicates with different refresh rates)
      List<Resolution> uniqueResolutions = new();
      HashSet<string> seen = new();

      foreach (Resolution res in Screen.resolutions)
      {
        string key = $"{res.width}x{res.height}";
        if (!seen.Contains(key))
        {
          seen.Add(key);
          uniqueResolutions.Add(res);
        }
      }

      _availableResolutions = uniqueResolutions.ToArray();
    }

    public void LoadSettings()
    {
      _currentSettings = new SettingsData();

      if (PlayerPrefs.GetInt(SettingsExistKey, 0) == 1)
      {
        _currentSettings.MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.5f);
        _currentSettings.SFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 0.7f);
        _currentSettings.Fullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        _currentSettings.ResolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey, -1);

        Debug.Log($"[Settings] Loaded: Music={_currentSettings.MusicVolume:F2}, SFX={_currentSettings.SFXVolume:F2}, Fullscreen={_currentSettings.Fullscreen}");
      }
      else
      {
        Debug.Log("[Settings] No saved settings, using defaults");
      }

      ApplySettings();
    }

    public void SaveSettings()
    {
      PlayerPrefs.SetFloat(MusicVolumeKey, _currentSettings.MusicVolume);
      PlayerPrefs.SetFloat(SFXVolumeKey, _currentSettings.SFXVolume);
      PlayerPrefs.SetInt(FullscreenKey, _currentSettings.Fullscreen ? 1 : 0);
      PlayerPrefs.SetInt(ResolutionIndexKey, _currentSettings.ResolutionIndex);
      PlayerPrefs.SetInt(SettingsExistKey, 1);
      PlayerPrefs.Save();

      Debug.Log("[Settings] Saved");
    }

    public void ApplySettings()
    {
      ApplySettings(_currentSettings);
    }

    public void ApplySettings(SettingsData data)
    {
      _currentSettings = data.Clone();

      // Apply audio settings
      AudioListener.volume = 1f; // Master volume
      ApplyAudioSettings();

      // Apply display settings
      ApplyDisplaySettings();

      OnSettingsChanged?.Invoke(_currentSettings);
    }

    private void ApplyAudioSettings()
    {
      // Set global volumes that AudioSources can reference
      // Music and SFX volumes are stored and accessed via SettingsService
      // Individual AudioSources should query these values

      Debug.Log($"[Settings] Applied audio: Music={_currentSettings.MusicVolume:F2}, SFX={_currentSettings.SFXVolume:F2}");
    }

    private void ApplyDisplaySettings()
    {
      // Apply fullscreen
      if (Screen.fullScreen != _currentSettings.Fullscreen)
      {
        Screen.fullScreen = _currentSettings.Fullscreen;
      }

      // Apply resolution
      if (_currentSettings.ResolutionIndex >= 0 && _currentSettings.ResolutionIndex < _availableResolutions.Length)
      {
        Resolution res = _availableResolutions[_currentSettings.ResolutionIndex];
        if (Screen.width != res.width || Screen.height != res.height)
        {
          Screen.SetResolution(res.width, res.height, _currentSettings.Fullscreen);
          Debug.Log($"[Settings] Applied resolution: {res.width}x{res.height}");
        }
      }
    }

    public void ResetToDefaults()
    {
      _currentSettings = new SettingsData();
      ApplySettings();
      SaveSettings();

      Debug.Log("[Settings] Reset to defaults");
    }

    public float GetMusicVolume() => _currentSettings.MusicVolume;
    public float GetSFXVolume() => _currentSettings.SFXVolume;

    public void SetMusicVolume(float volume)
    {
      _currentSettings.MusicVolume = Mathf.Clamp01(volume);
      OnSettingsChanged?.Invoke(_currentSettings);
    }

    public void SetSFXVolume(float volume)
    {
      _currentSettings.SFXVolume = Mathf.Clamp01(volume);
      OnSettingsChanged?.Invoke(_currentSettings);
    }
  }

  #endregion

  #region Audio Helper

  /// <summary>
  /// Helper component for AudioSources to automatically apply settings volume
  /// </summary>
  public class SettingsAudioSource : MonoBehaviour
  {
    public enum AudioType { Music, SFX }

    [SerializeField] private AudioType _audioType = AudioType.SFX;
    [SerializeField] private float _baseVolume = 1f;

    private AudioSource _audioSource;

    private void Awake()
    {
      _audioSource = GetComponent<AudioSource>();
      if (_audioSource == null)
        _audioSource = gameObject.AddComponent<AudioSource>();

      ApplyVolume();
      SettingsService.Instance.OnSettingsChanged += OnSettingsChanged;
    }

    private void OnDestroy()
    {
      if (SettingsService.Instance != null)
        SettingsService.Instance.OnSettingsChanged -= OnSettingsChanged;
    }

    private void OnSettingsChanged(SettingsData data)
    {
      ApplyVolume();
    }

    private void ApplyVolume()
    {
      float settingsVolume = _audioType == AudioType.Music
        ? SettingsService.Instance.GetMusicVolume()
        : SettingsService.Instance.GetSFXVolume();

      _audioSource.volume = _baseVolume * settingsVolume;
    }

    public void SetBaseVolume(float volume)
    {
      _baseVolume = volume;
      ApplyVolume();
    }
  }

  #endregion
}
