using System;

namespace Proteus.Infrastructure.Messaging.Tests
{
    public static class TestingDateTimeProviderUtility
    {
         public static DateTime OneYearFromNowUtc()
         {
             return new DateTime(DateTime.UtcNow.Year + 1, DateTime.UtcNow.Month, DateTime.UtcNow.Day);
         }
    }
}