namespace Code.Gameplay.Features.Pedestrian
{
  /// <summary>
  /// Types of pedestrians in the game
  /// </summary>
  public enum PedestrianKind
  {
    StudentNerd,    // Schoolboy with backpack, bent forward
    Salaryman,      // Office worker, gray suit
    Grandma,        // Old lady, slow, pink
    OldMan,         // Old man, slow, brown
    Teenager        // Young person, colorful
  }

  /// <summary>
  /// Category affects gameplay (protected = penalty for hitting)
  /// </summary>
  public enum PedestrianCategory
  {
    Normal,     // Can be hit for quests
    Protected   // Hitting causes penalty (elderly)
  }
}
