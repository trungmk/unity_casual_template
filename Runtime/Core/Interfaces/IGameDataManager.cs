using System;
using System.Collections.Generic;

namespace Core
{
    public interface IGameDataManager
    {
        bool IsInitialized { get; }
        IGameDefinitionManager GameDefinitionManager { get; }
        void InitLocalData();
        List<T> LoadGameDefinition<T>() where T : IGameDefinition;
        T LoadLocalData<T>() where T : ILocalData;
        void SaveLocalData<T>() where T : ILocalData;
        void SaveLocalData<T>(object obj) where T : ILocalData;
        
        event Action OnLoadDataCompleted;
        event Action OnLoadLocalDataCompleted;
    }
} 