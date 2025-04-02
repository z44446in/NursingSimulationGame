using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace NursingGame.Editor
{
    /// <summary>
    /// GenericInteractionData의 커스텀 에디터
    /// 인스펙터에서 시각적으로 상호작용 데이터를 편집할 수 있게 해줍니다.
    /// </summary>
    [CustomEditor(typeof(GenericInteractionData))]
    public class GenericInteractionDataEditor : UnityEditor.Editor
    {
        // 폴드아웃 상태
        private bool showBasicInfo = true;
        private bool showSteps = true;
        private bool showVisualEffects = false;
        private bool showSettings = false;
        
        // 특수 기능 폴드아웃 상태
        private Dictionary<int, bool> showInitialObjectsFoldout = new Dictionary<int, bool>();
        private Dictionary<int, bool> showMultiStageDragFoldout = new Dictionary<int, bool>();
        private Dictionary<int, bool> showConditionalTouchFoldout = new Dictionary<int, bool>();
        
        // 드래그 컨트롤 변수
        private int selectedStepIndex = -1;
        private int selectedControl = -1;
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            GenericInteractionData data = (GenericInteractionData)target;
            
            // 스타일 설정
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 13;
            
            // 제목
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("범용 상호작용 데이터 에디터", headerStyle);
            EditorGUILayout.Space(5);
            
            // 기본 정보 섹션
            showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "기본 정보", true);
            if (showBasicInfo)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("interactionId"), new GUIContent("상호작용 ID"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("interactionName"), new GUIContent("상호작용 이름"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), new GUIContent("설명"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"), new GUIContent("아이콘"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 상호작용 단계 섹션
            SerializedProperty stepsProperty = serializedObject.FindProperty("steps");
            showSteps = EditorGUILayout.Foldout(showSteps, $"상호작용 단계 ({stepsProperty.arraySize})", true);
            if (showSteps)
            {
                EditorGUI.indentLevel++;
                
                // 단계 추가 버튼
                if (GUILayout.Button("새 단계 추가", GUILayout.Height(25)))
                {
                    stepsProperty.arraySize++;
                    var newStep = stepsProperty.GetArrayElementAtIndex(stepsProperty.arraySize - 1);
                    newStep.FindPropertyRelative("stepId").stringValue = "step_" + System.Guid.NewGuid().ToString().Substring(0, 8);
                    newStep.FindPropertyRelative("stepName").stringValue = "단계 " + stepsProperty.arraySize;
                    newStep.FindPropertyRelative("interactionType").enumValueIndex = 0; // None
                }
                
                // 각 단계 표시
                for (int i = 0; i < stepsProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("box");
                    
                    // 단계 기본 정보
                    SerializedProperty stepProp = stepsProperty.GetArrayElementAtIndex(i);
                    SerializedProperty stepIdProp = stepProp.FindPropertyRelative("stepId");
                    SerializedProperty stepNameProp = stepProp.FindPropertyRelative("stepName");
                    SerializedProperty interactionTypeProp = stepProp.FindPropertyRelative("interactionType");
                    
                    // 헤더
                    EditorGUILayout.BeginHorizontal();
                    bool stepFoldout = EditorPrefs.GetBool($"GenericInteractionStep_{target.name}_{i}", true);
                    bool newStepFoldout = EditorGUILayout.Foldout(stepFoldout, $"단계 {i+1}: {stepNameProp.stringValue}", true);
                    
                    // 단계 삭제 버튼
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        if (EditorUtility.DisplayDialog("단계 삭제", $"정말로 단계 '{stepNameProp.stringValue}'를 삭제하시겠습니까?", "삭제", "취소"))
                        {
                            stepsProperty.DeleteArrayElementAtIndex(i);
                            serializedObject.ApplyModifiedProperties();
                            return;
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    // 폴드아웃 상태 저장
                    if (newStepFoldout != stepFoldout)
                    {
                        EditorPrefs.SetBool($"GenericInteractionStep_{target.name}_{i}", newStepFoldout);
                    }
                    
                    if (newStepFoldout)
                    {
                        EditorGUI.indentLevel++;
                        
                        // 기본 속성
                        EditorGUILayout.PropertyField(stepIdProp, new GUIContent("단계 ID"));
                        EditorGUILayout.PropertyField(stepNameProp, new GUIContent("단계 이름"));
                        EditorGUILayout.PropertyField(interactionTypeProp, new GUIContent("상호작용 유형"));
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("guideText"), new GUIContent("가이드 텍스트"));
                        
                        // 새로운 고급 기능 섹션
                        DrawAdvancedFeaturesSection(stepProp, i);
                        
                        // 상호작용 유형별 설정
                        InteractionType interactionType = (InteractionType)interactionTypeProp.enumValueIndex;
                        
                        switch (interactionType)
                        {
                            case InteractionType.Drag:
                                DrawDragSettings(stepProp, i);
                                break;
                                
                            case InteractionType.SingleClick:
                            case InteractionType.MultipleClick:
                                DrawClickSettings(stepProp, i);
                                break;
                                
                            case InteractionType.Quiz:
                                DrawQuizSettings(stepProp);
                                break;
                                
                            // 추가 상호작용 유형들에 대한 설정
                            case InteractionType.SwipeUp:
                            case InteractionType.SwipeDown:
                            case InteractionType.SwipeLeft:
                            case InteractionType.SwipeRight:
                                DrawSwipeSettings(stepProp);
                                break;
                                
                            case InteractionType.LongPress:
                            case InteractionType.DoubleTap:
                                DrawTimedSettings(stepProp);
                                break;
                        }
                        
                        // 시각적 가이드 설정
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("시각적 가이드", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("tutorialArrowSprite"), new GUIContent("화살표 스프라이트"));
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("tutorialArrowPosition"), new GUIContent("화살표 위치"));
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("tutorialArrowRotation"), new GUIContent("화살표 회전"));
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("highlightSprite"), new GUIContent("하이라이트 스프라이트"));
                        
                        // 피드백 설정
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("피드백", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("successMessage"), new GUIContent("성공 메시지"));
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("errorMessage"), new GUIContent("오류 메시지"));
                        
                        // 고급 설정
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("고급 설정", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("stepTimeLimit"), new GUIContent("단계 시간 제한 (초)"));
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("requiredCompletedStepIds"), new GUIContent("필요 완료 단계 ID"));
                        EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("isOptional"), new GUIContent("선택 사항"));
                        
                        EditorGUI.indentLevel--;
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 시각 및 음향 효과 섹션
            showVisualEffects = EditorGUILayout.Foldout(showVisualEffects, "시각 및 음향 효과", true);
            if (showVisualEffects)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("successSound"), new GUIContent("성공 효과음"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("errorSound"), new GUIContent("오류 효과음"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("successFeedbackSprite"), new GUIContent("성공 피드백 이미지"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("errorFeedbackSprite"), new GUIContent("오류 피드백 이미지"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 설정 섹션
            showSettings = EditorGUILayout.Foldout(showSettings, "설정", true);
            if (showSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isOrderImportant"), new GUIContent("순서 중요"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("allowSkipSteps"), new GUIContent("단계 건너뛰기 허용"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("timeLimit"), new GUIContent("시간 제한 (초)"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 시스템 등록 버튼
            if (GUILayout.Button("상호작용 시스템에 등록", GUILayout.Height(30)))
            {
                data.RegisterToInteractionSystem();
            }
            
            // ID 자동 생성 버튼
            if (GUILayout.Button("ID 자동 생성", GUILayout.Height(30)))
            {
                serializedObject.FindProperty("interactionId").stringValue = System.Guid.NewGuid().ToString().Substring(0, 8);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        // 고급 기능 섹션 그리기
        private void DrawAdvancedFeaturesSection(SerializedProperty stepProp, int stepIndex)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("고급 기능 (메르트리얼 증류수 절차)", EditorStyles.boldLabel);
            
            // 초기 오브젝트 설정
            DrawInitialObjectsSettings(stepProp, stepIndex);
            
            // 다중 단계 드래그 설정
            DrawMultiStageDragSettings(stepProp, stepIndex);
            
            // 조건부 터치 설정
            DrawConditionalTouchSettings(stepProp, stepIndex);
            
            // 추가 시각 효과
            SerializedProperty showErrorBorderProp = stepProp.FindPropertyRelative("showErrorBorderFlash");
            SerializedProperty disableTouchDurationProp = stepProp.FindPropertyRelative("disableTouchDuration");
            SerializedProperty createWaterEffectProp = stepProp.FindPropertyRelative("createWaterEffect");
            SerializedProperty waterEffectPositionProp = stepProp.FindPropertyRelative("waterEffectPosition");
            SerializedProperty createWaterImageProp = stepProp.FindPropertyRelative("createWaterImageOnObject");
            
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("추가 시각 효과", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(showErrorBorderProp, new GUIContent("오류 테두리 효과"));
            
            if (showErrorBorderProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(disableTouchDurationProp, new GUIContent("터치 비활성화 시간 (초)"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.PropertyField(createWaterEffectProp, new GUIContent("물 효과 표시"));
            
            if (createWaterEffectProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(waterEffectPositionProp, new GUIContent("물 효과 위치"));
                EditorGUILayout.PropertyField(createWaterImageProp, new GUIContent("물 이미지 생성"));
                EditorGUI.indentLevel--;
            }
        }
        
        // 초기 오브젝트 설정 UI 그리기
        private void DrawInitialObjectsSettings(SerializedProperty stepProp, int stepIndex)
        {
            if (!showInitialObjectsFoldout.ContainsKey(stepIndex))
            {
                showInitialObjectsFoldout[stepIndex] = false;
            }
            
            SerializedProperty createInitialObjectsProp = stepProp.FindPropertyRelative("createInitialObjects");
            SerializedProperty initialObjectsProp = stepProp.FindPropertyRelative("initialObjects");
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            createInitialObjectsProp.boolValue = EditorGUILayout.Toggle(createInitialObjectsProp.boolValue, GUILayout.Width(20));
            showInitialObjectsFoldout[stepIndex] = EditorGUILayout.Foldout(showInitialObjectsFoldout[stepIndex], "초기 오브젝트 생성", true);
            EditorGUILayout.EndHorizontal();
            
            if (createInitialObjectsProp.boolValue && showInitialObjectsFoldout[stepIndex])
            {
                EditorGUI.indentLevel++;
                
                // 오브젝트 추가 버튼
                if (GUILayout.Button("새 오브젝트 추가", GUILayout.Height(20)))
                {
                    initialObjectsProp.arraySize++;
                    var newObj = initialObjectsProp.GetArrayElementAtIndex(initialObjectsProp.arraySize - 1);
                    newObj.FindPropertyRelative("objectId").stringValue = "object_" + System.Guid.NewGuid().ToString().Substring(0, 8);
                    newObj.FindPropertyRelative("objectName").stringValue = "오브젝트 " + initialObjectsProp.arraySize;
                    newObj.FindPropertyRelative("scale").vector3Value = Vector3.one;
                    newObj.FindPropertyRelative("tag").stringValue = "Untagged";
                }
                
                // 각 오브젝트 정보 표시
                for (int i = 0; i < initialObjectsProp.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    SerializedProperty objProp = initialObjectsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.LabelField($"오브젝트 {i+1}", EditorStyles.boldLabel);
                    
                    EditorGUILayout.PropertyField(objProp.FindPropertyRelative("objectId"), new GUIContent("오브젝트 ID"));
                    EditorGUILayout.PropertyField(objProp.FindPropertyRelative("objectName"), new GUIContent("오브젝트 이름"));
                    EditorGUILayout.PropertyField(objProp.FindPropertyRelative("objectSprite"), new GUIContent("스프라이트"));
                    EditorGUILayout.PropertyField(objProp.FindPropertyRelative("position"), new GUIContent("위치"));
                    EditorGUILayout.PropertyField(objProp.FindPropertyRelative("rotation"), new GUIContent("회전"));
                    EditorGUILayout.PropertyField(objProp.FindPropertyRelative("scale"), new GUIContent("스케일"));
                    EditorGUILayout.PropertyField(objProp.FindPropertyRelative("tag"), new GUIContent("태그"));
                    
                    // 커스텀 프리팹 설정
                    SerializedProperty useCustomPrefabProp = objProp.FindPropertyRelative("useCustomPrefab");
                    EditorGUILayout.PropertyField(useCustomPrefabProp, new GUIContent("커스텀 프리팹 사용"));
                    
                    if (useCustomPrefabProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(objProp.FindPropertyRelative("customPrefab"), new GUIContent("커스텀 프리팹"));
                    }
                    
                    // 오브젝트 삭제 버튼
                    if (GUILayout.Button("삭제", GUILayout.Height(20)))
                    {
                        initialObjectsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        // 다중 단계 드래그 설정 UI 그리기
        private void DrawMultiStageDragSettings(SerializedProperty stepProp, int stepIndex)
        {
            if (!showMultiStageDragFoldout.ContainsKey(stepIndex))
            {
                showMultiStageDragFoldout[stepIndex] = false;
            }
            
            SerializedProperty useMultiStageDragProp = stepProp.FindPropertyRelative("useMultiStageDrag");
            SerializedProperty multiStageDragStepsProp = stepProp.FindPropertyRelative("multiStageDragSteps");
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            useMultiStageDragProp.boolValue = EditorGUILayout.Toggle(useMultiStageDragProp.boolValue, GUILayout.Width(20));
            showMultiStageDragFoldout[stepIndex] = EditorGUILayout.Foldout(showMultiStageDragFoldout[stepIndex], "다중 단계 드래그", true);
            EditorGUILayout.EndHorizontal();
            
            if (useMultiStageDragProp.boolValue && showMultiStageDragFoldout[stepIndex])
            {
                EditorGUI.indentLevel++;
                
                // 드래그 단계 추가 버튼
                if (GUILayout.Button("새 드래그 단계 추가", GUILayout.Height(20)))
                {
                    multiStageDragStepsProp.arraySize++;
                    var newStep = multiStageDragStepsProp.GetArrayElementAtIndex(multiStageDragStepsProp.arraySize - 1);
                    newStep.FindPropertyRelative("stepId").stringValue = "drag_" + System.Guid.NewGuid().ToString().Substring(0, 8);
                    newStep.FindPropertyRelative("dragAngleTolerance").floatValue = 30f;
                }
                
                // 각 드래그 단계 정보 표시
                for (int i = 0; i < multiStageDragStepsProp.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    SerializedProperty dragStepProp = multiStageDragStepsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.LabelField($"드래그 단계 {i+1}", EditorStyles.boldLabel);
                    
                    EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("stepId"), new GUIContent("단계 ID"));
                    
                    // 드래그 화살표 설정
                    EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("arrowPosition"), new GUIContent("화살표 위치"));
                    EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("arrowRotation"), new GUIContent("화살표 회전"));
                    
                    // 드래그 방향 설정
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("드래그 방향");
                    
                    SerializedProperty angleProp = dragStepProp.FindPropertyRelative("requiredDragAngle");
                    
                    // 원형 방향 선택기 그리기
                    Rect rect = EditorGUILayout.GetControlRect(false, 100);
                    float newAngle = DrawAngleSelector(rect, angleProp.floatValue, stepIndex);
                    if (newAngle != angleProp.floatValue)
                    {
                        angleProp.floatValue = newAngle;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.PropertyField(angleProp, new GUIContent("필요 드래그 각도"));
                    EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("dragAngleTolerance"), new GUIContent("허용 오차 범위"));
                    
                    // 드래그 시작 오브젝트 설정
                    SerializedProperty requireStartOnObjectProp = dragStepProp.FindPropertyRelative("requireStartOnObject");
                    EditorGUILayout.PropertyField(requireStartOnObjectProp, new GUIContent("특정 오브젝트에서 시작"));
                    
                    if (requireStartOnObjectProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("requiredStartTag"), new GUIContent("시작 오브젝트 태그"));
                    }
                    
                    // 타겟 오프셋 설정
                    EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("targetPositionOffset"), new GUIContent("타겟 위치 오프셋"));
                    EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("targetRotationOffset"), new GUIContent("타겟 회전 오프셋"));
                    
                    // 드래그 단계 삭제 버튼
                    if (GUILayout.Button("삭제", GUILayout.Height(20)))
                    {
                        multiStageDragStepsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        // 조건부 터치 설정 UI 그리기
        private void DrawConditionalTouchSettings(SerializedProperty stepProp, int stepIndex)
        {
            if (!showConditionalTouchFoldout.ContainsKey(stepIndex))
            {
                showConditionalTouchFoldout[stepIndex] = false;
            }
            
            SerializedProperty useConditionalTouchProp = stepProp.FindPropertyRelative("useConditionalTouch");
            SerializedProperty touchOptionsProp = stepProp.FindPropertyRelative("touchOptions");
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            useConditionalTouchProp.boolValue = EditorGUILayout.Toggle(useConditionalTouchProp.boolValue, GUILayout.Width(20));
            showConditionalTouchFoldout[stepIndex] = EditorGUILayout.Foldout(showConditionalTouchFoldout[stepIndex], "조건부 터치", true);
            EditorGUILayout.EndHorizontal();
            
            if (useConditionalTouchProp.boolValue && showConditionalTouchFoldout[stepIndex])
            {
                EditorGUI.indentLevel++;
                
                // 터치 옵션 추가 버튼
                if (GUILayout.Button("새 터치 옵션 추가", GUILayout.Height(20)))
                {
                    touchOptionsProp.arraySize++;
                    var newOption = touchOptionsProp.GetArrayElementAtIndex(touchOptionsProp.arraySize - 1);
                    newOption.FindPropertyRelative("optionId").stringValue = "touch_" + System.Guid.NewGuid().ToString().Substring(0, 8);
                }
                
                // 각 터치 옵션 정보 표시
                for (int i = 0; i < touchOptionsProp.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    SerializedProperty optionProp = touchOptionsProp.GetArrayElementAtIndex(i);
                    
                    // 색상 표시 (올바른 옵션은 초록색, 잘못된 옵션은 빨간색)
                    SerializedProperty isCorrectProp = optionProp.FindPropertyRelative("isCorrectOption");
                    Color originalColor = GUI.backgroundColor;
                    
                    if (isCorrectProp.boolValue)
                    {
                        GUI.backgroundColor = new Color(0.6f, 1.0f, 0.6f); // 옅은 초록색
                    }
                    else
                    {
                        GUI.backgroundColor = new Color(1.0f, 0.6f, 0.6f); // 옅은 빨간색
                    }
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUI.backgroundColor = originalColor;
                    
                    EditorGUILayout.LabelField($"터치 옵션 {i+1} " + (isCorrectProp.boolValue ? "(정답)" : "(오답)"), EditorStyles.boldLabel);
                    
                    EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("optionId"), new GUIContent("옵션 ID"));
                    EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("targetTag"), new GUIContent("타겟 태그"));
                    EditorGUILayout.PropertyField(isCorrectProp, new GUIContent("정답 옵션"));
                    
                    EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("successMessage"), new GUIContent("성공 메시지"));
                    EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("errorMessage"), new GUIContent("오류 메시지"));
                    
                    // 오류 효과 설정
                    if (!isCorrectProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("showErrorBorderFlash"), new GUIContent("오류 테두리 효과"));
                        EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("disableTouchDuration"), new GUIContent("터치 비활성화 시간 (초)"));
                        EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("errorEntryText"), new GUIContent("오류 기록 텍스트"));
                    }
                    
                    // 물 효과 설정 (정답 옵션만)
                    if (isCorrectProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("waterEffectPosition"), new GUIContent("물 효과 위치"));
                        EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("createWaterImageOnObject"), new GUIContent("물 이미지 생성"));
                    }
                    
                    // 터치 옵션 삭제 버튼
                    if (GUILayout.Button("삭제", GUILayout.Height(20)))
                    {
                        touchOptionsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        // 드래그 설정 UI 그리기
        private void DrawDragSettings(SerializedProperty stepProp, int stepIndex)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("드래그 설정", EditorStyles.boldLabel);
            
            // 드래그 방향 시각화
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("드래그 방향");
            
            SerializedProperty angleProp = stepProp.FindPropertyRelative("requiredDragAngle");
            
            // 원형 방향 선택기 그리기
            Rect rect = EditorGUILayout.GetControlRect(false, 100);
            float newAngle = DrawAngleSelector(rect, angleProp.floatValue, stepIndex);
            if (newAngle != angleProp.floatValue)
            {
                angleProp.floatValue = newAngle;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(angleProp, new GUIContent("필요 드래그 각도"));
            EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("dragAngleTolerance"), new GUIContent("허용 오차 범위"));
            EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("dragDistance"), new GUIContent("필요 드래그 거리"));
        }
        
        // 클릭 설정 UI 그리기
        private void DrawClickSettings(SerializedProperty stepProp, int stepIndex)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("클릭 설정", EditorStyles.boldLabel);
            
            SerializedProperty areaProp = stepProp.FindPropertyRelative("validClickArea");
            
            // 클릭 영역 시각화
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("클릭 가능 영역", EditorStyles.boldLabel);
            
            Rect rectArea = EditorGUILayout.GetControlRect(false, 100);
            Rect newArea = DrawRectArea(rectArea, areaProp.rectValue, stepIndex);
            areaProp.rectValue = newArea;
            
            EditorGUILayout.PropertyField(areaProp, new GUIContent("클릭 영역 좌표"));
            EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("validTags"), new GUIContent("유효 클릭 태그"));
            EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("requiredClicks"), new GUIContent("필요 클릭 횟수"));
            
            EditorGUILayout.EndVertical();
        }
        
        // 퀴즈 설정 UI 그리기
        private void DrawQuizSettings(SerializedProperty stepProp)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("퀴즈 설정", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("quizQuestion"), new GUIContent("질문"));
            EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("quizOptions"), new GUIContent("선택지"));
            
            // 퀴즈 선택지 개수에 따라 정답 인덱스 제한
            SerializedProperty optionsProp = stepProp.FindPropertyRelative("quizOptions");
            SerializedProperty correctIndexProp = stepProp.FindPropertyRelative("correctOptionIndex");
            
            int optionsCount = optionsProp.arraySize;
            if (optionsCount > 0)
            {
                correctIndexProp.intValue = EditorGUILayout.IntSlider(
                    new GUIContent("정답 선택지 인덱스"),
                    correctIndexProp.intValue,
                    0,
                    optionsCount - 1
                );
            }
            else
            {
                EditorGUILayout.PropertyField(correctIndexProp, new GUIContent("정답 선택지 인덱스"));
            }
        }
        
        // 스와이프 설정 UI 그리기
        private void DrawSwipeSettings(SerializedProperty stepProp)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("스와이프 설정", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("dragDistance"), new GUIContent("필요 스와이프 거리"));
        }
        
        // 시간 기반 설정 UI 그리기
        private void DrawTimedSettings(SerializedProperty stepProp)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("시간 기반 설정", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(stepProp.FindPropertyRelative("stepTimeLimit"), new GUIContent("필요 유지 시간 (초)"));
        }
        
        // 각도 선택기 GUI 그리기
        private float DrawAngleSelector(Rect rect, float currentAngle, int stepIndex)
        {
            // 중심점
            Vector2 center = new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f);
            float radius = Mathf.Min(rect.width, rect.height) * 0.4f;
            
            // 원 그리기
            Handles.color = Color.gray;
            Handles.DrawWireDisc(center, Vector3.forward, radius);
            
            // 각도 눈금 그리기
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30 * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 start = center + direction * radius * 0.8f;
                Vector2 end = center + direction * radius;
                
                Handles.DrawLine(start, end);
                
                // 주요 방향에 라벨 표시
                if (i % 3 == 0)
                {
                    string label = "";
                    switch (i)
                    {
                        case 0: label = "→"; break;  // 오른쪽
                        case 3: label = "↑"; break;  // 위
                        case 6: label = "←"; break;  // 왼쪽
                        case 9: label = "↓"; break;  // 아래
                    }
                    
                    Vector2 labelPos = center + direction * (radius + 15);
                    GUI.Label(new Rect(labelPos.x - 10, labelPos.y - 10, 20, 20), label);
                }
            }
            
            // 현재 각도 방향 그리기
            float currentRad = currentAngle * Mathf.Deg2Rad;
            Vector2 currentDir = new Vector2(Mathf.Cos(currentRad), Mathf.Sin(currentRad));
            
            Handles.color = Color.red;
            Handles.DrawLine(center, center + currentDir * radius);
            
            // 드래그 처리
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                selectedStepIndex = stepIndex;
                selectedControl = 0;  // 0은 각도 선택기를 의미
                
                GUIUtility.hotControl = controlId;
                Event.current.Use();
            }
            
            if (Event.current.type == EventType.MouseDrag && selectedStepIndex == stepIndex && selectedControl == 0)
            {
                Vector2 mousePos = Event.current.mousePosition;
                Vector2 direction = mousePos - center;
                float newAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                if (newAngle < 0) newAngle += 360;
                
                GUI.changed = true;
                Event.current.Use();
                return newAngle;
            }
            
            if (Event.current.type == EventType.MouseUp && selectedStepIndex == stepIndex && selectedControl == 0)
            {
                selectedStepIndex = -1;
                selectedControl = -1;
                GUIUtility.hotControl = 0;
                Event.current.Use();
            }
            
            return currentAngle;
        }
        
        // 사각형 영역 GUI 그리기
        private Rect DrawRectArea(Rect canvas, Rect currentArea, int stepIndex)
        {
            // 스케일 계산 (화면 좌표 vs 게임 좌표)
            float scaleX = canvas.width / 800f;  // 800x600 가정
            float scaleY = canvas.height / 600f;
            
            // 화면 상의 사각형 위치
            Rect displayRect = new Rect(
                canvas.x + currentArea.x * scaleX,
                canvas.y + currentArea.y * scaleY,
                currentArea.width * scaleX,
                currentArea.height * scaleY
            );
            
            // 배경 영역 그리기
            EditorGUI.DrawRect(canvas, new Color(0.1f, 0.1f, 0.1f, 0.2f));
            
            // 클릭 영역 그리기
            EditorGUI.DrawRect(displayRect, new Color(0.2f, 0.8f, 0.2f, 0.3f));
            
            GUI.Box(displayRect, "클릭 영역");
            
            // 커스텀 핸들 구현
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            Event e = Event.current;
            
            // 핸들 크기
            float handleSize = 8f;
            
            // 모서리 핸들 위치 계산
            Rect topLeft = new Rect(displayRect.x - handleSize/2, displayRect.y - handleSize/2, handleSize, handleSize);
            Rect topRight = new Rect(displayRect.x + displayRect.width - handleSize/2, displayRect.y - handleSize/2, handleSize, handleSize);
            Rect bottomLeft = new Rect(displayRect.x - handleSize/2, displayRect.y + displayRect.height - handleSize/2, handleSize, handleSize);
            Rect bottomRight = new Rect(displayRect.x + displayRect.width - handleSize/2, displayRect.y + displayRect.height - handleSize/2, handleSize, handleSize);
            
            // 중앙 핸들 위치 계산
            Rect topCenter = new Rect(displayRect.x + displayRect.width/2 - handleSize/2, displayRect.y - handleSize/2, handleSize, handleSize);
            Rect bottomCenter = new Rect(displayRect.x + displayRect.width/2 - handleSize/2, displayRect.y + displayRect.height - handleSize/2, handleSize, handleSize);
            Rect leftCenter = new Rect(displayRect.x - handleSize/2, displayRect.y + displayRect.height/2 - handleSize/2, handleSize, handleSize);
            Rect rightCenter = new Rect(displayRect.x + displayRect.width - handleSize/2, displayRect.y + displayRect.height/2 - handleSize/2, handleSize, handleSize);
            
            // 핸들 그리기
            EditorGUI.DrawRect(topLeft, Color.white);
            EditorGUI.DrawRect(topRight, Color.white);
            EditorGUI.DrawRect(bottomLeft, Color.white);
            EditorGUI.DrawRect(bottomRight, Color.white);
            EditorGUI.DrawRect(topCenter, Color.white);
            EditorGUI.DrawRect(bottomCenter, Color.white);
            EditorGUI.DrawRect(leftCenter, Color.white);
            EditorGUI.DrawRect(rightCenter, Color.white);
            
            // 드래그 처리
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (topLeft.Contains(e.mousePosition))
                    {
                        selectedStepIndex = stepIndex;
                        selectedControl = 1;  // 1은 topLeft를 의미
                        GUIUtility.hotControl = controlID;
                        e.Use();
                    }
                    else if (topRight.Contains(e.mousePosition))
                    {
                        selectedStepIndex = stepIndex;
                        selectedControl = 2;  // 2는 topRight를 의미
                        GUIUtility.hotControl = controlID;
                        e.Use();
                    }
                    else if (bottomLeft.Contains(e.mousePosition))
                    {
                        selectedStepIndex = stepIndex;
                        selectedControl = 3;  // 3은 bottomLeft를 의미
                        GUIUtility.hotControl = controlID;
                        e.Use();
                    }
                    else if (bottomRight.Contains(e.mousePosition))
                    {
                        selectedStepIndex = stepIndex;
                        selectedControl = 4;  // 4는 bottomRight를 의미
                        GUIUtility.hotControl = controlID;
                        e.Use();
                    }
                    else if (topCenter.Contains(e.mousePosition))
                    {
                        selectedStepIndex = stepIndex;
                        selectedControl = 5;  // 5는 topCenter를 의미
                        GUIUtility.hotControl = controlID;
                        e.Use();
                    }
                    else if (bottomCenter.Contains(e.mousePosition))
                    {
                        selectedStepIndex = stepIndex;
                        selectedControl = 6;  // 6은 bottomCenter를 의미
                        GUIUtility.hotControl = controlID;
                        e.Use();
                    }
                    else if (leftCenter.Contains(e.mousePosition))
                    {
                        selectedStepIndex = stepIndex;
                        selectedControl = 7;  // 7은 leftCenter를 의미
                        GUIUtility.hotControl = controlID;
                        e.Use();
                    }
                    else if (rightCenter.Contains(e.mousePosition))
                    {
                        selectedStepIndex = stepIndex;
                        selectedControl = 8;  // 8은 rightCenter를 의미
                        GUIUtility.hotControl = controlID;
                        e.Use();
                    }
                    else if (displayRect.Contains(e.mousePosition))
                    {
                        selectedStepIndex = stepIndex;
                        selectedControl = 9;  // 9는 전체 영역을 의미
                        GUIUtility.hotControl = controlID;
                        e.Use();
                    }
                    break;
                    
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID && selectedStepIndex == stepIndex && selectedControl != -1)
                    {
                        Vector2 mouseDelta = e.delta;
                        
                        // 선택된 컨트롤에 따라 사각형 조정
                        switch (selectedControl)
                        {
                            case 1: // topLeft
                                displayRect.x += mouseDelta.x;
                                displayRect.y += mouseDelta.y;
                                displayRect.width -= mouseDelta.x;
                                displayRect.height -= mouseDelta.y;
                                break;
                                
                            case 2: // topRight
                                displayRect.y += mouseDelta.y;
                                displayRect.width += mouseDelta.x;
                                displayRect.height -= mouseDelta.y;
                                break;
                                
                            case 3: // bottomLeft
                                displayRect.x += mouseDelta.x;
                                displayRect.width -= mouseDelta.x;
                                displayRect.height += mouseDelta.y;
                                break;
                                
                            case 4: // bottomRight
                                displayRect.width += mouseDelta.x;
                                displayRect.height += mouseDelta.y;
                                break;
                                
                            case 5: // topCenter
                                displayRect.y += mouseDelta.y;
                                displayRect.height -= mouseDelta.y;
                                break;
                                
                            case 6: // bottomCenter
                                displayRect.height += mouseDelta.y;
                                break;
                                
                            case 7: // leftCenter
                                displayRect.x += mouseDelta.x;
                                displayRect.width -= mouseDelta.x;
                                break;
                                
                            case 8: // rightCenter
                                displayRect.width += mouseDelta.x;
                                break;
                                
                            case 9: // 전체 영역
                                displayRect.x += mouseDelta.x;
                                displayRect.y += mouseDelta.y;
                                break;
                        }
                        
                        // 화면 좌표에서 게임 좌표로 변환
                        Rect gameRect = new Rect(
                            (displayRect.x - canvas.x) / scaleX,
                            (displayRect.y - canvas.y) / scaleY,
                            displayRect.width / scaleX,
                            displayRect.height / scaleY
                        );
                        
                        GUI.changed = true;
                        e.Use();
                        
                        return gameRect;
                    }
                    break;
                    
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID && selectedStepIndex == stepIndex)
                    {
                        selectedStepIndex = -1;
                        selectedControl = -1;
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                    break;
            }
            
            return currentArea;
        }
    }
}