using System;
using System.Collections.Generic;
using Code.Configs.Global;
using UnityEngine;

namespace Code.Gameplay.Features.Feedback.Services
{
  public interface IFloatingTextService
  {
    void SpawnFloatingText(Vector3 worldPosition, string text, Color color);
    void SpawnMoneyText(Vector3 worldPosition, int amount, bool isGain);
    void Update();
  }

  public class FloatingTextService : IFloatingTextService
  {
    private readonly FeedbackConfig _config;
    private readonly List<FloatingTextInstance> _activeTexts = new(16);
    private Camera _mainCamera;

    public FloatingTextService(FeedbackConfig config)
    {
      _config = config ?? throw new ArgumentNullException(nameof(config),
        "FeedbackConfig is required! Assign it in LevelConfig.");
    }

    public void SpawnFloatingText(Vector3 worldPosition, string text, Color color)
    {
      EnsureCamera();

      GameObject textObj = CreateTextObject(text, color);
      FloatingTextInstance instance = new FloatingTextInstance
      {
        GameObject = textObj,
        WorldPosition = worldPosition,
        StartTime = Time.time,
        Duration = _config.TextDuration,
        Canvas = textObj.GetComponentInParent<Canvas>()
      };

      _activeTexts.Add(instance);
    }

    public void SpawnMoneyText(Vector3 worldPosition, int amount, bool isGain)
    {
      string prefix = isGain ? "+" : "";
      string text = $"{prefix}{amount}";
      Color color = isGain ? _config.RewardColor : _config.PenaltyColor;

      SpawnFloatingText(worldPosition, text, color);
    }

    private void EnsureCamera()
    {
      if (_mainCamera == null)
        _mainCamera = Camera.main;
    }

    private GameObject CreateTextObject(string text, Color color)
    {
      GameObject canvasObj = new GameObject("FloatingTextCanvas");

      Canvas canvas = canvasObj.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.WorldSpace;
      canvas.sortingOrder = 500;

      canvasObj.transform.localScale = Vector3.one * 0.01f;

      GameObject textObj = new GameObject("FloatingText");
      textObj.transform.SetParent(canvasObj.transform, false);

      UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
      uiText.text = text;
      uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      uiText.fontSize = _config.FontSize * 3;
      uiText.fontStyle = FontStyle.Bold;
      uiText.color = color;
      uiText.alignment = TextAnchor.MiddleCenter;

      UnityEngine.UI.Outline outline = textObj.AddComponent<UnityEngine.UI.Outline>();
      outline.effectColor = Color.black;
      outline.effectDistance = new Vector2(2, -2);

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.sizeDelta = new Vector2(300f, 100f);

      return canvasObj;
    }

    public void Update()
    {
      EnsureCamera();

      for (int i = _activeTexts.Count - 1; i >= 0; i--)
      {
        FloatingTextInstance instance = _activeTexts[i];

        float elapsed = Time.time - instance.StartTime;
        float t = elapsed / instance.Duration;

        if (t >= 1f)
        {
          if (instance.GameObject != null)
            UnityEngine.Object.Destroy(instance.GameObject);

          _activeTexts.RemoveAt(i);
          continue;
        }

        Vector3 offset = Vector3.up * (_config.TextRiseSpeed * elapsed);
        instance.GameObject.transform.position = instance.WorldPosition + offset;

        if (_mainCamera != null)
        {
          instance.GameObject.transform.rotation = _mainCamera.transform.rotation;
        }

        float alpha = 1f - (t * t);
        UnityEngine.UI.Text uiText = instance.GameObject.GetComponentInChildren<UnityEngine.UI.Text>();
        if (uiText != null)
        {
          Color c = uiText.color;
          c.a = alpha;
          uiText.color = c;
        }
      }
    }

    private class FloatingTextInstance
    {
      public GameObject GameObject;
      public Vector3 WorldPosition;
      public float StartTime;
      public float Duration;
      public Canvas Canvas;
    }
  }
}
