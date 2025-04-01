using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

/// <summary>
/// ProcedureStepData의 커스텀 에디터
/// 인스펙터에서 시각적으로 간호 절차 단계를 편집할 수 있게 해줍니다.
/// </summary>
[CustomEditor(typeof(ProcedureStepData))]
public class ProcedureStepDataEditor : Editor
{
    private SerializedProperty stepIdProp;
    private SerializedProperty stepNameProp;
    private SerializedProperty descriptionProp;
    private SerializedProperty isOrderImportantProp;
    private SerializedProperty isRequiredProp;
    private SerializedProperty scoreWeightProp;
    private SerializedProperty actionsProp;
    private SerializedProperty stepIconProp;
    private SerializedProperty guideTextProp;
    private SerializedProperty waitForAllActionsProp;
    private SerializedProperty autoAdvanceDelayProp;
    private SerializedProperty backgroundImageProp;
    private SerializedProperty backgroundSoundProp;
    
    private ReorderableList actionsList;
    
    private bool showBasicInfo = true;
    private bool showActionsSection = true;
    private bool showUISection = true;
    private bool showCompletionSection = true;
    
    private GUIStyle headerStyle;
    private GUIStyle subHeaderStyle;
    
    private void OnEnable()
    {
        stepIdProp = serializedObject.FindProperty("stepId");
        stepNameProp = serializedObject.FindProperty("stepName");
        descriptionProp = serializedObject.FindProperty("description");
        isOrderImportantProp = serializedObject.FindProperty("isOrderImportant");
        isRequiredProp = serializedObject.FindProperty("isRequired");
        scoreWeightProp = serializedObject.FindProperty("scoreWeight");
        actionsProp = serializedObject.FindProperty("actions");
        stepIconProp = serializedObject.FindProperty("stepIcon");
        guideTextProp = serializedObject.FindProperty("guideText");
        waitForAllActionsProp = serializedObject.FindProperty("waitForAllActions");
        autoAdvanceDelayProp = serializedObject.FindProperty("autoAdvanceDelay");
        backgroundImageProp = serializedObject.FindProperty("backgroundImage");
        backgroundSoundProp = serializedObject.FindProperty("backgroundSound");
        
        // ReorderableList 초기화
        InitializeActionsList();
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // 스타일 초기화
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.margin.top = 10;
            headerStyle.margin.bottom = 5;
        }
        
        if (subHeaderStyle == null)
        {
            subHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
            subHeaderStyle.fontSize = 12;
            subHeaderStyle.margin.top = 8;
            subHeaderStyle.margin.bottom = 4;
        }
        
        // 타이틀
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("간호 절차 단계 설정", headerStyle);
        EditorGUILayout.Space(5);
        
        // 미리보기 카드 (옵션)
        DrawPreviewCard();
        
        // 기본 정보 섹션
        showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "기본 정보", true);
        if (showBasicInfo)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(stepIdProp, new GUIContent("단계 ID"));
            EditorGUILayout.PropertyField(stepNameProp, new GUIContent("단계 이름"));
            EditorGUILayout.PropertyField(descriptionProp, new GUIContent("설명"));
            EditorGUILayout.PropertyField(isRequiredProp, new GUIContent("필수 단계 여부"));
            EditorGUILayout.PropertyField(scoreWeightProp, new GUIContent("점수 가중치"));
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        // 행동 섹션
        showActionsSection = EditorGUILayout.Foldout(showActionsSection, "간호 행동 목록", true);
        if (showActionsSection)
        {
            EditorGUILayout.PropertyField(isOrderImportantProp, new GUIContent("순서 중요 여부"));
            
            if (isOrderImportantProp.boolValue)
            {
                EditorGUILayout.HelpBox("행동들이 지정된 순서대로 수행되어야 합니다.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("행동들은 어떤 순서로든 수행할 수 있습니다.", MessageType.Info);
            }
            
            EditorGUILayout.Space(5);
            
            // 행동 목록 표시
            actionsList.DoLayoutList();
            
            // 요약 정보
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            ProcedureStepData stepData = (ProcedureStepData)target;
            List<Item> requiredItems = stepData.GetAllRequiredItems();
            
            EditorGUILayout.LabelField("필요 아이템 요약", subHeaderStyle);
            if (requiredItems.Count > 0)
            {
                EditorGUI.indentLevel++;
                foreach (var item in requiredItems)
                {
                    EditorGUILayout.LabelField("• " + (item != null ? item.itemName : "Missing Item"));
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("필요한 아이템이 없습니다.");
            }
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.Space(10);
        
        // UI 섹션
        showUISection = EditorGUILayout.Foldout(showUISection, "UI 설정", true);
        if (showUISection)
        {
            EditorGUI.indentLevel++;
            
            // 스텝 아이콘 (미리보기 포함)
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(stepIconProp, new GUIContent("단계 아이콘"));
            
            // 아이콘 미리보기
            if (stepIconProp.objectReferenceValue != null)
            {
                GUILayout.Box((stepIconProp.objectReferenceValue as Sprite).texture, 
                    GUILayout.Width(64), GUILayout.Height(64));
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(guideTextProp, new GUIContent("가이드 텍스트"));
            EditorGUILayout.PropertyField(backgroundImageProp, new GUIContent("배경 이미지"));
            
            // 배경 이미지 미리보기
            if (backgroundImageProp.objectReferenceValue != null)
            {
                Texture2D tex = (backgroundImageProp.objectReferenceValue as Sprite).texture;
                float ratio = (float)tex.width / tex.height;
                float previewWidth = EditorGUIUtility.currentViewWidth - 40;
                float previewHeight = previewWidth / ratio;
                
                previewHeight = Mathf.Min(previewHeight, 150); // 최대 높이 제한
                previewWidth = previewHeight * ratio;  // 비율 유지
                
                Rect rect = EditorGUILayout.GetControlRect(false, previewHeight);
                rect.width = previewWidth;
                rect.x = (EditorGUIUtility.currentViewWidth - previewWidth) * 0.5f;
                
                EditorGUI.DrawPreviewTexture(rect, tex);
            }
            
            EditorGUILayout.PropertyField(backgroundSoundProp, new GUIContent("배경 소리"));
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        // 완료 조건 섹션
        showCompletionSection = EditorGUILayout.Foldout(showCompletionSection, "완료 조건", true);
        if (showCompletionSection)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(waitForAllActionsProp, new GUIContent("모든 행동 완료 대기"));
            
            if (!waitForAllActionsProp.boolValue)
            {
                EditorGUILayout.HelpBox("일부 행동만 완료해도 단계가 완료됩니다. (필수 행동은 항상 필요)", MessageType.Warning);
            }
            
            EditorGUILayout.PropertyField(autoAdvanceDelayProp, new GUIContent("자동 진행 지연 시간"));
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        // 유틸리티 버튼
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("ID 자동 생성", GUILayout.Height(30)))
        {
            stepIdProp.stringValue = System.Guid.NewGuid().ToString().Substring(0, 8);
        }
        
        if (GUILayout.Button("행동 만들기", GUILayout.Height(30)))
        {
            CreateNewAction();
        }
        
        EditorGUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawPreviewCard()
    {
        ProcedureStepData stepData = (ProcedureStepData)target;
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        if (!string.IsNullOrEmpty(stepData.stepName))
        {
            EditorGUILayout.LabelField(stepData.stepName, EditorStyles.boldLabel);
        }
        else
        {
            EditorGUILayout.LabelField(target.name, EditorStyles.boldLabel);
        }
        
        if (!string.IsNullOrEmpty(stepData.description))
        {
            EditorGUILayout.LabelField(stepData.description, EditorStyles.wordWrappedLabel);
        }
        
        if (stepData.actions != null)
        {
            EditorGUILayout.LabelField($"행동 {stepData.actions.Count}개", EditorStyles.miniBoldLabel);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void InitializeActionsList()
    {
        actionsList = new ReorderableList(serializedObject, actionsProp, true, true, true, true);
        
        // 헤더 설정
        actionsList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "간호 행동 리스트");
        };
        
        // 요소 높이 설정
        actionsList.elementHeightCallback = (int index) => {
            SerializedProperty element = actionsProp.GetArrayElementAtIndex(index);
            if (element.objectReferenceValue == null)
                return EditorGUIUtility.singleLineHeight + 4;
                
            NursingActionData action = element.objectReferenceValue as NursingActionData;
            return EditorGUIUtility.singleLineHeight * 2 + 6;
        };
        
        // 요소 그리기 설정
        actionsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            SerializedProperty element = actionsProp.GetArrayElementAtIndex(index);
            rect.y += 2;
            
            if (element.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    element, 
                    new GUIContent($"행동 {index + 1}")
                );
                return;
            }
            
            NursingActionData action = element.objectReferenceValue as NursingActionData;
            
            // 행동 이름과 객체 참조 표시
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element, 
                new GUIContent($"행동 {index + 1}: {action.actionName}")
            );
            
            // 추가 정보 표시 (순서가 중요한 경우 번호 강조)
            string info = "";
            if (action.isRequired)
            {
                info += "필수 | ";
            }
            
            if (action.requiredItems.Count > 0)
            {
                info += $"아이템 {action.requiredItems.Count}개 | ";
            }
            
            info += action.interactionType.ToString();
            
            EditorGUI.LabelField(
                new Rect(rect.x + 15, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width - 15, EditorGUIUtility.singleLineHeight),
                info,
                EditorStyles.miniLabel
            );
        };
        
        // 요소 추가 설정
        actionsList.onAddCallback = (ReorderableList list) => {
            actionsProp.arraySize++;
            serializedObject.ApplyModifiedProperties();
        };
        
        // 요소 선택 설정
        actionsList.onSelectCallback = (ReorderableList list) => {
            SerializedProperty element = actionsProp.GetArrayElementAtIndex(list.index);
            if (element.objectReferenceValue != null)
            {
                Selection.activeObject = element.objectReferenceValue;
            }
        };
    }
    
    private void CreateNewAction()
    {
        ProcedureStepData stepData = (ProcedureStepData)target;
        
        // 저장 경로 선택 대화상자
        string path = EditorUtility.SaveFilePanelInProject(
            "새 간호 행동 생성",
            $"Action_{stepData.stepName}_{stepData.actions.Count + 1}",
            "asset",
            "생성할 간호 행동 에셋의 이름을 입력하세요."
        );
        
        if (string.IsNullOrEmpty(path))
            return;
            
        // 간호 행동 생성
        NursingActionData actionData = CreateInstance<NursingActionData>();
        actionData.actionId = System.Guid.NewGuid().ToString().Substring(0, 8);
        actionData.actionName = $"행동 {stepData.actions.Count + 1}";
        
        // 에셋 저장
        AssetDatabase.CreateAsset(actionData, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // 행동 목록에 추가
        actionsProp.arraySize++;
        actionsProp.GetArrayElementAtIndex(actionsProp.arraySize - 1).objectReferenceValue = actionData;
        serializedObject.ApplyModifiedProperties();
        
        // 새 행동 선택
        Selection.activeObject = actionData;
    }
}