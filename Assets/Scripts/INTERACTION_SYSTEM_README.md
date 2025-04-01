# 범용 간호 상호작용 시스템 사용 가이드

이 문서는 간호 시뮬레이션 게임의 범용 상호작용 시스템 사용 방법을 설명합니다. 이 시스템은 모든 종류의 간호 절차와 아이템에 적용 가능하도록 설계되었습니다.

## 시스템 구조

범용 상호작용 시스템은 다음과 같은 주요 구성 요소로 이루어져 있습니다:

1. **BaseInteractionSystem**: 모든 상호작용을 처리하는 기본 클래스입니다.
2. **GenericInteractionData**: 다양한 상호작용을 정의하는 ScriptableObject입니다.
3. **GenericProcedureData**: 여러 상호작용을 하나의 간호 절차로 묶는 ScriptableObject입니다.
4. **커스텀 에디터**: 상호작용과 절차를 시각적으로 편집할 수 있는 에디터 도구입니다.

## 상호작용 유형

이 시스템은 다음과 같은 다양한 상호작용 유형을 지원합니다:

- **SingleClick**: 특정 영역 클릭
- **MultipleClick**: 여러 번 클릭
- **Drag**: 특정 방향으로 드래그
- **LongPress**: 길게 누르기
- **DoubleTap**: 더블 탭
- **Draw**: 특정 패턴 그리기
- **Rotate**: 회전
- **Pinch**: 핀치 (확대/축소)
- **Swipe**: 다양한 방향 스와이프
- **Quiz**: 질문과 답변
- **OrderSequence**: 순서 맞추기
- 기타 다양한 상호작용 유형들...

## 사용 방법

### 1. 상호작용 생성하기

1. Project 창에서 우클릭 후 Create > Nursing > Generic > Interaction Data 선택
2. 생성된 GenericInteractionData에 기본 정보 입력:
   - 상호작용 ID, 이름, 설명
   - 필요한 상호작용 단계 추가
   - 각 단계별 상호작용 유형과 설정 지정

### 2. 간호 절차 생성하기

1. Project 창에서 우클릭 후 Create > Nursing > Generic > Procedure Data 선택
2. 생성된 GenericProcedureData에 정보 입력:
   - 절차 ID, 이름, 설명
   - 단계 추가 및 순서 설정
   - 각 단계에 필요한 상호작용 등록
   - 평가 기준과 설정 지정

### 3. 실제 게임에 적용하기

```csharp
// 상호작용 시스템 참조 가져오기
BaseInteractionSystem interactionSystem = FindObjectOfType<BaseInteractionSystem>();

// 상호작용 데이터 등록
GenericInteractionData myInteraction = Resources.Load<GenericInteractionData>("상호작용경로");
myInteraction.RegisterToInteractionSystem();

// 상호작용 시작하기
interactionSystem.StartInteraction("상호작용ID");

// 이벤트 구독하기
interactionSystem.OnInteractionStarted += HandleInteractionStarted;
interactionSystem.OnInteractionCompleted += HandleInteractionCompleted;
interactionSystem.OnInteractionError += HandleInteractionError;
```

## 시각적 에디터 기능

### 상호작용 에디터 (GenericInteractionDataEditor)

- **드래그 방향 설정**: 원형 슬라이더로 드래그 방향 시각적 지정
- **클릭 영역 설정**: 사각형으로 클릭 영역 시각적 지정
- **단계별 설정**: 각 상호작용 단계별 세부 설정
- **가이드 설정**: 튜토리얼 화살표와 피드백 설정

### 간호 절차 에디터 (GenericProcedureDataEditor)

- **단계 관리**: ReorderableList로 단계 순서 조정
- **단계별 설정**: 각 단계의 상세 정보 설정
- **필수 아이템 관리**: 절차에 필요한 아이템 자동 추출
- **평가 기준 설정**: 점수 기준과 시간 제한 등 설정

## 확장 방법

새로운 상호작용 유형을 추가하려면:

1. InteractionType 열거형에 새 상호작용 유형 추가
2. BaseInteractionSystem 클래스에 새 상호작용 처리 메서드 추가
3. GenericInteractionDataEditor에 새 상호작용 유형에 대한 UI 추가

## 예시: 혈당 측정 절차 만들기

1. 상호작용 데이터 생성:
   - 알코올 솜 닦기 (Drag 상호작용)
   - 채혈침 장착 (Click 상호작용)
   - 채혈기 사용 (LongPress 상호작용)
   - 혈당 측정기 사용 (MultipleClick 상호작용)

2. 절차 데이터 생성:
   - 각 단계를 순서대로 배치
   - 필요 아이템 등록 (알코올 솜, 채혈침, 채혈기, 혈당 측정기)
   - 시간 제한과 점수 기준 설정

3. 게임에서 사용:
   - 절차 시작 시 절차 데이터 로드 및 등록
   - UI로 현재 단계 표시
   - 완료 시 점수 및 피드백 제공