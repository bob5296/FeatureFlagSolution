namespace FeatureFlagCore.Exceptions;

/// <summary>
/// Exception thrown when a feature flag is not found.
/// </summary>
public class FeatureFlagNotFoundException : Exception
{
    public string FeatureKey { get; }
    
    public FeatureFlagNotFoundException(string featureKey)
        : base($"Feature flag with key '{featureKey}' was not found.")
    {
        FeatureKey = featureKey;
    }
}

/// <summary>
/// Exception thrown when attempting to create a feature flag with a duplicate key.
/// </summary>
public class DuplicateFeatureFlagException : Exception
{
    public string FeatureKey { get; }
    
    public DuplicateFeatureFlagException(string featureKey)
        : base($"A feature flag with key '{featureKey}' already exists.")
    {
        FeatureKey = featureKey;
    }
}

/// <summary>
/// Exception thrown when an override already exists.
/// </summary>
public class DuplicateOverrideException : Exception
{
    public string FeatureKey { get; }
    public string OverrideId { get; }
    public string OverrideType { get; }
    
    public DuplicateOverrideException(string featureKey, string overrideId, string overrideType)
        : base($"A {overrideType} override for '{overrideId}' already exists on feature flag '{featureKey}'.")
    {
        FeatureKey = featureKey;
        OverrideId = overrideId;
        OverrideType = overrideType;
    }
}

/// <summary>
/// Exception thrown when an override is not found.
/// </summary>
public class OverrideNotFoundException : Exception
{
    public string FeatureKey { get; }
    public string OverrideId { get; }
    public string OverrideType { get; }
    
    public OverrideNotFoundException(string featureKey, string overrideId, string overrideType)
        : base($"No {overrideType} override for '{overrideId}' exists on feature flag '{featureKey}'.")
    {
        FeatureKey = featureKey;
        OverrideId = overrideId;
        OverrideType = overrideType;
    }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }
    
    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }
    
    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
