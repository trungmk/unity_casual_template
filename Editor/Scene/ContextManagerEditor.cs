using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Core
{
    [CustomEditor(typeof(CoreSceneManager), true)]
    public class ContextManagerEditor : UnityEditor.Editor
    {
        CoreSceneManager _contextManager;
        
        const string TYPE_CONTEXT = "t:SceneSO";
    
        private void OnEnable()
        {
            _contextManager = target as CoreSceneManager;
        }
    
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Scenes"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("InitializeScene"), true);          
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratePath"), true);
    
            if (GUILayout.Button("Set Up And Generate"))
            {
                FindAllContext();
                GenerateContextFile();
                RefreshAllScenes();
            }
    
            serializedObject.ApplyModifiedProperties();
        }
    
        private void GenerateContextFile()
        {
            string directoryPath = Path.Combine(Application.dataPath, _contextManager.GeneratePath);
            string savePath = Path.Combine(directoryPath, "SceneGenerated.cs");
            string generated = String.Empty;
    
            List<ClassGenerateBase> classGenList = new List<ClassGenerateBase>();
            classGenList.Add(new ContextNameGenerate());
            classGenList.Add(new ContextAssignedGenerate());
            StringBuilder buildClassGen = new StringBuilder();
            
            for (int i = 0; i < _contextManager.Scenes.Count; i++)
            {
                SceneSO sceneSO = _contextManager.Scenes[i];
                if(!sceneSO.IsGameContext)
                {
                    continue;
                }
    
                for (int j = 0; j < classGenList.Count; j++)
                {
                    ClassGenerateBase contextClass = classGenList[j];
                    contextClass.SetupCode(sceneSO);
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
    
        private void FindAllContext()
        {
            string filter = string.Format (TYPE_CONTEXT);
            string[] guids = AssetDatabase.FindAssets(filter);
            _contextManager.Scenes.Clear();
    
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                SceneSO context = AssetDatabase.LoadAssetAtPath(assetPath, typeof(SceneSO)) as SceneSO;
                context.GUID = guid;
                _contextManager.Scenes.Add(context);
            }
        }
    
        private void RefreshAllScenes()
        {
            for (int i = 0; i < _contextManager.Scenes.Count; i++)
            {
                SceneSO context = _contextManager.Scenes[i];
                if(!context.IsGameContext)
                {
                    continue;
                }
    
                Scene scene = EditorSceneManager.OpenScene(context.ScenePath, OpenSceneMode.Additive);
                EditorSceneManager.CloseScene(scene, false);
            }
        }
    }
}

#endif