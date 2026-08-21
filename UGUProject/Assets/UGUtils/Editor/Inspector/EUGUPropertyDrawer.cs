using UnityEditor;
using UnityEngine;

namespace UGUE.Editor.UGUProperty
{
    [CustomPropertyDrawer(typeof(UGU.Runtime.UGUPropertyBase), true)]
    public class EUGUPropertyDrawer : PropertyDrawer
    {
        private const string ValueFieldName = "m_value";

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            var valueProp = property.FindPropertyRelative(ValueFieldName);
            if (valueProp != null)
            {
                EditorGUI.PropertyField(
                    position,
                    valueProp,
                    label,
                    true);
            }
            else
            {
                EditorGUI.LabelField(
                    position,
                    label,
                    new GUIContent("(不支持序列化的类型)"));
            }
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            var valueProp = property.FindPropertyRelative(ValueFieldName);
            if (valueProp != null)
            {
                return EditorGUI.GetPropertyHeight(
                    valueProp,
                    label,
                    true);
            }
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
