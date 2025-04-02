# 멸균증류수 절차 구현 가이드

## 현재 에디터 분석과 개선 방안

dillitedwaterprocedure.txt에 명시된 멸균증류수 절차를 현재 에디터 시스템으로 구현하기 위해서는 몇 가지 추가 기능이 필요합니다.

### 1. 현재 시스템 지원 기능

현재 GenericInteractionData와 GenericProcedureData 에디터는 다음 기능을 지원합니다:

- 다양한 상호작용 유형(SingleClick, Drag, Quiz 등)
- 시각적 가이드 시스템(화살표 표시)
- 퀴즈 및 선택지 기반 상호작용
- 다양한 피드백 메커니즘(성공/실패 메시지)
- 원하는 순서대로 단계 구성

### 2. 부족한 기능 및 개선 필요사항

dillitedwaterprocedure.txt의 요구사항과 현재 에디터를 비교하면 다음 기능들이 부족합니다:

1. **다중 단계 드래그 상호작용**: 뚜껑을 열기 위해 두 번의 연속 드래그가 필요함
2. **조건부 터치 처리**: 터치한 오브젝트에 따라 다른 반응 분기
3. **오류 시각 효과**: 빨간색 테두리 깜빡임
4. **시간 기반 터치 비활성화**: 오류 후 5초간 터치 불가
5. **단계별 오브젝트 변형**: 뚜껑 움직임, 물 이미지 생성 등

### 3. 에디터 확장 방안

멸균증류수 절차를 인스펙터에서 쉽게 구현할 수 있도록 다음과 같은 에디터 확장을 제안합니다:

#### 3.1. `GenericInteractionData.cs` 확장

```csharp
// 멸균증류수 절차를 위한 추가 필드
[Header("초기 오브젝트 생성 설정")]
public bool createInitialObjects = false;
public List<InitialObjectData> initialObjects = new List<InitialObjectData>();

[System.Serializable]
public class InitialObjectData
{
    public string objectId;
    public string objectName;
    public Sprite objectSprite;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale = Vector3.one;
    public string tag = "Untagged";
    public bool useCustomPrefab = false;
    public GameObject customPrefab;
}

[Header("다중 단계 드래그 설정")]
public bool useMultiStageDrag = false;
public List<MultiStageDragSettings> multiStageDragSteps = new List<MultiStageDragSettings>();

[System.Serializable]
public class MultiStageDragSettings
{
    public string stepId;
    public Vector2 arrowPosition;
    public float arrowRotation;
    public float requiredDragAngle;
    public float dragAngleTolerance = 30f;
    public bool requireStartOnObject = true;
    public string requiredStartTag;
    public Vector3 targetPositionOffset;
    public Vector3 targetRotationOffset;
}

[Header("조건부 터치 설정")]
public bool useConditionalTouch = false;
public List<ConditionalTouchOption> touchOptions = new List<ConditionalTouchOption>();

[System.Serializable]
public class ConditionalTouchOption
{
    public string optionId;
    public string targetTag;
    public string successMessage;
    public string errorMessage;
    public bool isCorrectOption = false;
    public bool showErrorBorderFlash = false;
    public float disableTouchDuration = 0f;
    public string errorEntryText = "";
    public Vector2 waterEffectPosition;
    public bool createWaterImageOnObject = false;
}
```

#### 3.2. `GenericInteractionDataEditor.cs` 확장

```csharp
// 초기 오브젝트 설정 UI 그리기
private void DrawInitialObjectsSettings(SerializedProperty stepProp)
{
    EditorGUILayout.Space(5);
    EditorGUILayout.LabelField("초기 오브젝트 생성 설정", EditorStyles.boldLabel);
    
    SerializedProperty createInitialObjectsProp = stepProp.FindPropertyRelative("createInitialObjects");
    EditorGUILayout.PropertyField(createInitialObjectsProp, new GUIContent("초기 오브젝트 생성"));
    
    if (createInitialObjectsProp.boolValue)
    {
        SerializedProperty initialObjectsProp = stepProp.FindPropertyRelative("initialObjects");
        EditorGUILayout.PropertyField(initialObjectsProp, new GUIContent("생성할 오브젝트"), true);
        
        // 각 오브젝트별 편집 UI
        if (initialObjectsProp.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            for (int i = 0; i < initialObjectsProp.arraySize; i++)
            {
                EditorGUILayout.BeginVertical("box");
                
                SerializedProperty objectProp = initialObjectsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.LabelField($"오브젝트 {i+1}", EditorStyles.boldLabel);
                
                EditorGUILayout.PropertyField(objectProp.FindPropertyRelative("objectId"), new GUIContent("오브젝트 ID"));
                EditorGUILayout.PropertyField(objectProp.FindPropertyRelative("objectName"), new GUIContent("오브젝트 이름"));
                EditorGUILayout.PropertyField(objectProp.FindPropertyRelative("objectSprite"), new GUIContent("스프라이트"));
                EditorGUILayout.PropertyField(objectProp.FindPropertyRelative("position"), new GUIContent("위치"));
                EditorGUILayout.PropertyField(objectProp.FindPropertyRelative("rotation"), new GUIContent("회전"));
                EditorGUILayout.PropertyField(objectProp.FindPropertyRelative("scale"), new GUIContent("크기"));
                EditorGUILayout.PropertyField(objectProp.FindPropertyRelative("tag"), new GUIContent("태그"));
                
                SerializedProperty useCustomPrefabProp = objectProp.FindPropertyRelative("useCustomPrefab");
                EditorGUILayout.PropertyField(useCustomPrefabProp, new GUIContent("커스텀 프리팹 사용"));
                
                if (useCustomPrefabProp.boolValue)
                {
                    EditorGUILayout.PropertyField(objectProp.FindPropertyRelative("customPrefab"), new GUIContent("커스텀 프리팹"));
                }
                
                // 미리보기 (스프라이트가 있는 경우)
                SerializedProperty spriteProp = objectProp.FindPropertyRelative("objectSprite");
                if (spriteProp.objectReferenceValue != null)
                {
                    Sprite sprite = (Sprite)spriteProp.objectReferenceValue;
                    Rect previewRect = EditorGUILayout.GetControlRect(false, 100);
                    EditorGUI.DrawPreviewTexture(previewRect, sprite.texture, null, ScaleMode.ScaleToFit);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
            
            EditorGUI.indentLevel--;
        }
        
        // 새 오브젝트 추가 버튼
        if (GUILayout.Button("오브젝트 추가"))
        {
            initialObjectsProp.arraySize++;
            SerializedProperty newObject = initialObjectsProp.GetArrayElementAtIndex(initialObjectsProp.arraySize - 1);
            newObject.FindPropertyRelative("objectId").stringValue = "obj_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            newObject.FindPropertyRelative("objectName").stringValue = $"오브젝트 {initialObjectsProp.arraySize}";
            newObject.FindPropertyRelative("scale").vector3Value = Vector3.one;
        }
    }
}

// 다중 단계 드래그 설정 UI 그리기
private void DrawMultiStageDragSettings(SerializedProperty stepProp)
{
    EditorGUILayout.Space(5);
    EditorGUILayout.LabelField("다중 단계 드래그 설정", EditorStyles.boldLabel);
    
    SerializedProperty useMultiStageDragProp = stepProp.FindPropertyRelative("useMultiStageDrag");
    EditorGUILayout.PropertyField(useMultiStageDragProp, new GUIContent("다중 단계 드래그 사용"));
    
    if (useMultiStageDragProp.boolValue)
    {
        SerializedProperty multiStageDragStepsProp = stepProp.FindPropertyRelative("multiStageDragSteps");
        EditorGUILayout.PropertyField(multiStageDragStepsProp, new GUIContent("드래그 단계"), true);
        
        // 각 단계별 편집 UI
        if (multiStageDragStepsProp.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            for (int i = 0; i < multiStageDragStepsProp.arraySize; i++)
            {
                EditorGUILayout.BeginVertical("box");
                
                SerializedProperty dragStepProp = multiStageDragStepsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.LabelField($"드래그 단계 {i+1}", EditorStyles.boldLabel);
                
                EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("stepId"), new GUIContent("단계 ID"));
                EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("arrowPosition"), new GUIContent("화살표 위치"));
                EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("arrowRotation"), new GUIContent("화살표 회전"));
                
                SerializedProperty angleProp = dragStepProp.FindPropertyRelative("requiredDragAngle");
                
                // 원형 방향 선택기 그리기 (기존 코드 활용)
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("드래그 방향");
                
                Rect rect = EditorGUILayout.GetControlRect(false, 100);
                float newAngle = DrawAngleSelector(rect, angleProp.floatValue, i);
                if (newAngle != angleProp.floatValue)
                {
                    angleProp.floatValue = newAngle;
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.PropertyField(angleProp, new GUIContent("필요 드래그 각도"));
                EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("dragAngleTolerance"), new GUIContent("허용 오차 범위"));
                EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("requireStartOnObject"), new GUIContent("오브젝트에서 시작 필요"));
                
                SerializedProperty requireStartTagProp = dragStepProp.FindPropertyRelative("requireStartOnObject");
                if (requireStartTagProp.boolValue)
                {
                    EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("requiredStartTag"), new GUIContent("시작 오브젝트 태그"));
                }
                
                EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("targetPositionOffset"), new GUIContent("목표 위치 변화"));
                EditorGUILayout.PropertyField(dragStepProp.FindPropertyRelative("targetRotationOffset"), new GUIContent("목표 회전 변화"));
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
            
            EditorGUI.indentLevel--;
        }
        
        // 새 단계 추가 버튼
        if (GUILayout.Button("드래그 단계 추가"))
        {
            multiStageDragStepsProp.arraySize++;
            SerializedProperty newStep = multiStageDragStepsProp.GetArrayElementAtIndex(multiStageDragStepsProp.arraySize - 1);
            newStep.FindPropertyRelative("stepId").stringValue = "dragstep_" + System.Guid.NewGuid().ToString().Substring(0, 8);
        }
    }
}

// 조건부 터치 설정 UI 그리기
private void DrawConditionalTouchSettings(SerializedProperty stepProp)
{
    EditorGUILayout.Space(5);
    EditorGUILayout.LabelField("조건부 터치 설정", EditorStyles.boldLabel);
    
    SerializedProperty useConditionalTouchProp = stepProp.FindPropertyRelative("useConditionalTouch");
    EditorGUILayout.PropertyField(useConditionalTouchProp, new GUIContent("조건부 터치 사용"));
    
    if (useConditionalTouchProp.boolValue)
    {
        SerializedProperty touchOptionsProp = stepProp.FindPropertyRelative("touchOptions");
        EditorGUILayout.PropertyField(touchOptionsProp, new GUIContent("터치 옵션"), true);
        
        // 각 옵션별 편집 UI
        if (touchOptionsProp.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            for (int i = 0; i < touchOptionsProp.arraySize; i++)
            {
                EditorGUILayout.BeginVertical("box");
                
                SerializedProperty optionProp = touchOptionsProp.GetArrayElementAtIndex(i);
                EditorGUILayout.LabelField($"터치 옵션 {i+1}", EditorStyles.boldLabel);
                
                EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("optionId"), new GUIContent("옵션 ID"));
                EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("targetTag"), new GUIContent("대상 태그"));
                EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("successMessage"), new GUIContent("성공 메시지"));
                EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("errorMessage"), new GUIContent("오류 메시지"));
                EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("isCorrectOption"), new GUIContent("올바른 옵션"));
                EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("showErrorBorderFlash"), new GUIContent("오류 테두리 깜빡임"));
                EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("disableTouchDuration"), new GUIContent("터치 비활성화 시간"));
                EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("errorEntryText"), new GUIContent("오류 기록 텍스트"));
                EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("waterEffectPosition"), new GUIContent("물 효과 위치"));
                EditorGUILayout.PropertyField(optionProp.FindPropertyRelative("createWaterImageOnObject"), new GUIContent("물 이미지 생성"));
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
            
            EditorGUI.indentLevel--;
        }
        
        // 새 옵션 추가 버튼
        if (GUILayout.Button("터치 옵션 추가"))
        {
            touchOptionsProp.arraySize++;
            SerializedProperty newOption = touchOptionsProp.GetArrayElementAtIndex(touchOptionsProp.arraySize - 1);
            newOption.FindPropertyRelative("optionId").stringValue = "touchoption_" + System.Guid.NewGuid().ToString().Substring(0, 8);
        }
    }
}
```

#### 3.3. `BaseInteractionSystem.cs` 확장

```csharp
// 초기 오브젝트 생성 처리
public virtual void CreateInitialObjects(string interactionId)
{
    if (!interactionsDatabase.ContainsKey(interactionId))
        return;
        
    var data = interactionsDatabase[interactionId];
    if (data.steps.Count == 0 || !data.steps[0].createInitialObjects)
        return;
    
    var initialObjects = data.steps[0].initialObjects;
    foreach (var objData in initialObjects)
    {
        GameObject createdObject = null;
        
        // 커스텀 프리팹 사용 여부에 따라 생성
        if (objData.useCustomPrefab && objData.customPrefab != null)
        {
            createdObject = Instantiate(objData.customPrefab, objData.position, Quaternion.Euler(objData.rotation));
        }
        else if (objData.objectSprite != null)
        {
            // 기본 스프라이트 오브젝트 생성
            createdObject = new GameObject(objData.objectName);
            createdObject.transform.position = objData.position;
            createdObject.transform.rotation = Quaternion.Euler(objData.rotation);
            createdObject.transform.localScale = objData.scale;
            
            // 스프라이트 렌더러 추가
            SpriteRenderer renderer = createdObject.AddComponent<SpriteRenderer>();
            renderer.sprite = objData.objectSprite;
            
            // 콜라이더 추가 (선택적)
            BoxCollider2D collider = createdObject.AddComponent<BoxCollider2D>();
            collider.size = renderer.sprite.bounds.size;
        }
        
        // 태그 설정
        if (createdObject != null && !string.IsNullOrEmpty(objData.tag))
        {
            createdObject.tag = objData.tag;
            
            // 생성된 오브젝트 저장 (필요시 참조)
            initialObjectsCache[objData.objectId] = createdObject;
        }
    }
}

// 다중 단계 드래그 처리 로직
public virtual void HandleMultiStageDrag(Vector2 start, Vector2 end, Vector2 direction, int stageIndex)
{
    if (!isInteractionActive || !interactionsDatabase.ContainsKey(currentInteractionId))
        return;
        
    var data = interactionsDatabase[currentInteractionId];
    if (currentStepIndex >= data.steps.Count)
        return;
        
    var step = data.steps[currentStepIndex];
    
    // 현재 단계가 다중 단계 드래그인지 확인
    if (!step.useMultiStageDrag || stageIndex >= step.multiStageDragSteps.Count)
    {
        ShowError("이 단계에서는 다중 단계 드래그를 사용할 수 없습니다.");
        return;
    }
    
    var dragStage = step.multiStageDragSteps[stageIndex];
    
    // 필요한 시작 지점 확인
    bool validStart = true;
    if (dragStage.requireStartOnObject)
    {
        GameObject hitObject = GetObjectAtPosition(start);
        validStart = (hitObject != null && hitObject.CompareTag(dragStage.requiredStartTag));
    }
    
    // 드래그 각도 계산
    float dragAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    if (dragAngle < 0) dragAngle += 360f;
    
    // 허용 오차 범위 확인
    float angleDifference = Mathf.Abs(Mathf.DeltaAngle(dragAngle, dragStage.requiredDragAngle));
    
    if (validStart && angleDifference <= dragStage.dragAngleTolerance)
    {
        // 성공 - 대상 오브젝트 변형
        Transform targetObj = GetDragTargetObject(dragStage.requiredStartTag);
        if (targetObj != null)
        {
            // 위치 및 회전 변경
            targetObj.DOLocalMove(targetObj.localPosition + dragStage.targetPositionOffset, 0.5f);
            targetObj.DOLocalRotate(targetObj.localEulerAngles + dragStage.targetRotationOffset, 0.5f);
        }
        
        // 스테이지 완료 처리
        CompleteMultiStageDrag(stageIndex);
    }
    else
    {
        // 오류 처리
        string errorMessage = validStart 
            ? "잘못된 드래그 방향입니다."
            : "올바른 위치에서 드래그를 시작하세요.";
        ShowError(errorMessage);
    }
}

// 드래그 대상 오브젝트 찾기
private Transform GetDragTargetObject(string tag)
{
    GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
    
    if (taggedObjects != null && taggedObjects.Length > 0)
    {
        return taggedObjects[0].transform;
    }
    
    // 생성된 초기 오브젝트에서 찾기
    foreach (var obj in initialObjectsCache.Values)
    {
        if (obj != null && obj.CompareTag(tag))
        {
            return obj.transform;
        }
    }
    
    return null;
}

// 조건부 터치 처리 로직
public virtual void HandleConditionalTouch(Vector2 position)
{
    if (!isInteractionActive || !interactionsDatabase.ContainsKey(currentInteractionId))
        return;
        
    var data = interactionsDatabase[currentInteractionId];
    if (currentStepIndex >= data.steps.Count)
        return;
        
    var step = data.steps[currentStepIndex];
    
    // 현재 단계가 조건부 터치인지 확인
    if (!step.useConditionalTouch)
    {
        ShowError("이 단계에서는 조건부 터치를 사용할 수 없습니다.");
        return;
    }
    
    // 터치한 오브젝트 확인
    GameObject hitObject = GetObjectAtPosition(position);
    if (hitObject == null)
    {
        ShowError("유효한 오브젝트를 터치하세요.");
        return;
    }
    
    // 해당 태그에 맞는 옵션 찾기
    ConditionalTouchOption matchedOption = null;
    foreach (var option in step.touchOptions)
    {
        if (hitObject.CompareTag(option.targetTag))
        {
            matchedOption = option;
            break;
        }
    }
    
    // 옵션이 없으면 기본 오류
    if (matchedOption == null)
    {
        ShowError("올바른 위치를 터치하세요.");
        return;
    }
    
    // 옵션에 따른 처리
    if (matchedOption.isCorrectOption)
    {
        // 성공 처리
        ShowSuccessFeedback();
        
        if (!string.IsNullOrEmpty(matchedOption.successMessage))
        {
            DialogueManager.Instance?.ShowSmallDialogue(matchedOption.successMessage);
        }
        
        // 물 효과 표시 및 물 이미지 생성
        if (matchedOption.waterEffectPosition != Vector2.zero)
        {
            ShowWaterEffect(matchedOption.waterEffectPosition);
        }
        
        if (matchedOption.createWaterImageOnObject)
        {
            CreateWaterImageOnObject(hitObject);
        }
        
        // 다음 단계 또는 완료 처리
        currentStepIndex++;
        if (currentStepIndex >= data.steps.Count)
        {
            CompleteInteraction();
        }
        else
        {
            ShowCurrentStepGuide();
        }
    }
    else
    {
        // 오류 처리
        ShowError(matchedOption.errorMessage);
        
        // 오류 테두리 깜빡임 효과
        if (matchedOption.showErrorBorderFlash)
        {
            ShowErrorBorderFlash();
        }
        
        // 터치 비활성화
        if (matchedOption.disableTouchDuration > 0)
        {
            DisableTouch(matchedOption.disableTouchDuration);
        }
        
        // 오류 기록
        if (!string.IsNullOrEmpty(matchedOption.errorEntryText))
        {
            RecordError(matchedOption.errorEntryText);
        }
    }
}
```

## 멸균증류수 절차 인스펙터 구현 방법

위의 에디터 확장이 구현된다고 가정하고, dillitedwaterprocedure.txt의 요구사항을 인스펙터에서 구현하는 단계적 방법을 안내합니다.

### 1. 멸균증류수 상호작용 데이터 생성

1. Project 창에서 `Create > Nursing > Generic > Interaction Data` 선택
2. 이름을 "DilutedWaterInteraction" 지정하고 저장
3. 인스펙터에서 기본 정보 작성:
   - Interaction ID: "dilutedWaterInteraction"
   - Interaction Name: "멸균증류수 절차"
   - Description: "멸균증류수 뚜껑을 열고 물을 붓는 절차"

### 2. 초기 오브젝트 설정 - 뚜껑 생성

1. "초기 오브젝트 생성" 체크
2. "오브젝트 추가" 버튼 클릭하여 뚜껑 오브젝트 생성
   - Object ID: "bottleCap"
   - Object Name: "멸균증류수 뚜껑"
   - 스프라이트: 뚜껑 이미지 할당
   - 위치: 뚜껑이 표시될 화면 위치 (X, Y, Z)
   - 회전: 초기 회전값 (0, 0, 0)
   - 크기: 적절한 크기 (예: 1, 1, 1)
   - 태그: "BottleCap"

### 3. 드래그로 뚜껑 여는 단계 구성

1. "새 단계 추가" 버튼 클릭
2. 단계 기본 정보:
   - Step ID: "openBottleCap"
   - Step Name: "뚜껑 열기"
   - Interaction Type: "MultiStageDrag" (새로 추가된 유형)
   - Guide Text: "드래그로 뚜껑을 여세요."

3. 다중 단계 드래그 설정:
   - 다중 단계 드래그 사용: 체크
   - 드래그 단계 추가 (첫 번째 드래그):
     - Step ID: "dragBottleDown"
     - Arrow Position: 화살표 위치 설정
     - Arrow Rotation: -90 (아래 방향)
     - Required Drag Angle: 270 (아래로)
     - Drag Angle Tolerance: 30
     - Require Start On Object: 체크
     - Required Start Tag: "BottleCap"
     - Target Position Offset: (0, -5, 0)
     - Target Rotation Offset: (0, 0, -30)

   - 드래그 단계 추가 (두 번째 드래그):
     - Step ID: "dragBottleSide"
     - Arrow Position: 화살표 위치 설정
     - Arrow Rotation: 180 (왼쪽 방향)
     - Required Drag Angle: 180 (왼쪽으로)
     - Drag Angle Tolerance: 30
     - Require Start On Object: 체크
     - Required Start Tag: "BottleCap"
     - Target Position Offset: (-200, 0, 0)
     - Target Rotation Offset: (0, 0, -90)

4. 피드백 메시지:
   - Success Message: "뚜껑을 성공적으로 열었습니다!"
   - Error Message: "올바른 방향으로 드래그하세요."

### 4. 퀴즈 단계 구성

1. "새 단계 추가" 버튼 클릭
2. 단계 기본 정보:
   - Step ID: "bottleCapQuiz"
   - Step Name: "뚜껑 퀴즈"
   - Interaction Type: "Quiz"
   - Guide Text: "뚜껑은 어떻게 들어야 할까요?"

3. 퀴즈 설정:
   - 질문: "뚜껑은 어떻게 들어야 할까요?"
   - 선택지:
     - 선택지 1: "안들고 책상에 둬도 된다."
     - 선택지 2: "[이미지1]"
     - 선택지 3: "[이미지2]"
   - 정답 선택지 인덱스: 1 (두 번째 옵션)
   - Success Message: "정확합니다!"
   - Error Message: "멸균상태를 유지하려면 뚜껑을 이렇게 들어야 해요!"

### 5. 물 붓기 단계 구성

1. "새 단계 추가" 버튼 클릭
2. 단계 기본 정보:
   - Step ID: "pourWater"
   - Step Name: "물 붓기"
   - Interaction Type: "ConditionalTouch" (새로 추가된 유형)
   - Guide Text: "멸균증류수를 부을 곳을 터치하세요."

3. 조건부 터치 설정:
   - 조건부 터치 사용: 체크
   - 터치 옵션 추가 (쓰레기통):
     - Option ID: "trashBinOption"
     - Target Tag: "TrashBin"
     - Success Message: "다음으로 멸균증류수를 부을 곳을 터치하세요."
     - Is Correct Option: 체크
     - Water Effect Position: 쓰레기통 위치 설정

   - 터치 옵션 추가 (종지):
     - Option ID: "containerOption"
     - Target Tag: "MedicineContainer"
     - Error Message: "병의 윗부분은 오염되어있을 수 있기 때문에, 소량의 물을 의료폐기물상자에 버리고 종지에 따라야 해."
     - Is Correct Option: 체크 해제
     - Show Error Border Flash: 체크
     - Disable Touch Duration: 5
     - Error Entry Text: "멸균증류수를 부을 때는 먼저 의료폐기물통에 소량 부은 다음 종지에 부어야한다."

   - 터치 옵션 추가 (기타 장소):
     - Option ID: "otherOption"
     - Target Tag: "Untagged"
     - Error Message: "....."
     - Is Correct Option: 체크 해제
     - Error Entry Text: "멸균증류수를 이상한 곳에 부으면 안됩니다."

### 6. 두 번째 물 붓기 단계 구성

1. "새 단계 추가" 버튼 클릭
2. 단계 기본 정보:
   - Step ID: "pourWaterSecond"
   - Step Name: "종지에 물 붓기"
   - Interaction Type: "ConditionalTouch"
   - Guide Text: "다음으로 멸균증류수를 부을 곳을 터치하세요."

3. 조건부 터치 설정:
   - 조건부 터치 사용: 체크
   - 터치 옵션 추가 (종지):
     - Option ID: "containerOption"
     - Target Tag: "MedicineContainer"
     - Success Message: "멸균증류수를 성공적으로 종지에 부었습니다!"
     - Is Correct Option: 체크
     - Water Effect Position: 종지 위치 설정
     - Create Water Image On Object: 체크

   - 터치 옵션 추가 (기타 장소):
     - Option ID: "otherOption"
     - Target Tag: "Untagged"
     - Error Message: "....."
     - Is Correct Option: 체크 해제
     - Error Entry Text: "멸균증류수를 이상한 곳에 부으면 안됩니다."

### 7. 절차 데이터 생성

1. Project 창에서 `Create > Nursing > Generic > Procedure Data` 선택
2. 이름을 "DilutedWaterProcedure" 지정하고 저장
3. 인스펙터에서 기본 정보 작성:
   - Procedure ID: "dilutedWaterProcedure"
   - Procedure Name: "멸균증류수 준비 절차"
   - Description: "멸균증류수 뚜껑을 열고 물을 올바른 순서로 붓는 간호 절차"

4. 간호 절차 단계 추가:
   - 이전에 생성한 "DilutedWaterInteraction" 상호작용을 참조하도록 설정
   - Required Items에 "distilledWater" 아이템 추가

### 8. 게임 오브젝트 설정

1. 필요한 오브젝트에 적절한 태그 설정:
   - 뚜껑 오브젝트: 기존에 "초기 오브젝트" 설정에서 생성됨
   - 쓰레기통 오브젝트: "TrashBin" 태그
   - 종지 오브젝트: "MedicineContainer" 태그

## 결론 및 개선 권장사항

dillitedwaterprocedure.txt의 멸균증류수 절차를 에디터에서 완전히 구현하기 위해서는 다음 확장이 필요합니다:

1. **초기 오브젝트 생성 기능**: 멸균증류수를 선택했을 때 자동으로 뚜껑과 같은 추가 오브젝트를 생성하는 기능
2. **다중 단계 드래그 지원**: 현재 단일 드래그만 지원하므로, 두 번의 연속 드래그를 지원하는 기능
3. **조건부 터치 기능**: 터치한 오브젝트에 따라 다른 동작을 지정할 수 있는 기능
4. **시각 효과 확장**: 깜빡임 효과, 오브젝트 변형, 물 이미지 생성 등
5. **시간 기반 비활성화**: 오류 후 일정 시간 동안 터치 비활성화 기능

이러한 확장이 구현되면, dillitedwaterprocedure.txt의 요구사항을 인스펙터에서 완전히 구현할 수 있으며, 다른 유사한 절차도 쉽게 구현할 수 있게 됩니다.

**에디터 확장을 위한 제안된 접근 방식**:

1. 기존 에디터 확장 코드를 기반으로 새로운 기능 구현
2. 단계적으로 필요한 기능을 추가하여 테스트
3. 사용하기 쉬운 UI로 다양한 시나리오 구성 지원
4. 위의 안내에 따라 에디터에서 멸균증류수 절차 구현

**픽업 아이템 함수와 연계 방법**:

IntermediateManager.cs의 PickupItem 함수가 호출될 때 멸균증류수 아이템의 상호작용 절차가 시작되도록 하기 위해서는:

```csharp
// IntermediateManager.cs의 PickupItem 메서드 확장
public void PickupItem(Item item)
{
    if (item == null) return;
    
    currentHeldItem = item;
    
    // 손 이미지 업데이트
    UpdateHandImage(item);
    
    // 멸균증류수 아이템 처리
    if (item.itemId == "distilledWater" || item.itemName == "멸균증류수")
    {
        // 베이스 인터랙션 시스템 찾기
        BaseInteractionSystem interactionSystem = FindObjectOfType<BaseInteractionSystem>();
        if (interactionSystem != null)
        {
            // 초기 오브젝트 생성 (뚜껑 등)
            interactionSystem.CreateInitialObjects("dilutedWaterInteraction");
            
            // 상호작용 시작
            interactionSystem.StartInteraction("dilutedWaterInteraction");
        }
        else
        {
            // 기존 방식대로 처리
            ProcessItemInteraction(item);
        }
    }
    else
    {
        // 기존 아이템 타입에 따른 처리
        ProcessItemInteraction(item);
    }
}