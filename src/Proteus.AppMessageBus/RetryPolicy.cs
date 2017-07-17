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
using Proteus.AppMessageBus.Serializable;

namespace Proteus.AppMessageBus
{
    public class RetryPolicy
    {
        public int Retries { get; private set; }
        public DateTime Expiry { get; private set; }

        public RetryPolicyState RetryPolicyState
        {
            get
            {
                return new RetryPolicyState() { Retries = Retries, Expiry = Expiry };
            }
        }


        public RetryPolicy()
            : this(0, TimeSpan.Zero)
        {
        }

        public RetryPolicy(int retries, TimeSpan durationUntilExpiry)
        {
            Expiry = DateTime.UtcNow + durationUntilExpiry;
            Retries = retries;
        }

        public RetryPolicy(RetryPolicyState state)
        {
            Retries = state.Retries;
            Expiry = state.Expiry;
        }
    }
}