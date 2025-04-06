using UnityEngine;
using UnityEditor;
using Nursing.Procedure;

namespace Nursing.Editor
{
    [CustomEditor(typeof(ProcedureData))]
    public class ProcedureDataEditor : UnityEditor.Editor
    {
        private SerializedProperty idProperty;
        private SerializedProperty displayNameProperty;
        private SerializedProperty descriptionProperty;
        private SerializedProperty stepsProperty;
        private SerializedProperty guideMessageProperty;
        
        private bool[] stepsFoldout;
        
        private void OnEnable()
        {
            idProperty = serializedObject.FindProperty("id");
            displayNameProperty = serializedObject.FindProperty("displayName");
            descriptionProperty = serializedObject.FindProperty("description");
            stepsProperty = serializedObject.FindProperty("steps");
            guideMessageProperty = serializedObject.FindProperty("guideMessage");
            
            UpdateStepsFoldout();
        }
        
        private void UpdateStepsFoldout()
        {
            if (stepsProperty.arraySize != (stepsFoldout?.Length ?? 0))
            {
                stepsFoldout = new bool[stepsProperty.arraySize];
                for (int i = 0; i < stepsFoldout.Length; i++)
                {
                    stepsFoldout[i] = false;
                }
            }
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("프로시저 데이터", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 기본 정보
            EditorGUILayout.PropertyField(idProperty, new GUIContent("ID", "프로시저의 고유 식별자"));
            EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("이름", "프로시저의 표시 이름"));
            EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("설명", "프로시저에 대한 설명"));
            EditorGUILayout.PropertyField(guideMessageProperty, new GUIContent("가이드 메시지", "프로시저의 기본 가이드 메시지"));
            
            EditorGUILayout.Space();
            
            // 스텝 목록
            EditorGUILayout.LabelField("프로시저 스텝", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"스텝 ({stepsProperty.arraySize})", EditorStyles.boldLabel);
            if (GUILayout.Button("스텝 추가", GUILayout.Width(120)))
            {
                stepsProperty.arraySize++;
                UpdateStepsFoldout();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 각 스텝 표시
            for (int i = 0; i < stepsProperty.arraySize; i++)
            {
                SerializedProperty stepProperty = stepsProperty.GetArrayElementAtIndex(i);
                
                // 스텝 헤더
                EditorGUILayout.BeginHorizontal();
                
                string stepName = stepProperty.FindPropertyRelative("name").stringValue;
                string displayName = string.IsNullOrEmpty(stepName) ? $"스텝 {i + 1}" : stepName;
                
                stepsFoldout[i] = EditorGUILayout.Foldout(stepsFoldout[i], displayName, true);
                
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    stepsProperty.DeleteArrayElementAtIndex(i);
                    UpdateStepsFoldout();
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
                
                if (stepsFoldout[i])
                {
                    EditorGUI.indentLevel++;
                    DrawStepProperties(stepProperty);
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.Space();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawStepProperties(SerializedProperty stepProperty)
        {
            // 스텝 기본 정보
            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("id"), new GUIContent("ID", "스텝의 고유 식별자"));
            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("name"), new GUIContent("이름", "스텝의 표시 이름"));
            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("guideMessage"), new GUIContent("가이드 메시지", "스텝의 가이드 메시지"));
            
            // 스텝 타입
            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("stepType"), new GUIContent("스텝 타입", "스텝의 타입"));
            
            // 순서 요구사항
            SerializedProperty requireOrderProperty = stepProperty.FindPropertyRelative("requireSpecificOrder");
            EditorGUILayout.PropertyField(requireOrderProperty, new GUIContent("특정 순서 필요", "이 스텝이 특정 순서로 실행되어야 하는지 여부"));
            
            if (requireOrderProperty.boolValue)
            {
                EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("requiredPreviousStepIds"), new GUIContent("이전 필수 스텝 ID", "이 스텝 전에 완료해야 하는 스텝의 ID 목록"));
                EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("incorrectOrderPenalty"), new GUIContent("순서 오류 패널티", "순서를 어겼을 때 적용할 패널티"));
            }
            
            // 오류 패널티
            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("incorrectActionPenalty"), new GUIContent("잘못된 동작 패널티", "잘못된 동작에 대한 패널티"));
            
            // 스텝 설정
            SerializedProperty settingsProperty = stepProperty.FindPropertyRelative("settings");
            EditorGUILayout.PropertyField(settingsProperty, new GUIContent("스텝 설정", "스텝의 세부 설정"), true);
        }
    }
}