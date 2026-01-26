namespace FeatureFlagCore.Entities;

/// <summary>
/// Context used when evaluating a feature flag.
/// Contains optional user ID and group IDs to determine the appropriate override.
/// </summary>
public class EvaluationContext
{
    /// <summary>
    /// The user ID for user-specific evaluation. If provided, user overrides take highest precedence.
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// The group IDs the user belongs to. Used for group-specific evaluation when no user override exists.
    /// </summary>
    public IList<string> GroupIds { get; set; } = new List<string>();
}
