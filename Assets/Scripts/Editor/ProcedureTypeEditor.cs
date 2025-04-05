using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// ProcedureType을 위한 커스텀 에디터
/// 간호 시술 유형에 대한 에디터 인터페이스 제공
/// </summary>
[CustomEditor(typeof(ProcedureType))]
public class ProcedureTypeEditor : Editor
{
    // SerializedProperties
    private SerializedProperty idProperty;
    private SerializedProperty displayNameProperty;
    private SerializedProperty descriptionProperty;
    private SerializedProperty procedureTypeProperty;
    private SerializedProperty guidelineVersionProperty;
    private SerializedProperty clinicalVersionProperty;
    private SerializedProperty procedureIconProperty;

    // UI 상태
    private bool basicInfoFoldout = true;
    private bool procedureInfoFoldout = true;
    private bool uiSettingsFoldout = true;

    private void OnEnable()
    {
        // SerializedProperties 초기화
        idProperty = serializedObject.FindProperty("id");
        displayNameProperty = serializedObject.FindProperty("displayName");
        descriptionProperty = serializedObject.FindProperty("description");
        procedureTypeProperty = serializedObject.FindProperty("procedureType");
        guidelineVersionProperty = serializedObject.FindProperty("guidelineVersion");
        clinicalVersionProperty = serializedObject.FindProperty("clinicalVersion");
        procedureIconProperty = serializedObject.FindProperty("procedureIcon");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);
        DrawHeader();
        EditorGUILayout.Space(10);

        // 기본 정보 섹션
        DrawBasicInfoSection();
        
        EditorGUILayout.Space(5);
        
        // 시술 정보 섹션
        DrawProcedureInfoSection();
        
        EditorGUILayout.Space(5);
        
        // UI 설정 섹션
        DrawUISettingsSection();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("시술 유형 설정", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawBasicInfoSection()
    {
        basicInfoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(basicInfoFoldout, "기본 정보");
        if (basicInfoFoldout)
        {
            EditorGUI.indentLevel++;
            
            // ID 필드
            EditorGUILayout.PropertyField(idProperty, new GUIContent("ID", "시술 유형의 고유 식별자"));

            // 표시 이름 필드
            EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("표시 이름", "화면에 표시될 시술 유형 이름"));

            // 설명 필드
            EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("설명", "시술 유형에 대한 상세 설명"));
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawProcedureInfoSection()
    {
        procedureInfoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(procedureInfoFoldout, "시술 정보");
        if (procedureInfoFoldout)
        {
            EditorGUI.indentLevel++;
            
            // 시술 유형 필드
            EditorGUILayout.PropertyField(procedureTypeProperty, new GUIContent("시술 유형", "이 시술의 유형 분류"));

            EditorGUILayout.Space(5);

            // 가이드라인 버전 필드
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(guidelineVersionProperty, new GUIContent("가이드라인 버전", "가이드라인에 따른 시술 데이터"));
            if (guidelineVersionProperty.objectReferenceValue == null)
            {
                if (GUILayout.Button("생성", GUILayout.Width(60)))
                {
                    CreateProcedureData("Guideline");
                }
            }
            EditorGUILayout.EndHorizontal();

            // 임상 버전 필드
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(clinicalVersionProperty, new GUIContent("임상 버전", "임상 환경에서의 시술 데이터"));
            if (clinicalVersionProperty.objectReferenceValue == null)
            {
                if (GUILayout.Button("생성", GUILayout.Width(60)))
                {
                    CreateProcedureData("Clinical");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawUISettingsSection()
    {
        uiSettingsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(uiSettingsFoldout, "UI 설정");
        if (uiSettingsFoldout)
        {
            EditorGUI.indentLevel++;
            
            // 아이콘 필드
            EditorGUILayout.PropertyField(procedureIconProperty, new GUIContent("시술 아이콘", "이 시술을 나타내는 UI 아이콘"));
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    // 새 ProcedureData 에셋 생성 헬퍼 메서드
    private void CreateProcedureData(string versionType)
    {
        ProcedureType procedureType = (ProcedureType)target;
        
        // 에셋 이름 생성
        string assetName = string.IsNullOrEmpty(procedureType.displayName) 
            ? $"New {versionType} Procedure" 
            : $"{procedureType.displayName} - {versionType}";
            
        // 에셋 저장 경로 선택 다이얼로그 표시
        string path = EditorUtility.SaveFilePanelInProject(
            $"Save {versionType} Procedure Data",
            assetName,
            "asset",
            $"시술 '{assetName}'의 {versionType} 버전 데이터를 저장할 위치를 선택하세요."
        );
        
        if (string.IsNullOrEmpty(path))
            return;
            
        // 새 ProcedureData 에셋 생성
        ProcedureData procedureData = CreateInstance<ProcedureData>();
        
        // 기본 정보 설정
        procedureData.id = procedureType.id + "_" + versionType.ToLower();
        procedureData.displayName = procedureType.displayName + " (" + versionType + ")";
        procedureData.description = procedureType.description;
        procedureData.procedureType = procedureType.procedureType;
        procedureData.isGuideline = versionType == "Guideline";

        // 에셋 저장
        AssetDatabase.CreateAsset(procedureData, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // 참조 설정
        if (versionType == "Guideline")
        {
            guidelineVersionProperty.objectReferenceValue = procedureData;
        }
        else if (versionType == "Clinical")
        {
            clinicalVersionProperty.objectReferenceValue = procedureData;
        }
        
        serializedObject.ApplyModifiedProperties();
        
        // 에디터에서 선택
        Selection.activeObject = procedureData;
        EditorGUIUtility.PingObject(procedureData);
    }
}