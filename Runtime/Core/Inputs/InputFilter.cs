using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Core
{
    public class InputFilter : IInputFilter
    {
        private readonly IList<IMobileInputHandler> _inputHandlers = new List<IMobileInputHandler>();

        public void AddMobileInputFilter(IMobileInputHandler inputHandler)
        {
            if (inputHandler == null)
            {
                throw new ArgumentNullException(nameof(inputHandler), "[InputFilter] InputHandler is null");
            }

            if (!_inputHandlers.Contains(inputHandler))
            {
                _inputHandlers.Add(inputHandler);
            }
        }

        public void RemoveMobileInputFilter(IMobileInputHandler inputHandler)
        {
            if (inputHandler == null)
            {
                throw new ArgumentNullException(nameof(inputHandler), "[InputFilter] InputHandler is null");
            }

            if (_inputHandlers.Contains(inputHandler))
            {
                _inputHandlers.Remove(inputHandler);
            }
        }

        public void OnUserPress(Vector3 target)
        {
            for (int i = 0; i < _inputHandlers.Count; i++)
            {
                if (_inputHandlers[i] != null && _inputHandlers[i].IsInputEnabled)
                {
                    _inputHandlers[i].OnTouchDown(target);
                }
            }
        }

        public void OnUserRelease(Vector3 target)
        {
            for (int i = 0; i < _inputHandlers.Count; i++)
            {
                if (_inputHandlers[i] != null && _inputHandlers[i].IsInputEnabled)
                {
                    _inputHandlers[i].OnTouchUp(target);
                }
            }
        }

        public void OnUserDrag(Vector3 target)
        {
            for (int i = 0; i < _inputHandlers.Count; i++)
            {
                if (_inputHandlers[i] != null && _inputHandlers[i].IsInputEnabled)
                {
                    _inputHandlers[i].OnDrag(target);
                }
            }
        }
    }
} 