namespace ClassLibrary1.Abstractions
{
    public interface IPublishEvent
    {
        void Publish<TEvent>(TEvent @event) where TEvent : Event;
    }
}