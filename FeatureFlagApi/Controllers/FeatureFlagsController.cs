using FeatureFlagApi.Models;
using FeatureFlagCore.Entities;
using FeatureFlagCore.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FeatureFlagsController : ControllerBase
{
    private readonly IFeatureFlagService _service;
    
    public FeatureFlagsController(IFeatureFlagService service)
    {
        _service = service;
    }
    
    #region Feature Flag CRUD
    
    /// <summary>
    /// Get all feature flags
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FeatureFlagResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FeatureFlagResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var flags = await _service.GetAllFeatureFlagsAsync(cancellationToken);
        return Ok(flags.Select(MapToResponse));
    }
    
    /// <summary>
    /// Get a specific feature flag by key
    /// </summary>
    [HttpGet("{key}")]
    [ProducesResponseType(typeof(FeatureFlagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeatureFlagResponse>> GetByKey(string key, CancellationToken cancellationToken)
    {
        var flag = await _service.GetFeatureFlagAsync(key, cancellationToken);
        return Ok(MapToResponse(flag));
    }
    
    /// <summary>
    /// Create a new feature flag
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FeatureFlagResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FeatureFlagResponse>> Create([FromBody] CreateFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        var flag = await _service.CreateFeatureFlagAsync(request.Key, request.IsEnabled, request.Description, cancellationToken);
        return CreatedAtAction(nameof(GetByKey), new { key = flag.Key }, MapToResponse(flag));
    }
    
    /// <summary>
    /// Update an existing feature flag's global state
    /// </summary>
    [HttpPut("{key}")]
    [ProducesResponseType(typeof(FeatureFlagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeatureFlagResponse>> Update(string key, [FromBody] UpdateFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        var flag = await _service.UpdateFeatureFlagAsync(key, request.IsEnabled, request.Description, cancellationToken);
        return Ok(MapToResponse(flag));
    }
    
    /// <summary>
    /// Delete a feature flag
    /// </summary>
    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string key, CancellationToken cancellationToken)
    {
        await _service.DeleteFeatureFlagAsync(key, cancellationToken);
        return NoContent();
    }
    
    #endregion
    
    #region Evaluation
    
    /// <summary>
    /// Evaluate a feature flag for the given context
    /// Precedence: User override > Group override > Global default
    /// </summary>
    [HttpPost("{key}/evaluate")]
    [ProducesResponseType(typeof(EvaluateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EvaluateResponse>> Evaluate(string key, [FromBody] EvaluateRequest? request, CancellationToken cancellationToken)
    {
        var context = request != null ? new EvaluationContext
        {
            UserId = request.UserId,
            GroupIds = request.GroupIds
        } : null;
        
        var isEnabled = await _service.EvaluateAsync(key, context, cancellationToken);
        
        return Ok(new EvaluateResponse
        {
            Key = key,
            IsEnabled = isEnabled
        });
    }
    
    #endregion
    
    #region User Overrides
    
    /// <summary>
    /// Add a user-specific override
    /// </summary>
    [HttpPost("{key}/users")]
    [ProducesResponseType(typeof(UserOverrideResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserOverrideResponse>> AddUserOverride(string key, [FromBody] AddOverrideRequest request, CancellationToken cancellationToken)
    {
        var userOverride = await _service.AddUserOverrideAsync(key, request.Id, request.IsEnabled, cancellationToken);
        return CreatedAtAction(nameof(GetByKey), new { key }, MapToUserOverrideResponse(userOverride));
    }
    
    /// <summary>
    /// Update a user-specific override
    /// </summary>
    [HttpPut("{key}/users/{userId}")]
    [ProducesResponseType(typeof(UserOverrideResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserOverrideResponse>> UpdateUserOverride(string key, string userId, [FromBody] UpdateOverrideRequest request, CancellationToken cancellationToken)
    {
        var userOverride = await _service.UpdateUserOverrideAsync(key, userId, request.IsEnabled, cancellationToken);
        return Ok(MapToUserOverrideResponse(userOverride));
    }
    
    /// <summary>
    /// Remove a user-specific override
    /// </summary>
    [HttpDelete("{key}/users/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveUserOverride(string key, string userId, CancellationToken cancellationToken)
    {
        await _service.RemoveUserOverrideAsync(key, userId, cancellationToken);
        return NoContent();
    }
    
    #endregion
    
    #region Group Overrides
    
    /// <summary>
    /// Add a group-specific override
    /// </summary>
    [HttpPost("{key}/groups")]
    [ProducesResponseType(typeof(GroupOverrideResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GroupOverrideResponse>> AddGroupOverride(string key, [FromBody] AddOverrideRequest request, CancellationToken cancellationToken)
    {
        var groupOverride = await _service.AddGroupOverrideAsync(key, request.Id, request.IsEnabled, cancellationToken);
        return CreatedAtAction(nameof(GetByKey), new { key }, MapToGroupOverrideResponse(groupOverride));
    }
    
    /// <summary>
    /// Update a group-specific override
    /// </summary>
    [HttpPut("{key}/groups/{groupId}")]
    [ProducesResponseType(typeof(GroupOverrideResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GroupOverrideResponse>> UpdateGroupOverride(string key, string groupId, [FromBody] UpdateOverrideRequest request, CancellationToken cancellationToken)
    {
        var groupOverride = await _service.UpdateGroupOverrideAsync(key, groupId, request.IsEnabled, cancellationToken);
        return Ok(MapToGroupOverrideResponse(groupOverride));
    }
    
    /// <summary>
    /// Remove a group-specific override
    /// </summary>
    [HttpDelete("{key}/groups/{groupId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveGroupOverride(string key, string groupId, CancellationToken cancellationToken)
    {
        await _service.RemoveGroupOverrideAsync(key, groupId, cancellationToken);
        return NoContent();
    }
    
    #endregion
    
    #region Mapping Helpers
    
    private static FeatureFlagResponse MapToResponse(FeatureFlag flag) => new()
    {
        Key = flag.Key,
        Description = flag.Description,
        IsEnabled = flag.IsEnabled,
        CreatedAt = flag.CreatedAt,
        UpdatedAt = flag.UpdatedAt,
        UserOverrides = flag.UserOverrides.Select(MapToUserOverrideResponse).ToList(),
        GroupOverrides = flag.GroupOverrides.Select(MapToGroupOverrideResponse).ToList()
    };
    
    private static UserOverrideResponse MapToUserOverrideResponse(UserOverride u) => new()
    {
        UserId = u.UserId,
        IsEnabled = u.IsEnabled,
        CreatedAt = u.CreatedAt
    };
    
    private static GroupOverrideResponse MapToGroupOverrideResponse(GroupOverride g) => new()
    {
        GroupId = g.GroupId,
        IsEnabled = g.IsEnabled,
        CreatedAt = g.CreatedAt
    };
    
    #endregion
}
