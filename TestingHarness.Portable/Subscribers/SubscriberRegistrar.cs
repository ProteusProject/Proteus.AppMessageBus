using Proteus.Infrastructure.Messaging.Portable;
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