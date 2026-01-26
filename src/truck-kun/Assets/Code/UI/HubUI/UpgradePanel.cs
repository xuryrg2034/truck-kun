using System.Collections.Generic;
using Code.Gameplay.Features.Economy;
using Code.Meta.Upgrades;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.HubUI
{
  public class UpgradePanel : HubPanelBase
  {
    private IMoneyService _moneyService;
    private IUpgradeService _upgradeService;

    private static readonly Color AccentColor = new Color(0.5f, 0.5f, 0.55f);
    private static readonly Color BuyButtonColor = new Color(0.2f, 0.6f, 0.3f);

    public void Initialize(IMoneyService moneyService, IUpgradeService upgradeService)
    {
      _moneyService = moneyService;
      _upgradeService = upgradeService;
    }

    protected override void CreatePanel()
    {
      _panel = CreatePanelBase("ГАРАЖ", AccentColor, new Vector2(650f, 520f));
      CreateContent();
      CreateCloseButton(_panel.transform);
    }

    protected override void OnShow()
    {
      RefreshPanel();
    }

    private void CreateContent()
    {
      // Content container
      GameObject content = new GameObject("Content");
      content.transform.SetParent(_panel.transform, false);

      RectTransform contentRect = content.AddComponent<RectTransform>();
      contentRect.anchorMin = new Vector2(0f, 0.15f);
      contentRect.anchorMax = new Vector2(1f, 0.88f);
      contentRect.offsetMin = new Vector2(20f, 0f);
      contentRect.offsetMax = new Vector2(-20f, 0f);

      VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
      layout.spacing = 15f;
      layout.padding = new RectOffset(10, 10, 15, 10);
      layout.childAlignment = TextAnchor.UpperCenter;
      layout.childControlWidth = true;
      layout.childControlHeight = false;
      layout.childForceExpandWidth = true;

      // Create upgrade items
      if (_upgradeService != null)
      {
        IReadOnlyList<UpgradeInfo> upgrades = _upgradeService.GetAllUpgrades();
        foreach (UpgradeInfo upgrade in upgrades)
        {
          CreateUpgradeItem(content.transform, upgrade);
        }
      }
    }

    private void CreateUpgradeItem(Transform parent, UpgradeInfo upgrade)
    {
      GameObject item = new GameObject($"Upgrade_{upgrade.Type}");
      item.transform.SetParent(parent, false);

      Image itemBg = item.AddComponent<Image>();
      itemBg.color = new Color(0.15f, 0.15f, 0.2f);

      LayoutElement layoutEl = item.AddComponent<LayoutElement>();
      layoutEl.preferredHeight = 100f;

      // Left side - info
      GameObject infoContainer = new GameObject("Info");
      infoContainer.transform.SetParent(item.transform, false);

      RectTransform infoRect = infoContainer.AddComponent<RectTransform>();
      infoRect.anchorMin = new Vector2(0f, 0f);
      infoRect.anchorMax = new Vector2(0.6f, 1f);
      infoRect.offsetMin = new Vector2(15f, 10f);
      infoRect.offsetMax = new Vector2(0f, -10f);

      VerticalLayoutGroup infoLayout = infoContainer.AddComponent<VerticalLayoutGroup>();
      infoLayout.spacing = 4f;
      infoLayout.childAlignment = TextAnchor.MiddleLeft;
      infoLayout.childControlWidth = true;
      infoLayout.childControlHeight = false;

      // Name
      Text nameText = CreateText(infoContainer.transform, upgrade.Name, 26, Color.white, TextAnchor.MiddleLeft);
      nameText.fontStyle = FontStyle.Bold;
      LayoutElement nameLayout = nameText.gameObject.AddComponent<LayoutElement>();
      nameLayout.preferredHeight = 30f;

      // Level progress
      string levelStr = upgrade.IsMaxed
        ? "Уровень: MAX"
        : $"Уровень: {upgrade.CurrentLevel} / {upgrade.MaxLevel}";

      Text levelText = CreateText(infoContainer.transform, levelStr, 20,
        upgrade.IsMaxed ? new Color(1f, 0.85f, 0.2f) : new Color(0.8f, 0.8f, 0.8f),
        TextAnchor.MiddleLeft);
      LayoutElement levelLayout = levelText.gameObject.AddComponent<LayoutElement>();
      levelLayout.preferredHeight = 24f;

      // Current bonus
      string bonusStr = upgrade.CurrentBonus > 0
        ? $"Текущий бонус: +{upgrade.CurrentBonus * 100:0}%"
        : "Бонус: нет";
      Text bonusText = CreateText(infoContainer.transform, bonusStr, 18,
        new Color(0.5f, 0.8f, 0.5f), TextAnchor.MiddleLeft);
      LayoutElement bonusLayout = bonusText.gameObject.AddComponent<LayoutElement>();
      bonusLayout.preferredHeight = 22f;

      // Description
      Text descText = CreateText(infoContainer.transform, upgrade.Description, 16,
        new Color(0.6f, 0.6f, 0.6f), TextAnchor.MiddleLeft);
      LayoutElement descLayout = descText.gameObject.AddComponent<LayoutElement>();
      descLayout.preferredHeight = 20f;

      // Right side - button or max indicator
      GameObject rightSide = new GameObject("RightSide");
      rightSide.transform.SetParent(item.transform, false);

      RectTransform rightRect = rightSide.AddComponent<RectTransform>();
      rightRect.anchorMin = new Vector2(0.62f, 0.15f);
      rightRect.anchorMax = new Vector2(0.97f, 0.85f);
      rightRect.offsetMin = Vector2.zero;
      rightRect.offsetMax = Vector2.zero;

      if (upgrade.IsMaxed)
      {
        // Max level indicator
        Image maxBg = rightSide.AddComponent<Image>();
        maxBg.color = new Color(0.3f, 0.25f, 0.1f);

        Text maxText = CreateText(rightSide.transform, "МАКСИМУМ", 22,
          new Color(1f, 0.85f, 0.2f));
        maxText.fontStyle = FontStyle.Bold;

        RectTransform maxTextRect = maxText.GetComponent<RectTransform>();
        maxTextRect.anchorMin = Vector2.zero;
        maxTextRect.anchorMax = Vector2.one;
        maxTextRect.offsetMin = Vector2.zero;
        maxTextRect.offsetMax = Vector2.zero;
      }
      else
      {
        // Buy button
        bool canAfford = _moneyService != null && _moneyService.Balance >= upgrade.NextCost;

        Image buttonBg = rightSide.AddComponent<Image>();
        buttonBg.color = canAfford ? BuyButtonColor : new Color(0.35f, 0.35f, 0.35f);

        Button button = rightSide.AddComponent<Button>();
        button.targetGraphic = buttonBg;
        button.interactable = canAfford;

        UpgradeType type = upgrade.Type;
        button.onClick.AddListener(() => OnBuyUpgrade(type));

        // Button content
        VerticalLayoutGroup buttonLayout = rightSide.AddComponent<VerticalLayoutGroup>();
        buttonLayout.childAlignment = TextAnchor.MiddleCenter;
        buttonLayout.childControlWidth = true;
        buttonLayout.childControlHeight = false;

        Text priceText = CreateText(rightSide.transform, $"{upgrade.NextCost}¥", 28, Color.white);
        priceText.fontStyle = FontStyle.Bold;
        LayoutElement priceLayout = priceText.gameObject.AddComponent<LayoutElement>();
        priceLayout.preferredHeight = 32f;

        string nextBonusStr = $"+{upgrade.NextBonus * 100:0}%";
        Text nextText = CreateText(rightSide.transform, nextBonusStr, 18,
          canAfford ? new Color(0.7f, 1f, 0.7f) : new Color(0.5f, 0.5f, 0.5f));
        LayoutElement nextLayout = nextText.gameObject.AddComponent<LayoutElement>();
        nextLayout.preferredHeight = 22f;
      }
    }

    private void OnBuyUpgrade(UpgradeType type)
    {
      if (_upgradeService == null)
        return;

      if (_upgradeService.PurchaseUpgrade(type))
      {
        RefreshPanel();
      }
    }

    private void RefreshPanel()
    {
      if (_panel == null)
        return;

      // Destroy and recreate content
      Transform content = _panel.transform.Find("Content");
      if (content != null)
        Destroy(content.gameObject);

      CreateContent();
    }
  }
}
