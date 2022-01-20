﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(AnyTest))]
public class AnyTestEditor : Editor
{
    AnyTest t;
    void OnEnabel()
    {
        t = target as AnyTest;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        AnyTest t = (AnyTest)target;
        if (GUILayout.Button("Run Test 1")) {
            t.RunTest1();
        }
        if (GUILayout.Button("Run Test 2")) {
            t.RunTest2();
        }
    }
}
