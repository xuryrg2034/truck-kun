using Code.Gameplay.Features.Physics;
using UnityEngine;

namespace Code.Gameplay.Features.Surface.Factory
{
  /// <summary>
  /// Factory for creating surface hazard GameObjects
  /// </summary>
  public static class SurfaceFactory
  {
    public static GameObject CreateSurface(SurfaceType type, Vector3 position, float length, float width)
    {
      GameObject surface = new GameObject($"Surface_{type}");
      surface.transform.position = position;

      // Create visual mesh
      GameObject visual = CreateVisualMesh(type, length, width);
      visual.transform.SetParent(surface.transform, false);

      // Add trigger collider
      BoxCollider trigger = surface.AddComponent<BoxCollider>();
      trigger.isTrigger = true;
      trigger.size = new Vector3(width, 0.5f, length);
      trigger.center = new Vector3(0f, 0.25f, length * 0.5f);

      // Add surface trigger component
      SurfaceTrigger surfaceTrigger = surface.AddComponent<SurfaceTrigger>();
      surfaceTrigger.Setup(type);

      // Add particle effects
      AddParticleEffects(surface, type);

      return surface;
    }

    private static GameObject CreateVisualMesh(SurfaceType type, float length, float width)
    {
      GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
      visual.name = "Visual";

      // Remove collider from visual (we use parent's trigger)
      Object.Destroy(visual.GetComponent<Collider>());

      // Scale and rotate to lay flat
      visual.transform.localScale = new Vector3(width, length, 1f);
      visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
      visual.transform.localPosition = new Vector3(0f, 0.01f, length * 0.5f);

      // Set material/color based on type
      Renderer renderer = visual.GetComponent<Renderer>();
      if (renderer != null)
      {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = GetSurfaceColor(type);
        SetupTransparentMaterial(mat);
        renderer.material = mat;
      }

      return visual;
    }

    private static Color GetSurfaceColor(SurfaceType type)
    {
      return type switch
      {
        SurfaceType.Oil => new Color(0.1f, 0.1f, 0.15f, 0.8f),
        SurfaceType.Grass => new Color(0.2f, 0.6f, 0.2f, 0.9f),
        SurfaceType.Ice => new Color(0.8f, 0.9f, 1f, 0.6f),
        SurfaceType.Puddle => new Color(0.3f, 0.4f, 0.6f, 0.7f),
        _ => new Color(0.5f, 0.5f, 0.5f, 0.5f)
      };
    }

    private static void SetupTransparentMaterial(Material mat)
    {
      mat.SetFloat("_Surface", 1);
      mat.SetFloat("_Blend", 0);
      mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
      mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
      mat.SetInt("_ZWrite", 0);
      mat.DisableKeyword("_ALPHATEST_ON");
      mat.EnableKeyword("_ALPHABLEND_ON");
      mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
      mat.renderQueue = 3000;
    }

    private static void AddParticleEffects(GameObject surface, SurfaceType type)
    {
      GameObject particleObj = new GameObject("Particles");
      particleObj.transform.SetParent(surface.transform, false);

      ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
      var main = ps.main;
      main.playOnAwake = false;
      main.loop = false;
      main.duration = 0.5f;
      main.startLifetime = 0.8f;
      main.startSpeed = 2f;
      main.startSize = 0.2f;
      main.maxParticles = 20;

      var emission = ps.emission;
      emission.rateOverTime = 0;
      emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });

      var shape = ps.shape;
      shape.shapeType = ParticleSystemShapeType.Box;
      shape.scale = new Vector3(1f, 0.1f, 1f);

      var colorOverLifetime = ps.colorOverLifetime;
      colorOverLifetime.enabled = true;

      Color startColor = type switch
      {
        SurfaceType.Oil => Color.black,
        SurfaceType.Grass => Color.green,
        SurfaceType.Ice => Color.cyan,
        SurfaceType.Puddle => Color.blue,
        _ => Color.gray
      };

      Gradient gradient = new Gradient();
      gradient.SetKeys(
        new[] { new GradientColorKey(startColor, 0f), new GradientColorKey(startColor, 1f) },
        new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
      );
      colorOverLifetime.color = gradient;
    }
  }
}
