using System;
using System.Collections.Generic;

namespace Core
{
    public interface IGameDefinitionWrapper
    {
        void Init<T>(List<T> value) where T : IGameDefinition;
        
        void Init();
        
        List<T> GetDefinitions<T>() where T : IGameDefinition;

        Type GetGameDefinitionType();
    }
}