using System;
using timrlink.net.Core.API;

namespace timrlink.net.CLI.Extensions
{
    /// <summary>
    /// Converversions from DateTime to DateTimeOffset
    /// </summary>
    public static class RecordExtensions
    {
        /// <summary>
        /// Get start time as DateTimeOffset
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static DateTimeOffset GetStartTimeOffset(this Record record)
        {
            return new DateTimeOffset(new DateTimeOffset(record.startTime).UtcDateTime).ToOffset(
                TimeSpan.Parse(record.startTimeZone.Trim('+')));
        }

        /// <summary>
        /// Get end time as DateTimeOffset
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static DateTimeOffset GetEndTimeOffset(this Record record)
        {
            return new DateTimeOffset(new DateTimeOffset(record.endTime).UtcDateTime).ToOffset(
                TimeSpan.Parse(record.endTimeZone.Trim('+')));
        }

        /// <summary>
        /// Get last modified as DateTimeOffset
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static DateTimeOffset GetLastModifiedOffset(this Record record)
        {
            return new DateTimeOffset(new DateTimeOffset(record.lastModifiedTime).UtcDateTime).ToOffset(
                TimeSpan.Parse(record.lastModifiedTimeZone.Trim('+')));
        }
    }
}