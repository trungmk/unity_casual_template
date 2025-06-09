using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace Core
{
    [CustomEditor(typeof(MonoUIConfig), true)]
    public class UIConfigEditor : UnityEditor.Editor
    {    
        private const string UI_TYPE = "t:Prefab";
        private const string ASSEST_FOLDER = "Assets/";
        private const string ADDRESSABLE_GROUP_UI = "UI"; // Set this to the desired group name
        private MonoUIConfig _uiConfig;
        private List<UIData> _viewDatas = new List<UIData>();
        public string UIPrefabPath => _uiConfig.UIPrefabPath;

        private void OnEnable()
        {
            _uiConfig = target as MonoUIConfig;
        }
    
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
    
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratedPath"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UIPrefabPath"), true);
    
            GUILayout.Label("Generate UI");
    
            if(GUILayout.Button("Generate UI Code"))
            {
                FindAllViews();
                GenerateCode();
            }

            if (GUILayout.Button("Add UI to Addressable Group"))
            {               
                AddPrefabToAddressableGroup(UIPrefabPath, ADDRESSABLE_GROUP_UI);
            }

            serializedObject.ApplyModifiedProperties();
        } 
    
        void FindAllViews()
        {
            _viewDatas.Clear();
            string filter = string.Format(UI_TYPE);
            string[] guids = AssetDatabase.FindAssets(filter);

            if (guids.Length == 0)
            {
                return;
            }
    
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }
                
                BaseView view = AssetDatabase.LoadAssetAtPath<BaseView>(assetPath);

                if (view != null)
                {
                    _viewDatas.Add(new UIData { FullPath = assetPath, ViewType = view.GetType(), Name = view.name });
                }
            }
        }
    
        private void GenerateCode()
        {
//            string templatePath = Path.Combine(Application.dataPath, GENERATED_FOLDER + "/UITemplate.txt");
//            string template = File.ReadAllText(templatePath);
    
            string directoryPath = Path.Combine(Application.dataPath, _uiConfig.GeneratedPath);
            string savePath = Path.Combine(directoryPath, "UIGenerated.cs");
    
//            string methodInit = string.Empty;
            string generated = string.Empty;
    
            List<ClassGenerateBase> classGenList = new List<ClassGenerateBase>();
            classGenList.Add(new UINameGenerate(this));
            
            UIAssigningGenerate assigningGenerated =  new UIAssigningGenerate(this);
            assigningGenerated.SetupDefaultCode();
            classGenList.Add(assigningGenerated);
    
            StringBuilder buildClassGen = new StringBuilder();
            for (int i = 0; i < _viewDatas.Count; i++)
            {
                UIData view = _viewDatas[i];
                
                for (int j = 0; j < classGenList.Count; j++)
                {
                    ClassGenerateBase contextClass = classGenList[j];
                    contextClass.SetupCode(view);
                    contextClass.SetupFunction();
                    contextClass.SetupProperty();
                }
            }
    
            for (int i = 0; i < classGenList.Count; i++)
            {
                ClassGenerateBase contextClass = classGenList[i];
                string classGenerated = contextClass.GenClass();
                buildClassGen.Append(classGenerated);
            }
    
            generated += buildClassGen.ToString();
            File.WriteAllText(savePath, generated);
            AssetDatabase.Refresh();
        }

        private void AddPrefabToAddressableGroup(string prefabPath, string groupName)
        {
            // Load Addressable settings
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
           

            // Find or create the group
            AddressableAssetGroup group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(groupName, false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            }

            string uiFolderPath = Path.Combine(ASSEST_FOLDER, prefabPath);
            string[] uiGUIDs = AssetDatabase.FindAssets(UI_TYPE, new string[] { uiFolderPath });
            foreach (string uiGUID in uiGUIDs)
            {
                string uiPath = AssetDatabase.GUIDToAssetPath(uiGUID);
                if (string.IsNullOrEmpty(uiPath))
                {
                    continue;
                }

                // Load the prefab
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(uiPath);
                if (prefab != null)
                {
                    // Add the prefab to the group
                    AddressableAssetEntry entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(uiPath), group);
                    entry.address = prefab.name;
                    // Save the settings
                    settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entry, true);
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}

