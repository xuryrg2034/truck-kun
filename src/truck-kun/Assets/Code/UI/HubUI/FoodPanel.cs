using Code.Gameplay.Features.Economy;
using Code.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.HubUI
{
  public class FoodPanel : HubPanelBase
  {
    private const int FoodCost = 100;

    private IMoneyService _moneyService;
    private Button _buyButton;
    private Text _statusText;

    private static readonly Color AccentColor = new Color(0.8f, 0.5f, 0.2f);
    private static readonly Color BuyButtonColor = new Color(0.2f, 0.6f, 0.3f);

    public void Initialize(IMoneyService moneyService)
    {
      _moneyService = moneyService;
    }

    protected override void CreatePanel()
    {
      _panel = CreatePanelBase("СТОЛОВАЯ", AccentColor, new Vector2(500f, 400f));
      CreateContent();
      CreateCloseButton(_panel.transform);
    }

    protected override void OnShow()
    {
      UpdateButtonState();
    }

    private void CreateContent()
    {
      // Content container
      GameObject content = new GameObject("Content");
      content.transform.SetParent(_panel.transform, false);

      RectTransform contentRect = content.AddComponent<RectTransform>();
      contentRect.anchorMin = new Vector2(0f, 0.2f);
      contentRect.anchorMax = new Vector2(1f, 0.88f);
      contentRect.offsetMin = new Vector2(30f, 0f);
      contentRect.offsetMax = new Vector2(-30f, 0f);

      VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
      layout.spacing = 20f;
      layout.padding = new RectOffset(10, 10, 20, 10);
      layout.childAlignment = TextAnchor.MiddleCenter;
      layout.childControlWidth = true;
      layout.childControlHeight = false;
      layout.childForceExpandWidth = true;

      // Icon placeholder
      GameObject iconObj = new GameObject("Icon");
      iconObj.transform.SetParent(content.transform, false);

      Image iconBg = iconObj.AddComponent<Image>();
      iconBg.color = new Color(0.9f, 0.7f, 0.4f);

      LayoutElement iconLayout = iconObj.AddComponent<LayoutElement>();
      iconLayout.preferredHeight = 80f;
      iconLayout.preferredWidth = 80f;

      Text iconText = CreateText(iconObj.transform, "🍱", 48, Color.white);
      RectTransform iconTextRect = iconText.GetComponent<RectTransform>();
      iconTextRect.anchorMin = Vector2.zero;
      iconTextRect.anchorMax = Vector2.one;
      iconTextRect.offsetMin = Vector2.zero;
      iconTextRect.offsetMax = Vector2.zero;

      // Description
      Text descText = CreateText(content.transform,
        "Перед выходом на работу нужно подкрепиться!\nЕда даёт силы на весь день.",
        20, new Color(0.8f, 0.8f, 0.8f));
      LayoutElement descLayout = descText.gameObject.AddComponent<LayoutElement>();
      descLayout.preferredHeight = 60f;

      // Price info
      GameObject priceContainer = new GameObject("PriceContainer");
      priceContainer.transform.SetParent(content.transform, false);

      Image priceBg = priceContainer.AddComponent<Image>();
      priceBg.color = new Color(0.15f, 0.15f, 0.2f);

      LayoutElement priceLayout = priceContainer.AddComponent<LayoutElement>();
      priceLayout.preferredHeight = 50f;

      Text priceText = CreateText(priceContainer.transform, $"Стоимость: {FoodCost}¥", 26,
        new Color(1f, 0.85f, 0.2f));
      priceText.fontStyle = FontStyle.Bold;

      RectTransform priceTextRect = priceText.GetComponent<RectTransform>();
      priceTextRect.anchorMin = Vector2.zero;
      priceTextRect.anchorMax = Vector2.one;
      priceTextRect.offsetMin = Vector2.zero;
      priceTextRect.offsetMax = Vector2.zero;

      // Status text
      GameObject statusObj = new GameObject("Status");
      statusObj.transform.SetParent(content.transform, false);

      _statusText = CreateText(statusObj.transform, "", 18, new Color(0.9f, 0.3f, 0.3f));
      LayoutElement statusLayout = statusObj.AddComponent<LayoutElement>();
      statusLayout.preferredHeight = 25f;

      RectTransform statusTextRect = _statusText.GetComponent<RectTransform>();
      statusTextRect.anchorMin = Vector2.zero;
      statusTextRect.anchorMax = Vector2.one;
      statusTextRect.offsetMin = Vector2.zero;
      statusTextRect.offsetMax = Vector2.zero;

      // Buy button
      GameObject buttonContainer = new GameObject("ButtonContainer");
      buttonContainer.transform.SetParent(content.transform, false);

      LayoutElement buttonContainerLayout = buttonContainer.AddComponent<LayoutElement>();
      buttonContainerLayout.preferredHeight = 60f;

      _buyButton = CreateButton(buttonContainer.transform, "КУПИТЬ И НАЧАТЬ ДЕНЬ", BuyButtonColor, OnBuyAndStart);

      RectTransform buttonRect = _buyButton.GetComponent<RectTransform>();
      buttonRect.anchorMin = new Vector2(0.1f, 0f);
      buttonRect.anchorMax = new Vector2(0.9f, 1f);
      buttonRect.offsetMin = Vector2.zero;
      buttonRect.offsetMax = Vector2.zero;
    }

    private void UpdateButtonState()
    {
      if (_buyButton == null)
        return;

      int currentMoney = GameStateService.Instance.PlayerMoney;
      bool canAfford = currentMoney >= FoodCost;
      _buyButton.interactable = canAfford;

      Image buttonImage = _buyButton.GetComponent<Image>();
      if (buttonImage != null)
      {
        buttonImage.color = canAfford ? BuyButtonColor : new Color(0.35f, 0.35f, 0.35f);
      }

      if (_statusText != null)
      {
        if (!canAfford)
        {
          int needed = FoodCost - currentMoney;
          _statusText.text = $"Не хватает {needed}¥";
          _statusText.color = new Color(0.9f, 0.3f, 0.3f);
        }
        else
        {
          _statusText.text = "Готов к выходу!";
          _statusText.color = new Color(0.3f, 0.9f, 0.3f);
        }
      }
    }

    private void OnBuyAndStart()
    {
      // Use GameStateService for persistent state
      GameStateService state = GameStateService.Instance;

      if (!state.SpendMoney(FoodCost))
        return;

      // Also update local money service for UI
      _moneyService?.SpendMoney(FoodCost);

      // Increment day number
      state.IncrementDay();

      // Use SceneTransitionService for smooth transition
      SceneTransitionService.Instance.LoadGameplay();
    }
  }
}
