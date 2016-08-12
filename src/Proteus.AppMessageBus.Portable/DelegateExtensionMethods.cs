using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Proteus.AppMessageBus.Portable
{
    public static class DelegateExtensionMethods
    {
        public static bool CanBeAwaited(this Delegate theDelegate)
        {
            var target = theDelegate.Target;
            var targetTypeInfo = target.GetType().GetTypeInfo();
            var matchingDeclaredMethods = targetTypeInfo.DeclaredMethods.Where(m => m.Name == theDelegate.GetMethodInfo().Name).ToArray();

            if (!matchingDeclaredMethods.Any())
            {
                return false;
            }

            var methodInfo = matchingDeclaredMethods.First();

            var returnTypeInfo = methodInfo.ReturnType.GetTypeInfo();

            if (returnTypeInfo.IsGenericType)
            {
                return returnTypeInfo.GetGenericTypeDefinition() == typeof(Task<>);
            }

            return returnTypeInfo.AsType() == typeof(Task);
        }
    }
}