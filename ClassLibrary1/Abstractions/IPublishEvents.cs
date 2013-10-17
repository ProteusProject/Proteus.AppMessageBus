namespace ClassLibrary1.Abstractions
{
    public interface IPublishEvents
    {
        void Publish<TEvent>(TEvent @event) where TEvent : Event;
    }
}