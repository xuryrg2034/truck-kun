using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Code.Editor
{
  public static class SceneCreator
  {
    [MenuItem("Truck-kun/Create Main Menu Scene")]
    public static void CreateMainMenuScene()
    {
      string scenePath = "Assets/Scenes/MainMenuScene.unity";

      // Check if scene already exists
      if (System.IO.File.Exists(scenePath))
      {
        if (!EditorUtility.DisplayDialog(
          "Scene Exists",
          "MainMenuScene.unity already exists. Do you want to overwrite it?",
          "Yes", "No"))
        {
          return;
        }
      }

      // Create new scene
      Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

      // Create Main Camera
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

      // Create EventSystem
      GameObject eventSystemObj = new GameObject("EventSystem");
      eventSystemObj.AddComponent<EventSystem>();
      eventSystemObj.AddComponent<InputSystemUIInputModule>();

      // Create MainMenu holder (MainMenuUI will be added by Bootstrap at runtime)
      // Or we can add it directly here
      GameObject mainMenuObj = new GameObject("MainMenu");

      // Try to add MainMenuUI component
      System.Type mainMenuUIType = System.Type.GetType("Code.UI.MainMenu.MainMenuUI, Assembly-CSharp");
      if (mainMenuUIType != null)
      {
        mainMenuObj.AddComponent(mainMenuUIType);
      }
      else
      {
        Debug.LogWarning("[SceneCreator] MainMenuUI type not found. Make sure to add it manually or let Bootstrap handle it.");
      }

      // Save the scene
      EditorSceneManager.SaveScene(newScene, scenePath);

      // Add to build settings
      AddSceneToBuildSettings(scenePath);

      Debug.Log($"[SceneCreator] MainMenuScene created at {scenePath}");
      EditorUtility.DisplayDialog("Success", "MainMenuScene.unity created successfully!\n\nThe scene has been added to Build Settings.", "OK");
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
      var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

      // Check if scene already exists in build settings
      bool exists = false;
      foreach (var scene in scenes)
      {
        if (scene.path == scenePath)
        {
          exists = true;
          break;
        }
      }

      if (!exists)
      {
        // Add as first scene (main menu should be index 0)
        scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"[SceneCreator] Added {scenePath} to Build Settings at index 0");
      }
    }

    [MenuItem("Truck-kun/Open Main Menu Scene")]
    public static void OpenMainMenuScene()
    {
      string scenePath = "Assets/Scenes/MainMenuScene.unity";

      if (System.IO.File.Exists(scenePath))
      {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
          EditorSceneManager.OpenScene(scenePath);
        }
      }
      else
      {
        EditorUtility.DisplayDialog("Scene Not Found", "MainMenuScene.unity does not exist. Use 'Truck-kun/Create Main Menu Scene' to create it.", "OK");
      }
    }
  }
}
