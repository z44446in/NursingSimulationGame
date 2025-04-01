using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// NursingActionData의 커스텀 에디터
/// 인스펙터에서 시각적으로 상호작용 데이터를 편집할 수 있게 해줍니다.
/// </summary>
[CustomEditor(typeof(NursingActionData))]
public class NursingActionDataEditor : Editor
{
    // 시리얼라이즈된 프로퍼티 참조
    SerializedProperty actionIdProp;
    SerializedProperty actionNameProp;
    SerializedProperty descriptionProp;
    SerializedProperty isRequiredProp;
    SerializedProperty scoreWeightProp;
    SerializedProperty requiredItemsProp;
    SerializedProperty hintTextProp;
    SerializedProperty feedbackMessageProp;
    SerializedProperty interactionTypeProp;
    SerializedProperty guideTextProp;
    SerializedProperty tutorialPrefabProp;
    
    // 드래그 관련 속성
    SerializedProperty useDragInteractionProp;
    SerializedProperty requiredDragAngleProp;
    SerializedProperty dragAngleToleranceProp;
    SerializedProperty dragArrowSpriteProp;
    SerializedProperty dragArrowPositionProp;
    SerializedProperty dragArrowRotationProp;
    SerializedProperty dragStepsRequiredProp;
    
    // 클릭 관련 속성
    SerializedProperty useClickInteractionProp;
    SerializedProperty validClickAreaProp;
    SerializedProperty validClickTargetTagsProp;
    SerializedProperty clickHighlightSpriteProp;
    
    // 퀴즈 관련 속성
    SerializedProperty useQuizInteractionProp;
    SerializedProperty quizQuestionProp;
    SerializedProperty quizOptionsProp;
    SerializedProperty correctOptionIndexProp;
    
    // 오류 관련 속성
    SerializedProperty errorMessageProp;
    SerializedProperty penaltyTypeProp;
    
    // 시각적 피드백 속성
    SerializedProperty successFeedbackSpriteProp;
    SerializedProperty successSoundProp;
    SerializedProperty errorFeedbackSpriteProp;
    SerializedProperty errorSoundProp;
    
    // 다음 단계 조건 속성
    SerializedProperty waitForUserInputProp;
    SerializedProperty autoAdvanceDelayProp;
    
    // 에디터 도우미 속성
    SerializedProperty showDragSettingsProp;
    SerializedProperty showClickSettingsProp;
    SerializedProperty showQuizSettingsProp;
    
    // 폴드아웃 상태
    private bool showBasicInfo = true;
    private bool showInteractionSettings = true;
    private bool showFeedbackSettings = true;
    private bool showAdvancedSettings = false;
    
    private void OnEnable()
    {
        // 속성 참조 설정
        actionIdProp = serializedObject.FindProperty("actionId");
        actionNameProp = serializedObject.FindProperty("actionName");
        descriptionProp = serializedObject.FindProperty("description");
        isRequiredProp = serializedObject.FindProperty("isRequired");
        scoreWeightProp = serializedObject.FindProperty("scoreWeight");
        requiredItemsProp = serializedObject.FindProperty("requiredItems");
        hintTextProp = serializedObject.FindProperty("hintText");
        feedbackMessageProp = serializedObject.FindProperty("feedbackMessage");
        interactionTypeProp = serializedObject.FindProperty("interactionType");
        guideTextProp = serializedObject.FindProperty("guideText");
        tutorialPrefabProp = serializedObject.FindProperty("tutorialPrefab");
        
        // 드래그 관련 속성
        useDragInteractionProp = serializedObject.FindProperty("useDragInteraction");
        requiredDragAngleProp = serializedObject.FindProperty("requiredDragAngle");
        dragAngleToleranceProp = serializedObject.FindProperty("dragAngleTolerance");
        dragArrowSpriteProp = serializedObject.FindProperty("dragArrowSprite");
        dragArrowPositionProp = serializedObject.FindProperty("dragArrowPosition");
        dragArrowRotationProp = serializedObject.FindProperty("dragArrowRotation");
        dragStepsRequiredProp = serializedObject.FindProperty("dragStepsRequired");
        
        // 클릭 관련 속성
        useClickInteractionProp = serializedObject.FindProperty("useClickInteraction");
        validClickAreaProp = serializedObject.FindProperty("validClickArea");
        validClickTargetTagsProp = serializedObject.FindProperty("validClickTargetTags");
        clickHighlightSpriteProp = serializedObject.FindProperty("clickHighlightSprite");
        
        // 퀴즈 관련 속성
        useQuizInteractionProp = serializedObject.FindProperty("useQuizInteraction");
        quizQuestionProp = serializedObject.FindProperty("quizQuestion");
        quizOptionsProp = serializedObject.FindProperty("quizOptions");
        correctOptionIndexProp = serializedObject.FindProperty("correctOptionIndex");
        
        // 오류 관련 속성
        errorMessageProp = serializedObject.FindProperty("errorMessage");
        penaltyTypeProp = serializedObject.FindProperty("penaltyType");
        
        // 시각적 피드백 속성
        successFeedbackSpriteProp = serializedObject.FindProperty("successFeedbackSprite");
        successSoundProp = serializedObject.FindProperty("successSound");
        errorFeedbackSpriteProp = serializedObject.FindProperty("errorFeedbackSprite");
        errorSoundProp = serializedObject.FindProperty("errorSound");
        
        // 다음 단계 조건 속성
        waitForUserInputProp = serializedObject.FindProperty("waitForUserInput");
        autoAdvanceDelayProp = serializedObject.FindProperty("autoAdvanceDelay");
        
        // 에디터 도우미 속성
        showDragSettingsProp = serializedObject.FindProperty("showDragSettings");
        showClickSettingsProp = serializedObject.FindProperty("showClickSettings");
        showQuizSettingsProp = serializedObject.FindProperty("showQuizSettings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        NursingActionData nursingAction = (NursingActionData)target;
        
        // 스타일 설정
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 13;
        
        // 제목
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("간호 행동 설정", headerStyle);
        EditorGUILayout.Space(5);
        
        // 기본 정보 섹션
        showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "기본 정보", true);
        if (showBasicInfo)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(actionIdProp, new GUIContent("행동 ID"));
            EditorGUILayout.PropertyField(actionNameProp, new GUIContent("행동 이름"));
            EditorGUILayout.PropertyField(descriptionProp, new GUIContent("설명"));
            EditorGUILayout.PropertyField(isRequiredProp, new GUIContent("필수 여부"));
            EditorGUILayout.PropertyField(scoreWeightProp, new GUIContent("점수 가중치"));
            EditorGUILayout.PropertyField(requiredItemsProp, new GUIContent("필요 아이템"));
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        // 상호작용 설정 섹션
        showInteractionSettings = EditorGUILayout.Foldout(showInteractionSettings, "상호작용 설정", true);
        if (showInteractionSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(interactionTypeProp, new GUIContent("상호작용 유형"));
            EditorGUILayout.PropertyField(guideTextProp, new GUIContent("가이드 텍스트"));
            EditorGUILayout.PropertyField(tutorialPrefabProp, new GUIContent("튜토리얼 프리팹"));
            
            InteractionType interactionType = (InteractionType)interactionTypeProp.enumValueIndex;
            
            EditorGUILayout.Space(5);
            
            // 상호작용 유형에 따라 해당 설정 표시
            switch (interactionType)
            {
                case InteractionType.Drag:
                    showDragSettingsProp.boolValue = true;
                    useDragInteractionProp.boolValue = true;
                    break;
                    
                case InteractionType.SingleClick:
                    showClickSettingsProp.boolValue = true;
                    useClickInteractionProp.boolValue = true;
                    break;
                    
                default:
                    break;
            }
            
            // 드래그 상호작용 설정
            useDragInteractionProp.boolValue = EditorGUILayout.Toggle(new GUIContent("드래그 상호작용 사용"), useDragInteractionProp.boolValue);
            if (useDragInteractionProp.boolValue)
            {
                showDragSettingsProp.boolValue = EditorGUILayout.Foldout(showDragSettingsProp.boolValue, "드래그 설정", true);
                if (showDragSettingsProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    
                    // 드래그 방향 시각화
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("드래그 방향");
                    
                    // 드래그 방향 원형 슬라이더 (커스텀 GUI)
                    Rect rect = EditorGUILayout.GetControlRect(false, 100);
                    float newAngle = DrawAngleSelector(rect, requiredDragAngleProp.floatValue);
                    if (newAngle != requiredDragAngleProp.floatValue)
                    {
                        requiredDragAngleProp.floatValue = newAngle;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.PropertyField(requiredDragAngleProp, new GUIContent("필요 드래그 각도"));
                    EditorGUILayout.PropertyField(dragAngleToleranceProp, new GUIContent("허용 오차 범위"));
                    EditorGUILayout.PropertyField(dragStepsRequiredProp, new GUIContent("필요 드래그 단계 수"));
                    EditorGUILayout.PropertyField(dragArrowSpriteProp, new GUIContent("드래그 화살표 이미지"));
                    EditorGUILayout.PropertyField(dragArrowPositionProp, new GUIContent("화살표 위치"));
                    EditorGUILayout.PropertyField(dragArrowRotationProp, new GUIContent("화살표 회전"));
                    
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUILayout.Space(5);
            
            // 클릭 상호작용 설정
            useClickInteractionProp.boolValue = EditorGUILayout.Toggle(new GUIContent("클릭 상호작용 사용"), useClickInteractionProp.boolValue);
            if (useClickInteractionProp.boolValue)
            {
                showClickSettingsProp.boolValue = EditorGUILayout.Foldout(showClickSettingsProp.boolValue, "클릭 설정", true);
                if (showClickSettingsProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    
                    // 클릭 영역 시각화
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("클릭 가능 영역", EditorStyles.boldLabel);
                    
                    Rect rectArea = EditorGUILayout.GetControlRect(false, 100);
                    Rect newArea = DrawRectArea(rectArea, validClickAreaProp.rectValue);
                    validClickAreaProp.rectValue = newArea;
                    
                    EditorGUILayout.PropertyField(validClickAreaProp, new GUIContent("클릭 영역 좌표"));
                    EditorGUILayout.PropertyField(validClickTargetTagsProp, new GUIContent("유효 클릭 태그"));
                    EditorGUILayout.PropertyField(clickHighlightSpriteProp, new GUIContent("하이라이트 이미지"));
                    
                    EditorGUILayout.EndVertical();
                    
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUILayout.Space(5);
            
            // 퀴즈 상호작용 설정
            useQuizInteractionProp.boolValue = EditorGUILayout.Toggle(new GUIContent("퀴즈 상호작용 사용"), useQuizInteractionProp.boolValue);
            if (useQuizInteractionProp.boolValue)
            {
                showQuizSettingsProp.boolValue = EditorGUILayout.Foldout(showQuizSettingsProp.boolValue, "퀴즈 설정", true);
                if (showQuizSettingsProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUILayout.PropertyField(quizQuestionProp, new GUIContent("질문"));
                    
                    EditorGUILayout.PropertyField(quizOptionsProp, new GUIContent("선택지"));
                    
                    // 퀴즈 선택지 개수에 따라 정답 인덱스 제한
                    int optionsCount = quizOptionsProp.arraySize;
                    if (optionsCount > 0)
                    {
                        correctOptionIndexProp.intValue = EditorGUILayout.IntSlider(
                            new GUIContent("정답 선택지 인덱스"),
                            correctOptionIndexProp.intValue,
                            0,
                            optionsCount - 1
                        );
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(correctOptionIndexProp, new GUIContent("정답 선택지 인덱스"));
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        // 피드백 설정 섹션
        showFeedbackSettings = EditorGUILayout.Foldout(showFeedbackSettings, "피드백 및 오류 설정", true);
        if (showFeedbackSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(hintTextProp, new GUIContent("힌트 텍스트"));
            EditorGUILayout.PropertyField(feedbackMessageProp, new GUIContent("피드백 메시지"));
            EditorGUILayout.PropertyField(errorMessageProp, new GUIContent("오류 메시지"));
            EditorGUILayout.PropertyField(penaltyTypeProp, new GUIContent("오류 수준"));
            
            EditorGUILayout.PropertyField(successFeedbackSpriteProp, new GUIContent("성공 피드백 이미지"));
            EditorGUILayout.PropertyField(successSoundProp, new GUIContent("성공 소리"));
            EditorGUILayout.PropertyField(errorFeedbackSpriteProp, new GUIContent("오류 피드백 이미지"));
            EditorGUILayout.PropertyField(errorSoundProp, new GUIContent("오류 소리"));
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        // 고급 설정 섹션
        showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "고급 설정", true);
        if (showAdvancedSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(waitForUserInputProp, new GUIContent("사용자 입력 대기"));
            
            if (!waitForUserInputProp.boolValue)
            {
                EditorGUILayout.PropertyField(autoAdvanceDelayProp, new GUIContent("자동 진행 지연 시간"));
            }
            
            EditorGUI.indentLevel--;
        }
        
        // ID 자동 생성 버튼
        EditorGUILayout.Space(10);
        if (GUILayout.Button("ID 자동 생성", GUILayout.Height(30)))
        {
            actionIdProp.stringValue = System.Guid.NewGuid().ToString().Substring(0, 8);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    // 각도 선택기 GUI 그리기
    private float DrawAngleSelector(Rect rect, float currentAngle)
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
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            Vector2 mousePos = Event.current.mousePosition;
            Vector2 direction = mousePos - center;
            float newAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (newAngle < 0) newAngle += 360;
            
            GUI.changed = true;
            Event.current.Use();
            return newAngle;
        }
        
        return currentAngle;
    }
    
    // 사각형 영역 GUI 그리기
    private Rect DrawRectArea(Rect canvas, Rect currentArea)
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
        
        // 드래그 핸들
        EditorGUI.BeginChangeCheck();
        Rect newRect = EditorGUI.RectHandle(displayRect);
        if (EditorGUI.EndChangeCheck())
        {
            // 화면 좌표에서 게임 좌표로 변환
            return new Rect(
                (newRect.x - canvas.x) / scaleX,
                (newRect.y - canvas.y) / scaleY,
                newRect.width / scaleX,
                newRect.height / scaleY
            );
        }
        
        return currentArea;
    }
}