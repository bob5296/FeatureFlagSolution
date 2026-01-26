using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FeatureFlagsController : ControllerBase
{
    public FeatureFlagsController()
    {
    }
}
