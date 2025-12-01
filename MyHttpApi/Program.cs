using Shared.Hosting.Extensions;             
using Shared.Data.Extensions;                
using Shared.ExternalApiIntegration.Extensions; 
using Shared.Data.Entities.Identity;         

namespace MyHttpApi
{
    public partial class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================================================================
            // 1. 
            // =========================================================================
            // This automatically configures:
            // - Configuration (appsettings + Env Vars + Key Vault)
            // - CORS Policy (Restrictive by Default)
            // - Logging (Serilog/JsonConsole)
            // - Observability (Azure Monitor Traces/Metrics/Logs)
            // - Security (Entra ID Auth + Role Augmentation)
            // - Documentation (Swagger with OAuth2)
            // - Validation (FluentValidation Filter)
            // - Caching (Distributed/Memory Agent)
            // - Health Checks (Base)
            builder.AddCustomServiceDefaults<ApplicationUser>(typeof(Program).Assembly);

            // =========================================================================
            // 2. SPECIFIC INFRASTRUCTURE (Opt-In)
            // =========================================================================

            // A. Database (SQL Server + EF Core + Audit Interceptors)
            // We add this explicitly because some microservices might be stateless.
            builder.Services.AddDatabaseInfrastructure(builder.Configuration);

            // B. External API Client (Resilient HTTP)
            // We add this explicitly because not every service calls downstream.
            // It reads from the "ExternalApi" section in appsettings.
            builder.Services.AddExternalApiIntegration();

            // =========================================================================
            // 3. LOCAL BUSINESS LOGIC
            // =========================================================================
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Add your Domain Services here
            // builder.Services.AddScoped<IOrderService, OrderService>();

            var app = builder.Build();

            // =========================================================================
            // 4. THE PIPELINE (One Line)
            // =========================================================================
            // This automatically sets up:
            // - Security Headers (CSP, HSTS, XSS, etc.)
            // - CORS Policy
            // - Global Exception Handling (ProblemDetails)
            // - Request Logging (Correlation IDs)
            // - Authentication & Authorization Middleware
            // - Swagger UI (with OAuth2 PKCE)
            // - Health Check Endpoints (/health)
            app.UseCustomHostingDefaults();
            app.MapControllers();
            app.Run();
        }
    }
}