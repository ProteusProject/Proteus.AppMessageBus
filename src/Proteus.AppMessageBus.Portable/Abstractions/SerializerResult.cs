#region License

/*
 * Copyright © 2013-2016 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

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