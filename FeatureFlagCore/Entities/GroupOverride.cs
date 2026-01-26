namespace FeatureFlagCore.Entities;

/// <summary>
/// Represents a group-specific override for a feature flag.
/// Group overrides take precedence over global defaults but are overridden by user overrides.
/// </summary>
public class GroupOverride
{
    public int Id { get; set; }
    
    /// <summary>
    /// The ID of the feature flag this override belongs to.
    /// </summary>
    public int FeatureFlagId { get; set; }
    
    /// <summary>
    /// Navigation property to the parent feature flag.
    /// </summary>
    public FeatureFlag FeatureFlag { get; set; } = null!;
    
    /// <summary>
    /// The unique identifier of the group this override applies to.
    /// </summary>
    public string GroupId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the feature is enabled for this specific group.
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// Timestamp when this override was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
