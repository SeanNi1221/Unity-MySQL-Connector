using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Sean21.MySQLConnector
{
[CustomEditor(typeof(MySQLBridge))]
[CanEditMultipleObjects]
public class MySQLBridgeEditor : Editor
{
    GUIStyle noteStyle = new GUIStyle();
    SerializedProperty prop;
    MySQLBridge bridge;
    void OnEnable() {
        bridge = target as MySQLBridge;
    }
    public override void OnInspectorGUI()
    {        
        //Note style
        noteStyle.fontStyle = FontStyle.Normal;
        noteStyle.alignment = TextAnchor.MiddleCenter;

        // Insert button before "request" field
        prop = serializedObject.GetIterator();
        while(prop.NextVisible(true))
        {
            if (prop.name == "request") {
                EditorGUILayout.LabelField("*Please initialize to apply changes*", noteStyle);
                if (GUILayout.Button("Initialize & Login MySQLBridge", GUILayout.Height(32))) MySQLBridge.i.Initialize();
                break;
            }
            EditorGUILayout.PropertyField(prop);
        }
        //Draw "request" field
        EditorGUILayout.PropertyField(prop);
        
        serializedObject.ApplyModifiedProperties();  

        // if( MySQLBridge.Request.operation!=null && !td.request.operation.isDone) Repaint();
        RequiresConstantRepaint();      
    }
}
}