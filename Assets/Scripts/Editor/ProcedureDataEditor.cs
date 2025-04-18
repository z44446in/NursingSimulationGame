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
        private SerializedProperty requiredItemsProperty;
        private SerializedProperty intermediateRequiredItemsProperty;
        private SerializedProperty hiddenInIntermediateItemsProperty;
        private SerializedProperty excludedAreaItemsProperty;


        private Dictionary<int, bool> stepFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> settingsFoldouts = new Dictionary<int, bool>();

        // 프로퍼티 추가
        private SerializedProperty unnecessaryItemsProperty;

        private GUIStyle headerStyle;
        private GUIStyle subheaderStyle;

        private void OnEnable()
        {
            idProperty = serializedObject.FindProperty("id");
            displayNameProperty = serializedObject.FindProperty("displayName");
            descriptionProperty = serializedObject.FindProperty("description");
            stepsProperty = serializedObject.FindProperty("steps");
            requiredItemsProperty = serializedObject.FindProperty("requiredItems");
            intermediateRequiredItemsProperty = serializedObject.FindProperty("intermediateRequiredItems");
            excludedAreaItemsProperty = serializedObject.FindProperty("excludedAreaItems");
            unnecessaryItemsProperty = serializedObject.FindProperty("unnecessaryItems");
            hiddenInIntermediateItemsProperty = serializedObject.FindProperty("hiddenInIntermediateItems");
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
            
            // 준비실 필수 아이템 섹션
            EditorGUILayout.LabelField("준비실 필수 아이템", headerStyle);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(requiredItemsProperty, true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            
            // 준비실 불필요 아이템 섹션
            EditorGUILayout.LabelField("준비실 불필요 아이템", headerStyle);
            EditorGUI.indentLevel++;

            // 기존 불필요 아이템 표시
            EditorGUILayout.PropertyField(unnecessaryItemsProperty, true);

            // 불필요 아이템 자동 추가 버튼
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("모든 불필요 아이템 불러오기", GUILayout.Width(200), GUILayout.Height(30)))
            {
                PopulateUnnecessaryItems();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // 중간 단계 필수 아이템 섹션
            EditorGUILayout.LabelField("중간 단계 필수 아이템", headerStyle);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(intermediateRequiredItemsProperty, true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            
             // 중간화면 미표시 아이템 섹션
    EditorGUILayout.LabelField("중간화면 미표시 아이템", headerStyle);
    EditorGUI.indentLevel++;
    EditorGUILayout.PropertyField(hiddenInIntermediateItemsProperty, true);
    EditorGUI.indentLevel--;
    EditorGUILayout.Space();
    
            // 준비실 제외 아이템 섹션
            EditorGUILayout.LabelField("준비실 제외 아이템", headerStyle);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(excludedAreaItemsProperty, true);
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


        private void PopulateUnnecessaryItems()
{
    ProcedureData procedureData = target as ProcedureData;
    
    // 모든 Item 에셋 로드
    List<Item> allItems = new List<Item>();
    string[] guids = AssetDatabase.FindAssets("t:Item");
    
    foreach (string guid in guids)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        Item item = AssetDatabase.LoadAssetAtPath<Item>(path);
        if (item != null)
        {
            allItems.Add(item);
        }
    }
    
    // 현재 필수 아이템 ID 목록
    HashSet<string> requiredItemIds = new HashSet<string>();
    foreach (var requiredItem in procedureData.requiredItems)
    {
        if (requiredItem.item != null)
            requiredItemIds.Add(requiredItem.item.itemId);
    }
    
    // 현재 불필요 아이템 ID 목록
    HashSet<string> unnecessaryItemIds = new HashSet<string>();
    if (procedureData.unnecessaryItems == null)
        procedureData.unnecessaryItems = new List<UnnecessaryItem>();
        
    foreach (var unnecessaryItem in procedureData.unnecessaryItems)
    {
        if (unnecessaryItem.item != null)
            unnecessaryItemIds.Add(unnecessaryItem.item.itemId);
    }
    
    // 필요하지 않고 아직 불필요 목록에 없는 아이템 추가
    int addedCount = 0;
    foreach (var item in allItems)
    {
        if (!requiredItemIds.Contains(item.itemId) && !unnecessaryItemIds.Contains(item.itemId))
        {
            procedureData.unnecessaryItems.Add(new UnnecessaryItem 
            { 
                item = item,
                unnecessaryReason = "이 술기에서는 필요하지 않습니다." // 기본 메시지
            });
            addedCount++;
        }
    }
    
    // 변경사항 저장
    EditorUtility.SetDirty(procedureData);
    AssetDatabase.SaveAssets();
    
    EditorUtility.DisplayDialog("불필요 아이템 추가 완료", 
        $"{addedCount}개의 불필요 아이템이 추가되었습니다.", "확인");
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

            SerializedProperty isRepeatableProp = stepProperty.FindPropertyRelative("isRepeatable");
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
                    // 스텝 ID 선택 버튼 추가
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("스텝 ID 선택", GUILayout.Width(150)))
                    {
                        // 현재 ProcedureData에서 스텝 목록 가져오기
                        ProcedureData procedureData = target as ProcedureData;
                        if (procedureData != null && procedureData.steps != null)
                        {
                            GenericMenu menu = new GenericMenu();
                            int currentStepIndex = index; // 현재 스텝 인덱스
                            
                            for (int i = 0; i < procedureData.steps.Count; i++)
                            {
                                if (i != currentStepIndex) // 자기 자신은 제외
                                {
                                    ProcedureStep otherStep = procedureData.steps[i];
                                    // 이미 목록에 있는지 확인
                                    bool isSelected = false;
                                    for (int j = 0; j < requiredPreviousStepIdsProp.arraySize; j++)
                                    {
                                        string existingId = requiredPreviousStepIdsProp.GetArrayElementAtIndex(j).stringValue;
                                        if (existingId == otherStep.id)
                                        {
                                            isSelected = true;
                                            break;
                                        }
                                    }
                                    
                                    // 메뉴 아이템 추가
                                    menu.AddItem(
                                        new GUIContent(otherStep.name + " (" + otherStep.id + ")"),
                                        isSelected,
                                        () => {
                                            if (!isSelected)
                                            {
                                                // 목록에 추가
                                                serializedObject.Update();
                                                requiredPreviousStepIdsProp.arraySize++;
                                                requiredPreviousStepIdsProp.GetArrayElementAtIndex(requiredPreviousStepIdsProp.arraySize - 1).stringValue = otherStep.id;
                                                serializedObject.ApplyModifiedProperties();
                                            }
                                        });
                                }
                            }
                            
                            menu.ShowAsContext();
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

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

        
                // 반복 설정 섹션 추가
             
                EditorGUILayout.PropertyField(isRepeatableProp, new GUIContent("반복 가능", "이 스텝이 반복적으로 수행될 수 있는지 여부"));
                
                EditorGUILayout.Space();
              // 자동 다음 스텝 설정
               SerializedProperty isAutoNextProp    = stepProperty.FindPropertyRelative("isAutoNext");
               SerializedProperty autoNextStepIdProp = stepProperty.FindPropertyRelative("autoNextStepId");
               
               EditorGUILayout.PropertyField(isAutoNextProp, new GUIContent("자동 이동 사용", "이 스텝 완료 후 자동으로 다음 스텝으로 이동할지 여부"));
               if (isAutoNextProp.boolValue)
               {
                   EditorGUILayout.PropertyField(autoNextStepIdProp, new GUIContent("다음 스텝 ID", "자동으로 넘어갈 스텝의 ID"));
                   // ID 선택 도우미 버튼
                  if (GUILayout.Button("스텝 ID 선택"))
                       ShowStepSelectionMenu(autoNextStepIdProp);
               }

                // 패널티 설정
                EditorGUILayout.PropertyField(incorrectActionPenaltyProp, new GUIContent("잘못된 행동 패널티", "잘못된 행동을 했을 때 적용할 패널티"));

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
        }



        // 스텝 ID 선택 메뉴 표시
         private void ShowStepSelectionMenu(SerializedProperty singleStringProp)
{
    ProcedureData data = target as ProcedureData;
    var menu = new GenericMenu();
    foreach (var step in data.steps)
    {
        bool on = singleStringProp.stringValue == step.id;
        menu.AddItem(new GUIContent($"{step.name} ({step.id})"), on, () => {
            serializedObject.Update();
            singleStringProp.stringValue = step.id;
            serializedObject.ApplyModifiedProperties();
        });
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
                // 여기에 추가
                case ProcedureStepType.InteractionOnly:
                    DrawInteractionOnlySettings(settingsProp);
                    break;

                    

            }
        }
        
        private void DrawInteractionOnlySettings(SerializedProperty settingsProp)
{
    // InteractionOnly 타입 활성화 플래그
    var isInteractionOnlyProp = settingsProp.FindPropertyRelative("isInteractionOnly");
    isInteractionOnlyProp.boolValue = true;

    // 연동할 InteractionData ID
    var onlyIdProp = settingsProp.FindPropertyRelative("OnlyinteractionDataId");
    EditorGUILayout.PropertyField(
        onlyIdProp,
        new GUIContent("InteractionData ID", "실행할 InteractionData 에셋의 ID")
    );

    // “인터랙션 데이터 찾기” 버튼
    EditorGUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    if (GUILayout.Button("인터랙션 데이터 찾기", GUILayout.Width(150)))
    {
        // 프로젝트 내 모든 InteractionData 에셋 검색
        string[] guids = AssetDatabase.FindAssets("t:InteractionData");
        if (guids.Length > 0)
        {
            var menu = new GenericMenu();
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<InteractionData>(path);
                if (data != null)
                {
                    bool selected = onlyIdProp.stringValue == data.id;
                    menu.AddItem(
                        new GUIContent($"{data.displayName} ({data.id})"),
                        selected,
                        () => {
                            serializedObject.Update();
                            onlyIdProp.stringValue = data.id;
                            serializedObject.ApplyModifiedProperties();
                        }
                    );
                }
            }
            menu.ShowAsContext();
        }
        else
        {
            EditorUtility.DisplayDialog(
                "인터랙션 데이터 없음",
                "프로젝트에 InteractionData 에셋이 없습니다.",
                "확인"
            );
        }
    }
    GUILayout.FlexibleSpace();
    EditorGUILayout.EndHorizontal();
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