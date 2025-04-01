using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

namespace NursingGame.Editor
{
    /// <summary>
    /// GenericProcedureData의 커스텀 에디터
    /// 인스펙터에서 시각적으로 간호 절차 데이터를 편집할 수 있게 해줍니다.
    /// </summary>
    [CustomEditor(typeof(GenericProcedureData))]
    public class GenericProcedureDataEditor : UnityEditor.Editor
    {
        // 폴드아웃 상태
        private bool showBasicInfo = true;
        private bool showSteps = true;
        private bool showUI = false;
        private bool showSettings = false;
        private bool showEvaluation = false;
        
        // 단계 리스트 리더러블
        private ReorderableList stepsList;
        
        private void OnEnable()
        {
            // 단계 목록 처리를 위한 ReorderableList 설정
            SerializedProperty stepsProperty = serializedObject.FindProperty("steps");
            stepsList = new ReorderableList(serializedObject, stepsProperty, true, true, true, true);
            
            // 리스트 헤더 그리기
            stepsList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "간호 절차 단계");
            };
            
            // 리스트 요소 그리기
            stepsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                SerializedProperty element = stepsProperty.GetArrayElementAtIndex(index);
                SerializedProperty nameProperty = element.FindPropertyRelative("stepName");
                
                string stepName = nameProperty.stringValue;
                if (string.IsNullOrEmpty(stepName))
                {
                    stepName = $"단계 {index + 1}";
                }
                
                SerializedProperty requiredProperty = element.FindPropertyRelative("isRequired");
                bool isRequired = requiredProperty.boolValue;
                
                // 단계 표시 (필수 여부에 따라 다르게 표시)
                string displayName = isRequired ? $"{stepName} (필수)" : $"{stepName} (선택)";
                
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, displayName);
            };
            
            // 리스트 요소 선택 시 콜백
            stepsList.onSelectCallback = (ReorderableList list) => {
                // 인스펙터 다시 그리기 요청
                Repaint();
            };
            
            // 리스트 요소 추가 콜백
            stepsList.onAddCallback = (ReorderableList list) => {
                int index = list.count;
                list.serializedProperty.arraySize++;
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                
                // 새 단계 기본값 설정
                SerializedProperty stepIdProperty = element.FindPropertyRelative("stepId");
                SerializedProperty stepNameProperty = element.FindPropertyRelative("stepName");
                SerializedProperty descriptionProperty = element.FindPropertyRelative("description");
                
                stepIdProperty.stringValue = "step_" + System.Guid.NewGuid().ToString().Substring(0, 8);
                stepNameProperty.stringValue = $"새 단계 {index + 1}";
                descriptionProperty.stringValue = "이 단계에 대한 설명을 입력하세요.";
                
                list.index = index;
            };
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            GenericProcedureData data = (GenericProcedureData)target;
            
            // 스타일 설정
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 13;
            
            // 제목
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("간호 절차 데이터 에디터", headerStyle);
            EditorGUILayout.Space(5);
            
            // 기본 정보 섹션
            showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "기본 정보", true);
            if (showBasicInfo)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("procedureId"), new GUIContent("절차 ID"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("procedureName"), new GUIContent("절차 이름"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), new GUIContent("설명"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"), new GUIContent("아이콘"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 간호 절차 단계 섹션
            showSteps = EditorGUILayout.Foldout(showSteps, "간호 절차 단계", true);
            if (showSteps)
            {
                EditorGUI.indentLevel++;
                
                // ReorderableList 그리기
                stepsList.DoLayoutList();
                
                // 선택된 단계 상세 정보 표시
                if (stepsList.index >= 0 && stepsList.index < stepsList.serializedProperty.arraySize)
                {
                    EditorGUILayout.Space(5);
                    SerializedProperty selectedStep = stepsList.serializedProperty.GetArrayElementAtIndex(stepsList.index);
                    
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"단계 {stepsList.index + 1} 상세 정보", EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);
                    
                    // 기본 정보
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("stepId"), new GUIContent("단계 ID"));
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("stepName"), new GUIContent("단계 이름"));
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("description"), new GUIContent("설명"));
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("stepIcon"), new GUIContent("아이콘"));
                    
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("단계 설정", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("isRequired"), new GUIContent("필수 여부"));
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("scoreWeight"), new GUIContent("점수 가중치"));
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("requiredItems"), new GUIContent("필요 아이템"));
                    
                    // 상호작용 목록
                    EditorGUILayout.Space(5);
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("interactions"), new GUIContent("상호작용 목록"));
                    
                    // UI 및 가이드
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("UI 및 가이드", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("guideText"), new GUIContent("가이드 텍스트"));
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("backgroundOverlay"), new GUIContent("배경 오버레이"));
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("stepAudio"), new GUIContent("단계 오디오"));
                    
                    // 설정
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("설정", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("waitForAllInteractions"), new GUIContent("모든 상호작용 대기"));
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("autoAdvanceDelay"), new GUIContent("자동 진행 지연 시간"));
                    EditorGUILayout.PropertyField(selectedStep.FindPropertyRelative("requiredCompletedStepIds"), new GUIContent("필요 완료 단계 ID"));
                    
                    EditorGUILayout.EndVertical();
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // UI 및 배경 섹션
            showUI = EditorGUILayout.Foldout(showUI, "UI 및 배경", true);
            if (showUI)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundImage"), new GUIContent("배경 이미지"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundMusic"), new GUIContent("배경 음악"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("themeColor"), new GUIContent("테마 색상"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 설정 섹션
            showSettings = EditorGUILayout.Foldout(showSettings, "설정", true);
            if (showSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isOrderImportant"), new GUIContent("순서 중요"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("allowSkipNonRequiredSteps"), new GUIContent("비필수 단계 건너뛰기 허용"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("timeLimit"), new GUIContent("시간 제한 (초)"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxScore"), new GUIContent("최대 점수"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 평가 기준 섹션
            showEvaluation = EditorGUILayout.Foldout(showEvaluation, "평가 기준", true);
            if (showEvaluation)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("perfectScoreThreshold"), new GUIContent("우수 기준 점수"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("goodScoreThreshold"), new GUIContent("양호 기준 점수"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("passScoreThreshold"), new GUIContent("통과 기준 점수"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 필수 아이템 버튼
            if (GUILayout.Button("필요 아이템 목록 가져오기", GUILayout.Height(30)))
            {
                List<Item> requiredItems = data.GetAllRequiredItems();
                string itemList = "필요 아이템 목록:\n";
                
                foreach (var item in requiredItems)
                {
                    if (item != null)
                    {
                        itemList += $"- {item.itemName} ({item.itemId})\n";
                    }
                }
                
                EditorUtility.DisplayDialog("필요 아이템 목록", itemList, "확인");
            }
            
            // ID 자동 생성 버튼
            if (GUILayout.Button("ID 자동 생성", GUILayout.Height(30)))
            {
                serializedObject.FindProperty("procedureId").stringValue = System.Guid.NewGuid().ToString().Substring(0, 8);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}