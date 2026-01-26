using FeatureFlagCore.Data;
using FeatureFlagCore.Entities;
using FeatureFlagCore.Exceptions;
using FeatureFlagCore.Interfaces;
using FeatureFlagCore.Services;
using FeatureFlags.Core.Data;
using FeatureFlags.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace FeatureFlags.Tests.Services;

public class FeatureFlagServiceTests : IDisposable
{
    private readonly FeatureFlagDbContext _context;
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagService _service;
    
    public FeatureFlagServiceTests()
    {
        _context = TestDatabaseHelper.CreateInMemoryContext();
        _repository = new FeatureFlagRepository(_context);
        _service = new FeatureFlagService(_repository);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
    
    #region Create Feature Flag Tests
    
    [Fact]
    public async Task CreateFeatureFlagAsync_WithValidData_ShouldCreateFlag()
    {
        // Act
        var result = await _service.CreateFeatureFlagAsync("test-feature", true, "Test description");
        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-feature", result.Key);
        Assert.True(result.IsEnabled);
        Assert.Equal("Test description", result.Description);
        result.Description.Should().Be("Test description");
    }
    
    [Fact]
    public async Task CreateFeatureFlagAsync_WithDuplicateKey_ShouldThrowException()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("duplicate-key", true);
        
        // Act
        var act = () => _service.CreateFeatureFlagAsync("duplicate-key", false);
        
        // Assert
        await act.Should().ThrowAsync<DuplicateFeatureFlagException>()
            .WithMessage("*duplicate-key*");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateFeatureFlagAsync_WithEmptyKey_ShouldThrowValidationException(string? key)
    {
        // Act
        var act = () => _service.CreateFeatureFlagAsync(key!, true);
        
        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
    
    [Fact]
    public async Task CreateFeatureFlagAsync_WithKeyExceeding100Chars_ShouldThrowValidationException()
    {
        // Arrange
        var longKey = new string('a', 101);
        
        // Act
        var act = () => _service.CreateFeatureFlagAsync(longKey, true);
        
        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
    
    #endregion
    
    #region Get Feature Flag Tests
    
    [Fact]
    public async Task GetFeatureFlagAsync_WithExistingKey_ShouldReturnFlag()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("existing-feature", true, "Description");
        
        // Act
        var result = await _service.GetFeatureFlagAsync("existing-feature");
        
        // Assert
        result.Should().NotBeNull();
        result.Key.Should().Be("existing-feature");
    }
    
    [Fact]
    public async Task GetFeatureFlagAsync_WithNonExistingKey_ShouldThrowNotFoundException()
    {
        // Act
        var act = () => _service.GetFeatureFlagAsync("non-existing");
        
        // Assert
        await act.Should().ThrowAsync<FeatureFlagNotFoundException>();
    }
    
    [Fact]
    public async Task GetAllFeatureFlagsAsync_ShouldReturnAllFlags()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("feature-1", true);
        await _service.CreateFeatureFlagAsync("feature-2", false);
        await _service.CreateFeatureFlagAsync("feature-3", true);
        
        // Act
        var result = await _service.GetAllFeatureFlagsAsync();
        
        // Assert
        result.Should().HaveCount(3);
    }
    
    #endregion
    
    #region Update Feature Flag Tests
    
    [Fact]
    public async Task UpdateFeatureFlagAsync_ShouldUpdateState()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("update-test", false);
        
        // Act
        var result = await _service.UpdateFeatureFlagAsync("update-test", true, "Updated description");
        
        // Assert
        result.IsEnabled.Should().BeTrue();
        result.Description.Should().Be("Updated description");
    }
    
    [Fact]
    public async Task UpdateFeatureFlagAsync_WithNonExistingKey_ShouldThrowNotFoundException()
    {
        // Act
        var act = () => _service.UpdateFeatureFlagAsync("non-existing", true);
        
        // Assert
        await act.Should().ThrowAsync<FeatureFlagNotFoundException>();
    }
    
    #endregion
    
    #region Delete Feature Flag Tests
    
    [Fact]
    public async Task DeleteFeatureFlagAsync_ShouldDeleteFlag()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("delete-test", true);
        
        // Act
        await _service.DeleteFeatureFlagAsync("delete-test");
        
        // Assert
        var act = () => _service.GetFeatureFlagAsync("delete-test");
        await act.Should().ThrowAsync<FeatureFlagNotFoundException>();
    }
    
    [Fact]
    public async Task DeleteFeatureFlagAsync_WithNonExistingKey_ShouldThrowNotFoundException()
    {
        // Act
        var act = () => _service.DeleteFeatureFlagAsync("non-existing");
        
        // Assert
        await act.Should().ThrowAsync<FeatureFlagNotFoundException>();
    }
    
    #endregion
    
    #region Evaluation Tests - Core Precedence Logic
    
    [Fact]
    public async Task EvaluateAsync_WithNoContext_ShouldReturnGlobalDefault()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("eval-global", true);
        
        // Act
        var result = await _service.EvaluateAsync("eval-global");
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task EvaluateAsync_WithEmptyContext_ShouldReturnGlobalDefault()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("eval-empty-context", false);
        
        // Act
        var result = await _service.EvaluateAsync("eval-empty-context", new EvaluationContext());
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task EvaluateAsync_WithUserOverride_ShouldReturnUserOverrideValue()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("eval-user-override", false);
        await _service.AddUserOverrideAsync("eval-user-override", "user123", true);
        
        var context = new EvaluationContext { UserId = "user123" };
        
        // Act
        var result = await _service.EvaluateAsync("eval-user-override", context);
        
        // Assert
        result.Should().BeTrue(); // User override takes precedence over global default
    }
    
    [Fact]
    public async Task EvaluateAsync_WithGroupOverride_ShouldReturnGroupOverrideValue()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("eval-group-override", false);
        await _service.AddGroupOverrideAsync("eval-group-override", "beta-testers", true);
        
        var context = new EvaluationContext { GroupIds = new List<string> { "beta-testers" } };
        
        // Act
        var result = await _service.EvaluateAsync("eval-group-override", context);
        
        // Assert
        result.Should().BeTrue(); // Group override takes precedence over global default
    }
    
    [Fact]
    public async Task EvaluateAsync_UserOverrideTakesPrecedenceOverGroupOverride()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("eval-precedence", false);
        await _service.AddGroupOverrideAsync("eval-precedence", "beta-testers", true);
        await _service.AddUserOverrideAsync("eval-precedence", "user123", false);
        
        var context = new EvaluationContext 
        { 
            UserId = "user123",
            GroupIds = new List<string> { "beta-testers" }
        };
        
        // Act
        var result = await _service.EvaluateAsync("eval-precedence", context);
        
        // Assert
        result.Should().BeFalse(); // User override (false) takes precedence over group override (true)
    }
    
    [Fact]
    public async Task EvaluateAsync_GroupOverrideTakesPrecedenceOverGlobalDefault()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("eval-group-precedence", false);
        await _service.AddGroupOverrideAsync("eval-group-precedence", "premium-users", true);
        
        var context = new EvaluationContext 
        { 
            UserId = "user-without-override",
            GroupIds = new List<string> { "premium-users" }
        };
        
        // Act
        var result = await _service.EvaluateAsync("eval-group-precedence", context);
        
        // Assert
        result.Should().BeTrue(); // Group override takes precedence over global default
    }
    
    [Fact]
    public async Task EvaluateAsync_WithMultipleGroups_ShouldUseFirstMatchingGroupOverride()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("eval-multi-group", false);
        await _service.AddGroupOverrideAsync("eval-multi-group", "group-a", true);
        await _service.AddGroupOverrideAsync("eval-multi-group", "group-b", false);
        
        var context = new EvaluationContext 
        { 
            GroupIds = new List<string> { "group-a", "group-b" }
        };
        
        // Act
        var result = await _service.EvaluateAsync("eval-multi-group", context);
        
        // Assert
        result.Should().BeTrue(); // First matching group (group-a) should be used
    }
    
    [Fact]
    public async Task EvaluateAsync_WithNonExistingFlag_ShouldThrowNotFoundException()
    {
        // Act
        var act = () => _service.EvaluateAsync("non-existing-flag");
        
        // Assert
        await act.Should().ThrowAsync<FeatureFlagNotFoundException>();
    }
    
    [Fact]
    public async Task EvaluateAsync_UserNotInAnyOverride_ShouldReturnGlobalDefault()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("eval-fallback", true);
        await _service.AddUserOverrideAsync("eval-fallback", "other-user", false);
        await _service.AddGroupOverrideAsync("eval-fallback", "other-group", false);
        
        var context = new EvaluationContext 
        { 
            UserId = "user-without-override",
            GroupIds = new List<string> { "group-without-override" }
        };
        
        // Act
        var result = await _service.EvaluateAsync("eval-fallback", context);
        
        // Assert
        result.Should().BeTrue(); // Falls back to global default
    }
    
    #endregion
    
    #region User Override Tests
    
    [Fact]
    public async Task AddUserOverrideAsync_ShouldCreateOverride()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("user-override-create", true);
        
        // Act
        var result = await _service.AddUserOverrideAsync("user-override-create", "user123", false);
        
        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be("user123");
        result.IsEnabled.Should().BeFalse();
    }
    
    [Fact]
    public async Task AddUserOverrideAsync_Duplicate_ShouldThrowException()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("user-override-dup", true);
        await _service.AddUserOverrideAsync("user-override-dup", "user123", false);
        
        // Act
        var act = () => _service.AddUserOverrideAsync("user-override-dup", "user123", true);
        
        // Assert
        await act.Should().ThrowAsync<DuplicateOverrideException>();
    }
    
    [Fact]
    public async Task UpdateUserOverrideAsync_ShouldUpdateOverride()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("user-override-update", true);
        await _service.AddUserOverrideAsync("user-override-update", "user123", false);
        
        // Act
        var result = await _service.UpdateUserOverrideAsync("user-override-update", "user123", true);
        
        // Assert
        result.IsEnabled.Should().BeTrue();
    }
    
    [Fact]
    public async Task RemoveUserOverrideAsync_ShouldRemoveOverride()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("user-override-remove", true);
        await _service.AddUserOverrideAsync("user-override-remove", "user123", false);
        
        // Act
        await _service.RemoveUserOverrideAsync("user-override-remove", "user123");
        
        // Assert - evaluation should now use global default
        var context = new EvaluationContext { UserId = "user123" };
        var result = await _service.EvaluateAsync("user-override-remove", context);
        result.Should().BeTrue(); // Back to global default
    }
    
    [Fact]
    public async Task RemoveUserOverrideAsync_NonExisting_ShouldThrowException()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("user-override-remove-none", true);
        
        // Act
        var act = () => _service.RemoveUserOverrideAsync("user-override-remove-none", "non-existing-user");
        
        // Assert
        await act.Should().ThrowAsync<OverrideNotFoundException>();
    }
    
    #endregion
    
    #region Group Override Tests
    
    [Fact]
    public async Task AddGroupOverrideAsync_ShouldCreateOverride()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("group-override-create", true);
        
        // Act
        var result = await _service.AddGroupOverrideAsync("group-override-create", "beta-testers", false);
        
        // Assert
        result.Should().NotBeNull();
        result.GroupId.Should().Be("beta-testers");
        result.IsEnabled.Should().BeFalse();
    }
    
    [Fact]
    public async Task AddGroupOverrideAsync_Duplicate_ShouldThrowException()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("group-override-dup", true);
        await _service.AddGroupOverrideAsync("group-override-dup", "beta-testers", false);
        
        // Act
        var act = () => _service.AddGroupOverrideAsync("group-override-dup", "beta-testers", true);
        
        // Assert
        await act.Should().ThrowAsync<DuplicateOverrideException>();
    }
    
    [Fact]
    public async Task UpdateGroupOverrideAsync_ShouldUpdateOverride()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("group-override-update", true);
        await _service.AddGroupOverrideAsync("group-override-update", "beta-testers", false);
        
        // Act
        var result = await _service.UpdateGroupOverrideAsync("group-override-update", "beta-testers", true);
        
        // Assert
        result.IsEnabled.Should().BeTrue();
    }
    
    [Fact]
    public async Task RemoveGroupOverrideAsync_ShouldRemoveOverride()
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("group-override-remove", false);
        await _service.AddGroupOverrideAsync("group-override-remove", "beta-testers", true);
        
        // Act
        await _service.RemoveGroupOverrideAsync("group-override-remove", "beta-testers");
        
        // Assert - evaluation should now use global default
        var context = new EvaluationContext { GroupIds = new List<string> { "beta-testers" } };
        var result = await _service.EvaluateAsync("group-override-remove", context);
        result.Should().BeFalse(); // Back to global default
    }
    
    #endregion
    
    #region Edge Cases
    
    [Fact]
    public async Task AddOverrideAsync_ToNonExistingFlag_ShouldThrowNotFoundException()
    {
        // Act
        var userAct = () => _service.AddUserOverrideAsync("non-existing", "user123", true);
        var groupAct = () => _service.AddGroupOverrideAsync("non-existing", "group123", true);
        
        // Assert
        await userAct.Should().ThrowAsync<FeatureFlagNotFoundException>();
        await groupAct.Should().ThrowAsync<FeatureFlagNotFoundException>();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddUserOverrideAsync_WithEmptyUserId_ShouldThrowValidationException(string userId)
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("validate-user-id", true);
        
        // Act
        var act = () => _service.AddUserOverrideAsync("validate-user-id", userId, true);
        
        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddGroupOverrideAsync_WithEmptyGroupId_ShouldThrowValidationException(string groupId)
    {
        // Arrange
        await _service.CreateFeatureFlagAsync("validate-group-id", true);
        
        // Act
        var act = () => _service.AddGroupOverrideAsync("validate-group-id", groupId, true);
        
        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
    
    #endregion
}
