using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Store data that serialized to JSON.
    /// </summary>
    public interface ILocalData
    {
        void InitAfterLoadData();
    }
}


