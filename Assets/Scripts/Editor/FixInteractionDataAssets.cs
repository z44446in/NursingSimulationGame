using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace NursingGame.Editor
{
    /// <summary>
    /// 인터랙션 데이터 에셋을 새 형식으로 업데이트하는 유틸리티
    /// </summary>
    public class FixInteractionDataAssets : EditorWindow
    {
        [MenuItem("Tools/Fix Interaction Data Assets")]
        public static void ShowWindow()
        {
            GetWindow<FixInteractionDataAssets>("Fix Interaction Data");
        }

        private void OnGUI()
        {
            GUILayout.Label("Fix Interaction Data Assets", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Update InteractionData Assets", GUILayout.Height(30)))
            {
                UpdateInteractionDataAssets();
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "This will update all InteractionData assets to use the new property names:\n" +
                "- interactionId -> id\n" +
                "- interactionName -> displayName", 
                MessageType.Info);
        }
        
        private void UpdateInteractionDataAssets()
        {
            // 모든 InteractionData 에셋 찾기
            string[] guids = AssetDatabase.FindAssets("t:InteractionData");
            int updatedCount = 0;
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                InteractionData asset = AssetDatabase.LoadAssetAtPath<InteractionData>(path);
                
                if (asset != null)
                {
                    // 이전 필드에서 새 필드로 값 복사 (직렬화된 프로퍼티 사용)
                    SerializedObject serializedObject = new SerializedObject(asset);
                    
                    SerializedProperty oldIdProperty = serializedObject.FindProperty("interactionId");
                    SerializedProperty oldNameProperty = serializedObject.FindProperty("interactionName");
                    SerializedProperty newIdProperty = serializedObject.FindProperty("id");
                    SerializedProperty newNameProperty = serializedObject.FindProperty("displayName");
                    
                    if (oldIdProperty != null && oldNameProperty != null && 
                        newIdProperty != null && newNameProperty != null)
                    {
                        // 이전 값이 있고 새 값이 비어있는 경우에만 복사
                        if (!string.IsNullOrEmpty(oldIdProperty.stringValue) && string.IsNullOrEmpty(newIdProperty.stringValue))
                        {
                            newIdProperty.stringValue = oldIdProperty.stringValue;
                            serializedObject.ApplyModifiedProperties();
                            Debug.Log($"Updated ID for asset: {path}");
                        }
                        
                        if (!string.IsNullOrEmpty(oldNameProperty.stringValue) && string.IsNullOrEmpty(newNameProperty.stringValue))
                        {
                            newNameProperty.stringValue = oldNameProperty.stringValue;
                            serializedObject.ApplyModifiedProperties();
                            Debug.Log($"Updated Name for asset: {path}");
                        }
                        
                        updatedCount++;
                    }
                    
                    EditorUtility.SetDirty(asset);
                }
            }
            
            AssetDatabase.SaveAssets();
            
            if (updatedCount > 0)
            {
                Debug.Log($"Updated {updatedCount} InteractionData assets");
                EditorUtility.DisplayDialog("Assets Updated", $"Successfully updated {updatedCount} InteractionData assets.", "OK");
            }
            else
            {
                Debug.Log("No InteractionData assets needed updating");
                EditorUtility.DisplayDialog("No Updates Needed", "No InteractionData assets needed updating.", "OK");
            }
        }
        
        /// <summary>
        /// 테스트용 에셋 생성 - 디버깅용
        /// </summary>
        [MenuItem("Tools/Create Test InteractionData")]
        public static void CreateTestInteractionData()
        {
            // 테스트 에셋 생성
            InteractionData newAsset = ScriptableObject.CreateInstance<InteractionData>();
            newAsset.id = System.Guid.NewGuid().ToString().Substring(0, 8);
            newAsset.displayName = "Test Interaction";
            newAsset.description = "This is a test interaction data asset";
            
            // 예제 단계 추가
            InteractionStepData step = new InteractionStepData
            {
                stepId = "step_" + System.Guid.NewGuid().ToString().Substring(0, 8),
                stepName = "Test Step",
                interactionType = InteractionType.SingleClick,
                guideText = "Click on the target"
            };
            
            newAsset.steps = new List<InteractionStepData> { step };
            
            // 에셋 저장
            string path = "Assets/ScriptableObjects/Catherization/TestInteractionData.asset";
            AssetDatabase.CreateAsset(newAsset, path);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Created test InteractionData asset at {path}");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newAsset;
        }
    }
}