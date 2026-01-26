using FeatureFlagCore.Entities;

namespace FeatureFlagCore.Interfaces;

/// <summary>
/// Service interface for feature flag business logic operations.
/// </summary>
public interface IFeatureFlagService
{
    // Feature flag CRUD operations
    Task<FeatureFlag> CreateFeatureFlagAsync(string key, bool isEnabled, string? description = null, CancellationToken cancellationToken = default);
    Task<FeatureFlag> GetFeatureFlagAsync(string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeatureFlag>> GetAllFeatureFlagsAsync(CancellationToken cancellationToken = default);
    Task<FeatureFlag> UpdateFeatureFlagAsync(string key, bool isEnabled, string? description = null, CancellationToken cancellationToken = default);
    Task DeleteFeatureFlagAsync(string key, CancellationToken cancellationToken = default);
    
    // Runtime evaluation
    /// <summary>
    /// Evaluates whether a feature is enabled for the given context.
    /// Precedence: User override > Group override > Global default
    /// </summary>
    Task<bool> EvaluateAsync(string key, EvaluationContext? context = null, CancellationToken cancellationToken = default);
    
    // User override operations
    Task<UserOverride> AddUserOverrideAsync(string key, string userId, bool isEnabled, CancellationToken cancellationToken = default);
    Task<UserOverride> UpdateUserOverrideAsync(string key, string userId, bool isEnabled, CancellationToken cancellationToken = default);
    Task RemoveUserOverrideAsync(string key, string userId, CancellationToken cancellationToken = default);
    
    // Group override operations
    Task<GroupOverride> AddGroupOverrideAsync(string key, string groupId, bool isEnabled, CancellationToken cancellationToken = default);
    Task<GroupOverride> UpdateGroupOverrideAsync(string key, string groupId, bool isEnabled, CancellationToken cancellationToken = default);
    Task RemoveGroupOverrideAsync(string key, string groupId, CancellationToken cancellationToken = default);
}
