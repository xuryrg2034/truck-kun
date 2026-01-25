using System;
using Code.UI.HubUI;
using UnityEngine;

namespace Code.Hub
{
  public enum ZoneType
  {
    Food,
    Quests,
    Garage,
    StartDay
  }

  public class InteractableZone : MonoBehaviour
  {
    [SerializeField] private ZoneType _zoneType;
    [SerializeField] private string _zoneName = "Zone";
    [SerializeField] private KeyCode _interactKey = KeyCode.E;

    private bool _playerInZone;
    private Action<ZoneType> _onInteract;

    public ZoneType Type => _zoneType;
    public string ZoneName => _zoneName;

    public void Initialize(Action<ZoneType> onInteract)
    {
      _onInteract = onInteract;
    }

    private void Update()
    {
      if (_playerInZone && Input.GetKeyDown(_interactKey))
      {
        _onInteract?.Invoke(_zoneType);
      }
    }

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player"))
      {
        _playerInZone = true;
        HubMainUI.Instance?.ShowInteractPrompt($"[{_interactKey}] {_zoneName}");
      }
    }

    private void OnTriggerExit(Collider other)
    {
      if (other.CompareTag("Player"))
      {
        _playerInZone = false;
        HubMainUI.Instance?.HideInteractPrompt();
      }
    }

    public static InteractableZone Create(Transform parent, ZoneType type, string name, Vector3 position, Vector3 size, Color color)
    {
      GameObject zoneObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
      zoneObj.name = $"Zone_{type}";
      zoneObj.transform.SetParent(parent, false);
      zoneObj.transform.position = position;
      zoneObj.transform.localScale = size;

      // Visual
      Renderer renderer = zoneObj.GetComponent<Renderer>();
      Material mat = new Material(Shader.Find("Standard"));
      mat.color = color;
      renderer.material = mat;

      // Trigger collider
      BoxCollider collider = zoneObj.GetComponent<BoxCollider>();
      collider.isTrigger = true;

      // Add larger trigger area
      GameObject triggerArea = new GameObject("TriggerArea");
      triggerArea.transform.SetParent(zoneObj.transform, false);
      triggerArea.transform.localPosition = Vector3.zero;

      BoxCollider triggerCollider = triggerArea.AddComponent<BoxCollider>();
      triggerCollider.isTrigger = true;
      triggerCollider.size = new Vector3(2f, 2f, 2f);

      // Zone component
      InteractableZone zone = zoneObj.AddComponent<InteractableZone>();
      zone._zoneType = type;
      zone._zoneName = name;

      return zone;
    }
  }
}
