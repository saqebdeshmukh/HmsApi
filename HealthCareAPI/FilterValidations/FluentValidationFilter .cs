using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HealthCareAPI.FilterValidations
{
    public class FluentValidationFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FluentValidationFilter> _logger;

        public FluentValidationFilter(IServiceProvider serviceProvider,
                                      ILogger<FluentValidationFilter> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var errors = new List<object>();

            foreach (var argument in context.ActionArguments)
            {
                if (argument.Value == null)
                    continue;

                var argumentType = argument.Value.GetType();

                // Handle collections (List<T>, IEnumerable<T>)
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(argumentType)
                    && argumentType != typeof(string))
                {
                    foreach (var item in (IEnumerable<object>)argument.Value)
                    {
                        await ValidateObject(item, errors);
                    }
                }
                else
                {
                    await ValidateObject(argument.Value, errors);
                }
            }

            if (errors.Any())
            {
                _logger.LogWarning("Validation failed: {@Errors}", errors);

                context.Result = new BadRequestObjectResult(new
                {
                    StatusCode = 400,
                    Message = "Validation Failed",
                    Errors = errors
                });

                return;
            }

            await next();
        }

        private async Task ValidateObject(object model, List<object> errors)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(model.GetType());
            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator == null)
                return;

            var validationContext = new ValidationContext<object>(model);
            ValidationResult result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                errors.AddRange(result.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Error = e.ErrorMessage
                }));
            }
        }
    }
}
