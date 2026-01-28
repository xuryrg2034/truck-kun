using UnityEngine;
using UnityEngine.UI;

namespace Code.Audio
{
  /// <summary>
  /// Static helpers for quick audio playback.
  /// </summary>
  public static class Audio
  {
    /// <summary>
    /// Get or create AudioService instance (fallback if Zenject not configured)
    /// </summary>
    private static AudioService GetService()
    {
      if (AudioService.Instance != null)
        return AudioService.Instance;

      // Fallback: create AudioService if not exists
      var existing = Object.FindFirstObjectByType<AudioService>();
      if (existing != null)
        return existing;

      var go = new GameObject("[AudioService]");
      Object.DontDestroyOnLoad(go);
      return go.AddComponent<AudioService>();
    }

    /// <summary>
    /// Play SFX by type
    /// </summary>
    public static void PlaySFX(SFXType type)
    {
      GetService()?.PlaySFX(type);
    }

    /// <summary>
    /// Play SFX at position
    /// </summary>
    public static void PlaySFX(SFXType type, Vector3 position)
    {
      GetService()?.PlaySFX(type, position);
    }

    /// <summary>
    /// Play music by type
    /// </summary>
    public static void PlayMusic(MusicType type)
    {
      GetService()?.PlayMusic(type);
    }

    /// <summary>
    /// Stop current music
    /// </summary>
    public static void StopMusic()
    {
      GetService()?.StopMusic();
    }

    /// <summary>
    /// Play UI click sound
    /// </summary>
    public static void UIClick()
    {
      PlaySFX(SFXType.UIClick);
    }

    /// <summary>
    /// Play hit sound with intensity
    /// </summary>
    public static void Hit(float intensity)
    {
      if (intensity > 0.7f)
        PlaySFX(SFXType.HitStrong);
      else
        PlaySFX(SFXType.Hit);
    }
  }

  /// <summary>
  /// Component for UI buttons to play sounds on click.
  /// Attach to any Button.
  /// </summary>
  [RequireComponent(typeof(Button))]
  public class UIButtonSound : MonoBehaviour
  {
    [SerializeField] private SFXType _clickSound = SFXType.UIClick;
    [SerializeField] private SFXType _hoverSound = SFXType.UIHover;
    [SerializeField] private bool _playHoverSound = false;

    private Button _button;

    private void Awake()
    {
      _button = GetComponent<Button>();
      _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
      Audio.PlaySFX(_clickSound);
    }

    public void OnPointerEnter()
    {
      if (_playHoverSound)
        Audio.PlaySFX(_hoverSound);
    }

    private void OnDestroy()
    {
      if (_button != null)
        _button.onClick.RemoveListener(OnClick);
    }
  }

  /// <summary>
  /// Component for playing music in a scene.
  /// Add to scene root object.
  /// </summary>
  public class SceneMusic : MonoBehaviour
  {
    [SerializeField] private MusicType _musicType = MusicType.Gameplay;
    [SerializeField] private bool _playOnStart = true;
    [SerializeField] private float _startDelay = 0f;

    private void Start()
    {
      if (_playOnStart)
      {
        if (_startDelay > 0)
          Invoke(nameof(PlayMusic), _startDelay);
        else
          PlayMusic();
      }
    }

    private void PlayMusic()
    {
      Audio.PlayMusic(_musicType);
    }

    private void OnDestroy()
    {
      // Optionally stop music when scene unloads
      // Audio.StopMusic();
    }
  }

  /// <summary>
  /// Plays sound on trigger enter.
  /// Useful for ambient zones.
  /// </summary>
  [RequireComponent(typeof(Collider))]
  public class TriggerSound : MonoBehaviour
  {
    [SerializeField] private SFXType _sound = SFXType.Success;
    [SerializeField] private bool _playOnce = true;
    [SerializeField] private string _triggerTag = "Player";

    private bool _played;

    private void OnTriggerEnter(Collider other)
    {
      if (_played && _playOnce)
        return;

      if (!string.IsNullOrEmpty(_triggerTag) && !other.CompareTag(_triggerTag))
        return;

      Audio.PlaySFX(_sound, transform.position);
      _played = true;
    }

    public void ResetTrigger()
    {
      _played = false;
    }
  }
}
