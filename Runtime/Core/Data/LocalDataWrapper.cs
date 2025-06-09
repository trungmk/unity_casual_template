using System;
using System.Globalization;
using System.IO;
using System.Text;
using Core;
using Newtonsoft.Json;
using UnityEngine;

public class LocalDataWrapper<T> : ILocalDataWrapper where T : class, ILocalData, new()
{
    private T _obj;

    const string LOCAL_DATA_FOLDER_NAME = "Profiles";
    
    const string LOCAL_DATA_FILE_NAME_TEMPLATE = "{0}.dat";
    
    public const string LOCAL_DATA_DEFAULT_NAME = "default";
    
    private string _gameDataFolderPath = string.Empty;
    
    private static JsonSerializerSettings _jsonSerializerSettings;
    
    public static JsonSerializerSettings JsonSerializerSettings
    {
        get
        {
            if (_jsonSerializerSettings == null)
            {
                _jsonSerializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
                };
            }

            return _jsonSerializerSettings;
        }
    }
    
    public virtual void Init()
    {
        // load storage files and deserialize here.
        _gameDataFolderPath = GetProfileFolderPath();
        if (!Directory.Exists(_gameDataFolderPath))
        {
            Directory.CreateDirectory(_gameDataFolderPath);
        }
            
        Type type = typeof(T);
        string dataName = type.Name;
        string dataFilePath = GetLocalDataPath(dataName);

        if (File.Exists (dataFilePath)) {
            _obj = LoadData<T> (dataFilePath);
            //_obj.InitAfterLoadData();
        }
        else
        {
            _obj = new T();
            _obj.InitAfterLoadData();
            SaveData<T>(_obj);
        }
    }

    public virtual void SaveData<T1>() where T1 : ILocalData
    {
        if (_obj == null)
        {
            return;
        }

        string value = JsonConvert.SerializeObject(_obj);
        string fileName = string.Format(LOCAL_DATA_FILE_NAME_TEMPLATE, typeof(T1).Name);
        string filePath = Path.Combine (_gameDataFolderPath, fileName);
        File.WriteAllText (filePath, value);
        // using (StreamWriter outputFile = new StreamWriter(filePath))
        // {
        //     foreach (char line in value)
        //         outputFile.WriteLine(line);
        // }
    }
    
    public virtual void SaveData<T1>(object obj) where T1 : ILocalData
    {
        if (_obj == null)
        {
            return;
        }

        _obj = obj as T;
        string fileName = string.Format(LOCAL_DATA_FILE_NAME_TEMPLATE, typeof(T1).Name);
        string filePath = Path.Combine (_gameDataFolderPath, fileName);
        var data = JsonConvert.SerializeObject(obj);
        File.WriteAllText(filePath, data);
    }

    public T1 GetData<T1>() where T1 : ILocalData
    {
        return (T1) Convert.ChangeType(_obj, typeof(T1));;
    }
    
    private string GetProfileFolderPath()
    {
        string folderPath = Path.Combine (Application.persistentDataPath, LOCAL_DATA_FOLDER_NAME);
        Directory.CreateDirectory (folderPath);

        return folderPath;
    }
    
    private string GetLocalDataPath (string dataName) 
    {
        string dataFileName = string.Format (LOCAL_DATA_FILE_NAME_TEMPLATE, dataName);
        return Path.Combine(_gameDataFolderPath, dataFileName);
    }
    
    private T1 LoadData<T1> (string path) where T1 : ILocalData
	{
        T1 data = default(T1);

       // byte[] values = File.ReadAllBytes(path);
            
        //string jsonData = Encoding.UTF8.GetString(values);
        
        string jsonData = File.ReadAllText (path);
        
        if (!string.Equals(jsonData, String.Empty))
        {
            data = JsonConvert.DeserializeObject<T1>(jsonData);
        }

        return data;
    }
}