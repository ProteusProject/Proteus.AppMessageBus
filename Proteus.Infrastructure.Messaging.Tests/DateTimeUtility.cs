using System;

namespace Proteus.Infrastructure.Messaging.Tests
{
    public static class DateTimeUtility
    {
        public static TimeSpan Positive_OneHourTimeSpan()
        {
            return new TimeSpan(1, 0, 0);
        }

        public static TimeSpan Negative_OneHourTimeSpan()
        {
            return new TimeSpan(-1, 0, 0);
        }
    }
}