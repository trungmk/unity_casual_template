using System;

namespace Core
{
    public interface IEventManager
    {
        void AddListener<T>(Action<T> action) where T : IEvent;
        void RemoveListener<T>(Action<T> action) where T : IEvent;
        void Dispatch<T>(T eventData) where T : IEvent;
    }
} 