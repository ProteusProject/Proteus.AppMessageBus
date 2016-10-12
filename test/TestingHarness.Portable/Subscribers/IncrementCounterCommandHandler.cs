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
using Proteus.AppMessageBus.Portable;
using Proteus.AppMessageBus.Portable.Abstractions;
using TestingHarness.Portable.Messages;

namespace TestingHarness.Portable.Subscribers
{
    public class IncrementCounterCommandHandler 
        : IHandleDurable<IncrementCounterWithAckCommand>, IHandleDurable<IncrementCounterWithoutAckCommand>
    {

        private readonly DurableMessageBus _bus;

        public IncrementCounterCommandHandler(DurableMessageBus bus)
        {
            _bus = bus;
        }

        public async void Handle(IncrementCounterWithAckCommand message)
        {
            //publish the event with some retries and a future expiry
            await _bus.PublishDurable(new CounterIncrementedWithAckEvent(), new RetryPolicy(3, TimeSpan.FromHours(1)));

            //now that event(s) are safely published, acknowledge the command
            await _bus.Acknowledge(message);
        }

        public async void Handle(IncrementCounterWithoutAckCommand message)
        {
            //publish the event with some retries and a future expiry
            await _bus.PublishDurable(new CounterIncrementedWithoutAckEvent(), new RetryPolicy(3, TimeSpan.FromHours(1)));

            //now that event(s) are safely published, acknowledge the command
            await _bus.Acknowledge(message);
        }
    }
}