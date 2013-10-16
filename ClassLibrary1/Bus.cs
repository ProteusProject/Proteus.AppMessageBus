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

        public void Send<TCommand>(TCommand command) where TCommand : Command
        {
            List<Action<IMessage>> handlers;
            if (_routes.TryGetValue(typeof(TCommand), out handlers))
            {
                if (handlers.Count != 1) throw new DuplicateHandlerRegisteredException(string.Format("There are {0} handlers registered for Command of type {1}.  Each Command must have exacty one handler registered.", handlers.Count, typeof(TCommand)));
                handlers[0](command);
            }
            else
            {
                throw new NoHandlerRegisteredException(string.Format("No handler registered for Command of type {0}.  Each Command must have exacty one handler registered.", typeof(TCommand)));
            }
        }

        public void Publish<TEvent>(TEvent @event) where TEvent : Event
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