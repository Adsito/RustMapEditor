using System;
using UnityEngine;
using UnityEditor;

namespace RustMapEditor.UI
{
    public static class Elements
    {
        #region Toolbar
        public static void BeginToolbarHorizontal()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        }

        public static void EndToolbarHorizontal()
        {
            EditorGUILayout.EndHorizontal();
        }

        public static void ToolbarLabel(GUIContent guiContent)
        {
            GUILayout.Label(guiContent, EditorStyles.toolbarButton);
        }

        public static bool ToolbarCheckBox(GUIContent guiContent, bool toggle)
        {
            return EditorGUILayout.ToggleLeft(guiContent, toggle, "ToolbarButton");
        }

        public static void ToolbarLabelField(GUIContent label, GUIContent label2)
        {
            EditorGUILayout.LabelField(label, label2);
        }

        public static string ToolbarTextField(string text)
        {
            return EditorGUILayout.TextField(text, EditorStyles.toolbarTextField);
        }

        public static string ToolbarDelayedTextField(string text)
        {
            return EditorGUILayout.DelayedTextField(text, EditorStyles.toolbarTextField);
        }

        public static int ToolbarIntField(int value)
        {
            return EditorGUILayout.IntField(value, EditorStyles.toolbarTextField);
        }

        public static int ToolbarDelayedIntField(int value)
        {
            return EditorGUILayout.DelayedIntField(value, EditorStyles.toolbarTextField);
        }

        public static bool ToolbarButton(GUIContent guiContent)
        {
            return GUILayout.Button(guiContent, EditorStyles.toolbarButton);
        }

        public static bool ToolbarToggle(GUIContent guiContent, bool toggle)
        {
            return toggle = GUILayout.Toggle(toggle, guiContent, "ToolbarButton");
        }

        public static Enum ToolbarEnumPopup(Enum enumGroup)
        {
            return EditorGUILayout.EnumPopup(enumGroup, EditorStyles.toolbarDropDown);
        }

        public static Enum ToolbarEnumFlagsField(Enum enumGroup)
        {
            return EditorGUILayout.EnumFlagsField(enumGroup, EditorStyles.toolbarDropDown);
        }

        public static float ToolbarDelayedFloatField(float value)
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

        public static void ToolbarMinMaxInt(GUIContent minContent, GUIContent maxContent, ref int minValue, ref int maxValue, int minLimit, int maxLimit)
        {
            float minValueTemp = minValue, maxValueTemp = maxValue;

            BeginToolbarHorizontal();
            ToolbarLabel(minContent);
            minValue = Mathf.Clamp(EditorGUILayout.DelayedIntField((int)minValue), minLimit, maxValue);
            ToolbarLabel(maxContent);
            maxValue = Mathf.Clamp(EditorGUILayout.DelayedIntField((int)maxValue), minValue, maxLimit);
            EndToolbarHorizontal();

            EditorGUILayout.MinMaxSlider(ref minValueTemp, ref maxValueTemp, minLimit, maxLimit);
            minValue = (int)minValueTemp; maxValue = (int)maxValueTemp;
        }

        public static int ToolbarIntSlider(GUIContent guiContent, int value, int leftValue, int rightValue)
        {
            BeginToolbarHorizontal();
            ToolbarLabel(guiContent);
            value = (int)EditorGUILayout.Slider(value, leftValue, rightValue);
            EndToolbarHorizontal();
            return value;
        }

        public static float ToolbarSlider(GUIContent guiContent, float value, float leftValue, float rightValue)
        {
            BeginToolbarHorizontal();
            ToolbarLabel(guiContent);
            value = EditorGUILayout.Slider(value, leftValue, rightValue);
            EndToolbarHorizontal();
            return value;
        }

        public static bool ToolbarToggleMinMax(GUIContent toggleContent, GUIContent minContent, GUIContent maxContent, bool toggle, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
        {
            BeginToolbarHorizontal();
            toggle = ToolbarToggle(toggleContent, toggle);
            ToolbarLabel(minContent);
            minValue = EditorGUILayout.DelayedFloatField(minValue);
            ToolbarLabel(maxContent);
            maxValue = EditorGUILayout.DelayedFloatField(maxValue);
            EndToolbarHorizontal();
            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
            return toggle;
        }
        #endregion
        #region Other
        public static void MiniBoldLabel(GUIContent guiContent)
        {
            GUILayout.Label(guiContent, EditorStyles.miniBoldLabel);
        }

        public static void BoldLabel(GUIContent guiContent)
        {
            GUILayout.Label(guiContent, EditorStyles.boldLabel);
        }

        public static void MiniLabel(GUIContent guiContent)
        {
            GUILayout.Label(guiContent, EditorStyles.miniLabel);
        }

        public static void Label(GUIContent guiContent)
        {
            GUILayout.Label(guiContent, EditorStyles.label);
        }
        #endregion
    }
}