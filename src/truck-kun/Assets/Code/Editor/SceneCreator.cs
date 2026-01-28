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

    [MenuItem("Truck-kun/Create Hub Scene")]
    public static void CreateHubScene()
    {
      string scenePath = "Assets/Scenes/HubScene.unity";

      // Check if scene already exists
      if (System.IO.File.Exists(scenePath))
      {
        if (!EditorUtility.DisplayDialog(
          "Scene Exists",
          "HubScene.unity already exists. Do you want to overwrite it?",
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
      camera.backgroundColor = new Color(0.08f, 0.06f, 0.12f);
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

      // Create Hub holder
      GameObject hubObj = new GameObject("Hub");

      // Try to add HubUI component
      System.Type hubUIType = System.Type.GetType("Code.UI.Hub.HubUI, Assembly-CSharp");
      if (hubUIType != null)
      {
        hubObj.AddComponent(hubUIType);
      }
      else
      {
        Debug.LogWarning("[SceneCreator] HubUI type not found. Make sure to create it first.");
      }

      // Save the scene
      EditorSceneManager.SaveScene(newScene, scenePath);

      // Add to build settings (after MainMenu, before GameScene)
      AddSceneToBuildSettingsAtIndex(scenePath, 1);

      Debug.Log($"[SceneCreator] HubScene created at {scenePath}");
      EditorUtility.DisplayDialog("Success", "HubScene.unity created successfully!\n\nThe scene has been added to Build Settings.", "OK");
    }

    private static void AddSceneToBuildSettingsAtIndex(string scenePath, int preferredIndex)
    {
      var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

      // Check if scene already exists in build settings
      for (int i = 0; i < scenes.Count; i++)
      {
        if (scenes[i].path == scenePath)
        {
          return; // Already exists
        }
      }

      // Insert at preferred index or at the end
      int index = Mathf.Min(preferredIndex, scenes.Count);
      scenes.Insert(index, new EditorBuildSettingsScene(scenePath, true));
      EditorBuildSettings.scenes = scenes.ToArray();
      Debug.Log($"[SceneCreator] Added {scenePath} to Build Settings at index {index}");
    }

    [MenuItem("Truck-kun/Create Game Scene")]
    public static void CreateGameScene()
    {
      string scenePath = "Assets/Scenes/GameScene.unity";

      // Check if scene already exists
      if (System.IO.File.Exists(scenePath))
      {
        if (!EditorUtility.DisplayDialog(
          "Scene Exists",
          "GameScene.unity already exists. Do you want to overwrite it?",
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
      camera.backgroundColor = new Color(0.2f, 0.2f, 0.25f);
      camera.orthographic = true;
      camera.orthographicSize = 10f;
      camera.nearClipPlane = 0.1f;
      camera.farClipPlane = 100f;
      cameraObj.AddComponent<AudioListener>();
      cameraObj.transform.position = new Vector3(0f, 0f, -10f);

      // Create EventSystem
      GameObject eventSystemObj = new GameObject("EventSystem");
      eventSystemObj.AddComponent<EventSystem>();
      eventSystemObj.AddComponent<InputSystemUIInputModule>();

      // Create EcsBootstrap holder
      GameObject bootstrapObj = new GameObject("[EcsBootstrap]");

      // Try to add EcsBootstrap component
      System.Type ecsBootstrapType = System.Type.GetType("Code.Infrastructure.Bootstrap.EcsBootstrap, Assembly-CSharp");
      if (ecsBootstrapType != null)
      {
        bootstrapObj.AddComponent(ecsBootstrapType);
      }
      else
      {
        Debug.LogWarning("[SceneCreator] EcsBootstrap type not found. Make sure to add it manually.");
      }

      // Create Directional Light
      GameObject lightObj = new GameObject("Directional Light");
      Light light = lightObj.AddComponent<Light>();
      light.type = LightType.Directional;
      light.color = new Color(1f, 0.95f, 0.85f);
      light.intensity = 1f;
      lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

      // Save the scene
      EditorSceneManager.SaveScene(newScene, scenePath);

      // Add to build settings
      AddSceneToBuildSettingsAtIndex(scenePath, 2);

      Debug.Log($"[SceneCreator] GameScene created at {scenePath}");
      EditorUtility.DisplayDialog("Success", "GameScene.unity created successfully!\n\nThe scene has been added to Build Settings.", "OK");
    }

    [MenuItem("Truck-kun/Create All Scenes")]
    public static void CreateAllScenes()
    {
      CreateMainMenuScene();
      CreateHubScene();
      CreateGameScene();
      EditorUtility.DisplayDialog("Success", "All scenes created!\n\nBuild Settings order:\n0. MainMenuScene\n1. HubScene\n2. GameScene", "OK");
    }
  }
}
