namespace FeatureFlagCore.Entities;

/// <summary>
/// Represents a user-specific override for a feature flag.
/// User overrides take precedence over group overrides and global defaults.
/// </summary>
public class UserOverride
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
    /// The unique identifier of the user this override applies to.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the feature is enabled for this specific user.
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// Timestamp when this override was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
