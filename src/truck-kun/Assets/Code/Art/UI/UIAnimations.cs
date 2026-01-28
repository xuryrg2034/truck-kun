using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Art.UI
{
  /// <summary>
  /// UI animation helpers using DOTween.
  /// Provides common animations for panels, buttons, etc.
  /// </summary>
  public static class UIAnimations
  {
    #region Panel Animations

    /// <summary>
    /// Fade in a canvas group
    /// </summary>
    public static Tween FadeIn(CanvasGroup canvasGroup, float duration = 0.3f)
    {
      canvasGroup.alpha = 0f;
      canvasGroup.gameObject.SetActive(true);
      return canvasGroup.DOFade(1f, duration).SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// Fade out a canvas group
    /// </summary>
    public static Tween FadeOut(CanvasGroup canvasGroup, float duration = 0.3f, bool deactivate = true)
    {
      return canvasGroup.DOFade(0f, duration)
        .SetEase(Ease.InQuad)
        .OnComplete(() =>
        {
          if (deactivate)
            canvasGroup.gameObject.SetActive(false);
        });
    }

    /// <summary>
    /// Slide panel in from direction
    /// </summary>
    public static Tween SlideIn(RectTransform rect, SlideDirection direction, float duration = 0.4f)
    {
      Vector2 startOffset = GetSlideOffset(rect, direction);
      Vector2 endPos = rect.anchoredPosition;

      rect.anchoredPosition = endPos + startOffset;
      rect.gameObject.SetActive(true);

      return rect.DOAnchorPos(endPos, duration).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Slide panel out to direction
    /// </summary>
    public static Tween SlideOut(RectTransform rect, SlideDirection direction, float duration = 0.3f, bool deactivate = true)
    {
      Vector2 endOffset = GetSlideOffset(rect, direction);
      Vector2 startPos = rect.anchoredPosition;

      return rect.DOAnchorPos(startPos + endOffset, duration)
        .SetEase(Ease.InBack)
        .OnComplete(() =>
        {
          rect.anchoredPosition = startPos;
          if (deactivate)
            rect.gameObject.SetActive(false);
        });
    }

    private static Vector2 GetSlideOffset(RectTransform rect, SlideDirection direction)
    {
      float width = rect.rect.width;
      float height = rect.rect.height;

      return direction switch
      {
        SlideDirection.Left => new Vector2(-width - 100f, 0),
        SlideDirection.Right => new Vector2(width + 100f, 0),
        SlideDirection.Up => new Vector2(0, height + 100f),
        SlideDirection.Down => new Vector2(0, -height - 100f),
        _ => Vector2.zero
      };
    }

    /// <summary>
    /// Scale pop in animation
    /// </summary>
    public static Tween PopIn(Transform transform, float duration = 0.3f)
    {
      transform.localScale = Vector3.zero;
      transform.gameObject.SetActive(true);
      return transform.DOScale(1f, duration).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Scale pop out animation
    /// </summary>
    public static Tween PopOut(Transform transform, float duration = 0.2f, bool deactivate = true)
    {
      return transform.DOScale(0f, duration)
        .SetEase(Ease.InBack)
        .OnComplete(() =>
        {
          transform.localScale = Vector3.one;
          if (deactivate)
            transform.gameObject.SetActive(false);
        });
    }

    #endregion

    #region Button Animations

    /// <summary>
    /// Bounce effect on button press
    /// </summary>
    public static Tween ButtonBounce(Transform button, float scale = 0.9f, float duration = 0.1f)
    {
      return DOTween.Sequence()
        .Append(button.DOScale(scale, duration).SetEase(Ease.OutQuad))
        .Append(button.DOScale(1f, duration).SetEase(Ease.OutBack));
    }

    /// <summary>
    /// Pulse effect for highlighting
    /// </summary>
    public static Tween ButtonPulse(Transform button, float scale = 1.1f, float duration = 0.5f)
    {
      return button.DOScale(scale, duration)
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary>
    /// Shake effect for errors
    /// </summary>
    public static Tween ButtonShake(Transform button, float strength = 10f, float duration = 0.3f)
    {
      return button.DOShakePosition(duration, new Vector3(strength, 0, 0), 20, 90, false, true);
    }

    #endregion

    #region Text Animations

    /// <summary>
    /// Typewriter effect for text
    /// </summary>
    public static Tween TypewriterText(Text text, string fullText, float duration = 1f)
    {
      text.text = "";
      return DOTween.To(
        () => 0f,
        x => text.text = fullText.Substring(0, Mathf.FloorToInt(x * fullText.Length)),
        1f,
        duration
      ).SetEase(Ease.Linear);
    }

    /// <summary>
    /// Counter animation for numbers
    /// </summary>
    public static Tween CounterAnimation(Text text, int startValue, int endValue, float duration = 1f, string format = "{0}")
    {
      int current = startValue;
      return DOTween.To(
        () => current,
        x =>
        {
          current = x;
          text.text = string.Format(format, current);
        },
        endValue,
        duration
      ).SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// Floating text animation (like damage numbers)
    /// </summary>
    public static Sequence FloatingText(RectTransform text, float floatHeight = 50f, float duration = 1f)
    {
      Vector2 startPos = text.anchoredPosition;
      CanvasGroup cg = text.GetComponent<CanvasGroup>();
      if (cg == null)
        cg = text.gameObject.AddComponent<CanvasGroup>();

      cg.alpha = 1f;
      text.localScale = Vector3.one * 0.5f;

      return DOTween.Sequence()
        .Append(text.DOScale(1f, duration * 0.2f).SetEase(Ease.OutBack))
        .Join(text.DOAnchorPosY(startPos.y + floatHeight, duration).SetEase(Ease.OutQuad))
        .Insert(duration * 0.5f, cg.DOFade(0f, duration * 0.5f))
        .OnComplete(() =>
        {
          text.anchoredPosition = startPos;
          Object.Destroy(text.gameObject);
        });
    }

    #endregion

    #region Color Animations

    /// <summary>
    /// Flash color effect
    /// </summary>
    public static Tween FlashColor(Graphic graphic, Color flashColor, float duration = 0.2f)
    {
      Color originalColor = graphic.color;
      return DOTween.Sequence()
        .Append(graphic.DOColor(flashColor, duration * 0.5f))
        .Append(graphic.DOColor(originalColor, duration * 0.5f));
    }

    /// <summary>
    /// Gradient color cycle
    /// </summary>
    public static Tween ColorCycle(Graphic graphic, Color[] colors, float cycleDuration = 2f)
    {
      Sequence seq = DOTween.Sequence();
      float stepDuration = cycleDuration / colors.Length;

      for (int i = 0; i < colors.Length; i++)
      {
        Color targetColor = colors[(i + 1) % colors.Length];
        seq.Append(graphic.DOColor(targetColor, stepDuration));
      }

      return seq.SetLoops(-1);
    }

    #endregion
  }

  public enum SlideDirection
  {
    Left,
    Right,
    Up,
    Down
  }

  /// <summary>
  /// Component for easy button animation setup
  /// </summary>
  [RequireComponent(typeof(Button))]
  public class AnimatedButton : MonoBehaviour
  {
    [SerializeField] private float _pressScale = 0.95f;
    [SerializeField] private float _pressDuration = 0.1f;
    [SerializeField] private bool _enableHoverPulse = false;

    private Button _button;
    private Tween _hoverTween;

    private void Awake()
    {
      _button = GetComponent<Button>();
      _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
      UIAnimations.ButtonBounce(transform, _pressScale, _pressDuration);
    }

    public void OnPointerEnter()
    {
      if (_enableHoverPulse)
      {
        _hoverTween = UIAnimations.ButtonPulse(transform, 1.05f);
      }
    }

    public void OnPointerExit()
    {
      _hoverTween?.Kill();
      transform.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
      _hoverTween?.Kill();
    }
  }
}
