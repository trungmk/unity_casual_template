using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(SlowButton), true)]
    [CanEditMultipleObjects]
    public class SlowButtonEditor : SelectableEditor
    {
        SerializedProperty m_OnClickProperty;
		SlowButton slowButton = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_OnClickProperty = serializedObject.FindProperty("m_OnClick");
			slowButton = target as SlowButton;
        }

        public override void OnInspectorGUI()
		{
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_OnClickProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
