using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Ols.ControlCenter.Application.Features.Dashboard;
using Ols.ControlCenter.Application.Features.Operations;

namespace Ols.ControlCenter.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.AddScoped<IOperationQueryService, OperationQueryService>();
        services.AddScoped<IDashboardQueryService, DashboardQueryService>();

        return services;
    }
}
