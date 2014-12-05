using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Proteus.Infrastructure.Messaging.Tests
{
    public static class AssertEx
    {
        public static async Task ThrowsAsync<TException>(Func<Task> func) where TException : Exception
        {
            try
            {
                await func();
            }
            catch (TException ex)
            {
                Assert.Pass(string.Format("Got expected exception: {0} ", typeof(TException).Name ));
            }
            catch (Exception)
            {
                Assert.Fail(string.Format("Did not get expected exception: {0} ", typeof(TException).Name));

            }
        }
    }
}