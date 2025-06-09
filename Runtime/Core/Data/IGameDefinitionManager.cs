using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public interface IGameDefinitionManager
    {
        void Init();

        void Init(IDictionary<string, object> values);
        
        List<T> GetDefinitions<T>() where T : IGameDefinition;

        event Action OnInitCompleted;
    }
}


