using UnityEngine;

namespace Code.Art.VFX
{
  /// <summary>
  /// Automatically creates VFX controllers on game start.
  /// </summary>
  public static class VFXBootstrap
  {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
      // Create VFX Manager object
      GameObject vfxManager = new GameObject("[VFXManager]");
      Object.DontDestroyOnLoad(vfxManager);

      // Add controllers
      vfxManager.AddComponent<HitEffectController>();
      vfxManager.AddComponent<CameraShakeController>();

      // Initialize default prefabs
      var hitController = vfxManager.GetComponent<HitEffectController>();
      hitController.CreateDefaultPrefabs();

      Debug.Log("[VFX] VFX system initialized");
    }
  }
}
