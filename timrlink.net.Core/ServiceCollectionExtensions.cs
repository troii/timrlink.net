using Microsoft.Extensions.DependencyInjection;
using timrlink.net.Core.Service;

namespace timrlink.net.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTimrLink(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddTransient<IUserService, UserService>()
                .AddTransient<IWorkItemService, WorkItemService>()
                .AddTransient<IWorkTimeService, WorkTimeService>()
                .AddTransient<ITaskService, TaskService>()
                .AddTransient<IProjectTimeService, ProjectTimeService>()
                .AddTransient<IGroupService, GroupService>()
                .AddTransient(provider => ServiceFactory.BuildTimrSync(provider))
                .AddTransient<LoggingEndpointBehaviour>()
                .AddTransient<LoggingMessageInspector>();
        }
    }
}
