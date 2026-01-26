using FeatureFlagCore.Data;
using FeatureFlagCore.Entities;
using FeatureFlagCore.Interfaces;
using FeatureFlagCore.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Core.Data;

public class FeatureFlagRepository : IFeatureFlagRepository
{
    private readonly FeatureFlagDbContext _context;
    
    public FeatureFlagRepository(FeatureFlagDbContext context)
    {
        _context = context;
    }
    
    public async Task<FeatureFlag?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .FirstOrDefaultAsync(f => f.Key == key, cancellationToken);
    }
    
    public async Task<FeatureFlag?> GetByKeyWithOverridesAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.UserOverrides)
            .Include(f => f.GroupOverrides)
            .FirstOrDefaultAsync(f => f.Key == key, cancellationToken);
    }
    
    public async Task<IReadOnlyList<FeatureFlag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.UserOverrides)
            .Include(f => f.GroupOverrides)
            .OrderBy(f => f.Key)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags.AnyAsync(f => f.Key == key, cancellationToken);
    }
    
    public async Task<FeatureFlag> CreateAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default)
    {
        if (await ExistsAsync(featureFlag.Key, cancellationToken))
        {
            throw new DuplicateFeatureFlagException(featureFlag.Key);
        }
        
        featureFlag.CreatedAt = DateTime.UtcNow;
        featureFlag.UpdatedAt = DateTime.UtcNow;
        
        _context.FeatureFlags.Add(featureFlag);
        await _context.SaveChangesAsync(cancellationToken);
        
        return featureFlag;
    }
    
    public async Task<FeatureFlag> UpdateAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default)
    {
        featureFlag.UpdatedAt = DateTime.UtcNow;
        _context.FeatureFlags.Update(featureFlag);
        await _context.SaveChangesAsync(cancellationToken);
        
        return featureFlag;
    }
    
    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var featureFlag = await GetByKeyAsync(key, cancellationToken)
            ?? throw new FeatureFlagNotFoundException(key);
        
        _context.FeatureFlags.Remove(featureFlag);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<UserOverride?> GetUserOverrideAsync(string key, string userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserOverrides
            .Include(u => u.FeatureFlag)
            .FirstOrDefaultAsync(u => u.FeatureFlag.Key == key && u.UserId == userId, cancellationToken);
    }
    
    public async Task<UserOverride> AddUserOverrideAsync(string key, string userId, bool isEnabled, CancellationToken cancellationToken = default)
    {
        var featureFlag = await GetByKeyAsync(key, cancellationToken)
            ?? throw new FeatureFlagNotFoundException(key);
        
        var existingOverride = await GetUserOverrideAsync(key, userId, cancellationToken);
        if (existingOverride != null)
        {
            throw new DuplicateOverrideException(key, userId, "user");
        }
        
        var userOverride = new UserOverride
        {
            FeatureFlagId = featureFlag.Id,
            UserId = userId,
            IsEnabled = isEnabled,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.UserOverrides.Add(userOverride);
        await _context.SaveChangesAsync(cancellationToken);
        
        return userOverride;
    }
    
    public async Task<UserOverride> UpdateUserOverrideAsync(string key, string userId, bool isEnabled, CancellationToken cancellationToken = default)
    {
        var userOverride = await GetUserOverrideAsync(key, userId, cancellationToken)
            ?? throw new OverrideNotFoundException(key, userId, "user");
        
        userOverride.IsEnabled = isEnabled;
        _context.UserOverrides.Update(userOverride);
        await _context.SaveChangesAsync(cancellationToken);
        
        return userOverride;
    }
    
    public async Task RemoveUserOverrideAsync(string key, string userId, CancellationToken cancellationToken = default)
    {
        var userOverride = await GetUserOverrideAsync(key, userId, cancellationToken)
            ?? throw new OverrideNotFoundException(key, userId, "user");
        
        _context.UserOverrides.Remove(userOverride);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<GroupOverride?> GetGroupOverrideAsync(string key, string groupId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupOverrides
            .Include(g => g.FeatureFlag)
            .FirstOrDefaultAsync(g => g.FeatureFlag.Key == key && g.GroupId == groupId, cancellationToken);
    }
    
    public async Task<GroupOverride> AddGroupOverrideAsync(string key, string groupId, bool isEnabled, CancellationToken cancellationToken = default)
    {
        var featureFlag = await GetByKeyAsync(key, cancellationToken)
            ?? throw new FeatureFlagNotFoundException(key);
        
        var existingOverride = await GetGroupOverrideAsync(key, groupId, cancellationToken);
        if (existingOverride != null)
        {
            throw new DuplicateOverrideException(key, groupId, "group");
        }
        
        var groupOverride = new GroupOverride
        {
            FeatureFlagId = featureFlag.Id,
            GroupId = groupId,
            IsEnabled = isEnabled,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.GroupOverrides.Add(groupOverride);
        await _context.SaveChangesAsync(cancellationToken);
        
        return groupOverride;
    }
    
    public async Task<GroupOverride> UpdateGroupOverrideAsync(string key, string groupId, bool isEnabled, CancellationToken cancellationToken = default)
    {
        var groupOverride = await GetGroupOverrideAsync(key, groupId, cancellationToken)
            ?? throw new OverrideNotFoundException(key, groupId, "group");
        
        groupOverride.IsEnabled = isEnabled;
        _context.GroupOverrides.Update(groupOverride);
        await _context.SaveChangesAsync(cancellationToken);
        
        return groupOverride;
    }
    
    public async Task RemoveGroupOverrideAsync(string key, string groupId, CancellationToken cancellationToken = default)
    {
        var groupOverride = await GetGroupOverrideAsync(key, groupId, cancellationToken)
            ?? throw new OverrideNotFoundException(key, groupId, "group");
        
        _context.GroupOverrides.Remove(groupOverride);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
