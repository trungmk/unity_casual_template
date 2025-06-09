using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public interface ILocalDataManager
    {
        Action OnLoadDataCompleted { get; set; }
        
        void Init();

        void SaveData<T>() where T : ILocalData;

        void SaveData<T>(object obj, bool suppressErrors = false) where T : ILocalData;


        T GetData<T>() where T : ILocalData;
    }
}


