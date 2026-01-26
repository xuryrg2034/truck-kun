using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Code.UI.MainMenu
{
  /// <summary>
  /// Bootstrap component for MainMenu scene.
  /// Creates all necessary scene objects at runtime.
  /// </summary>
  public class MainMenuBootstrap : MonoBehaviour
  {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
      // Only initialize if we're in the MainMenu scene
      if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenuScene")
        return;

      // Check if bootstrap already exists
      if (FindFirstObjectByType<MainMenuBootstrap>() != null)
        return;

      // Create bootstrap
      GameObject bootstrapObj = new GameObject("[MainMenuBootstrap]");
      bootstrapObj.AddComponent<MainMenuBootstrap>();
    }

    private void Awake()
    {
      SetupScene();
    }

    private void SetupScene()
    {
      // Create camera if none exists
      if (Camera.main == null)
      {
        CreateCamera();
      }

      // Create EventSystem if none exists
      if (FindFirstObjectByType<EventSystem>() == null)
      {
        CreateEventSystem();
      }

      // Create MainMenuUI
      CreateMainMenu();

      Debug.Log("[MainMenuBootstrap] Scene initialized");
    }

    private void CreateCamera()
    {
      GameObject cameraObj = new GameObject("Main Camera");
      cameraObj.tag = "MainCamera";

      Camera camera = cameraObj.AddComponent<Camera>();
      camera.clearFlags = CameraClearFlags.SolidColor;
      camera.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
      camera.orthographic = false;
      camera.fieldOfView = 60f;
      camera.nearClipPlane = 0.1f;
      camera.farClipPlane = 100f;

      cameraObj.AddComponent<AudioListener>();

      cameraObj.transform.position = new Vector3(0f, 0f, -10f);
    }

    private void CreateEventSystem()
    {
      GameObject eventSystemObj = new GameObject("EventSystem");

      eventSystemObj.AddComponent<EventSystem>();
      eventSystemObj.AddComponent<InputSystemUIInputModule>();
    }

    private void CreateMainMenu()
    {
      GameObject menuObj = new GameObject("MainMenu");
      menuObj.AddComponent<MainMenuUI>();
    }
  }
}
