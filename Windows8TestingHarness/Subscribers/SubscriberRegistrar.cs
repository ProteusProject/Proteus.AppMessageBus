using Proteus.Infrastructure.Messaging.Portable;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;
using TestingHarness.Portable.Messages;

namespace Windows8TestingHarness.Subscribers
{
    public class SubscriberRegistrar
    {
        private readonly IRegisterMessageSubscriptions _messageBus;

        public SubscriberRegistrar(IRegisterMessageSubscriptions messageBus)
        {
            _messageBus = messageBus;
        }

        public void RegisterMessageBusSubscribers()
        {
            _messageBus.RegisterSubscriptionFor<ChangeNameCommand>(new ChangeNameCommandHandler().Handle);
            _messageBus.RegisterSubscriptionFor<NameChangedEvent>(new NameChangedEventViewModelHandler().Handle);
            _messageBus.RegisterSubscriptionFor<NameChangedEvent>(new NameChangedEventPersistenceHandler().Handle);

            _messageBus.RegisterSubscriptionFor<IncrementCounterWithAckCommand>(new IncrementCounterCommandHandler().Handle);
            _messageBus.RegisterSubscriptionFor<IncrementCounterWithoutAckCommand>(new IncrementCounterCommandHandler().Handle);

            _messageBus.RegisterSubscriptionFor<CounterIncrementedWithAckEvent>(new CounterIncrementedViewModelEventHandler().Handle);
            _messageBus.RegisterSubscriptionFor<CounterIncrementedWithoutAckEvent>(new CounterIncrementedViewModelEventHandler().Handle);
        }
    }
}