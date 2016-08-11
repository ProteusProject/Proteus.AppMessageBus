using System;

namespace Proteus.Infrastructure.Messaging.Tests
{
    public static class DateTimeUtility
    {
        public static TimeSpan PositiveOneHourTimeSpan
        {
            get { return TimeSpan.FromHours(1); }
        }

        public static TimeSpan NegativeOneHourTimeSpan
        {
            get { return TimeSpan.FromHours(-1); }
        }
    }
}