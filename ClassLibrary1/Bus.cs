using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using ClassLibrary1.Abstractions;

namespace ClassLibrary1
{
    public class Bus : ISendCommand, IPublishEvent
    {
        private readonly Dictionary<Type, List<Action<IMessage>>> _routes = new Dictionary<Type, List<Action<IMessage>>>();

        public void RegisterHandlerFor<TMessage>(Action<TMessage> handler) where TMessage : IMessage
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
                if (handlers.Count != 1) throw new DuplicateHandlerRegisteredException("cannot send to more than one handler");
                handlers[0](command);
            }
            else
            {
                throw new NoHandlerRegisteredException("no handler registered");
            }
        }

        public void Publish<T>(T @event) where T : Event
        {
            List<Action<IMessage>> handlers;
            if (!_routes.TryGetValue(@event.GetType(), out handlers)) return;
            foreach (var handler in handlers)
            {
                var handler1 = handler;
                
                //threadpool dispatch disabled
                //ThreadPool.QueueUserWorkItem(x => handler1(@event));
                handler1(@event);
            }
        }
    }

    public class NoHandlerRegisteredException : InvalidOperationException
    {
        public NoHandlerRegisteredException()
        {
        }

        public NoHandlerRegisteredException(string message) : base(message)
        {
        }

        public NoHandlerRegisteredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoHandlerRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class DuplicateHandlerRegisteredException : InvalidOperationException
    {
        public DuplicateHandlerRegisteredException()
        {
        }

        public DuplicateHandlerRegisteredException(string message)
            : base(message)
        {
        }

        public DuplicateHandlerRegisteredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DuplicateHandlerRegisteredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}