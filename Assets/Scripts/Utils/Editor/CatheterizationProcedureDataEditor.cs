using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

/// <summary>
/// 유치도뇨 시술 데이터를 위한 커스텀 에디터
/// </summary>
[CustomEditor(typeof(CatheterizationProcedureData))]
public class CatheterizationProcedureDataEditor : Editor
{
    private SerializedProperty procedureTypeProp;
    private SerializedProperty procedureNameProp;
    private SerializedProperty descriptionProp;
    private SerializedProperty stepsProp;
    private SerializedProperty timeLimitProp;
    private SerializedProperty maxScoreProp;
    private SerializedProperty timeBonusProp;
    private SerializedProperty maxTimeBonusProp;
    private SerializedProperty procedureIconProp;
    private SerializedProperty procedureBannerProp;
    private SerializedProperty procedureColorProp;
    private SerializedProperty backgroundMusicProp;
    private SerializedProperty completionSoundProp;
    
    private ReorderableList stepsList;
    
    private bool showBasicInfo = true;
    private bool showStepsList = true;
    private bool showSettings = true;
    private bool showUISettings = true;
    private bool showAudioSettings = true;
    
    private void OnEnable()
    {
        procedureTypeProp = serializedObject.FindProperty("procedureType");
        procedureNameProp = serializedObject.FindProperty("procedureName");
        descriptionProp = serializedObject.FindProperty("description");
        stepsProp = serializedObject.FindProperty("steps");
        timeLimitProp = serializedObject.FindProperty("timeLimit");
        maxScoreProp = serializedObject.FindProperty("maxScore");
        timeBonusProp = serializedObject.FindProperty("timeBonus");
        maxTimeBonusProp = serializedObject.FindProperty("maxTimeBonus");
        procedureIconProp = serializedObject.FindProperty("procedureIcon");
        procedureBannerProp = serializedObject.FindProperty("procedureBanner");
        procedureColorProp = serializedObject.FindProperty("procedureColor");
        backgroundMusicProp = serializedObject.FindProperty("backgroundMusic");
        completionSoundProp = serializedObject.FindProperty("completionSound");
        
        // 단계 리스트 초기화
        InitializeStepsList();
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        CatheterizationProcedureData procedureData = (CatheterizationProcedureData)target;
        
        // 헤더
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("유치도뇨 시술 데이터", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // 기본 정보 섹션
        showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "기본 정보", true);
        if (showBasicInfo)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(procedureTypeProp, new GUIContent("시술 유형"));
            EditorGUILayout.PropertyField(procedureNameProp, new GUIContent("시술 이름"));
            EditorGUILayout.PropertyField(descriptionProp, new GUIContent("설명"));
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        // 단계 목록 섹션
        showStepsList = EditorGUILayout.Foldout(showStepsList, "시술 단계 목록", true);
        if (showStepsList)
        {
            // 단계 리스트 표시
            stepsList.DoLayoutList();
            
            if (stepsProp.arraySize > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("시술 단계 순서", EditorStyles.boldLabel);
                
                // 단계 흐름 시각화
                DrawStepsFlow();
                
                EditorGUILayout.EndVertical();
            }
            
            // 아이템 요약
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("필요 아이템 요약", EditorStyles.boldLabel);
            
            List<Item> allItems = procedureData.GetAllRequiredItems();
            if (allItems.Count > 0)
            {
                foreach (Item item in allItems)
                {
                    if (item != null)
                    {
                        EditorGUILayout.LabelField("• " + item.itemName);
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("필요한 아이템이 없습니다.");
            }
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.Space(10);
        
        // 설정 섹션
        showSettings = EditorGUILayout.Foldout(showSettings, "시술 설정", true);
        if (showSettings)
        {
            EditorGUI.indentLevel++;
            
            // 시간 설정
            EditorGUILayout.LabelField("시간 설정", EditorStyles.boldLabel);
            
            // 분:초 형식으로 표시
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("제한 시간");
            
            float timeInSeconds = timeLimitProp.floatValue;
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            
            int newMinutes = EditorGUILayout.IntField(minutes, GUILayout.Width(30));
            EditorGUILayout.LabelField("분", GUILayout.Width(20));
            int newSeconds = EditorGUILayout.IntField(seconds, GUILayout.Width(30));
            EditorGUILayout.LabelField("초", GUILayout.Width(20));
            
            if (newMinutes != minutes || newSeconds != seconds)
            {
                timeLimitProp.floatValue = newMinutes * 60 + newSeconds;
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 점수 설정
            EditorGUILayout.LabelField("점수 설정", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(maxScoreProp, new GUIContent("최대 점수"));
            EditorGUILayout.PropertyField(timeBonusProp, new GUIContent("시간 보너스 계수"));
            EditorGUILayout.PropertyField(maxTimeBonusProp, new GUIContent("최대 시간 보너스"));
            
            EditorGUILayout.HelpBox(
                $"시간 보너스 계산: (남은 시간 / 제한 시간) * {timeBonusProp.floatValue} * {maxTimeBonusProp.intValue}", 
                MessageType.Info
            );
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        // UI 설정 섹션
        showUISettings = EditorGUILayout.Foldout(showUISettings, "UI 설정", true);
        if (showUISettings)
        {
            EditorGUI.indentLevel++;
            
            // 아이콘 (미리보기 포함)
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(procedureIconProp, new GUIContent("시술 아이콘"));
            
            if (procedureIconProp.objectReferenceValue != null)
            {
                GUILayout.Box((procedureIconProp.objectReferenceValue as Sprite).texture, 
                    GUILayout.Width(48), GUILayout.Height(48));
            }
            EditorGUILayout.EndHorizontal();
            
            // 배너 (미리보기 포함)
            EditorGUILayout.PropertyField(procedureBannerProp, new GUIContent("시술 배너"));
            
            if (procedureBannerProp.objectReferenceValue != null)
            {
                Texture2D tex = (procedureBannerProp.objectReferenceValue as Sprite).texture;
                float ratio = (float)tex.width / tex.height;
                float previewWidth = EditorGUIUtility.currentViewWidth - 40;
                float previewHeight = previewWidth / ratio;
                
                previewHeight = Mathf.Min(previewHeight, 80); // 최대 높이 제한
                previewWidth = previewHeight * ratio;  // 비율 유지
                
                Rect rect = EditorGUILayout.GetControlRect(false, previewHeight);
                rect.width = previewWidth;
                rect.x = (EditorGUIUtility.currentViewWidth - previewWidth) * 0.5f;
                
                EditorGUI.DrawPreviewTexture(rect, tex);
            }
            
            // 색상 (미리보기 포함)
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(procedureColorProp, new GUIContent("시술 색상"));
            
            // 색상 견본 표시
            Rect colorRect = EditorGUILayout.GetControlRect(false, 20, GUILayout.Width(40));
            EditorGUI.DrawRect(colorRect, procedureColorProp.colorValue);
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        // 오디오 설정 섹션
        showAudioSettings = EditorGUILayout.Foldout(showAudioSettings, "오디오 설정", true);
        if (showAudioSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(backgroundMusicProp, new GUIContent("배경 음악"));
            EditorGUILayout.PropertyField(completionSoundProp, new GUIContent("완료 효과음"));
            
            // 오디오 미리듣기 버튼
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("배경 음악 미리듣기", EditorStyles.miniButton))
            {
                if (backgroundMusicProp.objectReferenceValue != null)
                {
                    AudioClip clip = backgroundMusicProp.objectReferenceValue as AudioClip;
                    PlayClip(clip);
                }
            }
            
            if (GUILayout.Button("완료 효과음 미리듣기", EditorStyles.miniButton))
            {
                if (completionSoundProp.objectReferenceValue != null)
                {
                    AudioClip clip = completionSoundProp.objectReferenceValue as AudioClip;
                    PlayClip(clip);
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(10);
        
        // 유틸리티 버튼
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("단계 생성", GUILayout.Height(30)))
        {
            CreateNewStep();
        }
        
        if (GUILayout.Button("단계 순서 검증", GUILayout.Height(30)))
        {
            ValidateStepOrder();
        }
        
        EditorGUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void InitializeStepsList()
    {
        stepsList = new ReorderableList(serializedObject, stepsProp, true, true, true, true);
        
        // 헤더 그리기
        stepsList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "시술 단계");
        };
        
        // 요소 높이 설정
        stepsList.elementHeightCallback = (int index) => {
            SerializedProperty element = stepsProp.GetArrayElementAtIndex(index);
            if (element.objectReferenceValue == null)
                return EditorGUIUtility.singleLineHeight + 4;
                
            ProcedureStepData step = element.objectReferenceValue as ProcedureStepData;
            return EditorGUIUtility.singleLineHeight * 2 + 6;
        };
        
        // 요소 그리기
        stepsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            SerializedProperty element = stepsProp.GetArrayElementAtIndex(index);
            rect.y += 2;
            
            if (element.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    element, 
                    new GUIContent($"단계 {index + 1}")
                );
                return;
            }
            
            ProcedureStepData step = element.objectReferenceValue as ProcedureStepData;
            
            // 단계 번호와 이름
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element, 
                new GUIContent($"단계 {index + 1}: {step.stepName}")
            );
            
            // 단계 정보 표시
            string info = "";
            if (step.isRequired)
            {
                info += "필수 | ";
            }
            
            info += $"행동 {step.actions.Count}개";
            
            if (step.isOrderImportant)
            {
                info += " | 순서 중요";
            }
            
            EditorGUI.LabelField(
                new Rect(rect.x + 15, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width - 15, EditorGUIUtility.singleLineHeight),
                info,
                EditorStyles.miniLabel
            );
        };
        
        // 요소 선택 콜백
        stepsList.onSelectCallback = (ReorderableList list) => {
            SerializedProperty element = stepsProp.GetArrayElementAtIndex(list.index);
            if (element.objectReferenceValue != null)
            {
                Selection.activeObject = element.objectReferenceValue;
            }
        };
    }
    
    private void DrawStepsFlow()
    {
        int stepCount = stepsProp.arraySize;
        if (stepCount == 0)
            return;
            
        // 단계 흐름 그리기
        EditorGUILayout.BeginHorizontal();
        
        GUIStyle boxStyle = new GUIStyle(EditorStyles.helpBox);
        boxStyle.alignment = TextAnchor.MiddleCenter;
        boxStyle.fontStyle = FontStyle.Bold;
        
        for (int i = 0; i < stepCount; i++)
        {
            SerializedProperty element = stepsProp.GetArrayElementAtIndex(i);
            if (element.objectReferenceValue == null)
                continue;
                
            ProcedureStepData step = element.objectReferenceValue as ProcedureStepData;
            
            // 단계 박스
            GUILayout.Box($"{i + 1}. {step.stepName}", boxStyle, GUILayout.Height(30));
            
            // 화살표 (마지막 단계 제외)
            if (i < stepCount - 1)
            {
                GUILayout.Label("→", GUILayout.Width(20));
            }
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void CreateNewStep()
    {
        CatheterizationProcedureData procedureData = (CatheterizationProcedureData)target;
        
        // 저장 경로 선택 대화상자
        string path = EditorUtility.SaveFilePanelInProject(
            "새 시술 단계 생성",
            $"Step_{procedureData.procedureName}_{procedureData.steps.Count + 1}",
            "asset",
            "생성할 시술 단계 에셋의 이름을 입력하세요."
        );
        
        if (string.IsNullOrEmpty(path))
            return;
            
        // 시술 단계 생성
        ProcedureStepData stepData = CreateInstance<ProcedureStepData>();
        stepData.stepId = System.Guid.NewGuid().ToString().Substring(0, 8);
        stepData.stepName = $"단계 {procedureData.steps.Count + 1}";
        
        // 에셋 저장
        AssetDatabase.CreateAsset(stepData, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // 단계 목록에 추가
        stepsProp.arraySize++;
        stepsProp.GetArrayElementAtIndex(stepsProp.arraySize - 1).objectReferenceValue = stepData;
        serializedObject.ApplyModifiedProperties();
        
        // 새 단계 선택
        Selection.activeObject = stepData;
    }
    
    private void ValidateStepOrder()
    {
        CatheterizationProcedureData procedureData = (CatheterizationProcedureData)target;
        
        List<string> issues = new List<string>();
        
        // 단계 순서 검증
        for (int i = 0; i < procedureData.steps.Count; i++)
        {
            ProcedureStepData step = procedureData.steps[i];
            
            if (step == null)
            {
                issues.Add($"단계 {i + 1}: 누락된 단계");
                continue;
            }
            
            // 행동 검증
            if (step.actions.Count == 0)
            {
                issues.Add($"단계 {i + 1} ({step.stepName}): 행동이 없음");
            }
            
            // 필수 단계이지만 필수 행동이 없는 경우
            if (step.isRequired)
            {
                bool hasRequiredAction = false;
                foreach (var action in step.actions)
                {
                    if (action != null && action.isRequired)
                    {
                        hasRequiredAction = true;
                        break;
                    }
                }
                
                if (!hasRequiredAction && step.actions.Count > 0)
                {
                    issues.Add($"단계 {i + 1} ({step.stepName}): 필수 단계이지만 필수 행동이 없음");
                }
            }
        }
        
        // 결과 표시
        string message;
        if (issues.Count == 0)
        {
            message = "모든 단계가 유효합니다!";
        }
        else
        {
            message = "다음 문제가 발견되었습니다:\n\n";
            foreach (var issue in issues)
            {
                message += "• " + issue + "\n";
            }
        }
        
        EditorUtility.DisplayDialog("단계 순서 검증 결과", message, "확인");
    }
    
    // 오디오 클립 재생
    private void PlayClip(AudioClip clip)
    {
        if (clip == null)
            return;
            
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod("PlayPreviewClip", 
            BindingFlags.Static | BindingFlags.Public, null, new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);
        
        method.Invoke(null, new object[] { clip, 0, false });
    }
}

// System.Reflection 네임스페이스와 필요한 using 문 추가
#region Using
using System.Reflection;
using UnityEditor.SceneManagement;
#endregion