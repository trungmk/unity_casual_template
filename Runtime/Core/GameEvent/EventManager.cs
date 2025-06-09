using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>  
    /// Optimize Event bus for Unity
    /// </summary>  
    public class EventManager : IEventManager
    {
        private readonly Dictionary<Type, List<Delegate>> _listeners = new();
        private readonly List<Action> _pendingActions = new();
 
        private int _isDispatching = 0;

        public void AddListener<T>(Action<T> action) where T : IEvent
        {
            var key = typeof(T);
            if (!_listeners.TryGetValue(key, out var list))
            {
                list = new List<Delegate>();
                _listeners[key] = list;
            }
            if (_isDispatching > 0)
            {
                _pendingActions.Add(() => list.Add(action));
            }
            else
            {
                // Prevent to add duplicate action
                if (!list.Contains(action))
                {
                    list.Add(action);
                }
            }
        }

        public void RemoveListener<T>(Action<T> action) where T : IEvent
        {
            var key = typeof(T);
            if (_listeners.TryGetValue(key, out var list))
            {
                if (_isDispatching > 0)
                {
                    _pendingActions.Add(() => list.Remove(action));
                }
                else
                {
                    list.Remove(action);
                }
            }
        }

        public void Dispatch<T>(T eventData) where T : IEvent
        {
            var key = typeof(T);
            if (!_listeners.TryGetValue(key, out var list) || list.Count == 0)
            {
                eventData?.Reset();

                return;
            }

            _isDispatching++;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] is Action<T> action && action != null)
                {
                    try
                    {
                        action.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventManager] Lỗi khi dispatch event {key}: {ex}");
                    }
                }
            }

            _isDispatching--;

            if (_isDispatching == 0)
                RunPendingActions();

            eventData?.Reset();
        }

        private void RunPendingActions()
        {
            foreach (var action in _pendingActions)
                action?.Invoke();
            _pendingActions.Clear();
        }

        public void ClearAll()
        {
            _listeners.Clear();
            _pendingActions.Clear();
            _isDispatching = 0;
        }
    }
}