using UnityEngine;
using UnityEditor;
using System.Collections;

public class UnityTools
{
    static public void DestroyAllChildren(Transform _transform)
    {
        foreach (Transform child in _transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}

#if UNITY_EDITOR 
// READ ONLY ATTRIBUTE
public class ReadOnlyAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
// !READ ONLY ATTRIBUTE
#endif
