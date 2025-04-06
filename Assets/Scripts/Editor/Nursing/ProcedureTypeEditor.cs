using UnityEngine;
using UnityEditor;
using Nursing.Procedure;
using System.IO;

namespace Nursing.Editor
{
    [CustomEditor(typeof(ProcedureType))]
    public class ProcedureTypeEditor : UnityEditor.Editor
    {
        private SerializedProperty idProperty;
        private SerializedProperty displayNameProperty;
        private SerializedProperty descriptionProperty;
        private SerializedProperty versionTypeProperty;
        private SerializedProperty procedureDataProperty;
        
        private void OnEnable()
        {
            idProperty = serializedObject.FindProperty("id");
            displayNameProperty = serializedObject.FindProperty("displayName");
            descriptionProperty = serializedObject.FindProperty("description");
            versionTypeProperty = serializedObject.FindProperty("versionType");
            procedureDataProperty = serializedObject.FindProperty("procedureData");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("프로시저 타입", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 기본 정보
            EditorGUILayout.PropertyField(idProperty, new GUIContent("ID", "프로시저 타입의 고유 식별자"));
            EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("이름", "프로시저 타입의 표시 이름"));
            EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("설명", "프로시저 타입에 대한 설명"));
            
            // 버전 타입
            EditorGUILayout.PropertyField(versionTypeProperty, new GUIContent("버전 타입", "프로시저의 버전 타입 (가이드라인/임상)"));
            
            // 프로시저 데이터
            EditorGUILayout.PropertyField(procedureDataProperty, new GUIContent("프로시저 데이터", "연결된 프로시저 데이터"));
            
            EditorGUILayout.Space();
            
            // 프로시저 데이터 생성 버튼
            if (GUILayout.Button("새 프로시저 데이터 생성"))
            {
                ProcedureType procedureType = (ProcedureType)target;
                string versionName = procedureType.versionType == ProcedureVersionType.Guideline ? "가이드라인" : "임상";
                CreateProcedureData(versionName);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void CreateProcedureData(string versionType)
        {
            ProcedureType procedureType = (ProcedureType)target;
            string name = string.IsNullOrEmpty(procedureType.displayName) ? "New Procedure" : procedureType.displayName;
            
            // 에셋 저장 경로 설정
            string path = EditorUtility.SaveFilePanelInProject(
                "새 프로시저 데이터 생성",
                $"{name}_{versionType}",
                "asset",
                "프로시저 데이터를 저장할 위치를 선택하세요."
            );
            
            if (string.IsNullOrEmpty(path))
                return;
            
            // 새 프로시저 데이터 생성
            ProcedureData newProcedureData = ScriptableObject.CreateInstance<ProcedureData>();
            newProcedureData.id = $"{procedureType.id}_{versionType.ToLowerInvariant()}";
            newProcedureData.displayName = $"{name} - {versionType}";
            newProcedureData.description = $"{procedureType.description} ({versionType} 버전)";
            
            // 에셋 저장
            AssetDatabase.CreateAsset(newProcedureData, path);
            AssetDatabase.SaveAssets();
            
            // 프로시저 타입에 연결
            procedureType.procedureData = newProcedureData;
            EditorUtility.SetDirty(procedureType);
            AssetDatabase.SaveAssets();
            
            // 생성된 에셋 선택
            Selection.activeObject = newProcedureData;
        }
    }
}