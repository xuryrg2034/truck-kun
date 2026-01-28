using Code.Infrastructure.View;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Common.Components
{
  [Game, Meta] public class Id : IComponent { [PrimaryEntityIndex] public int Value; }
  [Game] public class WorldPosition : IComponent { public Vector3 Value; }
  [Game] public class TransformComponent : IComponent { public Transform Value; }
  [Game] public class View : IComponent { public IEntityView Value; }
  [Game] public class ViewPrefab : IComponent { public EntityBehaviour Value; }
}
