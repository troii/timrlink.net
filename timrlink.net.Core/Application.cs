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

namespace timrlink.net.Core
{
    public abstract class Application
    {
        private const string DEFAULT_HOST = "http://timrsync.timr.com/timr";

        private readonly IServiceProvider serviceProvider;

        protected ITaskService TaskService => serviceProvider.GetService<ITaskService>();
        protected IWorkTimeService WorkTimeService => serviceProvider.GetService<IWorkTimeService>();
        protected IProjectTimeService ProjectTimeService => serviceProvider.GetService<IProjectTimeService>();
        protected ILoggerFactory LoggerFactory => serviceProvider.GetService<ILoggerFactory>();

        protected IConfigurationRoot Configuration { get; }

        public ILogger Logger { get; }

        protected Application()
        {
            var confBuilder = new ConfigurationBuilder();
            Configuration = SetupConfiguration(confBuilder);

            serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddScoped<ITaskService, TaskService>()
                .AddScoped<IWorkTimeService, WorkTimeService>()
                .AddScoped<IProjectTimeService, ProjectTimeService>()
                .AddSingleton(BindTimrSync(Configuration))
                .BuildServiceProvider();

            ConfigureLogger(LoggerFactory);
            Logger = LoggerFactory.CreateLogger(GetType());
        }

        public abstract void Run();

        public virtual IConfigurationRoot SetupConfiguration(IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.Build();
        }

        public virtual void ConfigureLogger(ILoggerFactory loggerFactory)
        {
        }

        private TimrSync BindTimrSync(IConfigurationRoot configuration)
        {
            var host = configuration.GetSection("timrSync:host")?.Value ?? DEFAULT_HOST;
            var identifier = configuration.GetSection("timrSync:identifier")?.Value;
            var token = configuration.GetSection("timrSync:token")?.Value;

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