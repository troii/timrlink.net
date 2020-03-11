using System;
using System.ServiceModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using timrlink.net.Core.API;
using timrlink.net.Core.Service;

namespace timrlink.net.Core
{
    public static class ServiceCollectionExtensions
    {
        private const string DEFAULT_HOST = "http://timrsync.timr.com/timr";

        public static IServiceCollection AddTimrLink(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddScoped<ITaskService, TaskService>()
                .AddScoped<IWorkTimeService, WorkTimeService>()
                .AddScoped<IProjectTimeService, ProjectTimeService>()
                .AddScoped<IUserService, UserService>()
                .AddScoped(serviceProvider => BindTimrSync(serviceProvider));
        }

        private static TimrSync BindTimrSync(IServiceProvider provider)
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            
            var host = configuration["timrSync:host"] ?? DEFAULT_HOST;
            var identifier = configuration["timrSync:identifier"];
            var token = configuration["timrSync:token"];
            bool debug = Boolean.TryParse(configuration["timrSync:debug"], out debug) && debug;

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
