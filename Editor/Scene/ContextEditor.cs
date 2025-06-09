using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    [CustomEditor(typeof(SceneSO), true)]
    public class ContextEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            SceneSO context = target as SceneSO;
            serializedObject.Update();
        
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Scene"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ScenePath"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SceneName"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IsGameContext"), true);

            if (GUILayout.Button("Resolve Scene Info"))
            {
                SetScenePath(context);
                SetSceneName(context);

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(context);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(context);
        }

        private void SetScenePath(SceneSO context)
        {
            context.ScenePath = AssetDatabase.GetAssetPath(context.Scene);
        }

        private void SetSceneName(SceneSO context)
        {
            context.SceneName = context.Scene.name;
        }
    }
}

