using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;

namespace timrlink.net.Core
{
    public abstract class Application
    {
        private readonly IServiceProvider serviceProvider;

        protected ILoggerFactory LoggerFactory => GetService<ILoggerFactory>();
        protected IUserService UserService => GetService<IUserService>();
        protected ITaskService TaskService => GetService<ITaskService>();
        protected IWorkTimeService WorkTimeService => GetService<IWorkTimeService>();
        protected IProjectTimeService ProjectTimeService => GetService<IProjectTimeService>();
        protected IConfiguration Configuration => GetService<IConfiguration>();

        public ILogger Logger { get; }

        protected Application()
        {
            var confBuilder = new ConfigurationBuilder();
            SetupConfiguration(confBuilder);
            var configuration = confBuilder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            serviceProvider = serviceCollection
                .AddSingleton<IConfiguration>(configuration)
                .AddLogging(builder => ConfigureLogger(builder, configuration))
                .AddTimrLink()
                .AddScoped<LoggingEndpointBehaviour>()
                .AddScoped<LoggingMessageInspector>()
                .BuildServiceProvider();

            Logger = serviceProvider.GetService<ILogger<Application>>();
        }

        public abstract Task<int> Run();

        protected virtual void SetupConfiguration(IConfigurationBuilder configurationBuilder)
        {
        }

        protected virtual void ConfigureLogger(ILoggingBuilder loggingBuilder, IConfigurationRoot configuration)
        {
        }

        protected virtual void ConfigureServices(IServiceCollection serviceCollection)
        {
        }

        protected T GetService<T>() => serviceProvider.GetService<T>();
    }
}
