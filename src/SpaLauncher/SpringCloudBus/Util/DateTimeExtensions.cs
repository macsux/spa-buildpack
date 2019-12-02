using System;

namespace SpaLauncher.SpringCloudBus.Util
{
    public static class DateTimeExtensions
    {
        static DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToEpochMillisececonds(this DateTime value) => (long) ((DateTime) value - _epoch).TotalMilliseconds;
        public static DateTime FromEpochMillisececonds(long value) => DateTimeOffset.FromUnixTimeMilliseconds(value).UtcDateTime;
    }
}