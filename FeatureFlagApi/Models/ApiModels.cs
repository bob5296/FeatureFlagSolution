using System.ComponentModel.DataAnnotations;

namespace FeatureFlagApi.Models;

#region Feature Flag DTOs

public record CreateFeatureFlagRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Key { get; init; } = string.Empty;
    
    public bool IsEnabled { get; init; }
    
    [StringLength(500)]
    public string? Description { get; init; }
}

public record UpdateFeatureFlagRequest
{
    public bool IsEnabled { get; init; }
    
    [StringLength(500)]
    public string? Description { get; init; }
}

public record FeatureFlagResponse
{
    public string Key { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsEnabled { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public IList<UserOverrideResponse> UserOverrides { get; init; } = new List<UserOverrideResponse>();
    public IList<GroupOverrideResponse> GroupOverrides { get; init; } = new List<GroupOverrideResponse>();
}

#endregion

#region Override DTOs

public record UserOverrideResponse
{
    public string UserId { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record GroupOverrideResponse
{
    public string GroupId { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AddOverrideRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Id { get; init; } = string.Empty;
    
    public bool IsEnabled { get; init; }
}

public record UpdateOverrideRequest
{
    public bool IsEnabled { get; init; }
}

#endregion

#region Evaluation DTOs

public record EvaluateRequest
{
    public string? UserId { get; init; }
    public IList<string> GroupIds { get; init; } = new List<string>();
}

public record EvaluateResponse
{
    public string Key { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}

#endregion

#region Error DTOs

public record ErrorResponse
{
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public IDictionary<string, string[]>? Errors { get; init; }
}

#endregion
