using System;
using System.IO;
using UnityEngine;
using ProtoBuf;
using Cysharp.Threading.Tasks;

namespace Core
{
    public class LocalDataProtoBufWrapper<T> : ILocalDataWrapper where T : ILocalData, new()
    {
        private const string LOCAL_DATA_FOLDER_NAME = "Profiles";
        private const string LOCAL_DATA_FILE_NAME_TEMPLATE = "{0}.dat";
        private const string BACKUP_FOLDER = "Backups";

        private T _obj;
        private string _gameDataFolderPath = string.Empty;
        private IEncryptionService _encryptionService;

        public LocalDataProtoBufWrapper(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public virtual void Init()
        {
            try
            {
                _gameDataFolderPath = GetProfileFolderPath();
                EnsureDirectoriesExist();
                string dataFilePath = GetLocalDataPath(typeof(T).Name);

                if (File.Exists(dataFilePath))
                {
                    _obj = LoadData<T>(dataFilePath);
                }
                else
                {
                    _obj = new T();
                    _obj.InitAfterLoadData();
                    SaveData<T>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error initializing {typeof(T).Name} data: {ex.Message}");
                throw;
            }
        }

        private void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(_gameDataFolderPath);
            Directory.CreateDirectory(Path.Combine(_gameDataFolderPath, BACKUP_FOLDER));
        }

        public async UniTask SaveDataAsync<T1>() where T1 : ILocalData
        {
            if (_obj == null)
            {
                Debug.LogWarning("Attempting to save null data");
                return;
            }

            await UniTask.RunOnThreadPool(() =>
            {
                try
                {
                    string filePath = GetLocalDataPath(typeof(T1).Name);
                    byte[] serializedData;

                    using (var ms = new MemoryStream())
                    {
                        Serializer.Serialize(ms, _obj);
                        serializedData = ms.ToArray();
                    }

                    string base64Data = Convert.ToBase64String(serializedData);
                    string encryptedData = _encryptionService.Encrypt(base64Data);
                    File.WriteAllText(filePath, encryptedData);
                    Debug.Log($"Async save completed for {typeof(T1).Name}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Async save failed: {ex.Message}");
                    throw;
                }
            });
        }

        public void SaveData<T1>() where T1 : ILocalData
        {
            SaveDataAsync<T1>().Forget();
        }

        public virtual void SaveData<T1>(object obj) where T1 : ILocalData
        {
            if (obj is T data)
            {
                _obj = data;
                SaveData<T1>();
            }
        }

        public T1 GetData<T1>() where T1 : ILocalData
        {
            if (_obj is T1 data)
            {
                return data;
            }

            return default;
        }

        private string GetProfileFolderPath()
        {
            return Path.Combine(Application.persistentDataPath, LOCAL_DATA_FOLDER_NAME);
        }

        private string GetLocalDataPath(string dataName)
        {
            string dataFileName = string.Format(LOCAL_DATA_FILE_NAME_TEMPLATE, dataName);
            return Path.Combine(_gameDataFolderPath, dataFileName);
        }

        private T1 LoadData<T1>(string path) where T1 : ILocalData
        {
            try
            {
                string encryptedData = File.ReadAllText(path);
                string base64Data = _encryptionService.Decrypt(encryptedData);
                byte[] serializedData = Convert.FromBase64String(base64Data);

                using (var ms = new MemoryStream(serializedData))
                {
                    return Serializer.Deserialize<T1>(ms);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Data loading failed: {ex.Message}");
            }

            return default;
        }
    }
}