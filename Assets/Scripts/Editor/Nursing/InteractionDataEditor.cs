using UnityEngine;
using UnityEditor;
using Nursing.Interaction;

namespace Nursing.Editor
{
    [CustomEditor(typeof(InteractionData))]
    public class InteractionDataEditor : UnityEditor.Editor
    {
        private SerializedProperty idProperty;
        private SerializedProperty displayNameProperty;
        private SerializedProperty descriptionProperty;
        private SerializedProperty stagesProperty;
        private SerializedProperty guideMessageProperty;
        
        private bool[] stagesFoldout;
        
        private void OnEnable()
        {
            idProperty = serializedObject.FindProperty("id");
            displayNameProperty = serializedObject.FindProperty("displayName");
            descriptionProperty = serializedObject.FindProperty("description");
            stagesProperty = serializedObject.FindProperty("stages");
            guideMessageProperty = serializedObject.FindProperty("guideMessage");
            
            UpdateStagesFoldout();
        }
        
        private void UpdateStagesFoldout()
        {
            if (stagesProperty.arraySize != (stagesFoldout?.Length ?? 0))
            {
                stagesFoldout = new bool[stagesProperty.arraySize];
                for (int i = 0; i < stagesFoldout.Length; i++)
                {
                    stagesFoldout[i] = false;
                }
            }
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("인터랙션 데이터", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 기본 정보
            EditorGUILayout.PropertyField(idProperty, new GUIContent("ID", "인터랙션의 고유 식별자"));
            EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("이름", "인터랙션의 표시 이름"));
            EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("설명", "인터랙션에 대한 설명"));
            EditorGUILayout.PropertyField(guideMessageProperty, new GUIContent("가이드 메시지", "인터랙션의 기본 가이드 메시지"));
            
            EditorGUILayout.Space();
            
            // 스테이지 목록
            EditorGUILayout.LabelField("인터랙션 스테이지", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"스테이지 ({stagesProperty.arraySize})", EditorStyles.boldLabel);
            if (GUILayout.Button("스테이지 추가", GUILayout.Width(120)))
            {
                stagesProperty.arraySize++;
                UpdateStagesFoldout();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 각 스테이지 표시
            for (int i = 0; i < stagesProperty.arraySize; i++)
            {
                SerializedProperty stageProperty = stagesProperty.GetArrayElementAtIndex(i);
                
                // 스테이지 헤더
                EditorGUILayout.BeginHorizontal();
                
                string stageName = stageProperty.FindPropertyRelative("name").stringValue;
                string displayName = string.IsNullOrEmpty(stageName) ? $"스테이지 {i + 1}" : stageName;
                
                stagesFoldout[i] = EditorGUILayout.Foldout(stagesFoldout[i], displayName, true);
                
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    stagesProperty.DeleteArrayElementAtIndex(i);
                    UpdateStagesFoldout();
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
                
                if (stagesFoldout[i])
                {
                    EditorGUI.indentLevel++;
                    DrawStageProperties(stageProperty);
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.Space();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawStageProperties(SerializedProperty stageProperty)
        {
            // 스테이지 기본 정보
            EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("id"), new GUIContent("ID", "스테이지의 고유 식별자"));
            EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("name"), new GUIContent("이름", "스테이지의 표시 이름"));
            EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("guideMessage"), new GUIContent("가이드 메시지", "스테이지의 가이드 메시지"));
            
            // 인터랙션 타입
            EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("interactionType"), new GUIContent("인터랙션 타입", "스테이지의 인터랙션 타입"));
            
            // 순서 요구사항
            SerializedProperty requireOrderProperty = stageProperty.FindPropertyRelative("requireSpecificOrder");
            EditorGUILayout.PropertyField(requireOrderProperty, new GUIContent("특정 순서 필요", "이 스테이지가 특정 순서로 실행되어야 하는지 여부"));
            
            if (requireOrderProperty.boolValue)
            {
                EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("requiredPreviousStageIds"), new GUIContent("이전 필수 스테이지 ID", "이 스테이지 전에 완료해야 하는 스테이지의 ID 목록"));
                EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("incorrectOrderPenalty"), new GUIContent("순서 오류 패널티", "순서를 어겼을 때 적용할 패널티"));
            }
            
            // 오류 패널티
            EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("incorrectInteractionPenalty"), new GUIContent("인터랙션 오류 패널티", "잘못된 인터랙션에 대한 패널티"));
            
            // 인터랙션 설정
            SerializedProperty settingsProperty = stageProperty.FindPropertyRelative("settings");
            EditorGUILayout.PropertyField(settingsProperty, new GUIContent("인터랙션 설정", "인터랙션의 세부 설정"), true);
        }
    }
}