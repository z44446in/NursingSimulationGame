using UnityEngine;
using UnityEditor;
using Nursing.Procedure;

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
        private SerializedProperty procedurePlayTypeProperty;

        private GUIStyle headerStyle;
        
        private void OnEnable()
        {
            idProperty = serializedObject.FindProperty("id");
            displayNameProperty = serializedObject.FindProperty("displayName");
            descriptionProperty = serializedObject.FindProperty("description");
            versionTypeProperty = serializedObject.FindProperty("versionType");
            procedureDataProperty = serializedObject.FindProperty("procedureData");
            procedurePlayTypeProperty = serializedObject.FindProperty("procedurePlayType");

            // 스타일 초기화는 OnInspectorGUI에서 수행
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // 스타일 초기화
            InitializeStyles();
            
            // 프로시저 정보 섹션
            EditorGUILayout.LabelField("프로시저 정보", headerStyle);
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(idProperty, new GUIContent("ID", "프로시저 타입의 고유 식별자"));
            EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("표시 이름", "프로시저 타입의 화면에 표시될 이름"));
            EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("설명", "프로시저 타입에 대한 설명"));
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            
            // 프로시저 버전 섹션
            EditorGUILayout.LabelField("프로시저 버전", headerStyle);
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(versionTypeProperty, new GUIContent("버전 타입", "프로시저의 버전 유형 (가이드라인 또는 임상)"));

            //  ProcedurePlayType 
            EditorGUILayout.PropertyField(procedurePlayTypeProperty, new GUIContent("플레이 타입", "프로시저의 플레이 유형 (연습 또는 실전)"));

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // 프로시저 데이터 섹션
            EditorGUILayout.LabelField("프로시저 데이터", headerStyle);
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(procedureDataProperty, new GUIContent("프로시저 데이터", "이 프로시저 타입에 연결된 프로시저 데이터"));
            
            // 프로시저 데이터 찾기 버튼
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("프로시저 데이터 찾기", GUILayout.Width(150)))
            {
                string[] guids = AssetDatabase.FindAssets("t:ProcedureData");
                if (guids.Length > 0)
                {
                    GenericMenu menu = new GenericMenu();
                    
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        ProcedureData data = AssetDatabase.LoadAssetAtPath<ProcedureData>(path);
                        
                        if (data != null)
                        {
                            menu.AddItem(new GUIContent(data.displayName + " (" + data.id + ")"), 
                                procedureDataProperty.objectReferenceValue == data,
                                () => {
                                    serializedObject.Update();
                                    procedureDataProperty.objectReferenceValue = data;
                                    serializedObject.ApplyModifiedProperties();
                                });
                        }
                    }
                    
                    menu.ShowAsContext();
                }
                else
                {
                    EditorUtility.DisplayDialog("프로시저 데이터 없음", "프로젝트에 프로시저 데이터가 없습니다.", "확인");
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            // 프로시저 데이터 생성 버튼
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("새 프로시저 데이터 생성", GUILayout.Width(150)))
            {
                string defaultName = "New Procedure_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string path = EditorUtility.SaveFilePanelInProject("프로시저 데이터 생성", defaultName, "asset", "새 프로시저 데이터의 저장 위치를 선택하세요.");
                
                if (!string.IsNullOrEmpty(path))
                {
                    ProcedureData newData = CreateInstance<ProcedureData>();
                    newData.id = System.IO.Path.GetFileNameWithoutExtension(path);
                    newData.displayName = System.IO.Path.GetFileNameWithoutExtension(path);
                    
                    AssetDatabase.CreateAsset(newData, path);
                    AssetDatabase.SaveAssets();
                    
                    serializedObject.Update();
                    procedureDataProperty.objectReferenceValue = newData;
                    serializedObject.ApplyModifiedProperties();
                    
                    EditorGUIUtility.PingObject(newData);
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
            
            serializedObject.ApplyModifiedProperties();
            
            // 프로시저 데이터 편집 버튼
            if (procedureDataProperty.objectReferenceValue != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("프로시저 데이터 편집", GUILayout.Width(150)))
                {
                    Selection.activeObject = procedureDataProperty.objectReferenceValue;
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel);
                headerStyle.fontSize = 14;
                headerStyle.margin = new RectOffset(0, 0, 10, 5);
            }
        }
    }
}