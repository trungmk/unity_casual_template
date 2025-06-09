using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public interface ILocalDataWrapper
    {
        void Init();

        void SaveData<T>() where T : ILocalData;
        
        void SaveData<T>(object obj) where T : ILocalData;
        
        T GetData<T>() where T : ILocalData;
    }
}

