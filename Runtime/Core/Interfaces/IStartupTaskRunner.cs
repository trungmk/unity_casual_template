using System;

namespace Core
{
    public interface IStartupTaskRunner
    {
        void Init();
        event Action OnInitializeCompleted;
    }
} 