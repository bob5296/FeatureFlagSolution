using FeatureFlagCore.Entities;
using FeatureFlagCore.Exceptions;
using FeatureFlagCore.Interfaces;

namespace FeatureFlagCore.Services;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureFlagRepository _repository;
    
    public FeatureFlagService(IFeatureFlagRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<FeatureFlag> CreateFeatureFlagAsync(string key, bool isEnabled, string? description = null, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        
        var featureFlag = new FeatureFlag
        {
            Key = key.Trim(),
            IsEnabled = isEnabled,
            Description = description?.Trim()
        };
        
        return await _repository.CreateAsync(featureFlag, cancellationToken);
    }
    
    public async Task<FeatureFlag> GetFeatureFlagAsync(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        
        var featureFlag = await _repository.GetByKeyWithOverridesAsync(key, cancellationToken)
            ?? throw new FeatureFlagNotFoundException(key);
        
        return featureFlag;
    }
    
    public async Task<IReadOnlyList<FeatureFlag>> GetAllFeatureFlagsAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
    
    public async Task<FeatureFlag> UpdateFeatureFlagAsync(string key, bool isEnabled, string? description = null, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        
        var featureFlag = await _repository.GetByKeyAsync(key, cancellationToken)
            ?? throw new FeatureFlagNotFoundException(key);
        
        featureFlag.IsEnabled = isEnabled;
        
        // Only update description if explicitly provided (null means keep existing)
        if (description != null)
        {
            featureFlag.Description = description.Trim();
        }
        
        return await _repository.UpdateAsync(featureFlag, cancellationToken);
    }
    
    public async Task DeleteFeatureFlagAsync(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        await _repository.DeleteAsync(key, cancellationToken);
    }
    
    /// <summary>
    /// Evaluates whether a feature is enabled based on the evaluation context.
    /// Precedence order:
    /// 1. User-specific override (if user ID is provided)
    /// 2. Group-specific override (first matching group if user belongs to multiple groups)
    /// 3. Global default state
    /// </summary>
    public async Task<bool> EvaluateAsync(string key, EvaluationContext? context = null, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        
        var featureFlag = await _repository.GetByKeyWithOverridesAsync(key, cancellationToken)
            ?? throw new FeatureFlagNotFoundException(key);
        
        // If no context provided, return global default
        if (context == null)
        {
            return featureFlag.IsEnabled;
        }
        
        // Check user-specific override first (highest precedence)
        if (!string.IsNullOrWhiteSpace(context.UserId))
        {
            var userOverride = featureFlag.UserOverrides
                .FirstOrDefault(u => u.UserId == context.UserId);
            
            if (userOverride != null)
            {
                return userOverride.IsEnabled;
            }
        }
        
        // Check group-specific overrides (second precedence)
        if (context.GroupIds.Count > 0)
        {
            // Find the first matching group override
            var groupOverride = featureFlag.GroupOverrides
                .FirstOrDefault(g => context.GroupIds.Contains(g.GroupId));
            
            if (groupOverride != null)
            {
                return groupOverride.IsEnabled;
            }
        }
        
        // Fall back to global default
        return featureFlag.IsEnabled;
    }
    
    public async Task<UserOverride> AddUserOverrideAsync(string key, string userId, bool isEnabled, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ValidateUserId(userId);
        
        return await _repository.AddUserOverrideAsync(key, userId.Trim(), isEnabled, cancellationToken);
    }
    
    public async Task<UserOverride> UpdateUserOverrideAsync(string key, string userId, bool isEnabled, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ValidateUserId(userId);
        
        return await _repository.UpdateUserOverrideAsync(key, userId.Trim(), isEnabled, cancellationToken);
    }
    
    public async Task RemoveUserOverrideAsync(string key, string userId, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ValidateUserId(userId);
        
        await _repository.RemoveUserOverrideAsync(key, userId.Trim(), cancellationToken);
    }
    
    public async Task<GroupOverride> AddGroupOverrideAsync(string key, string groupId, bool isEnabled, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ValidateGroupId(groupId);
        
        return await _repository.AddGroupOverrideAsync(key, groupId.Trim(), isEnabled, cancellationToken);
    }
    
    public async Task<GroupOverride> UpdateGroupOverrideAsync(string key, string groupId, bool isEnabled, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ValidateGroupId(groupId);
        
        return await _repository.UpdateGroupOverrideAsync(key, groupId.Trim(), isEnabled, cancellationToken);
    }
    
    public async Task RemoveGroupOverrideAsync(string key, string groupId, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ValidateGroupId(groupId);
        
        await _repository.RemoveGroupOverrideAsync(key, groupId.Trim(), cancellationToken);
    }
    
    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ValidationException("Feature flag key cannot be empty.");
        }
        
        if (key.Length > 100)
        {
            throw new ValidationException("Feature flag key cannot exceed 100 characters.");
        }
    }
    
    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ValidationException("User ID cannot be empty.");
        }
        
        if (userId.Length > 100)
        {
            throw new ValidationException("User ID cannot exceed 100 characters.");
        }
    }
    
    private static void ValidateGroupId(string groupId)
    {
        if (string.IsNullOrWhiteSpace(groupId))
        {
            throw new ValidationException("Group ID cannot be empty.");
        }
        
        if (groupId.Length > 100)
        {
            throw new ValidationException("Group ID cannot exceed 100 characters.");
        }
    }
}