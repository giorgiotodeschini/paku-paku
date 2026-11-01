namespace PakuPaku.Domain;

public record LevelDefinition(
    int GhostFrightenedDurationFrames, // Fright duration in tick (60Hz)
    int PlayerSpeed,                   // Player sub-pixel speed (usually 1, but decreases when eating)
    int GhostSpeedNormal,              // Ghost normal speed
    int GhostSpeedFrightened           // The frightened ghosts slow down!
);