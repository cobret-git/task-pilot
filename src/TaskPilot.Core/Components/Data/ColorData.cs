namespace TaskPilot.Core.Components.Data
{
    /// <summary>
    /// Represents a named color with its hex value for UI display.
    /// </summary>
    /// <param name="Name">The display name of the color.</param>
    /// <param name="HexValue">The hex color value without # prefix (e.g., "FF5733").</param>
    public record class ColorData(string Name, string HexValue);
}
