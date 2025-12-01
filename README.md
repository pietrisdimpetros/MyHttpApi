feat: initial implementation of .NET 10 Enterprise Microservice Foundation

Establishes the core shared libraries and reference architecture for the platform.

Key Capabilities:
- Shared.Hosting: "Service Defaults" pattern to aggregate all infrastructure.
- Shared.Data: Resilient EF Core setup with automatic "AuditLogs" table schema contract.
- Shared.Security: Entra ID integration with database-driven Role Augmentation.
- Shared.Observability: Native Azure Monitor Distro implementation (Tracing, Metrics, Logs).
- Shared.Logging: Structured JSON logging with Correlation ID propagation.
- Shared.Validation: Global Async Action Filter for FluentValidation integration.
- Shared.Documentation: Swashbuckle setup with OAuth2 Authorization Code Flow (PKCE).
- Shared.ExternalIntegration: Resilient HTTP Client (Retry/CircuitBreaker) with OAuth2.
- Shared.Caching: Standardized Cache-Aside agent (InMemory/Distributed).

Includes 'MyWebApi' as the reference implementation.
