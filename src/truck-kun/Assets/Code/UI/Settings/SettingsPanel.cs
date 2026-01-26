using System;
using System.Collections.Generic;
using Code.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Settings
{
  /// <summary>
  /// Reusable settings panel UI component
  /// </summary>
  public class SettingsPanel : MonoBehaviour
  {
    private static readonly Color PanelColor = new Color(0.12f, 0.12f, 0.15f, 0.98f);
    private static readonly Color SliderFillColor = new Color(0.2f, 0.65f, 0.3f);
    private static readonly Color SliderBgColor = new Color(0.2f, 0.2f, 0.25f);
    private static readonly Color ButtonColor = new Color(0.5f, 0.45f, 0.55f);
    private static readonly Color DangerColor = new Color(0.75f, 0.25f, 0.25f);
    private static readonly Color ToggleOnColor = new Color(0.2f, 0.65f, 0.3f);
    private static readonly Color ToggleOffColor = new Color(0.4f, 0.4f, 0.45f);

    private GameObject _panel;
    private GameObject _overlay;
    private Slider _musicSlider;
    private Slider _sfxSlider;
    private Toggle _fullscreenToggle;
    private Dropdown _resolutionDropdown;
    private Text _musicValueText;
    private Text _sfxValueText;
    private GameObject _deleteSaveButton;

    private SettingsData _tempSettings;
    private Action _onClose;
    private bool _showDeleteSave;

    public bool IsOpen => _panel != null && _panel.activeSelf;

    /// <summary>
    /// Show settings panel
    /// </summary>
    /// <param name="parent">Parent transform for the panel</param>
    /// <param name="onClose">Callback when panel is closed</param>
    /// <param name="showDeleteSave">Show delete save button (for main menu)</param>
    public void Show(Transform parent, Action onClose = null, bool showDeleteSave = false)
    {
      _onClose = onClose;
      _showDeleteSave = showDeleteSave;
      _tempSettings = SettingsService.Instance.CurrentSettings.Clone();

      if (_panel == null)
        CreatePanel(parent);

      // Show/hide delete save button
      if (_deleteSaveButton != null)
        _deleteSaveButton.SetActive(_showDeleteSave && GameStateService.Instance.HasSaveData());

      UpdateUI();
      _panel.SetActive(true);
    }

    public void Hide()
    {
      if (_panel != null)
        _panel.SetActive(false);

      _onClose?.Invoke();
    }

    private void CreatePanel(Transform parent)
    {
      // Root object
      _panel = new GameObject("SettingsPanel");
      _panel.transform.SetParent(parent, false);

      RectTransform panelRect = _panel.AddComponent<RectTransform>();
      panelRect.anchorMin = Vector2.zero;
      panelRect.anchorMax = Vector2.one;
      panelRect.offsetMin = Vector2.zero;
      panelRect.offsetMax = Vector2.zero;

      // Overlay
      _overlay = CreateOverlay(_panel.transform);

      // Panel content
      GameObject content = CreatePanelContent(_panel.transform);

      // Title
      CreateTitle(content.transform);

      // Settings container
      GameObject settingsContainer = CreateSettingsContainer(content.transform);

      // Music volume
      _musicSlider = CreateVolumeSlider(settingsContainer.transform, "Музыка", _tempSettings.MusicVolume,
        out _musicValueText, OnMusicVolumeChanged);

      // SFX volume
      _sfxSlider = CreateVolumeSlider(settingsContainer.transform, "Звуки", _tempSettings.SFXVolume,
        out _sfxValueText, OnSFXVolumeChanged);

      // Spacer
      CreateSpacer(settingsContainer.transform, 15f);

      // Fullscreen toggle
      _fullscreenToggle = CreateToggle(settingsContainer.transform, "Полный экран", _tempSettings.Fullscreen,
        OnFullscreenChanged);

      // Resolution dropdown
      _resolutionDropdown = CreateResolutionDropdown(settingsContainer.transform);

      // Spacer
      CreateSpacer(settingsContainer.transform, 15f);

      // Delete save button (optional, for main menu)
      _deleteSaveButton = CreateDeleteSaveButton(settingsContainer.transform);
      _deleteSaveButton.SetActive(false); // Hidden by default

      // Spacer
      CreateSpacer(settingsContainer.transform, 10f);

      // Buttons
      CreateButtons(content.transform);
    }

    private GameObject CreateDeleteSaveButton(Transform parent)
    {
      GameObject buttonObj = new GameObject("DeleteSaveButton");
      buttonObj.transform.SetParent(parent, false);

      Image buttonBg = buttonObj.AddComponent<Image>();
      buttonBg.color = DangerColor;

      Button button = buttonObj.AddComponent<Button>();
      button.targetGraphic = buttonBg;

      ColorBlock colors = button.colors;
      colors.normalColor = DangerColor;
      colors.highlightedColor = DangerColor * 1.2f;
      colors.pressedColor = DangerColor * 0.8f;
      button.colors = colors;

      button.onClick.AddListener(OnDeleteSaveClicked);

      LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
      layout.preferredHeight = 45f;

      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(buttonObj.transform, false);

      Text text = textObj.AddComponent<Text>();
      text.text = "УДАЛИТЬ СОХРАНЕНИЕ";
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = 20;
      text.fontStyle = FontStyle.Bold;
      text.color = Color.white;
      text.alignment = TextAnchor.MiddleCenter;

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.anchorMin = Vector2.zero;
      textRect.anchorMax = Vector2.one;
      textRect.offsetMin = Vector2.zero;
      textRect.offsetMax = Vector2.zero;

      return buttonObj;
    }

    private GameObject CreateOverlay(Transform parent)
    {
      GameObject overlay = new GameObject("Overlay");
      overlay.transform.SetParent(parent, false);

      Image overlayImage = overlay.AddComponent<Image>();
      overlayImage.color = new Color(0f, 0f, 0f, 0.8f);

      RectTransform overlayRect = overlay.GetComponent<RectTransform>();
      overlayRect.anchorMin = Vector2.zero;
      overlayRect.anchorMax = Vector2.one;
      overlayRect.offsetMin = Vector2.zero;
      overlayRect.offsetMax = Vector2.zero;

      Button overlayButton = overlay.AddComponent<Button>();
      overlayButton.onClick.AddListener(OnCloseClicked);

      return overlay;
    }

    private GameObject CreatePanelContent(Transform parent)
    {
      GameObject content = new GameObject("Content");
      content.transform.SetParent(parent, false);

      Image contentBg = content.AddComponent<Image>();
      contentBg.color = PanelColor;

      RectTransform contentRect = content.GetComponent<RectTransform>();
      contentRect.anchorMin = new Vector2(0.5f, 0.5f);
      contentRect.anchorMax = new Vector2(0.5f, 0.5f);
      contentRect.sizeDelta = new Vector2(550f, 500f);

      return content;
    }

    private void CreateTitle(Transform parent)
    {
      GameObject titleObj = new GameObject("Title");
      titleObj.transform.SetParent(parent, false);

      Text titleText = titleObj.AddComponent<Text>();
      titleText.text = "НАСТРОЙКИ";
      titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      titleText.fontSize = 36;
      titleText.fontStyle = FontStyle.Bold;
      titleText.color = Color.white;
      titleText.alignment = TextAnchor.MiddleCenter;

      RectTransform titleRect = titleObj.GetComponent<RectTransform>();
      titleRect.anchorMin = new Vector2(0f, 0.88f);
      titleRect.anchorMax = new Vector2(1f, 1f);
      titleRect.offsetMin = Vector2.zero;
      titleRect.offsetMax = Vector2.zero;
    }

    private GameObject CreateSettingsContainer(Transform parent)
    {
      GameObject container = new GameObject("SettingsContainer");
      container.transform.SetParent(parent, false);

      RectTransform containerRect = container.AddComponent<RectTransform>();
      containerRect.anchorMin = new Vector2(0f, 0.22f);
      containerRect.anchorMax = new Vector2(1f, 0.88f);
      containerRect.offsetMin = new Vector2(40f, 0f);
      containerRect.offsetMax = new Vector2(-40f, -10f);

      VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
      layout.spacing = 12f;
      layout.childAlignment = TextAnchor.UpperCenter;
      layout.childControlWidth = true;
      layout.childControlHeight = false;
      layout.childForceExpandWidth = true;

      return container;
    }

    private Slider CreateVolumeSlider(Transform parent, string label, float initialValue, out Text valueText, Action<float> onChanged)
    {
      GameObject container = new GameObject("Slider_" + label);
      container.transform.SetParent(parent, false);

      LayoutElement containerLayout = container.AddComponent<LayoutElement>();
      containerLayout.preferredHeight = 55f;

      // Label
      GameObject labelObj = new GameObject("Label");
      labelObj.transform.SetParent(container.transform, false);

      Text labelText = labelObj.AddComponent<Text>();
      labelText.text = label;
      labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      labelText.fontSize = 24;
      labelText.color = Color.white;
      labelText.alignment = TextAnchor.MiddleLeft;

      RectTransform labelRect = labelObj.GetComponent<RectTransform>();
      labelRect.anchorMin = new Vector2(0f, 0.5f);
      labelRect.anchorMax = new Vector2(0.25f, 1f);
      labelRect.offsetMin = Vector2.zero;
      labelRect.offsetMax = Vector2.zero;

      // Slider background
      GameObject sliderBgObj = new GameObject("SliderBackground");
      sliderBgObj.transform.SetParent(container.transform, false);

      Image sliderBg = sliderBgObj.AddComponent<Image>();
      sliderBg.color = SliderBgColor;

      RectTransform sliderBgRect = sliderBgObj.GetComponent<RectTransform>();
      sliderBgRect.anchorMin = new Vector2(0.28f, 0.25f);
      sliderBgRect.anchorMax = new Vector2(0.85f, 0.75f);
      sliderBgRect.offsetMin = Vector2.zero;
      sliderBgRect.offsetMax = Vector2.zero;

      // Slider fill area
      GameObject fillAreaObj = new GameObject("FillArea");
      fillAreaObj.transform.SetParent(sliderBgObj.transform, false);

      RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
      fillAreaRect.anchorMin = Vector2.zero;
      fillAreaRect.anchorMax = Vector2.one;
      fillAreaRect.offsetMin = Vector2.zero;
      fillAreaRect.offsetMax = Vector2.zero;

      // Slider fill
      GameObject sliderFillObj = new GameObject("Fill");
      sliderFillObj.transform.SetParent(fillAreaObj.transform, false);

      Image sliderFill = sliderFillObj.AddComponent<Image>();
      sliderFill.color = SliderFillColor;

      RectTransform sliderFillRect = sliderFillObj.GetComponent<RectTransform>();
      sliderFillRect.anchorMin = Vector2.zero;
      sliderFillRect.anchorMax = new Vector2(initialValue, 1f);
      sliderFillRect.offsetMin = Vector2.zero;
      sliderFillRect.offsetMax = Vector2.zero;

      // Slider component
      Slider slider = sliderBgObj.AddComponent<Slider>();
      slider.fillRect = sliderFillRect;
      slider.minValue = 0f;
      slider.maxValue = 1f;
      slider.value = initialValue;

      // Value text
      GameObject valueObj = new GameObject("Value");
      valueObj.transform.SetParent(container.transform, false);

      valueText = valueObj.AddComponent<Text>();
      valueText.text = $"{Mathf.RoundToInt(initialValue * 100)}%";
      valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      valueText.fontSize = 22;
      valueText.color = new Color(0.8f, 0.8f, 0.8f);
      valueText.alignment = TextAnchor.MiddleRight;

      RectTransform valueRect = valueObj.GetComponent<RectTransform>();
      valueRect.anchorMin = new Vector2(0.87f, 0.25f);
      valueRect.anchorMax = new Vector2(1f, 0.75f);
      valueRect.offsetMin = Vector2.zero;
      valueRect.offsetMax = Vector2.zero;

      Text capturedValueText = valueText;
      slider.onValueChanged.AddListener((v) =>
      {
        sliderFillRect.anchorMax = new Vector2(v, 1f);
        capturedValueText.text = $"{Mathf.RoundToInt(v * 100)}%";
        onChanged?.Invoke(v);
      });

      return slider;
    }

    private Toggle CreateToggle(Transform parent, string label, bool initialValue, Action<bool> onChanged)
    {
      GameObject container = new GameObject("Toggle_" + label);
      container.transform.SetParent(parent, false);

      LayoutElement containerLayout = container.AddComponent<LayoutElement>();
      containerLayout.preferredHeight = 50f;

      // Label
      GameObject labelObj = new GameObject("Label");
      labelObj.transform.SetParent(container.transform, false);

      Text labelText = labelObj.AddComponent<Text>();
      labelText.text = label;
      labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      labelText.fontSize = 24;
      labelText.color = Color.white;
      labelText.alignment = TextAnchor.MiddleLeft;

      RectTransform labelRect = labelObj.GetComponent<RectTransform>();
      labelRect.anchorMin = new Vector2(0f, 0f);
      labelRect.anchorMax = new Vector2(0.6f, 1f);
      labelRect.offsetMin = Vector2.zero;
      labelRect.offsetMax = Vector2.zero;

      // Toggle background
      GameObject toggleBgObj = new GameObject("ToggleBackground");
      toggleBgObj.transform.SetParent(container.transform, false);

      Image toggleBg = toggleBgObj.AddComponent<Image>();
      toggleBg.color = initialValue ? ToggleOnColor : ToggleOffColor;

      RectTransform toggleBgRect = toggleBgObj.GetComponent<RectTransform>();
      toggleBgRect.anchorMin = new Vector2(0.65f, 0.15f);
      toggleBgRect.anchorMax = new Vector2(0.85f, 0.85f);
      toggleBgRect.offsetMin = Vector2.zero;
      toggleBgRect.offsetMax = Vector2.zero;

      // Toggle checkmark
      GameObject checkmarkObj = new GameObject("Checkmark");
      checkmarkObj.transform.SetParent(toggleBgObj.transform, false);

      Text checkmark = checkmarkObj.AddComponent<Text>();
      checkmark.text = initialValue ? "ДА" : "НЕТ";
      checkmark.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      checkmark.fontSize = 18;
      checkmark.fontStyle = FontStyle.Bold;
      checkmark.color = Color.white;
      checkmark.alignment = TextAnchor.MiddleCenter;

      RectTransform checkmarkRect = checkmarkObj.GetComponent<RectTransform>();
      checkmarkRect.anchorMin = Vector2.zero;
      checkmarkRect.anchorMax = Vector2.one;
      checkmarkRect.offsetMin = Vector2.zero;
      checkmarkRect.offsetMax = Vector2.zero;

      // Toggle component
      Toggle toggle = toggleBgObj.AddComponent<Toggle>();
      toggle.isOn = initialValue;
      toggle.graphic = toggleBg;

      toggle.onValueChanged.AddListener((v) =>
      {
        toggleBg.color = v ? ToggleOnColor : ToggleOffColor;
        checkmark.text = v ? "ДА" : "НЕТ";
        onChanged?.Invoke(v);
      });

      return toggle;
    }

    private Dropdown CreateResolutionDropdown(Transform parent)
    {
      GameObject container = new GameObject("Resolution");
      container.transform.SetParent(parent, false);

      LayoutElement containerLayout = container.AddComponent<LayoutElement>();
      containerLayout.preferredHeight = 50f;

      // Label
      GameObject labelObj = new GameObject("Label");
      labelObj.transform.SetParent(container.transform, false);

      Text labelText = labelObj.AddComponent<Text>();
      labelText.text = "Разрешение";
      labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      labelText.fontSize = 24;
      labelText.color = Color.white;
      labelText.alignment = TextAnchor.MiddleLeft;

      RectTransform labelRect = labelObj.GetComponent<RectTransform>();
      labelRect.anchorMin = new Vector2(0f, 0f);
      labelRect.anchorMax = new Vector2(0.4f, 1f);
      labelRect.offsetMin = Vector2.zero;
      labelRect.offsetMax = Vector2.zero;

      // Dropdown background
      GameObject dropdownObj = new GameObject("Dropdown");
      dropdownObj.transform.SetParent(container.transform, false);

      Image dropdownBg = dropdownObj.AddComponent<Image>();
      dropdownBg.color = SliderBgColor;

      RectTransform dropdownRect = dropdownObj.GetComponent<RectTransform>();
      dropdownRect.anchorMin = new Vector2(0.42f, 0.1f);
      dropdownRect.anchorMax = new Vector2(1f, 0.9f);
      dropdownRect.offsetMin = Vector2.zero;
      dropdownRect.offsetMax = Vector2.zero;

      // Dropdown label
      GameObject dropdownLabelObj = new GameObject("Label");
      dropdownLabelObj.transform.SetParent(dropdownObj.transform, false);

      Text dropdownLabel = dropdownLabelObj.AddComponent<Text>();
      dropdownLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      dropdownLabel.fontSize = 20;
      dropdownLabel.color = Color.white;
      dropdownLabel.alignment = TextAnchor.MiddleCenter;

      RectTransform dropdownLabelRect = dropdownLabelObj.GetComponent<RectTransform>();
      dropdownLabelRect.anchorMin = Vector2.zero;
      dropdownLabelRect.anchorMax = Vector2.one;
      dropdownLabelRect.offsetMin = new Vector2(10f, 0f);
      dropdownLabelRect.offsetMax = new Vector2(-25f, 0f);

      // Arrow
      GameObject arrowObj = new GameObject("Arrow");
      arrowObj.transform.SetParent(dropdownObj.transform, false);

      Text arrow = arrowObj.AddComponent<Text>();
      arrow.text = "▼";
      arrow.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      arrow.fontSize = 16;
      arrow.color = Color.white;
      arrow.alignment = TextAnchor.MiddleCenter;

      RectTransform arrowRect = arrowObj.GetComponent<RectTransform>();
      arrowRect.anchorMin = new Vector2(0.85f, 0f);
      arrowRect.anchorMax = new Vector2(1f, 1f);
      arrowRect.offsetMin = Vector2.zero;
      arrowRect.offsetMax = Vector2.zero;

      // Template
      GameObject templateObj = new GameObject("Template");
      templateObj.transform.SetParent(dropdownObj.transform, false);
      templateObj.SetActive(false);

      Image templateBg = templateObj.AddComponent<Image>();
      templateBg.color = new Color(0.15f, 0.15f, 0.18f);

      RectTransform templateRect = templateObj.GetComponent<RectTransform>();
      templateRect.anchorMin = new Vector2(0f, 0f);
      templateRect.anchorMax = new Vector2(1f, 0f);
      templateRect.pivot = new Vector2(0.5f, 1f);
      templateRect.sizeDelta = new Vector2(0f, 200f);

      ScrollRect scrollRect = templateObj.AddComponent<ScrollRect>();

      // Viewport
      GameObject viewportObj = new GameObject("Viewport");
      viewportObj.transform.SetParent(templateObj.transform, false);

      Image viewportImage = viewportObj.AddComponent<Image>();
      viewportImage.color = Color.white;

      Mask viewportMask = viewportObj.AddComponent<Mask>();
      viewportMask.showMaskGraphic = false;

      RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
      viewportRect.anchorMin = Vector2.zero;
      viewportRect.anchorMax = Vector2.one;
      viewportRect.offsetMin = Vector2.zero;
      viewportRect.offsetMax = Vector2.zero;

      scrollRect.viewport = viewportRect;

      // Content
      GameObject contentObj = new GameObject("Content");
      contentObj.transform.SetParent(viewportObj.transform, false);

      RectTransform contentRect = contentObj.AddComponent<RectTransform>();
      contentRect.anchorMin = new Vector2(0f, 1f);
      contentRect.anchorMax = new Vector2(1f, 1f);
      contentRect.pivot = new Vector2(0.5f, 1f);
      contentRect.sizeDelta = new Vector2(0f, 28f);

      scrollRect.content = contentRect;

      // Item
      GameObject itemObj = new GameObject("Item");
      itemObj.transform.SetParent(contentObj.transform, false);

      Image itemBg = itemObj.AddComponent<Image>();
      itemBg.color = new Color(0.2f, 0.2f, 0.25f);

      Toggle itemToggle = itemObj.AddComponent<Toggle>();

      RectTransform itemRect = itemObj.GetComponent<RectTransform>();
      itemRect.anchorMin = new Vector2(0f, 0.5f);
      itemRect.anchorMax = new Vector2(1f, 0.5f);
      itemRect.sizeDelta = new Vector2(0f, 28f);

      // Item label
      GameObject itemLabelObj = new GameObject("Item Label");
      itemLabelObj.transform.SetParent(itemObj.transform, false);

      Text itemLabel = itemLabelObj.AddComponent<Text>();
      itemLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      itemLabel.fontSize = 18;
      itemLabel.color = Color.white;
      itemLabel.alignment = TextAnchor.MiddleCenter;

      RectTransform itemLabelRect = itemLabelObj.GetComponent<RectTransform>();
      itemLabelRect.anchorMin = Vector2.zero;
      itemLabelRect.anchorMax = Vector2.one;
      itemLabelRect.offsetMin = Vector2.zero;
      itemLabelRect.offsetMax = Vector2.zero;

      // Dropdown component
      Dropdown dropdown = dropdownObj.AddComponent<Dropdown>();
      dropdown.template = templateRect;
      dropdown.captionText = dropdownLabel;
      dropdown.itemText = itemLabel;

      // Populate options
      Resolution[] resolutions = SettingsService.Instance.AvailableResolutions;
      List<Dropdown.OptionData> options = new();

      int currentIndex = 0;
      for (int i = 0; i < resolutions.Length; i++)
      {
        Resolution res = resolutions[i];
        options.Add(new Dropdown.OptionData($"{res.width} x {res.height}"));

        if (res.width == Screen.width && res.height == Screen.height)
          currentIndex = i;
      }

      dropdown.options = options;
      dropdown.value = _tempSettings.ResolutionIndex >= 0 ? _tempSettings.ResolutionIndex : currentIndex;
      dropdown.onValueChanged.AddListener(OnResolutionChanged);

      return dropdown;
    }

    private void CreateSpacer(Transform parent, float height)
    {
      GameObject spacer = new GameObject("Spacer");
      spacer.transform.SetParent(parent, false);

      LayoutElement layout = spacer.AddComponent<LayoutElement>();
      layout.preferredHeight = height;
    }

    private void CreateButtons(Transform parent)
    {
      GameObject buttonsContainer = new GameObject("Buttons");
      buttonsContainer.transform.SetParent(parent, false);

      RectTransform buttonsRect = buttonsContainer.AddComponent<RectTransform>();
      buttonsRect.anchorMin = new Vector2(0f, 0f);
      buttonsRect.anchorMax = new Vector2(1f, 0.2f);
      buttonsRect.offsetMin = new Vector2(30f, 15f);
      buttonsRect.offsetMax = new Vector2(-30f, -5f);

      HorizontalLayoutGroup layout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
      layout.spacing = 15f;
      layout.childAlignment = TextAnchor.MiddleCenter;
      layout.childControlWidth = true;
      layout.childControlHeight = true;
      layout.childForceExpandWidth = true;

      CreateButton(buttonsContainer.transform, "СБРОС", DangerColor, OnResetClicked);
      CreateButton(buttonsContainer.transform, "ЗАКРЫТЬ", ButtonColor, OnCloseClicked);
    }

    private void CreateButton(Transform parent, string label, Color color, Action onClick)
    {
      GameObject buttonObj = new GameObject("Button_" + label);
      buttonObj.transform.SetParent(parent, false);

      Image buttonBg = buttonObj.AddComponent<Image>();
      buttonBg.color = color;

      Button button = buttonObj.AddComponent<Button>();
      button.targetGraphic = buttonBg;

      ColorBlock colors = button.colors;
      colors.normalColor = color;
      colors.highlightedColor = color * 1.2f;
      colors.pressedColor = color * 0.8f;
      button.colors = colors;

      button.onClick.AddListener(() => onClick?.Invoke());

      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(buttonObj.transform, false);

      Text text = textObj.AddComponent<Text>();
      text.text = label;
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = 22;
      text.fontStyle = FontStyle.Bold;
      text.color = Color.white;
      text.alignment = TextAnchor.MiddleCenter;

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.anchorMin = Vector2.zero;
      textRect.anchorMax = Vector2.one;
      textRect.offsetMin = Vector2.zero;
      textRect.offsetMax = Vector2.zero;
    }

    private void UpdateUI()
    {
      if (_musicSlider != null)
        _musicSlider.value = _tempSettings.MusicVolume;

      if (_sfxSlider != null)
        _sfxSlider.value = _tempSettings.SFXVolume;

      if (_fullscreenToggle != null)
        _fullscreenToggle.isOn = _tempSettings.Fullscreen;
    }

    #region Callbacks

    private void OnMusicVolumeChanged(float value)
    {
      _tempSettings.MusicVolume = value;
      SettingsService.Instance.SetMusicVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
      _tempSettings.SFXVolume = value;
      SettingsService.Instance.SetSFXVolume(value);
    }

    private void OnFullscreenChanged(bool value)
    {
      _tempSettings.Fullscreen = value;
    }

    private void OnResolutionChanged(int index)
    {
      _tempSettings.ResolutionIndex = index;
    }

    private void OnResetClicked()
    {
      SettingsService.Instance.ResetToDefaults();
      _tempSettings = SettingsService.Instance.CurrentSettings.Clone();
      UpdateUI();
    }

    private void OnDeleteSaveClicked()
    {
      GameStateService.Instance.Reset();

      // Hide delete save button since there's no save anymore
      if (_deleteSaveButton != null)
        _deleteSaveButton.SetActive(false);
    }

    private void OnCloseClicked()
    {
      // Apply and save settings
      SettingsService.Instance.ApplySettings(_tempSettings);
      SettingsService.Instance.SaveSettings();
      Hide();
    }

    #endregion

    private void OnDestroy()
    {
      if (_panel != null)
        Destroy(_panel);
    }
  }
}
