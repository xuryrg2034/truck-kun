using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Code.Infrastructure
{
  /// <summary>
  /// Automatically replaces StandaloneInputModule with InputSystemUIInputModule
  /// to fix compatibility with the new Input System.
  /// </summary>
  public static class InputSystemFixer
  {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeEarly()
    {
      // Subscribe to scene loads for future scenes
      UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeAfterScene()
    {
      // Fix the initial scene immediately after it loads (before first Update)
      FixEventSystems();
    }

    private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
      FixEventSystems();
    }

    public static void FixEventSystems()
    {
      // Find all EventSystems in the scene
      EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

      foreach (EventSystem eventSystem in eventSystems)
      {
        // Check if it has StandaloneInputModule
        StandaloneInputModule standaloneModule = eventSystem.GetComponent<StandaloneInputModule>();

        if (standaloneModule != null)
        {
          // Check if it already has InputSystemUIInputModule
          InputSystemUIInputModule inputSystemModule = eventSystem.GetComponent<InputSystemUIInputModule>();

          if (inputSystemModule == null)
          {
            // Add InputSystemUIInputModule
            inputSystemModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            Debug.Log($"[InputSystemFixer] Added InputSystemUIInputModule to {eventSystem.gameObject.name}");
          }

          // Remove StandaloneInputModule
          Object.DestroyImmediate(standaloneModule);
          Debug.Log($"[InputSystemFixer] Removed StandaloneInputModule from {eventSystem.gameObject.name}");
        }
      }
    }
  }
}
