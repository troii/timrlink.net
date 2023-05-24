using System;
using System.Runtime.Serialization;

namespace timrlink.net.Core
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException()
        {
        }

        protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ConfigurationException(string message) : base(message)
        {
        }

        public ConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    
    public class MissingConfigurationException : ConfigurationException
    {
        public MissingConfigurationException(string section, string parameter)
            : base($"Missing parameter '{parameter}' in section '{section}'.")
        {
        }
    }
}
