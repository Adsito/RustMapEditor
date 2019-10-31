using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class EditorUI
{
    public static void BeginToolbarHorizontal()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
    }
    public static void EndToolbarHorizontal()
    {
        GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.MaxWidth(0));
        EditorGUILayout.EndHorizontal();
    }
    public static void ToolbarLabel(GUIContent guiContent)
    {
        GUILayout.Label(guiContent, EditorStyles.toolbarButton);
    }
    public static bool ToolbarButton(GUIContent guiContent)
    {
        return GUILayout.Button(guiContent, EditorStyles.toolbarButton);
    }
    public static bool ToolbarToggle(GUIContent guiContent, ref bool toggle)
    {
        return toggle = GUILayout.Toggle(toggle, guiContent, "ToolbarButton");
    }
    public static Enum ToolbarEnumPopup(Enum enumGroup)
    {
        return EditorGUILayout.EnumPopup(enumGroup, EditorStyles.toolbarDropDown);
    }
    public static float ToolbarDelayedFloatField(ref float value)
    {
        return value = EditorGUILayout.DelayedFloatField(value, EditorStyles.toolbarTextField);
    }
    public static void ToolbarMinMax(GUIContent minContent, GUIContent maxContent, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
    {
        BeginToolbarHorizontal();
        ToolbarLabel(minContent);
        minValue = EditorGUILayout.DelayedFloatField(minValue);
        ToolbarLabel(maxContent);
        maxValue = EditorGUILayout.DelayedFloatField(maxValue);
        EndToolbarHorizontal();
        EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
    }
    public static void ToolbarMinMaxInt(GUIContent minContent, GUIContent maxContent, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
    {
        BeginToolbarHorizontal();
        ToolbarLabel(minContent);
        minValue = Mathf.Clamp(EditorGUILayout.DelayedIntField((int)minValue), minLimit, maxValue);
        ToolbarLabel(maxContent);
        maxValue = Mathf.Clamp(EditorGUILayout.DelayedIntField((int)maxValue), minValue, maxLimit);
        EndToolbarHorizontal();
        EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
    }
    public static float ToolbarIntSlider(GUIContent guiContent, ref int value, int leftValue, int rightValue)
    {
        BeginToolbarHorizontal();
        ToolbarLabel(guiContent);
        value = EditorGUILayout.IntSlider(value, leftValue, rightValue);
        EndToolbarHorizontal();
        return value;
    }
    public static void ToolbarToggleMinMax(GUIContent toggleContent, GUIContent minContent, GUIContent maxContent, ref bool toggle, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
    {
        BeginToolbarHorizontal();
        ToolbarToggle(toggleContent, ref toggle);
        ToolbarLabel(minContent);
        minValue = EditorGUILayout.DelayedFloatField(minValue);
        ToolbarLabel(maxContent);
        maxValue = EditorGUILayout.DelayedFloatField(maxValue);
        EndToolbarHorizontal();
        EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
    }
}