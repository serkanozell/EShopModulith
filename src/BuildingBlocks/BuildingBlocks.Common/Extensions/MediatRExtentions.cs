using BuildingBlocks.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Common.Extensions
{
    public static class MediatRExtentions
    {
        public static IServiceCollection AddMediatRWithAssemblies(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblies(assemblies);
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
                config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

            services.AddValidatorsFromAssemblies(assemblies);

            return services;
        }
    }
}