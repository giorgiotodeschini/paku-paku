namespace PakuPaku.Domain;

public static class LevelProgression
{
    private static readonly Dictionary<int, LevelDefinition> Levels = new()
    {
        { 1, new LevelDefinition(360, 1, 1, 1) }, // Level 1: 6s of fright (360 frames)
        { 2, new LevelDefinition(300, 1, 1, 1) }, 
        { 5, new LevelDefinition(180, 1, 1, 1) }, // Level 5: The ghosts speed up in other versions, or the scare factor drops.
        { 21, new LevelDefinition(0, 1, 1, 1) }  // Level 21+: no visible fright
    };

    public static LevelDefinition GetConfigForLevel(int level)
    {
        // If the level is extremely high, it returns the last available configuration.
        int maxDefinedLevel = Levels.Keys.Where(k => k <= level).Max();
        return Levels[maxDefinedLevel];
    }
}