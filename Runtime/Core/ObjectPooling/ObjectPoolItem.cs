using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core
{
    public enum PoolItemType
    {
        None = 0,
        General,
        Obstacle,
        Environment,
        Ground,
        FX,
        Enemy,
        Player,
        UI
    }

    [Serializable]
    public class PoolItem
    {
        public GameObject GameObjectToPool;
        public PoolItemType PoolType = PoolItemType.General;
        public string AddressName;
        public int AmountToPool = 0;
        public bool NeedModifyAfterInit;
    }
}

