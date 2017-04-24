using System;

namespace timrlink.net.Core
{
    public class MissingConfigurationException : Exception
    {
        public MissingConfigurationException(string section, string parameter)
            : base($"Missing parameter '{parameter}' in section '{section}'.")
        {
        }
    }
}