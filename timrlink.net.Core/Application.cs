using System.Linq;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;
using Task = System.Threading.Tasks.Task;

namespace timrlink.net.Core
{
    public abstract class Application
    {
        private const string DEFAULT_HOST = "http://timrsync.timr.com/timr";

        private readonly IServiceProvider serviceProvider;

        protected ILoggerFactory LoggerFactory => GetService<ILoggerFactory>();
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
                .AddScoped<ITaskService, TaskService>()
                .AddScoped<IWorkTimeService, WorkTimeService>()
                .AddScoped<IProjectTimeService, ProjectTimeService>()
                .AddScoped(serviceProvider => BindTimrSync(configuration, serviceProvider))
                .AddScoped<LoggingEndpointBehaviour>()
                .AddScoped<LoggingMessageInspector>()
                .BuildServiceProvider();

            Logger = serviceProvider.GetService<ILogger<Application>>();
        }

        public abstract Task Run();

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

        private static TimrSync BindTimrSync(IConfiguration configuration, IServiceProvider provider)
        {
            var host = configuration["timrSync:host"] ?? DEFAULT_HOST;
            var identifier = configuration["timrSync:identifier"];
            var token = configuration["timrSync:token"];
            bool debug = Boolean.TryParse(configuration["debug"], out debug) && debug;

            if (identifier == null)
            {
                throw new MissingConfigurationException("timrSync", "identifier");
            }

            if (token == null)
            {
                throw new MissingConfigurationException("timrSync", "token");
            }

            var endpoint = new EndpointAddress(new Uri(new Uri(host + "/"), "timrsync"));

            var binding = new BasicHttpBinding();
            binding.Security.Mode = endpoint.Uri.Scheme == "https" ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.TransportCredentialOnly;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            binding.MaxBufferSize = int.MaxValue;
            binding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.AllowCookies = true;

            var channelFactory = new ChannelFactory<TimrSync>(binding, endpoint);
            channelFactory.Credentials.UserName.UserName = identifier;
            channelFactory.Credentials.UserName.Password = token;

            if (debug)
            {
                channelFactory.Endpoint.EndpointBehaviors.Add(provider.GetRequiredService<LoggingEndpointBehaviour>());
            }

            return channelFactory.CreateChannel();
        }
    }
}
