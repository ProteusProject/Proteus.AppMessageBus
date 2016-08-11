using System;
using Proteus.AppMessageBus.Portable.Abstractions;

namespace Proteus.AppMessageBus.Portable.Serializable
{
    public class EvenvelopeState<TMessage> where TMessage : IDurableMessage
    {
        public string SubscriberKey { get; set; }
        public Guid Id { get; set; }
        public int RetriesRemaining { get; set; }
        public TMessage Message { get; set; }
        public RetryPolicyState RetryPolicyState { get; set; }
        public Guid AcknowledgementId { get; set; }
        
        public Envelope<TMessage> GetEnvelope()
        {
            return new Envelope<TMessage>(this);
        }
    }
}