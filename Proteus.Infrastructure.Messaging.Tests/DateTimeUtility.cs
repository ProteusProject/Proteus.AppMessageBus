using System;

namespace Proteus.Infrastructure.Messaging.Tests
{
    public static class DateTimeUtility
    {
        public static TimeSpan PositiveOneHourTimeSpan
        {
            get { return new TimeSpan(1, 0, 0); }
        }

        public static TimeSpan NegativeOneHourTimeSpan
        {
            get { return new TimeSpan(-1, 0, 0); }
        }
    }
}