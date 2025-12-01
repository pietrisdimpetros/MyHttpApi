using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Shared.Validation.Configuration;

namespace Shared.Validation.Filters
{
    public sealed class ValidationFilter(IOptions<ValidationOptions> options) : IAsyncActionFilter
    {
        private readonly ValidationOptions _config = options.Value;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. Find arguments that are not null (Potential DTOs)
            var validatableArguments = context.ActionArguments
                .Where(arg => arg.Value is not null)
                .Select(arg => arg.Value!)
                .ToList();

            if (validatableArguments.Count == 0)
            {
                await next();
                return;
            }

            var serviceProvider = context.HttpContext.RequestServices;
            var validationFailures = new Dictionary<string, string[]>();

            // 2. Loop through arguments and check if a Validator exists for them
            foreach (var argument in validatableArguments)
            {
                var argumentType = argument.GetType();

                // Dynamically resolve IValidator<T>
                var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

                // If we find a validator in DI, run it
                if (serviceProvider.GetService(validatorType) is IValidator validator)
                {
                    // Create context (allows us to pass FailFast flag)
                    var validationContext = new ValidationContext<object>(argument);

                    if (_config.EnableFailFast)
                    {
                        validationContext.RootContextData["FailFast"] = true;
                    }

                    // Async validation to support Database checks (e.g., "Email Exists?")
                    var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                    if (!result.IsValid)
                    {
                        // Group errors by property name
                        foreach (var error in result.Errors)
                        {
                            if (!validationFailures.TryGetValue(error.PropertyName, out string[]? value))
                            {
                                value = [];
                                validationFailures[error.PropertyName] = value;
                            }

                            validationFailures[error.PropertyName] =
                                [.. value, error.ErrorMessage];
                        }
                    }
                }
            }

            // 3. If any errors found, return 400 Bad Request
            if (validationFailures.Count > 0)
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Failed",
                    Detail = "One or more validation errors occurred.",
                    Instance = context.HttpContext.Request.Path,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
                };

                // Standardized Error Format
                problemDetails.Extensions["errors"] = validationFailures;
                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                context.Result = new BadRequestObjectResult(problemDetails);
                return;
            }

            // 4. If valid, proceed to Controller
            await next();
        }
    }
}
