using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

namespace NursingGame.Editor
{
    /// <summary>
    /// InteractionData의 커스텀 에디터
    /// 인스펙터에서 시각적으로 상호작용 데이터를 편집할 수 있게 해줍니다.
    /// </summary>
    [CustomEditor(typeof(InteractionData))]
    public class GenericInteractionDataEditor : UnityEditor.Editor
    {
        // 폴드아웃 상태
        private bool showBasicInfo = true;
        private bool showSteps = true;
        private bool showVisualEffects = false;
        private bool showSettings = false;
        
        // 단계 리스트
        private ReorderableList stepsList;
        private int selectedStepIndex = -1;
        
        private void OnEnable()
        {
            // 단계 목록 처리를 위한 ReorderableList 설정
            SerializedProperty stepsProperty = serializedObject.FindProperty("steps");
            
            if (stepsProperty == null)
            {
                Debug.LogError("steps 속성을 찾을 수 없습니다. 스크립트 업데이트가 필요할 수 있습니다.");
                return;
            }
            
            stepsList = new ReorderableList(serializedObject, stepsProperty, true, true, true, true);
            
            // 리스트 헤더 그리기
            stepsList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "상호작용 단계");
            };
            
            // 리스트 요소 그리기
            stepsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                SerializedProperty element = stepsProperty.GetArrayElementAtIndex(index);
                SerializedProperty nameProperty = element.FindPropertyRelative("stepName");
                
                string stepName = nameProperty != null ? nameProperty.stringValue : "";
                if (string.IsNullOrEmpty(stepName))
                {
                    stepName = $"단계 {index + 1}";
                }
                
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, stepName);
            };
            
            // 리스트 요소 선택 시 콜백
            stepsList.onSelectCallback = (ReorderableList list) => {
                selectedStepIndex = list.index;
                Repaint();
            };
            
            // 리스트 요소 추가 콜백
            stepsList.onAddCallback = (ReorderableList list) => {
                int index = list.count;
                list.serializedProperty.arraySize++;
                
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                
                // 기본값 설정
                SerializedProperty stepIdProperty = element.FindPropertyRelative("stepId");
                SerializedProperty stepNameProperty = element.FindPropertyRelative("stepName");
                SerializedProperty interactionTypeProperty = element.FindPropertyRelative("interactionType");
                
                if (stepIdProperty != null)
                    stepIdProperty.stringValue = "step_" + System.Guid.NewGuid().ToString().Substring(0, 8);
                
                if (stepNameProperty != null)
                    stepNameProperty.stringValue = $"새 단계 {index + 1}";
                
                if (interactionTypeProperty != null)
                    interactionTypeProperty.enumValueIndex = 0; // None
                
                list.index = index;
                selectedStepIndex = index;
                
                serializedObject.ApplyModifiedProperties();
            };
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            InteractionData data = target as InteractionData;
            if (data == null) 
            {
                EditorGUILayout.HelpBox("타겟이 InteractionData가 아닙니다.", MessageType.Error);
                return;
            }
            
            // 스타일 설정
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 13;
            
            // 제목
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("상호작용 데이터 에디터", headerStyle);
            EditorGUILayout.Space(5);
            
            // 기본 정보 섹션
            showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "기본 정보", true);
            if (showBasicInfo)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("id"), new GUIContent("상호작용 ID"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"), new GUIContent("상호작용 이름"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), new GUIContent("설명"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 단계 섹션
            showSteps = EditorGUILayout.Foldout(showSteps, "상호작용 단계", true);
            if (showSteps)
            {
                EditorGUI.indentLevel++;
                
                // ReorderableList 그리기
                stepsList?.DoLayoutList();
                
                // 선택된 단계의 상세 정보
                if (selectedStepIndex >= 0 && selectedStepIndex < stepsList.serializedProperty.arraySize)
                {
                    EditorGUILayout.Space(5);
                    SerializedProperty stepProperty = stepsList.serializedProperty.GetArrayElementAtIndex(selectedStepIndex);
                    
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"단계 {selectedStepIndex + 1} 상세 정보", EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);
                    
                    // 기본 정보
                    EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("stepId"), new GUIContent("단계 ID"));
                    EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("stepName"), new GUIContent("단계 이름"));
                    EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("interactionType"), new GUIContent("상호작용 유형"));
                    EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("guideText"), new GUIContent("가이드 텍스트"));
                    
                    // 유형별 설정
                    SerializedProperty interactionTypeProperty = stepProperty.FindPropertyRelative("interactionType");
                    if (interactionTypeProperty != null)
                    {
                        EditorGUILayout.Space(5);
                        
                        int typeIndex = interactionTypeProperty.enumValueIndex;
                        InteractionType interactionType = (InteractionType)typeIndex;
                        
                        if (interactionType == InteractionType.Drag || 
                            interactionType == InteractionType.SwipeUp || 
                            interactionType == InteractionType.SwipeDown || 
                            interactionType == InteractionType.SwipeLeft || 
                            interactionType == InteractionType.SwipeRight)
                        {
                            EditorGUILayout.LabelField("드래그 설정", EditorStyles.boldLabel);
                            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("requiredDragAngle"), new GUIContent("드래그 각도"));
                            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("dragAngleTolerance"), new GUIContent("각도 허용 범위"));
                            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("dragDistance"), new GUIContent("드래그 거리"));
                            
                            // 다중 단계 드래그
                            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("useMultiStageDrag"), new GUIContent("다중 단계 드래그"));
                            SerializedProperty useMultiStageDragProperty = stepProperty.FindPropertyRelative("useMultiStageDrag");
                            if (useMultiStageDragProperty != null && useMultiStageDragProperty.boolValue)
                            {
                                EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("multiStageDragSteps"), new GUIContent("드래그 단계"), true);
                            }
                        }
                        else if (interactionType == InteractionType.SingleClick || interactionType == InteractionType.MultipleClick)
                        {
                            EditorGUILayout.LabelField("클릭 설정", EditorStyles.boldLabel);
                            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("validClickArea"), new GUIContent("유효 클릭 영역"));
                            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("validTags"), new GUIContent("유효 태그"), true);
                        }
                        else if (interactionType == InteractionType.Quiz)
                        {
                            EditorGUILayout.LabelField("퀴즈 설정", EditorStyles.boldLabel);
                            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("quizQuestion"), new GUIContent("질문"));
                            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("quizOptions"), new GUIContent("선택지"), true);
                            EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("correctOptionIndex"), new GUIContent("정답 인덱스"));
                        }
                    }
                    
                    // 피드백 설정
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("피드백 설정", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("successMessage"), new GUIContent("성공 메시지"));
                    EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("errorMessage"), new GUIContent("오류 메시지"));
                    EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("showErrorBorderFlash"), new GUIContent("오류 테두리 효과"));
                    
                    // 시각 효과 설정
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("시각 효과", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("tutorialArrowSprite"), new GUIContent("튜토리얼 화살표"));
                    EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("tutorialArrowPosition"), new GUIContent("화살표 위치"));
                    EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("tutorialArrowRotation"), new GUIContent("화살표 회전"));
                    
                    // 물 효과 설정
                    EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("createWaterEffect"), new GUIContent("물 효과 생성"));
                    SerializedProperty createWaterEffectProperty = stepProperty.FindPropertyRelative("createWaterEffect");
                    if (createWaterEffectProperty != null && createWaterEffectProperty.boolValue)
                    {
                        EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("waterEffectPosition"), new GUIContent("물 효과 위치"));
                        EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("createWaterImageOnObject"), new GUIContent("물 이미지 생성"));
                    }
                    
                    // 조건부 터치 설정
                    EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("useConditionalTouch"), new GUIContent("조건부 터치 사용"));
                    SerializedProperty useConditionalTouchProperty = stepProperty.FindPropertyRelative("useConditionalTouch");
                    if (useConditionalTouchProperty != null && useConditionalTouchProperty.boolValue)
                    {
                        EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("touchOptions"), new GUIContent("터치 옵션"), true);
                    }
                    
                    EditorGUILayout.EndVertical();
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 피드백 섹션
            showVisualEffects = EditorGUILayout.Foldout(showVisualEffects, "피드백 설정", true);
            if (showVisualEffects)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("successSound"), new GUIContent("성공 효과음"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("errorSound"), new GUIContent("오류 효과음"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("successMessage"), new GUIContent("성공 메시지"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("errorMessage"), new GUIContent("오류 메시지"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 초기 오브젝트 섹션
            showSettings = EditorGUILayout.Foldout(showSettings, "초기 오브젝트 설정", true);
            if (showSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("initialObjects"), new GUIContent("초기 오브젝트"), true);
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // ID 자동 생성 버튼
            if (GUILayout.Button(new GUIContent("ID 자동 생성"), GUILayout.Height(30)))
            {
                SerializedProperty idProperty = serializedObject.FindProperty("id");
                if (idProperty != null)
                {
                    idProperty.stringValue = System.Guid.NewGuid().ToString().Substring(0, 8);
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                }
            }
            
            // 변경 사항 적용
            serializedObject.ApplyModifiedProperties();
            
            // 변경사항이 있으면 에디터 재렌더링 요청
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}