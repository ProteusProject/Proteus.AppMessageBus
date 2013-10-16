using System;
using System.Collections.Generic;
using System.Threading;
using ClassLibrary1.Abstractions;

namespace ClassLibrary1
{
    public class Bus : ISendCommand, IPublishEvent
    {
        private readonly Dictionary<Type, List<Action<IMessage>>> _routes = new Dictionary<Type, List<Action<IMessage>>>();

        public void RegisterHandler<TMessage>(Action<TMessage> handler) where TMessage : IMessage
        {
            List<Action<IMessage>> handlers;
            if (!_routes.TryGetValue(typeof(TMessage), out handlers))
            {
                handlers = new List<Action<IMessage>>();
                _routes.Add(typeof(TMessage), handlers);
            }
            handlers.Add(DelegateAdjuster.CastArgument<IMessage, TMessage>(x => handler(x)));
        }

        public void Send<T>(T command) where T : Command
        {
            List<Action<IMessage>> handlers;
            if (_routes.TryGetValue(typeof(T), out handlers))
            {
                if (handlers.Count != 1) throw new InvalidOperationException("cannot send to more than one handler");
                handlers[0](command);
            }
            else
            {
                throw new InvalidOperationException("no handler registered");
            }
        }

        public void Publish<T>(T @event) where T : Event
        {
            List<Action<IMessage>> handlers;
            if (!_routes.TryGetValue(@event.GetType(), out handlers)) return;
            foreach (var handler in handlers)
            {
                //dispatch on thread pool for added awesomeness
                var handler1 = handler;
                ThreadPool.QueueUserWorkItem(x => handler1(@event));
            }
        }
    }
}