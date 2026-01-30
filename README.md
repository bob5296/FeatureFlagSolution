# Feature Flag Service

A .NET 10 feature flag management system with REST API, supporting user and group-specific overrides with hierarchical evaluation.

## Features

- **Feature Flag Management**: Create, read, update, delete feature flags
- **User Overrides**: Per-user feature flag states
- **Group Overrides**: Per-group feature flag states  
- **Hierarchical Evaluation**: User > Group > Global precedence
- **SQLite Database**: Lightweight persistence with Entity Framework Core
- **REST API**: Full CRUD operations with Swagger documentation
- **Comprehensive Testing**: 32 unit tests covering all scenarios

## Prerequisites

- .NET SDK 10.0 or later

## Getting Started from GitHub

```bash
# 1. Clone the repository from GitHub
git clone https://https://github.com/bob5296/FeatureFlagSolution
cd FeatureFlagSolution

# 2. Run the API locally
cd FeatureFlagApi
dotnet run

# API available at http://localhost:5195
# Swagger UI at http://localhost:5195/swagger
```
## Using Swagger to Explore and Test Endpoints

1. Start the API (`cd FeatureFlagApi && dotnet run`).
2. Open a browser and navigate to `http://localhost:5195/swagger`.
3. Use the Swagger UI to:
   - Inspect all available endpoints
   - View request/response schemas
   - Execute requests directly from the browser for manual testing

## Key API Endpoints

### Feature Flags

```bash
GET    /api/FeatureFlags           # List all flags
GET    /api/FeatureFlags/{key}     # Get specific flag
POST   /api/FeatureFlags           # Create flag
PUT    /api/FeatureFlags/{key}     # Update flag
DELETE /api/FeatureFlags/{key}     # Delete flag
```

### Evaluation

```bash
POST /api/FeatureFlags/{key}/evaluate
{
  "userId": "user123",
  "groupIds": ["beta-testers", "premium"]
}
```

### User Overrides

```bash
POST   /api/FeatureFlags/{key}/users        # Add user override
PUT    /api/FeatureFlags/{key}/users/{id}   # Update user override
DELETE /api/FeatureFlags/{key}/users/{id}   # Remove user override
```

### Group Overrides

```bash
POST   /api/FeatureFlags/{key}/groups       # Add group override
PUT    /api/FeatureFlags/{key}/groups/{id}  # Update group override
DELETE /api/FeatureFlags/{key}/groups/{id}  # Remove group override
```

## Example cURL Usage

With the API running locally on `http://localhost:5195`:

```bash
# Create feature flag
curl -X POST "http://localhost:5195/api/FeatureFlags" \
  -H "Content-Type: application/json" \
  -d '{"key": "new-dashboard", "isEnabled": false, "description": "New dashboard UI"}'

# Add group override
curl -X POST "http://localhost:5195/api/FeatureFlags/new-dashboard/groups" \
  -H "Content-Type: application/json" \
  -d '{"id": "beta-testers", "isEnabled": true}'

# Evaluate flag
curl -X POST "http://localhost:5195/api/FeatureFlags/new-dashboard/evaluate" \
  -H "Content-Type: application/json" \
  -d '{"userId": "user123", "groupIds": ["beta-testers"]}'
```

## Evaluation Logic

1. **User Override** (highest priority)
2. **Group Override** (first matching group)
3. **Global Default** (fallback)

## Project Structure

```
FeatureFlagSolution/
├── FeatureFlagApi/          # REST API layer
├── FeatureFlagCore/         # Business logic & data access
└── FeatureFlagTests/        # Unit tests (32 tests)
```

## Running Tests

From the solution root:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

This will run all tests in `FeatureFlagTests` against both the core library and the API.

## Database

A SQLite database file (`featureflags.db`) is created automatically on first run in the API project directory.

## Known Limitations

- No authentication/authorization built-in
- No caching (each evaluation hits the database)
- SQLite may not scale for high-concurrency production use
- No audit trail for changes

## Testing

The project includes 36 unit tests covering:

- Feature flag CRUD operations
- Evaluation precedence logic (user > group > global)
- User and group override management
- Validation and error handling
- Edge cases


## Assumptions

1. **Single database instance**  
   The app currently runs against a single SQLite database. That keeps local setup simple and avoids external dependencies. For a real production environment, you’d almost certainly want to move to something like PostgreSQL or SQL Server with proper connection pooling and operational tooling.

2. **Group order matters**  
   When a user belongs to multiple groups that have overrides, the order of the groups you pass in matters. The evaluation logic walks the list in order and uses the first matching group it finds.

3. **Case-sensitive identifiers**  
   Feature flag keys, user IDs, and group IDs are treated as case-sensitive. `"NewDashboard"` and `"newdashboard"` are considered different values.

4. **No authentication/auth (yet)**  
   The API is intentionally left unauthenticated to keep the sample small and easy to work with. In any real deployment, you should put it behind proper authentication/authorization (e.g., API keys, JWTs, OAuth, etc.).

5. **Synchronous evaluation**  
   Each evaluation call hits the database directly. That’s fine for a demo or low-traffic usage, but under heavier load you’d want some form of caching in front of the database.

## Tradeoffs

### Why it’s built this way

- **SQLite for simplicity**  
  SQLite gives us a zero-config, file-based database that “just works” locally. For production, you can switch to PostgreSQL or SQL Server by changing the connection string and provider configuration.

- **Repository pattern**  
  Data access is wrapped behind repositories so the core business logic doesn’t depend directly on Entity Framework. This makes it easier to change persistence later (e.g., introduce caching, move to a different database) without rewriting the evaluation logic.

- **Eager loading for evaluation**  
  During evaluation we eagerly load the related overrides with the feature flag. That avoids N+1 query issues and keeps evaluation predictable, at the cost of pulling a bit more data up front.

### If I had more time

1. **Add caching**  
   Introduce a caching layer (e.g., in-memory or Redis) for frequently used flags to take read pressure off the database and improve latency.

2. **Audit logging**  
   Record who changed which flag (and when) so you have a clear audit trail for debugging and compliance.

3. **Integration tests**  
   Add end-to-end API tests (e.g., using `WebApplicationFactory`) to validate the full stack: routing, DI, EF, and evaluation logic working together.

4. **Health checks**  
   Expose a `/health` endpoint to integrate with monitoring systems and quickly verify database connectivity and basic app health.

5. **Metrics and observability**  
   Wire up metrics via OpenTelemetry or similar (e.g., evaluation count, success/error rates, and latency) to understand how the system behaves in real-world traffic.

6. **Implement clean code architecture and architecture tests**
   Would segregate the project structure either into vertical slice or clean code/ Onion architecture.
   Add architecture tests using NArch to enfore clean code is not being violated.
7. **Add authentication/authorization**
 Secure endpoints using OAuth2.0 code flow
 8. **Clean way to add extensions**
 9. **Clean registration of en
 10. **User Id validation**
    User Ids are assumed to be valid strings. No additional validation is performed on user ids.
 11. **DDD**
    Implement proper DDD for entities, currently it is scaterred in repository.
 12. Add CI/CD using Docker image
 13. Use directory.props to centralize dependency management

