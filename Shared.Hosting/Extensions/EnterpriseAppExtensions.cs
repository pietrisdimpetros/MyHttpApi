using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Shared.Documentation.Extensions;
using Shared.Logging.Extensions;
namespace Shared.Hosting.Extensions
{
    public static class EnterpriseAppExtensions
    {
        public static WebApplication UseCustomHostingDefaults(this WebApplication app)
        {
            // 0. Security Headers & Protocol
            app.UseHttpsRedirection(); // Force HTTPS
            app.UseHsts();

            // 1. Error Handling (Global ProblemDetails)
            app.UseExceptionHandler();
            app.UseStatusCodePages();

            // 2. CORS
            app.UseCors("CustomCorsPolicy");


            // 3. Observability & Logs
            app.UseEnterpriseLogging();
            // (Observability usually hooks largely into DI, but if you have metrics middleware:)
            // app.UseObservabilityEndpoints(); 

            // 4. Documentation (Swagger)
            // Only in non-prod or if explicitly enabled
            if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Documentation:EnableInProd"))
            {
                app.UseEnterpriseDocumentation();
            }

            // 5. Security
            app.UseAuthentication();
            app.UseAuthorization();

            // 6. Health Checks Endpoint
            app.MapHealthChecks("/health");
            app.MapHealthChecks("/alive");

            return app;
        }
    }
}