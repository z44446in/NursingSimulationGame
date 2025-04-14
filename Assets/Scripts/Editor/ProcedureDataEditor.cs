using UnityEngine;
using UnityEditor;
using Nursing.Procedure;
using Nursing.Interaction;
using System.Collections.Generic;

namespace Nursing.Editor
{
    [CustomEditor(typeof(ProcedureData))]
    public class ProcedureDataEditor : UnityEditor.Editor
    {
        private SerializedProperty idProperty;
        private SerializedProperty displayNameProperty;
        private SerializedProperty descriptionProperty;
        private SerializedProperty stepsProperty;
        private SerializedProperty guideMessageProperty;

        private Dictionary<int, bool> stepFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> settingsFoldouts = new Dictionary<int, bool>();

        private GUIStyle headerStyle;
        private GUIStyle subheaderStyle;

        private void OnEnable()
        {
            idProperty = serializedObject.FindProperty("id");
            displayNameProperty = serializedObject.FindProperty("displayName");
            descriptionProperty = serializedObject.FindProperty("description");
            stepsProperty = serializedObject.FindProperty("steps");


            // 스타일 초기화는 OnInspectorGUI에서 수행
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 스타일 초기화
            InitializeStyles();

            // 기본 정보 섹션
            EditorGUILayout.LabelField("기본 정보", headerStyle);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(idProperty, new GUIContent("ID", "프로시저의 고유 식별자"));
            EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("표시 이름", "프로시저의 화면에 표시될 이름"));
            EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("설명", "프로시저에 대한 설명"));


            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // 프로시저 스텝 섹션
            EditorGUILayout.LabelField("프로시저 스텝", headerStyle);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("스텝 추가", GUILayout.Width(150)))
            {
                AddStep();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 스텝 표시
            for (int i = 0; i < stepsProperty.arraySize; i++)
            {
                DrawStep(i);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel);
                headerStyle.fontSize = 14;
                headerStyle.margin = new RectOffset(0, 0, 10, 5);
            }

            if (subheaderStyle == null)
            {
                subheaderStyle = new GUIStyle(EditorStyles.boldLabel);
                subheaderStyle.fontSize = 12;
                subheaderStyle.margin = new RectOffset(0, 0, 5, 3);
            }
        }

        private void AddStep()
        {
            // 새 스텝 추가
            stepsProperty.arraySize++;
            int newIndex = stepsProperty.arraySize - 1;

            // 기본값 설정
            SerializedProperty newStep = stepsProperty.GetArrayElementAtIndex(newIndex);

            SerializedProperty idProp = newStep.FindPropertyRelative("id");
            idProp.stringValue = "step_" + newIndex;

            SerializedProperty nameProp = newStep.FindPropertyRelative("name");
            nameProp.stringValue = "스텝 " + newIndex;

            // 새 스텝의 폴드아웃 상태를 열림으로 설정
            stepFoldouts[newIndex] = true;
            settingsFoldouts[newIndex] = true;
        }

        private void DrawStep(int index)
        {
            SerializedProperty stepProperty = stepsProperty.GetArrayElementAtIndex(index);

            SerializedProperty idProp = stepProperty.FindPropertyRelative("id");
            SerializedProperty nameProp = stepProperty.FindPropertyRelative("name");
            SerializedProperty guideMessageProp = stepProperty.FindPropertyRelative("guideMessage");
            SerializedProperty stepTypeProp = stepProperty.FindPropertyRelative("stepType");
            SerializedProperty requireSpecificOrderProp = stepProperty.FindPropertyRelative("requireSpecificOrder");
            SerializedProperty requiredPreviousStepIdsProp = stepProperty.FindPropertyRelative("requiredPreviousStepIds");
            SerializedProperty previousStepPenaltiesProp = stepProperty.FindPropertyRelative("previousStepPenalties"); // 추가
          
            SerializedProperty incorrectActionPenaltyProp = stepProperty.FindPropertyRelative("incorrectActionPenalty");
            SerializedProperty settingsProp = stepProperty.FindPropertyRelative("settings");

            // 새로 추가된 필드들에 대한 SerializedProperty
            SerializedProperty restrictNextStepsProp = stepProperty.FindPropertyRelative("restrictNextSteps");
            SerializedProperty allowedNextStepIdsProp = stepProperty.FindPropertyRelative("allowedNextStepIds");
            SerializedProperty invalidNextStepPenaltyProp = stepProperty.FindPropertyRelative("invalidNextStepPenalty");
            SerializedProperty canBeSkippedProp = stepProperty.FindPropertyRelative("canBeSkipped");

            // 기존 폴드아웃 관련 코드...
            if (!stepFoldouts.ContainsKey(index))
            {
                stepFoldouts[index] = false;
            }

            // 스텝 헤더 (폴드아웃)
            EditorGUILayout.BeginHorizontal();

            stepFoldouts[index] = EditorGUILayout.Foldout(stepFoldouts[index], "", true);

            // 스텝 헤더 표시 (이름 + 스텝 타입)
            EditorGUILayout.LabelField("스텝 " + index + ": " + nameProp.stringValue + " (" + stepTypeProp.enumDisplayNames[stepTypeProp.enumValueIndex] + ")", EditorStyles.boldLabel);

            // 삭제 버튼
            if (GUILayout.Button("삭제", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("스텝 삭제", "정말로 이 스텝을 삭제하시겠습니까?", "삭제", "취소"))
                {
                    DeleteStep(index);
                    serializedObject.ApplyModifiedProperties(); // 삭제 적용

                    return; // 더 이상 처리하지 않고 리턴
                }
            }

            // 위로 이동 버튼
            GUI.enabled = index > 0;
            if (GUILayout.Button("↑", GUILayout.Width(25)))
            {
                MoveStep(index, index - 1);
                return; // 더 이상 처리하지 않고 리턴
            }

            // 아래로 이동 버튼
            GUI.enabled = index < stepsProperty.arraySize - 1;
            if (GUILayout.Button("↓", GUILayout.Width(25)))
            {
                MoveStep(index, index + 1);
                return; // 더 이상 처리하지 않고 리턴
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            // 폴드아웃이 열려있는 경우 세부 정보 표시
            if (stepFoldouts[index])
            {
                EditorGUI.indentLevel++;

                // 기본 정보
                EditorGUILayout.PropertyField(idProp, new GUIContent("ID", "스텝의 고유 식별자"));
                EditorGUILayout.PropertyField(nameProp, new GUIContent("이름", "스텝의 이름"));
                EditorGUILayout.PropertyField(guideMessageProp, new GUIContent("가이드 메시지", "이 스텝 실행 시 표시될 가이드 메시지"));

                EditorGUILayout.Space();

                // 스텝 타입
                EditorGUILayout.PropertyField(stepTypeProp, new GUIContent("스텝 타입", "이 스텝의 유형"));

                EditorGUILayout.Space();

                // 인터랙션 설정
                if (!settingsFoldouts.ContainsKey(index))
                {
                    settingsFoldouts[index] = true;
                }

                settingsFoldouts[index] = EditorGUILayout.Foldout(settingsFoldouts[index], "스텝 설정", true);

                if (settingsFoldouts[index])
                {
                    EditorGUI.indentLevel++;

                    // 스텝 타입에 따른 설정 표시
                    DrawStepSettings(settingsProp, (ProcedureStepType)stepTypeProp.enumValueIndex);

                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space();

                // 순서 요구사항
                EditorGUILayout.PropertyField(requireSpecificOrderProp, new GUIContent("특정 순서 요구", "이 스텝이 특정 순서로 실행되어야 하는지 여부"));

                if (requireSpecificOrderProp.boolValue)
                {
                    EditorGUILayout.PropertyField(requiredPreviousStepIdsProp, new GUIContent("필요한 이전 스텝 ID", "이 스텝 전에 완료되어야 하는 스텝 ID 목록"));

                    // 이전 스텝별 패널티 설정
                    EditorGUILayout.PropertyField(previousStepPenaltiesProp, new GUIContent("이전 스텝별 패널티", "각 이전 스텝이 누락되었을 때 적용될 패널티"));

                    // 요소 수 불일치 경고 표시
                    if (previousStepPenaltiesProp.arraySize != requiredPreviousStepIdsProp.arraySize)
                    {
                        EditorGUILayout.HelpBox(
                            "이전 스텝 ID 수와 패널티 수가 일치하지 않습니다. 각 이전 스텝에 대응하는 패널티를 설정하세요.",
                            MessageType.Warning
                        );

                        // 자동 일치 버튼
                        if (GUILayout.Button("패널티 개수 자동 조정"))
                        {
                            previousStepPenaltiesProp.arraySize = requiredPreviousStepIdsProp.arraySize;
                        }
                    }

                   
                }

                EditorGUILayout.Space();

                // 다음 스텝 제한 관련 새 필드들
                
                EditorGUILayout.PropertyField(restrictNextStepsProp, new GUIContent("다음 스텝 제한 사용", "완료 후 특정 다음 스텝만 허용할지 여부"));

                if (restrictNextStepsProp.boolValue)
                {
                    EditorGUILayout.PropertyField(allowedNextStepIdsProp, new GUIContent("허용된 다음 스텝 ID", "이 스텝 완료 후 허용되는 다음 스텝 ID 목록"));
                    EditorGUILayout.PropertyField(invalidNextStepPenaltyProp, new GUIContent("허용되지 않은 스텝 패널티", "허용되지 않은 다음 스텝 시도 시 적용할 패널티"));

                    // 스텝 ID 선택 도우미 버튼
                    if (GUILayout.Button("다음 스텝 ID 선택"))
                    {
                        ShowStepSelectionMenu(allowedNextStepIdsProp);
                    }
                }

                EditorGUILayout.Space();

                // 생략 가능 설정
                
                EditorGUILayout.PropertyField(canBeSkippedProp, new GUIContent("생략 가능", "이 스텝이 완료되지 않아도 전체 프로시저가 완료될 수 있는지 여부"));

                EditorGUILayout.Space();

                // 패널티 설정
                EditorGUILayout.PropertyField(incorrectActionPenaltyProp, new GUIContent("잘못된 행동 패널티", "잘못된 행동을 했을 때 적용할 패널티"));

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
        }



        // 스텝 ID 선택 메뉴 표시
        private void ShowStepSelectionMenu(SerializedProperty stepIdsProperty)
        {
            // 현재 프로시저 데이터 가져오기
            ProcedureData procedureData = target as ProcedureData;
            if (procedureData == null || procedureData.steps == null)
                return;

            GenericMenu menu = new GenericMenu();

            // 현재 선택된 스텝 ID 목록 가져오기
            List<string> selectedIds = new List<string>();
            for (int i = 0; i < stepIdsProperty.arraySize; i++)
            {
                selectedIds.Add(stepIdsProperty.GetArrayElementAtIndex(i).stringValue);
            }

            // 모든 스텝에 대해 메뉴 항목 추가
            for (int i = 0; i < procedureData.steps.Count; i++)
            {
                ProcedureStep step = procedureData.steps[i];
                string itemText = $"{i}: {step.name} ({step.id})";
                bool isSelected = selectedIds.Contains(step.id);

                menu.AddItem(
                    new GUIContent(itemText),
                    isSelected,
                    (object userData) => {
                        string stepId = (string)userData;
                        serializedObject.Update();

                        if (selectedIds.Contains(stepId))
                        {
                            // 이미 선택된 경우, 제거
                            for (int j = 0; j < stepIdsProperty.arraySize; j++)
                            {
                                if (stepIdsProperty.GetArrayElementAtIndex(j).stringValue == stepId)
                                {
                                    stepIdsProperty.DeleteArrayElementAtIndex(j);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // 선택되지 않은 경우, 추가
                            int newIndex = stepIdsProperty.arraySize;
                            stepIdsProperty.arraySize++;
                            stepIdsProperty.GetArrayElementAtIndex(newIndex).stringValue = stepId;
                        }

                        serializedObject.ApplyModifiedProperties();
                    },
                    step.id
                );
            }

            menu.ShowAsContext();
        }
        private void DrawStepSettings(SerializedProperty settingsProp, ProcedureStepType stepType)
        {
            switch (stepType)
            {
                case ProcedureStepType.ItemClick:
                    DrawItemClickSettings(settingsProp);
                    break;
                    
                case ProcedureStepType.ActionButtonClick:
                    DrawActionButtonClickSettings(settingsProp);
                    break;
                    
                case ProcedureStepType.PlayerInteraction:
                    DrawPlayerInteractionSettings(settingsProp);
                    break;
            }
        }
        
        #region 스텝 타입별 설정 드로잉 메서드
        
        private void DrawItemClickSettings(SerializedProperty settingsProp)
        {
            SerializedProperty isItemClickProp = settingsProp.FindPropertyRelative("isItemClick");
            SerializedProperty itemIdProp = settingsProp.FindPropertyRelative("itemId");
            SerializedProperty interactionDataIdProp = settingsProp.FindPropertyRelative("interactionDataId");
            
            // 이 스텝 타입을 활성화하기 위한 플래그
            isItemClickProp.boolValue = true;
            
            // 기본 설정
            EditorGUILayout.PropertyField(itemIdProp, new GUIContent("아이템 ID", "클릭할 아이템의 ID"));

            // 아이템 찾기 버튼 추가
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("아이템 찾기", GUILayout.Width(150)))
            {
                // 아이템 찾기 기능
                string[] guids = AssetDatabase.FindAssets("t:Item");
                if (guids.Length > 0)
                {
                    GenericMenu menu = new GenericMenu();

                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Item item = AssetDatabase.LoadAssetAtPath<Item>(path);

                        if (item != null)
                        {
                            menu.AddItem(
                                new GUIContent(item.itemName + " (" + item.itemId + ")"),
                                itemIdProp.stringValue == item.itemId,
                                () => {
                                    serializedObject.Update();
                                    itemIdProp.stringValue = item.itemId;
                                    serializedObject.ApplyModifiedProperties();
                                });
                        }
                    }

                    menu.ShowAsContext();
                }
                else
                {
                    EditorUtility.DisplayDialog("아이템 없음", "프로젝트에 아이템이 없습니다.", "확인");
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();


            // 인터랙션 데이터 설정
            EditorGUILayout.PropertyField(interactionDataIdProp, new GUIContent("인터랙션 데이터 ID", "실행할 인터랙션 데이터의 ID"));
            
            // 인터랙션 데이터 찾기 버튼
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("인터랙션 데이터 찾기", GUILayout.Width(150)))
            {
                // 인터랙션 데이터 찾기 기능 (선택적 구현)
                string[] guids = AssetDatabase.FindAssets("t:InteractionData");
                if (guids.Length > 0)
                {
                    GenericMenu menu = new GenericMenu();
                    
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        InteractionData data = AssetDatabase.LoadAssetAtPath<InteractionData>(path);
                        
                        if (data != null)
                        {
                         
                            
                            menu.AddItem(
                                new GUIContent(data.displayName + " (" + data.id + ")"), 
                                interactionDataIdProp.stringValue == data.id,
                                () => {
                                    serializedObject.Update();
                                    // 경로 또는 고유 식별자로 ID 저장
                                    interactionDataIdProp.stringValue = data.id;
                                    serializedObject.ApplyModifiedProperties();
                                });
                        }
                    }
                    
                    menu.ShowAsContext();
                }
                else
                {
                    EditorUtility.DisplayDialog("인터랙션 데이터 없음", "프로젝트에 인터랙션 데이터가 없습니다.", "확인");
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawActionButtonClickSettings(SerializedProperty settingsProp)
        {
            SerializedProperty isActionButtonClickProp = settingsProp.FindPropertyRelative("isActionButtonClick");
            SerializedProperty correctButtonIdsProp = settingsProp.FindPropertyRelative("correctButtonIds");
            SerializedProperty requireAllButtonsProp = settingsProp.FindPropertyRelative("requireAllButtons");
            
            // 이 스텝 타입을 활성화하기 위한 플래그
            isActionButtonClickProp.boolValue = true;
            
            // 기본 설정
            EditorGUILayout.PropertyField(correctButtonIdsProp, new GUIContent("올바른 버튼 ID", "올바른 버튼의 ID 목록"));
            EditorGUILayout.PropertyField(requireAllButtonsProp, new GUIContent("모든 버튼 필요", "모든 올바른 버튼을 클릭해야 하는지 여부"));
        }
        
        private void DrawPlayerInteractionSettings(SerializedProperty settingsProp)
        {
            SerializedProperty isPlayerInteractionProp = settingsProp.FindPropertyRelative("isPlayerInteraction");
            SerializedProperty validInteractionTagsProp = settingsProp.FindPropertyRelative("validInteractionTags");
            
            // 이 스텝 타입을 활성화하기 위한 플래그
            isPlayerInteractionProp.boolValue = true;
            
            // 기본 설정
            EditorGUILayout.PropertyField(validInteractionTagsProp, new GUIContent("유효한 상호작용 태그", "유효한 플레이어 상호작용 태그 목록"));
        }
        
        #endregion
        
        private void DeleteStep(int index)
        {
            stepsProperty.DeleteArrayElementAtIndex(index);
            
            // 폴드아웃 상태 업데이트
            var newFoldouts = new Dictionary<int, bool>();
            var newSettingsFoldouts = new Dictionary<int, bool>();

            serializedObject.ApplyModifiedProperties();

            stepFoldouts.Clear();
            settingsFoldouts.Clear();

            for (int i = 0; i < stepsProperty.arraySize; i++)
            {
                stepFoldouts[i] = false;      // 기본은 열려있게 설정하거나 false로 설정 가능
                settingsFoldouts[i] = false;
            }


        }
        
        private void MoveStep(int fromIndex, int toIndex)
        {
            stepsProperty.MoveArrayElement(fromIndex, toIndex);
            
            // 폴드아웃 상태 교환
            bool tempFoldout = stepFoldouts[fromIndex];
            stepFoldouts[fromIndex] = stepFoldouts[toIndex];
            stepFoldouts[toIndex] = tempFoldout;
            
            tempFoldout = settingsFoldouts[fromIndex];
            settingsFoldouts[fromIndex] = settingsFoldouts[toIndex];
            settingsFoldouts[toIndex] = tempFoldout;
        }
    }
}