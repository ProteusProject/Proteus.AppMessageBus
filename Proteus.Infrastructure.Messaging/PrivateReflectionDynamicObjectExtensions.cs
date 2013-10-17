using System.Diagnostics;

namespace Proteus.Infrastructure.Messaging
{
    [DebuggerStepThrough]
    public static class PrivateReflectionDynamicObjectExtensions
    {
        public static dynamic AsDynamic(this object o)
        {
            return PrivateReflectionDynamicObject.WrapObjectIfNeeded(o);
        }
    }
}