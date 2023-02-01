using System;

namespace timrlink.net.CLI.Actions
{
    internal struct DateSpan
    {
        public DateSpan(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }

        public DateTime From { get; }
        
        public DateTime To { get; }
    }
}
