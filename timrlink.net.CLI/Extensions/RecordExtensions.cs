using System;
using timrlink.net.Core.API;

public static class RecordExtensions
{
    public static DateTimeOffset GetStartTimeOffset(this Record record)
    {
        return new DateTimeOffset(new DateTimeOffset(record.startTime).UtcDateTime).ToOffset(
            TimeSpan.Parse(record.startTimeZone.Trim('+')));
    }

    public static DateTimeOffset GetEndTimeOffset(this Record record)
    {
        return new DateTimeOffset(new DateTimeOffset(record.endTime).UtcDateTime).ToOffset(
            TimeSpan.Parse(record.endTimeZone.Trim('+')));
    }

    public static DateTimeOffset GetLastModifiedOffset(this Record record)
    {
        return new DateTimeOffset(new DateTimeOffset(record.lastModifiedTime).UtcDateTime).ToOffset(
            TimeSpan.Parse(record.lastModifiedTimeZone.Trim('+')));
    }
}