using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

namespace NursingGame.Editor
{
    /// <summary>
    /// ProcedureStep의 커스텀 에디터
    /// 인스펙터에서 시각적으로 간호 절차 단계를 편집할 수 있게 해줍니다.
    /// </summary>
    [CustomEditor(typeof(ProcedureStep))]
    public class ProcedureStepDataEditor : UnityEditor.Editor
    {
        private SerializedProperty stepIdProp;
        private SerializedProperty stepNameProp;
        private SerializedProperty descriptionProp;
        private SerializedProperty isOrderImportantProp;
        private SerializedProperty isRequiredProp;
        private SerializedProperty scoreWeightProp;
        private SerializedProperty interactionDataIdProp;
        private SerializedProperty stepIconProp;
        private SerializedProperty guideTextProp;
        private SerializedProperty dialogueEntriesProp;
        private SerializedProperty stepTypeProp;
        private SerializedProperty autoAdvanceDelayProp;
        private SerializedProperty backgroundImageProp;
        private SerializedProperty timeLimitProp;
        
        private bool showBasicInfo = true;
        private bool showInteractionSection = true;
        private bool showDialogueSection = true;
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
            interactionDataIdProp = serializedObject.FindProperty("interactionDataId");
            stepIconProp = serializedObject.FindProperty("stepIcon");
            guideTextProp = serializedObject.FindProperty("guideText");
            dialogueEntriesProp = serializedObject.FindProperty("dialogueEntries");
            stepTypeProp = serializedObject.FindProperty("stepType");
            autoAdvanceDelayProp = serializedObject.FindProperty("autoAdvanceDelay");
            backgroundImageProp = serializedObject.FindProperty("backgroundImage");
            timeLimitProp = serializedObject.FindProperty("timeLimit");
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
            
            // 기본 정보 섹션
            showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "기본 정보", true);
            if (showBasicInfo)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(stepIdProp, new GUIContent("단계 ID"));
                EditorGUILayout.PropertyField(stepNameProp, new GUIContent("단계 이름"));
                EditorGUILayout.PropertyField(descriptionProp, new GUIContent("설명"));
                EditorGUILayout.PropertyField(stepTypeProp, new GUIContent("단계 유형"));
                EditorGUILayout.PropertyField(isRequiredProp, new GUIContent("필수 단계 여부"));
                EditorGUILayout.PropertyField(scoreWeightProp, new GUIContent("점수 가중치"));
                EditorGUILayout.PropertyField(timeLimitProp, new GUIContent("시간 제한 (초)"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 상호작용 섹션
            if (stepTypeProp.enumValueIndex == (int)StepType.Interaction)
            {
                showInteractionSection = EditorGUILayout.Foldout(showInteractionSection, "상호작용 설정", true);
                if (showInteractionSection)
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUILayout.PropertyField(isOrderImportantProp, new GUIContent("순서 중요 여부"));
                    
                    if (isOrderImportantProp.boolValue)
                    {
                        EditorGUILayout.HelpBox("상호작용이 지정된 순서대로 수행되어야 합니다.", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("상호작용은 어떤 순서로든 수행할 수 있습니다.", MessageType.Info);
                    }
                    
                    EditorGUILayout.PropertyField(interactionDataIdProp, new GUIContent("상호작용 데이터 ID"));
                    
                    // 요약 정보
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.LabelField("필요 아이템 요약", subHeaderStyle);
                    
                    SerializedProperty requiredItemsProp = serializedObject.FindProperty("requiredItems");
                    if (requiredItemsProp != null && requiredItemsProp.arraySize > 0)
                    {
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < requiredItemsProp.arraySize; i++)
                        {
                            var itemProp = requiredItemsProp.GetArrayElementAtIndex(i).FindPropertyRelative("item");
                            if (itemProp != null && itemProp.objectReferenceValue != null)
                            {
                                // Object 필드를 통해 이름 가져오기
                                var itemObj = itemProp.objectReferenceValue;
                                string itemName = itemObj.name;
                                
                                // itemName 필드 등록 시도 (타입 참조 없이)
                                SerializedObject itemSO = new SerializedObject(itemObj);
                                SerializedProperty itemNameProp = itemSO.FindProperty("itemName");
                                if (itemNameProp != null && !string.IsNullOrEmpty(itemNameProp.stringValue))
                                {
                                    itemName = itemNameProp.stringValue;
                                }
                                
                                EditorGUILayout.LabelField("• " + itemName);
                            }
                            else
                            {
                                EditorGUILayout.LabelField("• Missing Item");
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        EditorGUILayout.LabelField("필요한 아이템이 없습니다.");
                    }
                    
                    EditorGUILayout.EndVertical();
                    
                    EditorGUI.indentLevel--;
                }
            }
            
            // 대화 섹션
            if (stepTypeProp.enumValueIndex == (int)StepType.Dialogue)
            {
                showDialogueSection = EditorGUILayout.Foldout(showDialogueSection, "대화 설정", true);
                if (showDialogueSection)
                {
                    EditorGUI.indentLevel++;
                    
                    EditorGUILayout.PropertyField(dialogueEntriesProp, new GUIContent("대화 항목"), true);
                    
                    EditorGUI.indentLevel--;
                }
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
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 완료 조건 섹션
            showCompletionSection = EditorGUILayout.Foldout(showCompletionSection, "완료 조건", true);
            if (showCompletionSection)
            {
                EditorGUI.indentLevel++;
                
                SerializedProperty requireAllItemsProp = serializedObject.FindProperty("requireAllItems");
                if (requireAllItemsProp != null)
                {
                    EditorGUILayout.PropertyField(requireAllItemsProp, new GUIContent("모든 아이템 필요"));
                }
                
                SerializedProperty requireCorrectResponsesProp = serializedObject.FindProperty("requireCorrectResponses");
                if (requireCorrectResponsesProp != null)
                {
                    EditorGUILayout.PropertyField(requireCorrectResponsesProp, new GUIContent("정확한 응답 필요"));
                }
                
                EditorGUILayout.PropertyField(autoAdvanceDelayProp, new GUIContent("자동 진행 지연 시간"));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // 유틸리티 버튼
            if (GUILayout.Button("ID 자동 생성", GUILayout.Height(30)))
            {
                stepIdProp.stringValue = System.Guid.NewGuid().ToString().Substring(0, 8);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}