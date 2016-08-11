using System;

namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public class SerializerResult<TResult>
    {
        private readonly TResult _value;
        
        public bool HasValue { get; private set; }
        
        public TResult Value
        {
            get
            {
                if (HasValue)
                {
                    return _value;
                }
                else
                {
                    throw new InvalidOperationException("Invalid attempt to access the \"Value\" property.  You cannot access the \"Value\" property if the \"HasValue\" property is False.");
                }
            }
            
        }

        public Exception Exception { get; private set; }

        public SerializerResult(TResult value, bool hasValue, Exception exception = null)
        {
            _value = value;
            HasValue = hasValue;
            Exception = exception;
        }
    }
}