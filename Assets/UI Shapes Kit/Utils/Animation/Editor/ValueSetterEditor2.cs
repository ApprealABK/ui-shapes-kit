﻿using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Reflection;
using System;

namespace io.github.apprealabk.ui.shapes.kit
{
    [CustomEditor(typeof(Co.Appreal.UI_ShapesKit.Utils.Animation.ValueSetter2))]
    public class FunctionDemoEditor2 : Editor
    {
        static FieldInfo[] targetFields;
        static string[] targetFieldNames;
        static string[] fieldNames;

        SerializedProperty floatValueProp;
        SerializedProperty colorValueProp;
        //	SerializedProperty boolValueProp;

        SerializedProperty isInArrayProp;
        SerializedProperty arrayItemIndexProp;
        SerializedProperty isInClassInArrayProp;


        string[] fieldTypes = new string[] {
        "Float",
        "Bool",
        "Color"
    };

        void OnEnable()
        {
            floatValueProp = serializedObject.FindProperty("FloatValue");
            colorValueProp = serializedObject.FindProperty("ColorValue");
            //		boolValueProp = serializedObject.FindProperty("BoolValue");

            isInArrayProp = serializedObject.FindProperty("IsInArray");
            arrayItemIndexProp = serializedObject.FindProperty("ArrayItemIndex");
            isInClassInArrayProp = serializedObject.FindProperty("IsInClass");
        }

        public override void OnInspectorGUI()
        {
            //		DrawDefaultInspector();

            Co.Appreal.UI_ShapesKit.Utils.Animation.ValueSetter2 obj = target as Co.Appreal.UI_ShapesKit.Utils.Animation.ValueSetter2;
            var targetType = obj.gameObject.GetComponent<Co.Appreal.UI_ShapesKit.IShape>().GetType();

            obj.FieldType = EditorGUILayout.Popup(obj.FieldType, fieldTypes);

            EditorGUILayout.PropertyField(isInArrayProp);

            if (obj.IsInArray)
            {
                EditorGUILayout.PropertyField(arrayItemIndexProp);
            }

            EditorGUILayout.PropertyField(isInClassInArrayProp);

            EditorGUILayout.Space();

            Type fieldType;
            switch (obj.FieldType)
            {
                case 0:
                    fieldType = typeof(float);
                    break;
                case 1:
                    fieldType = typeof(bool);
                    break;
                case 2:
                    fieldType = typeof(Color);
                    break;
                default:
                    fieldType = typeof(float);
                    break;
            }

            var targetFieldInfos = targetType
                .GetFields(Co.Appreal.UI_ShapesKit.Utils.Animation.ValueSetter.binding);

            targetFieldNames = targetFieldInfos
                .Where(x => x.FieldType.Namespace.StartsWith("Co.Appreal.UI_ShapesKit"))
                .Select(x => x.Name.ToString())
                .ToArray();

            if (obj != null)
            {
                int fieldIndex;

                int targetFieldIndex = GetIndexInNameArray(targetFieldNames, obj.TargetFieldName);

                targetFieldIndex = EditorGUILayout.Popup(targetFieldIndex, targetFieldNames);

                obj.TargetTypeName = targetFieldInfos[targetFieldIndex].FieldType.ToString();
                obj.TargetFieldName = targetFieldNames[targetFieldIndex];


                // set field data
                var fields = targetFieldInfos[targetFieldIndex].FieldType
                    .GetFields(Co.Appreal.UI_ShapesKit.Utils.Animation.ValueSetter.binding) // Instance methods, both public and private/protected
                    .Where(x => !x.Name.Contains("Adjusted"));

                if (obj.IsInArray)
                {
                    fields = fields.Where(x => x.FieldType.IsArray);
                }
                else
                {
                    fields = fields.Where(x => x.FieldType == fieldType);
                }


                fieldNames = fields
                    .Select(x => x.Name)
                    .ToArray();

                fieldIndex = GetIndexInNameArray(fieldNames, obj.FieldName);

                if (fieldNames != null && fieldNames.Length > 0)
                {
                    obj.FieldName = fieldNames[EditorGUILayout.Popup(fieldIndex, fieldNames)];
                }

                if (obj.IsInArray)
                {
                    FieldInfo fieldNameInfo = targetFieldInfos[targetFieldIndex].FieldType.GetField(obj.FieldName);

                    if (fieldNameInfo == null)
                    {
                        serializedObject.ApplyModifiedProperties();
                        return;
                    }

                    Type arrayFieldType = fieldNameInfo.FieldType.GetElementType();

                    string[] arrayFieldNames = arrayFieldType
                        .GetFields(Co.Appreal.UI_ShapesKit.Utils.Animation.ValueSetter.binding)
                        .Where(x => x.FieldType == fieldType)
                        .Select(x => x.Name)
                        .ToArray();

                    int arrayFieldIndex = GetIndexInNameArray(arrayFieldNames, obj.ArrayFieldName);

                    if (!obj.IsInClass)
                    {
                        if (arrayFieldNames != null && arrayFieldNames.Length > 0)
                        {
                            obj.ArrayFieldName = arrayFieldNames[EditorGUILayout.Popup(arrayFieldIndex, arrayFieldNames)];
                        }
                    }
                    else
                    {
                        int targetClassFieldIndex = 0;

                        var targetClassFields = arrayFieldType
                            .GetFields(Co.Appreal.UI_ShapesKit.Utils.Animation.ValueSetter.binding)
                            .Where(x => x.FieldType.IsClass)
                            .Where(x => x.FieldType.Namespace.StartsWith("Co.Appreal.UI_ShapesKit"));

                        string[] targetClassFieldNames = targetClassFields
                            .Select(x => x.Name)
                            .ToArray();

                        if (targetClassFieldNames.Length > 0)
                        {
                            targetClassFieldIndex = GetIndexInNameArray(targetClassFieldNames, obj.TargetClassFieldName);

                            obj.TargetClassFieldName = targetClassFieldNames[EditorGUILayout.Popup(targetClassFieldIndex, targetClassFieldNames)];

                            var classFieldNames = arrayFieldType.
                                GetField(obj.TargetClassFieldName, Co.Appreal.UI_ShapesKit.Utils.Animation.ValueSetter.binding)
                                .FieldType
                                .GetFields(Co.Appreal.UI_ShapesKit.Utils.Animation.ValueSetter.binding)
                                .Where(x => x.FieldType == fieldType)
                                .Select(x => x.Name)
                                .ToArray();

                            int classFieldIndex = GetIndexInNameArray(classFieldNames, obj.ClassFieldName);

                            if (classFieldNames.Length > 0)
                            {
                                obj.ClassFieldName = classFieldNames[EditorGUILayout.Popup(classFieldIndex, classFieldNames)];
                            }
                        }
                    }
                }
                else if (obj.IsInClass)
                {
                    Type targetFieldType = targetFieldInfos[targetFieldIndex].FieldType;

                    var fieldNames = targetFieldType
                        .GetFields(Co.Appreal.UI_ShapesKit.Utils.Animation.ValueSetter.binding)
                        .Where(x => x.FieldType.IsClass)
                        .Where(x => x.FieldType.Namespace.StartsWith("Co.Appreal.UI_ShapesKit"))
                        .Select(x => x.Name)
                        .ToArray();

                    if (fieldNames.Length == 0)
                        return;

                    int index = GetIndexInNameArray(fieldNames, obj.TargetClassFieldName);

                    obj.TargetClassFieldName = fieldNames[EditorGUILayout.Popup(index, fieldNames)];

                    fieldNames = targetFieldInfos[targetFieldIndex].FieldType
                        .GetField(obj.TargetClassFieldName, Co.Appreal.UI_ShapesKit.Utils.Animation.ValueSetter.binding)
                        .FieldType
                        .GetFields(Co.Appreal.UI_ShapesKit.Utils.Animation.ValueSetter.binding)
                        .Where(x => x.FieldType == fieldType)
                        .Select(x => x.Name)
                        .ToArray();

                    if (fieldNames.Length == 0)
                        return;

                    index = GetIndexInNameArray(fieldNames, obj.ClassFieldName);
                    obj.ClassFieldName = fieldNames[EditorGUILayout.Popup(index, fieldNames)];
                }

                EditorGUILayout.Space();

                if (obj.FieldType == 0 || obj.FieldType == 1)
                {
                    EditorGUILayout.PropertyField(floatValueProp);
                }
                else if (obj.FieldType == 2)
                {
                    EditorGUILayout.PropertyField(colorValueProp);
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        int GetIndexInNameArray(string[] names, string name)
        {
            try
            {
                return names
                    .Select((v, i) => new { Name = v, Index = i })
                    .First(x => x.Name == name)
                    .Index;
            }
            catch
            {
                return 0;
            }
        }
    }
}