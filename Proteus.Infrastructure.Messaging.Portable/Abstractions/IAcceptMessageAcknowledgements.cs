namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface IAcceptMessageAcknowledgements
    {
        void Acknowledge<TEnvelope>(TEnvelope envelope) where TEnvelope : Envelope<IMessage>;
    }
}