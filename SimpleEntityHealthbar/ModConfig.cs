using Vintagestory.API.MathTools;

namespace SimpleEntityHealthbar;

class ModConfig
{
    public static ModConfig Instance { get; set; } = new ModConfig();

    /// <summary>
    /// Show hud healthbar (top center screen).
    /// </summary>
    public bool ShowHudHealthBar { get; set; } = true;
    
    /// <summary>
    /// Show Nameplates healthbar.
    /// </summary>
    public bool ShowNameplates { get; set; } = false;
    
    /// <summary>
    /// Max distance to pick targets
    /// </summary>
    public int MaxTargetDistance { get; set; } = 15;
}