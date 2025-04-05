using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

namespace NursingGame.Editor
{
    /// <summary>
    /// ProcedureData를 위한 커스텀 에디터
    /// </summary>
    [CustomEditor(typeof(ProcedureData))]
    public class ProcedureDataEditor : UnityEditor.Editor
    {
        // 폴드아웃 상태
        private bool showBasicInfo = true;
        private bool showSteps = true;
        private bool showEvaluation = false;
        private bool showUI = false;
        private bool showAudio = false;
        
        // 단계 리스트
        private ReorderableList stepsList;
        private int selectedStepIndex = -1;
        
        // 서브 에디터들
        private Dictionary<int, bool> stepFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> itemInteractionFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> actionButtonFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, ReorderableList> actionButtonLists = new Dictionary<int, ReorderableList>();
        
        private void OnEnable()
        {
            // 단계 목록 초기화
            InitializeStepsList();
        }
        
        /// <summary>
        /// 단계 목록 초기화
        /// </summary>
        private void InitializeStepsList()
        {
            SerializedProperty stepsProperty = serializedObject.FindProperty("steps");
            
            stepsList = new ReorderableList(serializedObject, stepsProperty, true, true, true, true);
            
            // 헤더 그리기
            stepsList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "시술 단계");
            };
            
            // 요소 그리기
            stepsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                SerializedProperty element = stepsProperty.GetArrayElementAtIndex(index);
                SerializedProperty nameProperty = element.FindPropertyRelative("stepName");
                SerializedProperty typeProperty = element.FindPropertyRelative("stepType");
                
                string stepName = nameProperty != null && !string.IsNullOrEmpty(nameProperty.stringValue) ? 
                    nameProperty.stringValue : $"단계 {index + 1}";
                
                string typeStr = "없음";
                if (typeProperty != null)
                {
                    typeStr = typeProperty.enumDisplayNames[typeProperty.enumValueIndex];
                }
                
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, $"{stepName} ({typeStr})");
            };
            
            // 선택 콜백
            stepsList.onSelectCallback = (ReorderableList list) => {
                selectedStepIndex = list.index;
                Repaint();
            };
            
            // 추가 콜백
            stepsList.onAddCallback = (ReorderableList list) => {
                int index = list.count;
                list.serializedProperty.arraySize++;
                
                SerializedProperty newStep = list.serializedProperty.GetArrayElementAtIndex(index);
                
                // 기본값 설정
                SerializedProperty idProperty = newStep.FindPropertyRelative("id");
                SerializedProperty nameProperty = newStep.FindPropertyRelative("stepName");
                
                if (idProperty != null)
                    idProperty.stringValue = System.Guid.NewGuid().ToString().Substring(0, 8);
                
                if (nameProperty != null)
                    nameProperty.stringValue = $"새 단계 {index + 1}";
                
                list.index = index;
                selectedStepIndex = index;
                
                // 폴드아웃 초기화
                stepFoldouts[index] = true;
                
                serializedObject.ApplyModifiedProperties();
            };
            
            // 제거 콜백
            stepsList.onRemoveCallback = (ReorderableList list) => {
                // 삭제 전 확인
                if (EditorUtility.DisplayDialog("단계 삭제", 
                    "이 단계를 삭제하시겠습니까?", "삭제", "취소"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    if (selectedStepIndex >= list.count)
                    {
                        selectedStepIndex = list.count - 1;
                    }
                }
            };
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            ProcedureData procedureData = (ProcedureData)target;
            
            // 스타일 설정
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 13;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("시술 데이터 에디터", headerStyle);
            EditorGUILayout.Space(5);
            
            // 기본 정보 섹션
            DrawBasicInfoSection(procedureData);
            
            EditorGUILayout.Space(10);
            
            // 단계 섹션
            DrawStepsSection();
            
            EditorGUILayout.Space(10);
            
            // 평가 섹션
            DrawEvaluationSection();
            
            EditorGUILayout.Space(10);
            
            // UI 섹션
            DrawUISection();
            
            EditorGUILayout.Space(10);
            
            // 오디오 섹션
            DrawAudioSection();
            
            EditorGUILayout.Space(10);
            
            // ID 자동 생성 버튼
            if (GUILayout.Button("ID 자동 생성", GUILayout.Height(30)))
            {
                SerializedProperty idProperty = serializedObject.FindProperty("id");
                if (idProperty != null)
                {
                    idProperty.stringValue = System.Guid.NewGuid().ToString().Substring(0, 8);
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                }
            }
            
            serializedObject.ApplyModifiedProperties();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
        
        /// <summary>
        /// 기본 정보 섹션 그리기
        /// </summary>
        private void DrawBasicInfoSection(ProcedureData procedureData)
        {
            showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "기본 정보", true);
            if (showBasicInfo)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("id"), new GUIContent("시술 ID"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"), new GUIContent("시술 이름"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), new GUIContent("설명"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("procedureType"), new GUIContent("시술 유형"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isGuidelineVersion"), new GUIContent("가이드라인 버전"));
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// 단계 섹션 그리기
        /// </summary>
        private void DrawStepsSection()
        {
            showSteps = EditorGUILayout.Foldout(showSteps, "시술 단계", true);
            if (showSteps)
            {
                EditorGUI.indentLevel++;
                
                // 단계 목록 그리기
                stepsList.DoLayoutList();
                
                // 선택된 단계가 있으면 상세 정보 표시
                if (selectedStepIndex >= 0 && selectedStepIndex < stepsList.serializedProperty.arraySize)
                {
                    EditorGUILayout.Space(5);
                    SerializedProperty stepProperty = stepsList.serializedProperty.GetArrayElementAtIndex(selectedStepIndex);
                    
                    // 단계 상세 정보 그리기
                    DrawStepDetails(stepProperty, selectedStepIndex);
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// 평가 섹션 그리기
        /// </summary>
        private void DrawEvaluationSection()
        {
            showEvaluation = EditorGUILayout.Foldout(showEvaluation, "평가 설정", true);
            if (showEvaluation)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxScore"), new GUIContent("최대 점수"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("timeLimit"), new GUIContent("제한 시간(초)"));
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// UI 섹션 그리기
        /// </summary>
        private void DrawUISection()
        {
            showUI = EditorGUILayout.Foldout(showUI, "UI 설정", true);
            if (showUI)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundImage"), new GUIContent("배경 이미지"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("titleColor"), new GUIContent("제목 색상"));
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// 오디오 섹션 그리기
        /// </summary>
        private void DrawAudioSection()
        {
            showAudio = EditorGUILayout.Foldout(showAudio, "음향 설정", true);
            if (showAudio)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundMusic"), new GUIContent("배경 음악"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("completionSound"), new GUIContent("완료 효과음"));
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// 단계 상세 정보 그리기
        /// </summary>
        private void DrawStepDetails(SerializedProperty stepProperty, int stepIndex)
        {
            // 단계 폴드아웃 초기화
            if (!stepFoldouts.ContainsKey(stepIndex))
            {
                stepFoldouts[stepIndex] = true;
            }
            
            // 액션 버튼 리스트 초기화
            if (!actionButtonLists.ContainsKey(stepIndex))
            {
                SerializedProperty actionButtonsProperty = stepProperty.FindPropertyRelative("actionButtons");
                if (actionButtonsProperty != null)
                {
                    actionButtonLists[stepIndex] = new ReorderableList(serializedObject, actionButtonsProperty, 
                        true, true, true, true);
                    
                    // 헤더 그리기
                    actionButtonLists[stepIndex].drawHeaderCallback = (Rect rect) => {
                        EditorGUI.LabelField(rect, "액션 버튼");
                    };
                    
                    // 요소 그리기
                    actionButtonLists[stepIndex].drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                        SerializedProperty element = actionButtonsProperty.GetArrayElementAtIndex(index);
                        SerializedProperty buttonTextProperty = element.FindPropertyRelative("buttonText");
                        SerializedProperty isCorrectProperty = element.FindPropertyRelative("isCorrectOption");
                        
                        string buttonText = buttonTextProperty != null && !string.IsNullOrEmpty(buttonTextProperty.stringValue) ? 
                            buttonTextProperty.stringValue : $"버튼 {index + 1}";
                        
                        string correctStr = isCorrectProperty != null && isCorrectProperty.boolValue ? "올바른 옵션" : "잘못된 옵션";
                        
                        rect.y += 2;
                        rect.height = EditorGUIUtility.singleLineHeight;
                        EditorGUI.LabelField(rect, $"{buttonText} ({correctStr})");
                    };
                    
                    // 추가 콜백
                    actionButtonLists[stepIndex].onAddCallback = (ReorderableList list) => {
                        int index = list.count;
                        list.serializedProperty.arraySize++;
                        
                        SerializedProperty newButton = list.serializedProperty.GetArrayElementAtIndex(index);
                        
                        // 기본값 설정
                        SerializedProperty buttonIdProperty = newButton.FindPropertyRelative("buttonId");
                        SerializedProperty buttonTextProperty = newButton.FindPropertyRelative("buttonText");
                        
                        if (buttonIdProperty != null)
                            buttonIdProperty.stringValue = System.Guid.NewGuid().ToString().Substring(0, 8);
                        
                        if (buttonTextProperty != null)
                            buttonTextProperty.stringValue = $"새 버튼 {index + 1}";
                        
                        serializedObject.ApplyModifiedProperties();
                    };
                }
            }
            
            EditorGUILayout.BeginVertical("box");
            
            // 단계 헤더
            stepFoldouts[stepIndex] = EditorGUILayout.Foldout(stepFoldouts[stepIndex], 
                $"단계 {stepIndex + 1} 상세 정보", true);
            
            if (stepFoldouts[stepIndex])
            {
                EditorGUILayout.Space(5);
                
                // 기본 정보
                EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("id"), new GUIContent("단계 ID"));
                EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("stepName"), new GUIContent("단계 이름"));
                EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("description"), new GUIContent("설명"));
                EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("guideMessage"), new GUIContent("가이드 메시지"));
                EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("stepType"), new GUIContent("단계 유형"));
                
                EditorGUILayout.Space(5);
                
                // 단계 순서
                EditorGUILayout.LabelField("단계 순서 설정", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("isRequired"), new GUIContent("필수 단계"));
                EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("isOrderImportant"), new GUIContent("순서 중요"));
                
                EditorGUILayout.Space(5);
                
                // 페널티 설정
                EditorGUILayout.LabelField("페널티 설정", EditorStyles.boldLabel);
                
                EditorGUILayout.LabelField("순서 위반 페널티", EditorStyles.boldLabel);
                DrawPenaltyDataEditor(stepProperty.FindPropertyRelative("orderViolationPenalty"));
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.LabelField("건너뛰기 페널티", EditorStyles.boldLabel);
                DrawPenaltyDataEditor(stepProperty.FindPropertyRelative("skipPenalty"));
                
                EditorGUILayout.Space(5);
                
                // 단계 유형별 설정
                SerializedProperty stepTypeProperty = stepProperty.FindPropertyRelative("stepType");
                if (stepTypeProperty != null)
                {
                    ProcedureStepType stepType = (ProcedureStepType)stepTypeProperty.enumValueIndex;
                    
                    // 아이템 상호작용 설정
                    if (stepType == ProcedureStepType.ItemInteraction)
                    {
                        DrawItemInteractionSettings(stepProperty, stepIndex);
                    }
                    
                    // 액션 버튼 설정
                    if (stepType == ProcedureStepType.ActionButton)
                    {
                        DrawActionButtonSettings(stepProperty, stepIndex);
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 페널티 데이터 에디터 그리기
        /// </summary>
        private void DrawPenaltyDataEditor(SerializedProperty penaltyProperty)
        {
            if (penaltyProperty == null) return;
            
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(penaltyProperty.FindPropertyRelative("penaltyType"), new GUIContent("페널티 유형"));
            EditorGUILayout.PropertyField(penaltyProperty.FindPropertyRelative("speaker"), new GUIContent("화자"));
            EditorGUILayout.PropertyField(penaltyProperty.FindPropertyRelative("penaltyMessage"), new GUIContent("페널티 메시지"));
            EditorGUILayout.PropertyField(penaltyProperty.FindPropertyRelative("databaseMessage"), new GUIContent("데이터베이스 메시지"));
            
            EditorGUI.indentLevel--;
        }
        
        /// <summary>
        /// 아이템 상호작용 설정 그리기
        /// </summary>
        private void DrawItemInteractionSettings(SerializedProperty stepProperty, int stepIndex)
        {
            // 아이템 상호작용 폴드아웃 초기화
            if (!itemInteractionFoldouts.ContainsKey(stepIndex))
            {
                itemInteractionFoldouts[stepIndex] = true;
            }
            
            // 아이템 상호작용 사용 여부
            SerializedProperty useItemInteractionProperty = stepProperty.FindPropertyRelative("useItemInteraction");
            useItemInteractionProperty.boolValue = EditorGUILayout.ToggleLeft("아이템 상호작용 사용", useItemInteractionProperty.boolValue);
            
            if (useItemInteractionProperty.boolValue)
            {
                EditorGUILayout.Space(5);
                itemInteractionFoldouts[stepIndex] = EditorGUILayout.Foldout(itemInteractionFoldouts[stepIndex], 
                    "아이템 상호작용 설정", true);
                
                if (itemInteractionFoldouts[stepIndex])
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUILayout.PropertyField(stepProperty.FindPropertyRelative("itemInteractionId"), new GUIContent("상호작용 ID"));
                    
                    EditorGUILayout.HelpBox("이 ID는 InteractionData 에셋의 ID를 참조해야 합니다.", MessageType.Info);
                    
                    if (GUILayout.Button("상호작용 ID 찾기", GUILayout.Height(25)))
                    {
                        // 상호작용 데이터 찾기
                        FindInteractionData();
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        /// <summary>
        /// 액션 버튼 설정 그리기
        /// </summary>
        private void DrawActionButtonSettings(SerializedProperty stepProperty, int stepIndex)
        {
            // 액션 버튼 폴드아웃 초기화
            if (!actionButtonFoldouts.ContainsKey(stepIndex))
            {
                actionButtonFoldouts[stepIndex] = true;
            }
            
            // 액션 버튼 사용 여부
            SerializedProperty useActionButtonProperty = stepProperty.FindPropertyRelative("useActionButton");
            useActionButtonProperty.boolValue = EditorGUILayout.ToggleLeft("액션 버튼 사용", useActionButtonProperty.boolValue);
            
            if (useActionButtonProperty.boolValue)
            {
                EditorGUILayout.Space(5);
                actionButtonFoldouts[stepIndex] = EditorGUILayout.Foldout(actionButtonFoldouts[stepIndex], 
                    "액션 버튼 설정", true);
                
                if (actionButtonFoldouts[stepIndex])
                {
                    EditorGUI.indentLevel++;
                    
                    // 액션 버튼 목록 그리기
                    actionButtonLists[stepIndex]?.DoLayoutList();
                    
                    // 선택된 액션 버튼이 있으면 상세 정보 표시
                    if (actionButtonLists[stepIndex] != null && 
                        actionButtonLists[stepIndex].index >= 0 && 
                        actionButtonLists[stepIndex].index < actionButtonLists[stepIndex].serializedProperty.arraySize)
                    {
                        EditorGUILayout.Space(5);
                        SerializedProperty buttonProperty = actionButtonLists[stepIndex].serializedProperty.GetArrayElementAtIndex(actionButtonLists[stepIndex].index);
                        
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField("버튼 상세 정보", EditorStyles.boldLabel);
                        
                        EditorGUILayout.PropertyField(buttonProperty.FindPropertyRelative("buttonId"), new GUIContent("버튼 ID"));
                        EditorGUILayout.PropertyField(buttonProperty.FindPropertyRelative("buttonText"), new GUIContent("버튼 텍스트"));
                        EditorGUILayout.PropertyField(buttonProperty.FindPropertyRelative("buttonIcon"), new GUIContent("버튼 아이콘"));
                        EditorGUILayout.PropertyField(buttonProperty.FindPropertyRelative("isCorrectOption"), new GUIContent("올바른 옵션"));
                        
                        // 다중 버튼 설정
                        SerializedProperty requiresMultipleButtonsProperty = buttonProperty.FindPropertyRelative("requiresMultipleButtons");
                        requiresMultipleButtonsProperty.boolValue = EditorGUILayout.ToggleLeft("다중 버튼 필요", requiresMultipleButtonsProperty.boolValue);
                        
                        if (requiresMultipleButtonsProperty.boolValue)
                        {
                            EditorGUILayout.PropertyField(buttonProperty.FindPropertyRelative("secondRequiredButtonId"), new GUIContent("두 번째 필요 버튼 ID"));
                        }
                        
                        // 잘못된 옵션이면 페널티 설정
                        SerializedProperty isCorrectProperty = buttonProperty.FindPropertyRelative("isCorrectOption");
                        if (isCorrectProperty != null && !isCorrectProperty.boolValue)
                        {
                            EditorGUILayout.Space(5);
                            EditorGUILayout.LabelField("잘못된 선택 시 페널티", EditorStyles.boldLabel);
                            DrawPenaltyDataEditor(buttonProperty.FindPropertyRelative("incorrectChoicePenalty"));
                        }
                        
                        EditorGUILayout.EndVertical();
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        /// <summary>
        /// 상호작용 데이터 찾기
        /// </summary>
        private void FindInteractionData()
        {
            // 상호작용 데이터를 가진 팝업 창 표시
            InteractionDataSelector.ShowWindow((InteractionData selectedData) => {
                if (selectedData != null && selectedStepIndex >= 0 && selectedStepIndex < stepsList.serializedProperty.arraySize)
                {
                    SerializedProperty stepProperty = stepsList.serializedProperty.GetArrayElementAtIndex(selectedStepIndex);
                    SerializedProperty itemInteractionIdProperty = stepProperty.FindPropertyRelative("itemInteractionId");
                    
                    if (itemInteractionIdProperty != null)
                    {
                        itemInteractionIdProperty.stringValue = selectedData.id;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }
                }
            });
        }
    }
    
    /// <summary>
    /// 상호작용 데이터 선택 창
    /// </summary>
    public class InteractionDataSelector : EditorWindow
    {
        private static System.Action<InteractionData> onSelectCallback;
        private static List<InteractionData> interactionDataList = new List<InteractionData>();
        private Vector2 scrollPosition;
        
        public static void ShowWindow(System.Action<InteractionData> callback)
        {
            onSelectCallback = callback;
            
            // 모든 InteractionData 에셋 찾기
            string[] guids = AssetDatabase.FindAssets("t:InteractionData");
            interactionDataList.Clear();
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InteractionData data = AssetDatabase.LoadAssetAtPath<InteractionData>(path);
                if (data != null)
                {
                    interactionDataList.Add(data);
                }
            }
            
            InteractionDataSelector window = EditorWindow.GetWindow<InteractionDataSelector>("상호작용 데이터 선택");
            window.minSize = new Vector2(300, 400);
            window.Show();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("상호작용 데이터 선택", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            if (interactionDataList.Count == 0)
            {
                EditorGUILayout.HelpBox("상호작용 데이터가 없습니다.", MessageType.Info);
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            for (int i = 0; i < interactionDataList.Count; i++)
            {
                InteractionData data = interactionDataList[i];
                
                EditorGUILayout.BeginHorizontal("box");
                
                string displayName = !string.IsNullOrEmpty(data.displayName) ? data.displayName : data.name;
                EditorGUILayout.LabelField($"{displayName} (ID: {data.id})");
                
                if (GUILayout.Button("선택", GUILayout.Width(60)))
                {
                    onSelectCallback?.Invoke(data);
                    Close();
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
    }
}