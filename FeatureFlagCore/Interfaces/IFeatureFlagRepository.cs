using FeatureFlagCore.Entities;

namespace FeatureFlagCore.Interfaces;

/// <summary>
/// Repository interface for feature flag data access operations.
/// </summary>
public interface IFeatureFlagRepository
{
    // Feature flag operations
    Task<FeatureFlag?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<FeatureFlag?> GetByKeyWithOverridesAsync(string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeatureFlag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<FeatureFlag> CreateAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default);
    Task<FeatureFlag> UpdateAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
    
    // User override operations
    Task<UserOverride?> GetUserOverrideAsync(string key, string userId, CancellationToken cancellationToken = default);
    Task<UserOverride> AddUserOverrideAsync(string key, string userId, bool isEnabled, CancellationToken cancellationToken = default);
    Task<UserOverride> UpdateUserOverrideAsync(string key, string userId, bool isEnabled, CancellationToken cancellationToken = default);
    Task RemoveUserOverrideAsync(string key, string userId, CancellationToken cancellationToken = default);
    
    // Group override operations
    Task<GroupOverride?> GetGroupOverrideAsync(string key, string groupId, CancellationToken cancellationToken = default);
    Task<GroupOverride> AddGroupOverrideAsync(string key, string groupId, bool isEnabled, CancellationToken cancellationToken = default);
    Task<GroupOverride> UpdateGroupOverrideAsync(string key, string groupId, bool isEnabled, CancellationToken cancellationToken = default);
    Task RemoveGroupOverrideAsync(string key, string groupId, CancellationToken cancellationToken = default);
}
