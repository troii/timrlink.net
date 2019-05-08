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
                .AddLogging()
                .AddScoped<ITaskService, TaskService>()
                .AddScoped<IWorkTimeService, WorkTimeService>()
                .AddScoped<IProjectTimeService, ProjectTimeService>()
                .AddSingleton(BindTimrSync(configuration))
                .AddSingleton<IConfiguration>(configuration)
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            ConfigureLogger(loggerFactory);

            Logger = loggerFactory.CreateLogger(GetType());
        }

        public abstract Task Run();

        protected virtual void SetupConfiguration(IConfigurationBuilder configurationBuilder)
        {
        }

        protected virtual void ConfigureLogger(ILoggerFactory loggerFactory)
        {
        }

        protected virtual void ConfigureServices(IServiceCollection serviceCollection)
        {
        }

        protected T GetService<T>() => serviceProvider.GetService<T>();

        private static TimrSync BindTimrSync(IConfiguration configuration)
        {
            var host = configuration["timrSync:host"] ?? DEFAULT_HOST;
            var identifier = configuration["timrSync:identifier"];
            var token = configuration["timrSync:token"];

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

            return channelFactory.CreateChannel();
        }
    }
}
