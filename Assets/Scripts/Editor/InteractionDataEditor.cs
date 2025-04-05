using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

namespace NursingGame.Editor
{
    /// <summary>
    /// InteractionData를 위한 커스텀 에디터
    /// </summary>
    [CustomEditor(typeof(InteractionData))]
    public class InteractionDataEditor : UnityEditor.Editor
    {
        // 폴드아웃 상태
        private bool showBasicInfo = true;
        private bool showStages = true;
        private bool showFeedback = false;
        
        // 단계 리스트
        private ReorderableList stagesList;
        private int selectedStageIndex = -1;
        
        // 서브 에디터들
        private Dictionary<int, bool> stageFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> dragSettingsFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> objectCreationFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> conditionalClickFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> sustainedClickFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> objectDeleteMoveFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> quizSettingsFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> miniGameFoldouts = new Dictionary<int, bool>();
        
        // 객체 생성 리스트
        private Dictionary<int, ReorderableList> objectsToCreateLists = new Dictionary<int, ReorderableList>();
        private Dictionary<int, ReorderableList> conditionalClickLists = new Dictionary<int, ReorderableList>();
        private Dictionary<int, ReorderableList> objectMovementLists = new Dictionary<int, ReorderableList>();
        
        private void OnEnable()
        {
            // 단계 목록 초기화
            InitializeStagesList();
        }
        
        /// <summary>
        /// 단계 목록 초기화
        /// </summary>
        private void InitializeStagesList()
        {
            SerializedProperty stagesProperty = serializedObject.FindProperty("stages");
            
            stagesList = new ReorderableList(serializedObject, stagesProperty, true, true, true, true);
            
            // 헤더 그리기
            stagesList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "상호작용 단계");
            };
            
            // 요소 그리기
            stagesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                SerializedProperty element = stagesProperty.GetArrayElementAtIndex(index);
                SerializedProperty nameProperty = element.FindPropertyRelative("stageName");
                SerializedProperty typeProperty = element.FindPropertyRelative("interactionType");
                
                string stageName = nameProperty != null && !string.IsNullOrEmpty(nameProperty.stringValue) ? 
                    nameProperty.stringValue : $"단계 {index + 1}";
                
                string typeStr = "없음";
                if (typeProperty != null)
                {
                    typeStr = typeProperty.enumDisplayNames[typeProperty.enumValueIndex];
                }
                
                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, $"{stageName} ({typeStr})");
            };
            
            // 선택 콜백
            stagesList.onSelectCallback = (ReorderableList list) => {
                selectedStageIndex = list.index;
                Repaint();
            };
            
            // 추가 콜백
            stagesList.onAddCallback = (ReorderableList list) => {
                int index = list.count;
                list.serializedProperty.arraySize++;
                
                SerializedProperty newStage = list.serializedProperty.GetArrayElementAtIndex(index);
                
                // 기본값 설정
                SerializedProperty idProperty = newStage.FindPropertyRelative("id");
                SerializedProperty nameProperty = newStage.FindPropertyRelative("stageName");
                
                if (idProperty != null)
                    idProperty.stringValue = System.Guid.NewGuid().ToString().Substring(0, 8);
                
                if (nameProperty != null)
                    nameProperty.stringValue = $"새 단계 {index + 1}";
                
                list.index = index;
                selectedStageIndex = index;
                
                // 모든 폴드아웃 초기화
                stageFoldouts[index] = true;
                
                serializedObject.ApplyModifiedProperties();
            };
            
            // 제거 콜백
            stagesList.onRemoveCallback = (ReorderableList list) => {
                // 삭제 전 확인
                if (EditorUtility.DisplayDialog("단계 삭제", 
                    "이 단계를 삭제하시겠습니까?", "삭제", "취소"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    if (selectedStageIndex >= list.count)
                    {
                        selectedStageIndex = list.count - 1;
                    }
                }
            };
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            InteractionData interactionData = (InteractionData)target;
            
            // 스타일 설정
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 13;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("상호작용 데이터 에디터", headerStyle);
            EditorGUILayout.Space(5);
            
            // 기본 정보 섹션
            DrawBasicInfoSection(interactionData);
            
            EditorGUILayout.Space(10);
            
            // 단계 섹션
            DrawStagesSection();
            
            EditorGUILayout.Space(10);
            
            // 피드백 섹션
            DrawFeedbackSection();
            
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
        private void DrawBasicInfoSection(InteractionData interactionData)
        {
            showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "기본 정보", true);
            if (showBasicInfo)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("id"), new GUIContent("상호작용 ID"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"), new GUIContent("상호작용 이름"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), new GUIContent("설명"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("guideMessage"), new GUIContent("가이드 메시지"));
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// 단계 섹션 그리기
        /// </summary>
        private void DrawStagesSection()
        {
            showStages = EditorGUILayout.Foldout(showStages, "상호작용 단계", true);
            if (showStages)
            {
                EditorGUI.indentLevel++;
                
                // 단계 목록 그리기
                stagesList.DoLayoutList();
                
                // 선택된 단계가 있으면 상세 정보 표시
                if (selectedStageIndex >= 0 && selectedStageIndex < stagesList.serializedProperty.arraySize)
                {
                    EditorGUILayout.Space(5);
                    SerializedProperty stageProperty = stagesList.serializedProperty.GetArrayElementAtIndex(selectedStageIndex);
                    
                    // 단계 상세 정보 그리기
                    DrawStageDetails(stageProperty, selectedStageIndex);
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// 피드백 섹션 그리기
        /// </summary>
        private void DrawFeedbackSection()
        {
            showFeedback = EditorGUILayout.Foldout(showFeedback, "피드백 설정", true);
            if (showFeedback)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("successSound"), new GUIContent("성공 효과음"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("errorSound"), new GUIContent("오류 효과음"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("successFeedbackSprite"), new GUIContent("성공 이미지"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("errorFeedbackSprite"), new GUIContent("오류 이미지"));
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// <summary>
        /// 단계 상세 정보 그리기
        /// </summary>
        private void DrawStageDetails(SerializedProperty stageProperty, int stageIndex)
        {
            // 단계 펼치기 상태 초기화
            if (!stageFoldouts.ContainsKey(stageIndex))
            {
                stageFoldouts[stageIndex] = true;
            }
            
            EditorGUILayout.BeginVertical("box");
            
            // 단계 헤더
            stageFoldouts[stageIndex] = EditorGUILayout.Foldout(stageFoldouts[stageIndex], 
                $"단계 {stageIndex + 1} 상세 정보", true);
            
            if (stageFoldouts[stageIndex])
            {
                EditorGUILayout.Space(5);
                
                // 기본 정보
                EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("id"), new GUIContent("단계 ID"));
                EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("stageName"), new GUIContent("단계 이름"));
                EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("guideMessage"), new GUIContent("가이드 메시지"));
                EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("interactionType"), new GUIContent("상호작용 유형"));
                
                EditorGUILayout.Space(5);
                
                // 단계 순서
                EditorGUILayout.LabelField("단계 순서 설정", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("isRequired"), new GUIContent("필수 단계"));
                EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("isOrderImportant"), new GUIContent("순서 중요"));
                
                EditorGUILayout.Space(5);
                
                // 페널티 설정
                EditorGUILayout.LabelField("페널티 설정", EditorStyles.boldLabel);
                DrawPenaltyDataEditor(stageProperty.FindPropertyRelative("penaltyData"));
                
                EditorGUILayout.Space(5);
                
                // 상호작용 유형별 설정
                SerializedProperty interactionTypeProperty = stageProperty.FindPropertyRelative("interactionType");
                if (interactionTypeProperty != null)
                {
                    InteractionType interactionType = (InteractionType)interactionTypeProperty.enumValueIndex;
                    
                    // 드래그 설정
                    if (interactionType == InteractionType.Drag || 
                        interactionType == InteractionType.TwoFingerDrag || 
                        interactionType == InteractionType.SwipeUp || 
                        interactionType == InteractionType.SwipeDown || 
                        interactionType == InteractionType.SwipeLeft || 
                        interactionType == InteractionType.SwipeRight)
                    {
                        DrawDragSettings(stageProperty, stageIndex);
                    }
                    
                    // 조건부 클릭 설정
                    if (interactionType == InteractionType.SingleClick)
                    {
                        DrawConditionalClickSettings(stageProperty, stageIndex);
                        DrawSustainedClickSettings(stageProperty, stageIndex);
                    }
                    
                    // 퀴즈 설정
                    if (interactionType == InteractionType.Quiz)
                    {
                        DrawQuizSettings(stageProperty, stageIndex);
                    }
                }
                
                // 공통 설정 - 오브젝트 생성
                DrawObjectCreationSettings(stageProperty, stageIndex);
                
                // 공통 설정 - 오브젝트 삭제/이동
                DrawObjectDeleteMoveSettings(stageProperty, stageIndex);
                
                // 미니게임 설정
                DrawMiniGameSettings(stageProperty, stageIndex);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// 페널티 데이터 에디터 그리기
        /// </summary>
        private void DrawPenaltyDataEditor(SerializedProperty penaltyProperty)
        {
            if (penaltyProperty == null) return;
            
            EditorGUILayout.PropertyField(penaltyProperty.FindPropertyRelative("penaltyType"), new GUIContent("페널티 유형"));
            EditorGUILayout.PropertyField(penaltyProperty.FindPropertyRelative("speaker"), new GUIContent("화자"));
            EditorGUILayout.PropertyField(penaltyProperty.FindPropertyRelative("penaltyMessage"), new GUIContent("페널티 메시지"));
            EditorGUILayout.PropertyField(penaltyProperty.FindPropertyRelative("databaseMessage"), new GUIContent("데이터베이스 메시지"));
        }
        
        /// <summary>
        /// 드래그 설정 그리기
        /// </summary>
        private void DrawDragSettings(SerializedProperty stageProperty, int stageIndex)
        {
            // 드래그 폴드아웃 초기화
            if (!dragSettingsFoldouts.ContainsKey(stageIndex))
            {
                dragSettingsFoldouts[stageIndex] = true;
            }
            
            // 드래그 사용 여부
            SerializedProperty useDragProperty = stageProperty.FindPropertyRelative("useDragInteraction");
            useDragProperty.boolValue = EditorGUILayout.ToggleLeft("드래그 상호작용 사용", useDragProperty.boolValue);
            
            if (useDragProperty.boolValue)
            {
                EditorGUILayout.Space(5);
                dragSettingsFoldouts[stageIndex] = EditorGUILayout.Foldout(dragSettingsFoldouts[stageIndex], 
                    "드래그 설정", true);
                
                if (dragSettingsFoldouts[stageIndex])
                {
                    EditorGUI.indentLevel++;
                    
                    // 기본 드래그 설정
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("requiredDragAngle"), new GUIContent("필요한 드래그 각도"));
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("dragAngleTolerance"), new GUIContent("각도 허용 오차"));
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("dragArrowSprite"), new GUIContent("드래그 화살표"));
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("dragArrowPosition"), new GUIContent("화살표 위치"));
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("dragArrowRotation"), new GUIContent("화살표 회전"));
                    
                    EditorGUILayout.Space(5);
                    
                    // 두 손가락 드래그 설정
                    SerializedProperty useTwoFingerDragProperty = stageProperty.FindPropertyRelative("useTwoFingerDrag");
                    useTwoFingerDragProperty.boolValue = EditorGUILayout.ToggleLeft("두 손가락 드래그 사용", useTwoFingerDragProperty.boolValue);
                    
                    if (useTwoFingerDragProperty.boolValue)
                    {
                        EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("requireSameDirection"), new GUIContent("같은 방향 필요"));
                        
                        SerializedProperty twoFingerSettingProperty = stageProperty.FindPropertyRelative("twoFingerDragSetting");
                        if (twoFingerSettingProperty != null)
                        {
                            EditorGUILayout.LabelField("첫 번째 손가락", EditorStyles.boldLabel);
                            EditorGUILayout.PropertyField(twoFingerSettingProperty.FindPropertyRelative("firstFingerDragAngle"), new GUIContent("드래그 각도"));
                            EditorGUILayout.PropertyField(twoFingerSettingProperty.FindPropertyRelative("firstFingerAngleTolerance"), new GUIContent("각도 허용 오차"));
                            
                            EditorGUILayout.LabelField("두 번째 손가락", EditorStyles.boldLabel);
                            EditorGUILayout.PropertyField(twoFingerSettingProperty.FindPropertyRelative("secondFingerDragAngle"), new GUIContent("드래그 각도"));
                            EditorGUILayout.PropertyField(twoFingerSettingProperty.FindPropertyRelative("secondFingerAngleTolerance"), new GUIContent("각도 허용 오차"));
                            
                            EditorGUILayout.PropertyField(twoFingerSettingProperty.FindPropertyRelative("minDragDistance"), new GUIContent("최소 드래그 거리"));
                        }
                    }
                    
                    EditorGUILayout.Space(5);
                    
                    // 드래그 대상 설정
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("dragTargetTags"), new GUIContent("드래그 대상 태그"));
                    
                    EditorGUILayout.Space(5);
                    
                    // 드래그 이동 설정
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("dragMoveType"), new GUIContent("드래그 이동 유형"));
                    SerializedProperty dragMoveTypeProperty = stageProperty.FindPropertyRelative("dragMoveType");
                    if (dragMoveTypeProperty != null)
                    {
                        DragMoveType dragMoveType = (DragMoveType)dragMoveTypeProperty.enumValueIndex;
                        
                        if (dragMoveType == DragMoveType.Fixed)
                        {
                            EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("fixedMovementDirection"), new GUIContent("고정 이동 방향"));
                            EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("fixedMovementAmount"), new GUIContent("고정 이동량"));
                        }
                        else if (dragMoveType == DragMoveType.FollowDrag)
                        {
                            EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("boundaryTag"), new GUIContent("경계 객체 태그"));
                        }
                        
                        EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("collisionTag"), new GUIContent("충돌 감지 태그"));
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        /// <summary>
        /// 오브젝트 생성 설정 그리기
        /// </summary>
        private void DrawObjectCreationSettings(SerializedProperty stageProperty, int stageIndex)
        {
            // 오브젝트 생성 폴드아웃 초기화
            if (!objectCreationFoldouts.ContainsKey(stageIndex))
            {
                objectCreationFoldouts[stageIndex] = true;
            }
            
            // 오브젝트 리스트 초기화
            if (!objectsToCreateLists.ContainsKey(stageIndex))
            {
                SerializedProperty objectsToCreateProperty = stageProperty.FindPropertyRelative("objectsToCreate");
                if (objectsToCreateProperty != null)
                {
                    objectsToCreateLists[stageIndex] = new ReorderableList(serializedObject, objectsToCreateProperty, 
                        true, true, true, true);
                    
                    // 헤더 그리기
                    objectsToCreateLists[stageIndex].drawHeaderCallback = (Rect rect) => {
                        EditorGUI.LabelField(rect, "생성할 오브젝트");
                    };
                    
                    // 요소 그리기
                    objectsToCreateLists[stageIndex].drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                        SerializedProperty element = objectsToCreateProperty.GetArrayElementAtIndex(index);
                        SerializedProperty nameProperty = element.FindPropertyRelative("objectName");
                        
                        string objectName = nameProperty != null && !string.IsNullOrEmpty(nameProperty.stringValue) ? 
                            nameProperty.stringValue : $"오브젝트 {index + 1}";
                        
                        rect.y += 2;
                        rect.height = EditorGUIUtility.singleLineHeight;
                        EditorGUI.LabelField(rect, objectName);
                    };
                    
                    // 추가 콜백
                    objectsToCreateLists[stageIndex].onAddCallback = (ReorderableList list) => {
                        int index = list.count;
                        list.serializedProperty.arraySize++;
                        
                        SerializedProperty newObj = list.serializedProperty.GetArrayElementAtIndex(index);
                        
                        // 기본값 설정
                        SerializedProperty nameProperty = newObj.FindPropertyRelative("objectName");
                        SerializedProperty scaleProperty = newObj.FindPropertyRelative("scale");
                        
                        if (nameProperty != null)
                            nameProperty.stringValue = $"새 오브젝트 {index + 1}";
                        
                        if (scaleProperty != null)
                            scaleProperty.vector3Value = Vector3.one;
                        
                        serializedObject.ApplyModifiedProperties();
                    };
                }
            }
            
            // 오브젝트 생성 사용 여부
            SerializedProperty createObjectsProperty = stageProperty.FindPropertyRelative("createObjects");
            createObjectsProperty.boolValue = EditorGUILayout.ToggleLeft("오브젝트 생성 사용", createObjectsProperty.boolValue);
            
            if (createObjectsProperty.boolValue)
            {
                EditorGUILayout.Space(5);
                objectCreationFoldouts[stageIndex] = EditorGUILayout.Foldout(objectCreationFoldouts[stageIndex], 
                    "오브젝트 생성 설정", true);
                
                if (objectCreationFoldouts[stageIndex])
                {
                    EditorGUI.indentLevel++;
                    
                    // 오브젝트 목록 그리기
                    objectsToCreateLists[stageIndex]?.DoLayoutList();
                    
                    // 선택된 오브젝트가 있으면 상세 정보 표시
                    if (objectsToCreateLists[stageIndex] != null && 
                        objectsToCreateLists[stageIndex].index >= 0 && 
                        objectsToCreateLists[stageIndex].index < objectsToCreateLists[stageIndex].serializedProperty.arraySize)
                    {
                        EditorGUILayout.Space(5);
                        SerializedProperty objectProperty = objectsToCreateLists[stageIndex].serializedProperty.GetArrayElementAtIndex(objectsToCreateLists[stageIndex].index);
                        
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField("오브젝트 상세 정보", EditorStyles.boldLabel);
                        
                        EditorGUILayout.PropertyField(objectProperty.FindPropertyRelative("tag"), new GUIContent("태그"));
                        EditorGUILayout.PropertyField(objectProperty.FindPropertyRelative("objectName"), new GUIContent("이름"));
                        EditorGUILayout.PropertyField(objectProperty.FindPropertyRelative("prefab"), new GUIContent("프리팹"));
                        EditorGUILayout.PropertyField(objectProperty.FindPropertyRelative("position"), new GUIContent("위치"));
                        EditorGUILayout.PropertyField(objectProperty.FindPropertyRelative("rotation"), new GUIContent("회전"));
                        EditorGUILayout.PropertyField(objectProperty.FindPropertyRelative("scale"), new GUIContent("크기"));
                        EditorGUILayout.PropertyField(objectProperty.FindPropertyRelative("setNativeSize"), new GUIContent("네이티브 사이즈 설정"));
                        
                        EditorGUILayout.EndVertical();
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        /// <summary>
        /// 조건부 클릭 설정 그리기
        /// </summary>
        private void DrawConditionalClickSettings(SerializedProperty stageProperty, int stageIndex)
        {
            // 조건부 클릭 폴드아웃 초기화
            if (!conditionalClickFoldouts.ContainsKey(stageIndex))
            {
                conditionalClickFoldouts[stageIndex] = true;
            }
            
            // 조건부 클릭 리스트 초기화
            if (!conditionalClickLists.ContainsKey(stageIndex))
            {
                SerializedProperty conditionalClickProperty = stageProperty.FindPropertyRelative("conditionalClickSettings");
                if (conditionalClickProperty != null)
                {
                    conditionalClickLists[stageIndex] = new ReorderableList(serializedObject, conditionalClickProperty, 
                        true, true, true, true);
                    
                    // 헤더 그리기
                    conditionalClickLists[stageIndex].drawHeaderCallback = (Rect rect) => {
                        EditorGUI.LabelField(rect, "조건부 클릭 설정");
                    };
                    
                    // 요소 그리기
                    conditionalClickLists[stageIndex].drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                        SerializedProperty element = conditionalClickProperty.GetArrayElementAtIndex(index);
                        SerializedProperty tagProperty = element.FindPropertyRelative("targetTag");
                        SerializedProperty isCorrectProperty = element.FindPropertyRelative("isCorrectOption");
                        
                        string targetTag = tagProperty != null && !string.IsNullOrEmpty(tagProperty.stringValue) ? 
                            tagProperty.stringValue : "태그 없음";
                        string correctStr = isCorrectProperty != null && isCorrectProperty.boolValue ? "올바른 옵션" : "잘못된 옵션";
                        
                        rect.y += 2;
                        rect.height = EditorGUIUtility.singleLineHeight;
                        EditorGUI.LabelField(rect, $"{targetTag} ({correctStr})");
                    };
                    
                    // 추가 콜백
                    conditionalClickLists[stageIndex].onAddCallback = (ReorderableList list) => {
                        int index = list.count;
                        list.serializedProperty.arraySize++;
                        serializedObject.ApplyModifiedProperties();
                    };
                }
            }
            
            // 조건부 클릭 사용 여부
            SerializedProperty useConditionalClickProperty = stageProperty.FindPropertyRelative("useConditionalClick");
            useConditionalClickProperty.boolValue = EditorGUILayout.ToggleLeft("조건부 클릭 사용", useConditionalClickProperty.boolValue);
            
            if (useConditionalClickProperty.boolValue)
            {
                EditorGUILayout.Space(5);
                conditionalClickFoldouts[stageIndex] = EditorGUILayout.Foldout(conditionalClickFoldouts[stageIndex], 
                    "조건부 클릭 설정", true);
                
                if (conditionalClickFoldouts[stageIndex])
                {
                    EditorGUI.indentLevel++;
                    
                    // 조건부 클릭 목록 그리기
                    conditionalClickLists[stageIndex]?.DoLayoutList();
                    
                    // 선택된 조건부 클릭이 있으면 상세 정보 표시
                    if (conditionalClickLists[stageIndex] != null && 
                        conditionalClickLists[stageIndex].index >= 0 && 
                        conditionalClickLists[stageIndex].index < conditionalClickLists[stageIndex].serializedProperty.arraySize)
                    {
                        EditorGUILayout.Space(5);
                        SerializedProperty clickProperty = conditionalClickLists[stageIndex].serializedProperty.GetArrayElementAtIndex(conditionalClickLists[stageIndex].index);
                        
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField("조건부 클릭 상세 정보", EditorStyles.boldLabel);
                        
                        EditorGUILayout.PropertyField(clickProperty.FindPropertyRelative("targetTag"), new GUIContent("대상 태그"));
                        EditorGUILayout.PropertyField(clickProperty.FindPropertyRelative("isCorrectOption"), new GUIContent("올바른 옵션"));
                        EditorGUILayout.PropertyField(clickProperty.FindPropertyRelative("successMessage"), new GUIContent("성공 메시지"));
                        
                        // 잘못된 옵션이면 페널티 설정 표시
                        SerializedProperty isCorrectProperty = clickProperty.FindPropertyRelative("isCorrectOption");
                        if (isCorrectProperty != null && !isCorrectProperty.boolValue)
                        {
                            EditorGUILayout.LabelField("잘못된 선택 시 페널티", EditorStyles.boldLabel);
                            DrawPenaltyDataEditor(clickProperty.FindPropertyRelative("incorrectChoicePenalty"));
                        }
                        
                        EditorGUILayout.EndVertical();
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        /// <summary>
        /// 지속 클릭 설정 그리기
        /// </summary>
        private void DrawSustainedClickSettings(SerializedProperty stageProperty, int stageIndex)
        {
            // 지속 클릭 폴드아웃 초기화
            if (!sustainedClickFoldouts.ContainsKey(stageIndex))
            {
                sustainedClickFoldouts[stageIndex] = true;
            }
            
            // 지속 클릭 사용 여부
            SerializedProperty useSustainedClickProperty = stageProperty.FindPropertyRelative("useSustainedClick");
            useSustainedClickProperty.boolValue = EditorGUILayout.ToggleLeft("지속 클릭 사용", useSustainedClickProperty.boolValue);
            
            if (useSustainedClickProperty.boolValue)
            {
                EditorGUILayout.Space(5);
                sustainedClickFoldouts[stageIndex] = EditorGUILayout.Foldout(sustainedClickFoldouts[stageIndex], 
                    "지속 클릭 설정", true);
                
                if (sustainedClickFoldouts[stageIndex])
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("requiredPressDuration"), new GUIContent("필요한 누르는 시간(초)"));
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("sustainedClickTargetTag"), new GUIContent("지속 클릭 대상 태그"));
                    
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("조기 해제 페널티", EditorStyles.boldLabel);
                    DrawPenaltyDataEditor(stageProperty.FindPropertyRelative("earlyReleaseData"));
                    
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        /// <summary>
        /// 오브젝트 삭제/이동 설정 그리기
        /// </summary>
        private void DrawObjectDeleteMoveSettings(SerializedProperty stageProperty, int stageIndex)
        {
            // 오브젝트 삭제/이동 폴드아웃 초기화
            if (!objectDeleteMoveFoldouts.ContainsKey(stageIndex))
            {
                objectDeleteMoveFoldouts[stageIndex] = true;
            }
            
            // 오브젝트 이동 리스트 초기화
            if (!objectMovementLists.ContainsKey(stageIndex))
            {
                SerializedProperty movementsProperty = stageProperty.FindPropertyRelative("objectMovements");
                if (movementsProperty != null)
                {
                    objectMovementLists[stageIndex] = new ReorderableList(serializedObject, movementsProperty, 
                        true, true, true, true);
                    
                    // 헤더 그리기
                    objectMovementLists[stageIndex].drawHeaderCallback = (Rect rect) => {
                        EditorGUI.LabelField(rect, "오브젝트 이동 설정");
                    };
                    
                    // 요소 그리기
                    objectMovementLists[stageIndex].drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                        SerializedProperty element = movementsProperty.GetArrayElementAtIndex(index);
                        SerializedProperty tagProperty = element.FindPropertyRelative("targetTag");
                        
                        string targetTag = tagProperty != null && !string.IsNullOrEmpty(tagProperty.stringValue) ? 
                            tagProperty.stringValue : "태그 없음";
                        
                        rect.y += 2;
                        rect.height = EditorGUIUtility.singleLineHeight;
                        EditorGUI.LabelField(rect, targetTag);
                    };
                    
                    // 추가 콜백
                    objectMovementLists[stageIndex].onAddCallback = (ReorderableList list) => {
                        int index = list.count;
                        list.serializedProperty.arraySize++;
                        serializedObject.ApplyModifiedProperties();
                    };
                }
            }
            
            // 오브젝트 삭제 사용 여부
            SerializedProperty deleteObjectsProperty = stageProperty.FindPropertyRelative("deleteObjects");
            deleteObjectsProperty.boolValue = EditorGUILayout.ToggleLeft("오브젝트 삭제 사용", deleteObjectsProperty.boolValue);
            
            // 오브젝트 이동 사용 여부
            SerializedProperty moveObjectsProperty = stageProperty.FindPropertyRelative("moveObjects");
            moveObjectsProperty.boolValue = EditorGUILayout.ToggleLeft("오브젝트 이동 사용", moveObjectsProperty.boolValue);
            
            if (deleteObjectsProperty.boolValue || moveObjectsProperty.boolValue)
            {
                EditorGUILayout.Space(5);
                objectDeleteMoveFoldouts[stageIndex] = EditorGUILayout.Foldout(objectDeleteMoveFoldouts[stageIndex], 
                    "오브젝트 삭제/이동 설정", true);
                
                if (objectDeleteMoveFoldouts[stageIndex])
                {
                    EditorGUI.indentLevel++;
                    
                    // 오브젝트 삭제 설정
                    if (deleteObjectsProperty.boolValue)
                    {
                        EditorGUILayout.LabelField("오브젝트 삭제 설정", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("tagsToDelete"), new GUIContent("삭제할 오브젝트 태그"));
                        EditorGUILayout.Space(5);
                    }
                    
                    // 오브젝트 이동 설정
                    if (moveObjectsProperty.boolValue)
                    {
                        EditorGUILayout.LabelField("오브젝트 이동 설정", EditorStyles.boldLabel);
                        
                        // 오브젝트 이동 목록 그리기
                        objectMovementLists[stageIndex]?.DoLayoutList();
                        
                        // 선택된 오브젝트 이동이 있으면 상세 정보 표시
                        if (objectMovementLists[stageIndex] != null && 
                            objectMovementLists[stageIndex].index >= 0 && 
                            objectMovementLists[stageIndex].index < objectMovementLists[stageIndex].serializedProperty.arraySize)
                        {
                            EditorGUILayout.Space(5);
                            SerializedProperty movementProperty = objectMovementLists[stageIndex].serializedProperty.GetArrayElementAtIndex(objectMovementLists[stageIndex].index);
                            
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField("이동 상세 정보", EditorStyles.boldLabel);
                            
                            EditorGUILayout.PropertyField(movementProperty.FindPropertyRelative("targetTag"), new GUIContent("대상 태그"));
                            EditorGUILayout.PropertyField(movementProperty.FindPropertyRelative("targetPosition"), new GUIContent("목표 위치"));
                            EditorGUILayout.PropertyField(movementProperty.FindPropertyRelative("targetRotation"), new GUIContent("목표 회전"));
                            EditorGUILayout.PropertyField(movementProperty.FindPropertyRelative("movementDuration"), new GUIContent("이동 지속 시간"));
                            EditorGUILayout.PropertyField(movementProperty.FindPropertyRelative("movementCurve"), new GUIContent("이동 곡선"));
                            
                            EditorGUILayout.EndVertical();
                        }
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        /// <summary>
        /// 퀴즈 설정 그리기
        /// </summary>
        private void DrawQuizSettings(SerializedProperty stageProperty, int stageIndex)
        {
            // 퀴즈 설정 폴드아웃 초기화
            if (!quizSettingsFoldouts.ContainsKey(stageIndex))
            {
                quizSettingsFoldouts[stageIndex] = true;
            }
            
            // 퀴즈 사용 여부
            SerializedProperty useQuizPopupProperty = stageProperty.FindPropertyRelative("useQuizPopup");
            useQuizPopupProperty.boolValue = EditorGUILayout.ToggleLeft("퀴즈 팝업 사용", useQuizPopupProperty.boolValue);
            
            if (useQuizPopupProperty.boolValue)
            {
                EditorGUILayout.Space(5);
                quizSettingsFoldouts[stageIndex] = EditorGUILayout.Foldout(quizSettingsFoldouts[stageIndex], 
                    "퀴즈 설정", true);
                
                if (quizSettingsFoldouts[stageIndex])
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("quizPrefab"), new GUIContent("퀴즈 프리팹"));
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("quizQuestion"), new GUIContent("질문"));
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("optionImages"), new GUIContent("옵션 이미지"));
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("optionTexts"), new GUIContent("옵션 텍스트"));
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("correctOptionIndex"), new GUIContent("정답 옵션 인덱스"));
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("quizTimeLimit"), new GUIContent("시간 제한(초)"));
                    
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        /// <summary>
        /// 미니게임 설정 그리기
        /// </summary>
        private void DrawMiniGameSettings(SerializedProperty stageProperty, int stageIndex)
        {
            // 미니게임 폴드아웃 초기화
            if (!miniGameFoldouts.ContainsKey(stageIndex))
            {
                miniGameFoldouts[stageIndex] = true;
            }
            
            // 미니게임 사용 여부
            SerializedProperty useMiniGameProperty = stageProperty.FindPropertyRelative("useMiniGame");
            useMiniGameProperty.boolValue = EditorGUILayout.ToggleLeft("미니게임 사용", useMiniGameProperty.boolValue);
            
            if (useMiniGameProperty.boolValue)
            {
                EditorGUILayout.Space(5);
                miniGameFoldouts[stageIndex] = EditorGUILayout.Foldout(miniGameFoldouts[stageIndex], 
                    "미니게임 설정", true);
                
                if (miniGameFoldouts[stageIndex])
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUILayout.PropertyField(stageProperty.FindPropertyRelative("miniGamePrefab"), new GUIContent("미니게임 프리팹"));
                    EditorGUILayout.HelpBox("미니게임 상세 설정은 미니게임 프리팹 내에서 관리됩니다.", MessageType.Info);
                    
                    EditorGUI.indentLevel--;
                }
            }
        }
    }
}