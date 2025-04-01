# 간호 시뮬레이션 상호작용 시스템 사용 가이드

이 문서는 간호 시뮬레이션 게임의 상호작용 시스템 사용 방법을 설명합니다.

## 상호작용 시스템 구조

상호작용 시스템은 다음과 같은 주요 구성 요소로 이루어져 있습니다:

1. **NursingActionData**: 개별 간호 행동(Action)의 데이터를 정의하는 ScriptableObject
2. **ProcedureStepData**: 여러 간호 행동을 묶은, 시술의 한 단계를 정의하는 ScriptableObject
3. **ItemInteractionHandler**: 아이템별 상호작용을 처리하는 컴포넌트
4. **InteractionManager**: 모든 상호작용을 관리하는 싱글톤 매니저
5. **InteractionDataRegistrar**: ScriptableObject 데이터를 런타임에 등록하는 유틸리티

## ScriptableObject 설정 방법

### 1. NursingActionData 생성하기

1. Project 창에서 우클릭 > Create > Nursing > Nursing Action Data
2. 인스펙터에서 다음 속성들을 설정:
   - 기본 정보 (ID, 이름, 설명, 필수 여부, 점수 가중치)
   - 상호작용 유형 (클릭, 드래그, 퀴즈 등)
   - 유형별 세부 설정 (드래그 각도, 클릭 영역, 퀴즈 문항 등)
   - 피드백 메시지와 오류 처리 방식
   - 시각 및 소리 피드백

### 2. ProcedureStepData 생성하기

1. Project 창에서 우클릭 > Create > Nursing > Procedure Step
2. 인스펙터에서 다음 속성들을 설정:
   - 단계 정보 (ID, 이름, 설명)
   - 순서 중요성, 필수 여부, 점수 가중치
   - 포함되는 간호 행동(Action) 리스트
   - UI 요소 (아이콘, 가이드 텍스트)
   - 완료 조건 (모든 행동 필요 여부, 자동 진행 딜레이)
   - 배경 이미지 및 소리

## 실제 구현 예시

### 멸균증류수 상호작용 예시

```csharp
// InteractionManager에 이벤트 등록하기
void Start() {
    // 이벤트 핸들러 등록
    ItemInteractionHandler handler = GetComponent<ItemInteractionHandler>();
    handler.OnStepCompleted += HandleStepCompleted;
    handler.OnInteractionCompleted += HandleInteractionCompleted;
    handler.OnInteractionError += HandleInteractionError;
}

// 상호작용 시작하기
void StartInteraction() {
    ItemInteractionHandler handler = GetComponent<ItemInteractionHandler>();
    handler.StartItemInteraction(distilledWaterItem);
}

// 이벤트 핸들러 구현
void HandleStepCompleted(Item item, int stepIndex) {
    Debug.Log($"Step {stepIndex} completed for {item.itemName}");
}

void HandleInteractionCompleted(Item item) {
    Debug.Log($"All interactions completed for {item.itemName}");
}

void HandleInteractionError(Item item, string errorMessage, int penaltyPoints) {
    Debug.Log($"Error: {errorMessage}, Penalty: {penaltyPoints}");
}
```

## 커스텀 에디터 기능

이 시스템은 편리한 에디터 기능을 제공합니다:

1. **드래그 방향 선택기**: 드래그 상호작용 설정시 원형 방향 선택기
2. **클릭 영역 시각화**: 클릭 상호작용 영역을 직관적으로 설정
3. **요소 자동 생성**: ID 자동 생성 버튼 등

## 상호작용 등록 방법

1. InteractionDataRegistrar 컴포넌트를 씬의 GameObject에 추가
2. NursingActionData와 ProcedureStepData를 할당
3. 게임 시작 시 자동으로 모든 상호작용 데이터를 InteractionManager에 등록

## 주의사항

1. ScriptableObject 데이터는 수정해도 프리팹에 자동 반영되지 않으므로, 수정 후 프리팹 업데이트가 필요할 수 있습니다.
2. 상호작용 순서가 중요한 경우, ProcedureStepData의 isOrderImportant를 true로 설정하세요.
3. 에디터 확장 기능을 사용하기 위해 항상 커스텀 에디터 스크립트를 Editor 폴더에 보관하세요.

## 확장 방법

새로운 상호작용 유형을 추가하려면:

1. InteractionType 열거형에 새 타입 추가
2. ItemInteractionHandler에 해당 타입 처리 로직 추가
3. NursingActionData에 필요한 필드 추가
4. NursingActionDataEditor에 UI 요소 추가