using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tile))]
[CanEditMultipleObjects]
class TileEditor : Editor
{
    SerializedProperty immovableProp;

    void OnEnable()
    {
        immovableProp = serializedObject.FindProperty("immovable");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(immovableProp, new GUIContent("Immovable"));
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var targetObject in serializedObject.targetObjects)
            {
                var tile = (Tile) targetObject;
                tile.transform.Find("Screws").gameObject.SetActive(immovableProp.boolValue);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
