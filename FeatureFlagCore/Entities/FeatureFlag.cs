namespace FeatureFlagCore.Entities;

/// <summary>
/// Represents a feature flag with a unique key, global default state, and optional description.
/// </summary>
public class FeatureFlag
{
    public int Id { get; set; }
    
    /// <summary>
    /// Unique identifier/key for the feature flag.
    /// </summary>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable description of the feature flag.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Global default state - applies when no user or group override exists.
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// Timestamp when the feature flag was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the feature flag was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User-specific overrides for this feature flag.
    /// </summary>
    public ICollection<UserOverride> UserOverrides { get; set; } = new List<UserOverride>();
    
    /// <summary>
    /// Group-specific overrides for this feature flag.
    /// </summary>
    public ICollection<GroupOverride> GroupOverrides { get; set; } = new List<GroupOverride>();
}
