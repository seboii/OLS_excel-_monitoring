using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Ols.ControlCenter.Application.Abstractions.DataIntegration;
using Ols.ControlCenter.Application.Abstractions.Risk;
using Ols.ControlCenter.Application.Features.Ai;
using Ols.ControlCenter.Application.Features.Alerts;
using Ols.ControlCenter.Application.Features.Boards;
using Ols.ControlCenter.Application.Features.Auth;
using Ols.ControlCenter.Application.Features.Comments;
using Ols.ControlCenter.Application.Features.Kpi;
using Ols.ControlCenter.Application.Features.Dashboard;
using Ols.ControlCenter.Application.Features.DataSources;
using Ols.ControlCenter.Application.Features.Finance;
using Ols.ControlCenter.Application.Features.Notifications;
using Ols.ControlCenter.Application.Features.Operations;
using Ols.ControlCenter.Application.Features.Risk;
using Ols.ControlCenter.Application.Features.Tasks;

namespace Ols.ControlCenter.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOperationQueryService, OperationQueryService>();
        services.AddScoped<IDashboardQueryService, DashboardQueryService>();
        services.AddScoped<IDataSourceService, DataSourceService>();
        services.AddScoped<IColumnMappingService, ColumnMappingService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IAiSummaryService, AiSummaryService>();
        services.AddScoped<IKpiService, KpiService>();
        services.AddScoped<ITrackingMetricsService, TrackingMetricsService>();
        services.AddScoped<IFinanceSummaryService, FinanceSummaryService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Risk motoru — 7 kural + motor
        services.AddScoped<IRiskRule, DelayRule>();
        services.AddScoped<IRiskRule, PaymentRiskRule>();
        services.AddScoped<IRiskRule, CustomerInfoRule>();
        services.AddScoped<IRiskRule, DocumentMissingRule>();
        services.AddScoped<IRiskRule, SeaDemurrageRule>();
        services.AddScoped<IRiskRule, NextActionMissingRule>();
        services.AddScoped<IRiskRule, CriticalCustomerRule>();
        services.AddScoped<IRiskEngine, RiskEngine>();
        services.AddScoped<IRiskThresholdService, RiskThresholdService>();

        return services;
    }
}
