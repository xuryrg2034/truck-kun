namespace Code.Gameplay.Features.Pedestrian.Extensions
{
  public static class PedestrianKindExtensions
  {
    public static string GetDisplayName(this PedestrianKind kind)
    {
      return kind switch
      {
        PedestrianKind.StudentNerd => "Student",
        PedestrianKind.Salaryman => "Salaryman",
        PedestrianKind.Grandma => "Grandma",
        PedestrianKind.OldMan => "Old Man",
        PedestrianKind.Teenager => "Teenager",
        _ => kind.ToString()
      };
    }

    public static string GetDisplayNameRu(this PedestrianKind kind)
    {
      return kind switch
      {
        PedestrianKind.StudentNerd => "Школьник",
        PedestrianKind.Salaryman => "Офисник",
        PedestrianKind.Grandma => "Бабушка",
        PedestrianKind.OldMan => "Дед",
        PedestrianKind.Teenager => "Подросток",
        _ => kind.ToString()
      };
    }

    public static bool IsProtectedType(this PedestrianKind kind)
    {
      return kind == PedestrianKind.Grandma || kind == PedestrianKind.OldMan;
    }
  }
}
