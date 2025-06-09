using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public abstract class GameDefinitionAssetWrapper<T> : ScriptableObject, IGameDefinitionWrapper where T : IGameDefinition
    {
        public List<T> Objects = new List<T>();

        public virtual void Init<T1>(List<T1> value) where T1 : IGameDefinition
        {
            
        }

        public void Init()
        {
            
        }

        public List<T1> GetDefinitions<T1>() where T1 : IGameDefinition
        {
            return Objects as List<T1>;
        }
        
        public Type GetGameDefinitionType()
        {
            return typeof(T);
        }
    }
}