namespace Code.Common.Entity
{
  public static class CreateEntity
  {
    public static GameEntity Empty() => Contexts.sharedInstance.game.CreateEntity();
  }

  public static class CreateInputEntity
  {
    public static InputEntity Empty() => Contexts.sharedInstance.input.CreateEntity();
  }

  public static class CreateMetaEntity
  {
    public static MetaEntity Empty() => Contexts.sharedInstance.meta.CreateEntity();
  }
}
