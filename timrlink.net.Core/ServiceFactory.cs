using System;
using System.ServiceModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;

namespace timrlink.net.Core
{
    public class ServiceFactory
    {
        private const string DEFAULT_HOST = "https://timrsync.timr.com/timr";

        public static TimrSync BuildTimrSync(IServiceProvider provider, string sectionName = "timrSync")
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var timrSyncSection = configuration.GetSection(sectionName);

            var host = timrSyncSection["host"] ?? DEFAULT_HOST;
            var identifier = timrSyncSection["identifier"];
            var token = timrSyncSection["token"];
            bool debug = bool.TryParse(timrSyncSection["debug"], out debug) && debug;

            if (identifier == null)
            {
                throw new MissingConfigurationException(sectionName, "identifier");
            }

            if (token == null)
            {
                throw new MissingConfigurationException(sectionName, "token");
            }

            var endpoint = new EndpointAddress(new Uri(new Uri(host + "/"), "timrsync"));

            var binding = new BasicHttpBinding
            {
                Security =
                {
                    Mode = endpoint.Uri.Scheme == "https" ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.TransportCredentialOnly,
                    Transport =
                    {
                        ClientCredentialType = HttpClientCredentialType.Basic
                    }
                },
                MaxBufferSize = int.MaxValue,
                ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max,
                MaxReceivedMessageSize = int.MaxValue,
                AllowCookies = true
            };

            var channelFactory = new ChannelFactory<TimrSync>(binding, endpoint);
            channelFactory.Credentials.UserName.UserName = identifier;
            channelFactory.Credentials.UserName.Password = token;

            if (debug)
            {
                channelFactory.Endpoint.EndpointBehaviors.Add(provider.GetRequiredService<LoggingEndpointBehaviour>());
            }

            return channelFactory.CreateChannel();
        }

        public static IUserService BuildUserService(ILoggerFactory loggerFactory, TimrSync timrSync)
        {
            return new UserService(loggerFactory.CreateLogger<UserService>(), timrSync);
        }

        public static ITaskService BuildTaskService(ILoggerFactory loggerFactory, TimrSync timrSync)
        {
            return new TaskService(loggerFactory.CreateLogger<TaskService>(), loggerFactory, timrSync);
        }

        public static IProjectTimeService BuildProjectTimeService(ILoggerFactory loggerFactory, TimrSync timrSync)
        {
            return new ProjectTimeService(loggerFactory.CreateLogger<ProjectTimeService>(), timrSync);
        }
        
        public static IWorkTimeService BuildWorkTimeService(ILoggerFactory loggerFactory, TimrSync timrSync)
        {
            return new WorkTimeService(loggerFactory.CreateLogger<WorkTimeService>(), timrSync);
        }

        public static IWorkItemService BuildWorkItemService(ILoggerFactory loggerFactory, TimrSync timrSync)
        {
            return new WorkItemService(loggerFactory.CreateLogger<WorkItemService>(), timrSync);
        }
    }
}
