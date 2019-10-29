using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class EditorUI
{
    public static Enum ToolbarEnumPopup(GUIContent guiContent, Enum enumGroup)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label(guiContent, EditorStyles.toolbarButton);
        enumGroup = EditorGUILayout.EnumPopup(enumGroup, EditorStyles.toolbarDropDown);
        EditorGUILayout.EndHorizontal();
        return enumGroup;
    }
    public static void ToolbarDelayedFloatField(ref float value)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.DelayedFloatField(value, EditorStyles.toolbarTextField);
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();
    }
    public static void ToolbarMinMax(GUIContent minContent, GUIContent maxContent, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label(minContent, EditorStyles.toolbarButton);
        minValue = EditorGUILayout.DelayedFloatField(minValue);
        GUILayout.Label(maxContent, EditorStyles.toolbarButton);
        maxValue = EditorGUILayout.DelayedFloatField(maxValue);
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
    }
    public static void ToolbarToggle(GUIContent guiContent, ref bool toggle)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        toggle = GUILayout.Toggle(toggle, guiContent, "ToolbarButton");
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();
    }
    public static void ToolbarToggleMinMax(GUIContent toggleContent, GUIContent minContent, GUIContent maxContent, ref bool toggle, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        toggle = GUILayout.Toggle(toggle, toggleContent, "ToolbarButton");
        GUILayout.Label(minContent, EditorStyles.toolbarButton);
        minValue = EditorGUILayout.DelayedFloatField(minValue);
        GUILayout.Label(maxContent, EditorStyles.toolbarButton);
        maxValue = EditorGUILayout.DelayedFloatField(maxValue);
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
    }
}
