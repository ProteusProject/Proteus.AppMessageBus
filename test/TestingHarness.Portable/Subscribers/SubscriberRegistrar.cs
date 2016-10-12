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

using Proteus.AppMessageBus.Portable;
using TestingHarness.Portable.Abstractions;
using TestingHarness.Portable.Messages;

namespace TestingHarness.Portable.Subscribers
{
    public class SubscriberRegistrar
    {
        private readonly DurableMessageBus _messageBus;
        private readonly IManageViewModels _modelManager;

        public SubscriberRegistrar(DurableMessageBus messageBus, IManageViewModels modelManager)
        {
            _messageBus = messageBus;
            _modelManager = modelManager;
        }

        public void RegisterMessageBusSubscribers()
        {
            _messageBus.RegisterSubscriptionFor<ChangeNameCommand>(new ChangeNameCommandHandler(_messageBus).Handle);
            _messageBus.RegisterSubscriptionFor<NameChangedEvent>(new NameChangedEventViewModelHandler(_modelManager).Handle);
            _messageBus.RegisterSubscriptionFor<NameChangedEvent>(new NameChangedEventPersistenceHandler().Handle);

            var incrementCounterCommandHandler = new IncrementCounterCommandHandler(_messageBus);
            _messageBus.RegisterSubscriptionFor<IncrementCounterWithAckCommand>(incrementCounterCommandHandler.Handle);
            _messageBus.RegisterSubscriptionFor<IncrementCounterWithoutAckCommand>(incrementCounterCommandHandler.Handle);

            var counterIncrementedViewModelEventHandler = new CounterIncrementedViewModelEventHandler(_messageBus, _modelManager);
            _messageBus.RegisterSubscriptionFor<CounterIncrementedWithAckEvent>(counterIncrementedViewModelEventHandler.Handle);
            _messageBus.RegisterSubscriptionFor<CounterIncrementedWithoutAckEvent>(counterIncrementedViewModelEventHandler.Handle);
        }
    }
}