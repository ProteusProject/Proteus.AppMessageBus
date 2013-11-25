using System;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable.Serializable
{
    public class EvenvelopeState<TMessage> where TMessage : IMessageTx
    {
        public int SubscriberIndex { get; set; }
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