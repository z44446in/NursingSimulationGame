# 간호 시뮬레이션 게임 - 에디터 사용 가이드

이 문서는 간호 시뮬레이션 게임의 에디터 도구 사용법을 설명합니다. 이 시스템은 코드를 작성하지 않고도 다양한 간호 절차와 상호작용을 쉽게 만들 수 있도록 설계되었습니다.

## 목차

1. [시스템 개요](#시스템-개요)
2. [새로운 간호 상호작용 만들기](#새로운-간호-상호작용-만들기)
3. [상호작용 데이터 설정하기](#상호작용-데이터-설정하기)
4. [간호 절차 만들기](#간호-절차-만들기)
5. [게임에 적용하기](#게임에-적용하기)
6. [에디터 도구 상세 설명](#에디터-도구-상세-설명)
7. [예시: 혈당 측정 절차 만들기](#예시-혈당-측정-절차-만들기)
8. [자주 묻는 질문](#자주-묻는-질문)

## 시스템 개요

이 시스템의 핵심 컴포넌트는 다음과 같습니다:

- **GenericInteractionData**: 개별 상호작용을 정의하는 ScriptableObject
- **GenericProcedureData**: 여러 상호작용을 하나의 간호 절차로 묶는 ScriptableObject
- **커스텀 에디터**: 상호작용과 절차를 시각적으로 구성할 수 있는 도구

## 새로운 간호 상호작용 만들기

### 1. 상호작용 데이터 생성하기

1. 유니티 에디터에서 Project 창의 빈 공간을 **우클릭**합니다.
2. 메뉴에서 **Create > Nursing > Generic > Interaction Data**를 선택합니다.
3. 생성된 파일의 이름을 의미 있게 변경합니다 (예: "DistilledWaterOpen").

![상호작용 데이터 생성](https://i.imgur.com/example1.png)

### 2. 기본 정보 입력하기

1. 생성된 상호작용 데이터를 선택하면 Inspector 창에 에디터가 표시됩니다.
2. **기본 정보** 섹션에서 다음 정보를 입력합니다:
   - **상호작용 ID**: 고유 식별자 (예: "DW_Open")
   - **상호작용 이름**: 읽기 쉬운 이름 (예: "멸균증류수 뚜껑 열기")
   - **설명**: 상호작용에 대한 설명
   - **아이콘**: 선택적으로 아이콘 이미지 설정

> 💡 **팁**: "ID 자동 생성" 버튼을 사용하여 고유한 ID를 자동으로 생성할 수 있습니다.

## 상호작용 데이터 설정하기

### 1. 상호작용 단계 추가하기

1. **상호작용 단계** 섹션에서 "새 단계 추가" 버튼을 클릭합니다.
2. 각 단계는 하나의 사용자 상호작용을 나타냅니다.
3. 단계마다 다음 정보를 설정합니다:
   - **단계 ID**: 자동으로 생성되지만 변경 가능
   - **단계 이름**: 읽기 쉬운 이름
   - **상호작용 유형**: 드롭다운에서 상호작용 유형 선택
     - SingleClick: 클릭
     - Drag: 드래그
     - LongPress: 길게 누르기
     - 기타 다양한 유형들...

### 2. 상호작용 유형별 설정

선택한 상호작용 유형에 따라 다른 설정 옵션이 표시됩니다:

#### 드래그 상호작용 설정

1. **드래그 설정** 섹션에서:
   - **필요 드래그 각도**: 원형 각도 선택기에서 드래그 방향 설정
   - **허용 오차 범위**: 사용자 드래그의 허용 각도 범위
   - **필요 드래그 거리**: 유효한 드래그로 인정할 최소 거리

![드래그 설정](https://i.imgur.com/example2.png)

#### 클릭 상호작용 설정

1. **클릭 설정** 섹션에서:
   - **클릭 영역 좌표**: 시각적 영역 편집기로 클릭 가능 영역 지정
   - **유효 클릭 태그**: 클릭할 수 있는 게임 오브젝트의 태그
   - **필요 클릭 횟수**: 완료에 필요한 클릭 횟수

![클릭 설정](https://i.imgur.com/example3.png)

#### 퀴즈 상호작용 설정

1. **퀴즈 설정** 섹션에서:
   - **질문**: 퀴즈 질문 입력
   - **선택지**: 배열 요소로 여러 선택지 추가
   - **정답 선택지 인덱스**: 정답의 인덱스 번호 (0부터 시작)

### 3. 시각적 가이드 및 피드백 설정

1. **시각적 가이드** 섹션에서:
   - **화살표 스프라이트**: 사용자에게 표시할 안내 화살표
   - **화살표 위치**: 화면상의 화살표 위치
   - **화살표 회전**: 화살표의 회전 각도
   - **하이라이트 스프라이트**: 강조 표시할 영역

2. **피드백** 섹션에서:
   - **성공 메시지**: 성공 시 표시할 메시지
   - **오류 메시지**: 실패 시 표시할 메시지

### 4. 상호작용 시스템에 등록

모든 설정을 완료한 후, Inspector 하단의 **"상호작용 시스템에 등록"** 버튼을 클릭하여 현재 씬에서 사용할 수 있도록 등록합니다.

## 간호 절차 만들기

여러 상호작용을 하나의 완전한 간호 절차로 조합합니다.

### 1. 절차 데이터 생성하기

1. Project 창에서 우클릭 후 **Create > Nursing > Generic > Procedure Data**를 선택합니다.
2. 생성된 파일의 이름을 의미 있게 변경합니다 (예: "UrinaryCatheterProcedure").

### 2. 절차 기본 정보 설정하기

1. Inspector에서 **기본 정보** 섹션을 작성합니다:
   - **절차 ID**: 고유 식별자
   - **절차 이름**: 간호 절차의 이름
   - **설명**: 절차에 대한 설명
   - **아이콘**: 절차를 나타내는 아이콘

### 3. 간호 절차 단계 추가하기

1. **간호 절차 단계** 섹션에서 "+" 버튼을 클릭하여 새 단계를 추가합니다.
2. 각 단계에 다음 정보를 설정합니다:
   - **단계 ID**: 자동 생성되지만 변경 가능
   - **단계 이름**: 해당 단계의 이름
   - **설명**: 단계에 대한 설명
   - **필수 여부**: 이 단계가 필수인지 선택 사항인지 지정
   - **점수 가중치**: 이 단계의 중요도 점수

3. **상호작용 목록**에서 이전에 만든 상호작용 데이터를 추가합니다.
   - 하나의 단계에 여러 상호작용을 추가할 수 있습니다.
   - 상호작용 순서가 중요한 경우 "모든 상호작용 대기" 옵션을 활성화합니다.

### 4. UI 및 배경 설정

1. **UI 및 배경** 섹션에서:
   - **배경 이미지**: 절차 진행 중 표시할 배경 이미지
   - **배경 음악**: 절차 진행 중 재생할 음악
   - **테마 색상**: 절차의 UI 색상 테마

### 5. 평가 기준 설정

1. **평가 기준** 섹션에서:
   - **우수 기준 점수**: 최고 등급 획득을 위한 점수
   - **양호 기준 점수**: 중간 등급 획득을 위한 점수
   - **통과 기준 점수**: 최소 통과 점수

### 6. 필요 아이템 확인

설정을 완료한 후, "필요 아이템 목록 가져오기" 버튼을 클릭하여 이 절차에 필요한 모든 아이템 목록을 확인할 수 있습니다.

## 게임에 적용하기

작성한 상호작용과 절차를 게임에 적용하는 방법입니다.

### 1. 씬에 상호작용 시스템 추가하기

1. 새 게임 오브젝트를 생성하고 이름을 "InteractionSystem"으로 지정합니다.
2. BaseInteractionSystem 컴포넌트를 추가합니다.
3. 필요한 참조(UI 요소, 사운드 등)를 설정합니다.

### 2. 스크립트에서 상호작용 시작하기

```csharp
// InteractionManager를 찾아 상호작용 시작
BaseInteractionSystem interactionSystem = FindObjectOfType<BaseInteractionSystem>();
if (interactionSystem != null)
{
    // 상호작용 ID로 상호작용 시작
    interactionSystem.StartInteraction("your_interaction_id");
}
```

### 3. 이벤트 구독하기

```csharp
void Start()
{
    BaseInteractionSystem interactionSystem = FindObjectOfType<BaseInteractionSystem>();
    if (interactionSystem != null)
    {
        // 이벤트 구독
        interactionSystem.OnInteractionStarted += HandleInteractionStarted;
        interactionSystem.OnInteractionCompleted += HandleInteractionCompleted;
        interactionSystem.OnInteractionError += HandleInteractionError;
    }
}

// 이벤트 핸들러들
private void HandleInteractionStarted(string interactionId, InteractionEventData data)
{
    Debug.Log($"상호작용 시작: {interactionId}");
}

private void HandleInteractionCompleted(string interactionId, InteractionEventData data)
{
    Debug.Log($"상호작용 완료: {interactionId}");
}

private void HandleInteractionError(string interactionId, InteractionEventData data, string errorMessage)
{
    Debug.Log($"오류 발생: {errorMessage}");
}
```

## 에디터 도구 상세 설명

### 상호작용 에디터 단축키

- **Ctrl+D**: 선택한 단계 복제
- **Delete**: 선택한 단계 삭제
- **드래그 & 드롭**: 단계 순서 재배치 (절차 에디터에서)

### 에디터 유의사항

1. 모든 ID는 고유해야 합니다. 자동 생성 기능을 사용하는 것이 좋습니다.
2. 상호작용을 수정한 후에는 **"상호작용 시스템에 등록"** 버튼을 다시 클릭해야 합니다.
3. 드래그 방향과 클릭 영역은 시각적 도구로 쉽게 설정할 수 있습니다.
4. 모든 변경사항은 자동으로 저장되지만, Unity 에디터의 Undo/Redo 기능을 사용할 수 있습니다.

## 예시: 혈당 측정 절차 만들기

이 예시는 혈당 측정 절차를 만드는 전체 과정을 보여줍니다.

### 1. 상호작용 데이터 생성

다음 상호작용 데이터를 만듭니다:

1. **AlcoholSwabAction**:
   - 상호작용 유형: Drag
   - 드래그 방향: 좌우 (각도: 0 또는 180)
   - 가이드 텍스트: "알코올 솜으로 손가락을 닦으세요"

2. **LancetAttachAction**:
   - 상호작용 유형: SingleClick
   - 클릭 영역: 채혈기 부분
   - 가이드 텍스트: "채혈침을 장착하세요"

3. **LancetPressAction**:
   - 상호작용 유형: LongPress
   - 가이드 텍스트: "채혈기를 길게 눌러 혈액을 채취하세요"
   - 유지 시간: 1.5초

4. **GlucometerUseAction**:
   - 상호작용 유형: MultipleClick
   - 클릭 횟수: 2
   - 가이드 텍스트: "혈당 측정기에 혈액을 묻히고 버튼을 누르세요"

### 2. 절차 데이터 생성

혈당 측정 절차 데이터를 만듭니다:

1. **GlucoseCheckProcedure**:
   - 순서 중요: 활성화
   - 단계 1: "손소독" - AlcoholSwabAction 포함
   - 단계 2: "채혈준비" - LancetAttachAction 포함
   - 단계 3: "채혈하기" - LancetPressAction 포함
   - 단계 4: "혈당측정" - GlucometerUseAction 포함
   - 시간 제한: 120초
   - 평가 기준: 우수=95점, 양호=80점, 통과=60점

### 3. 게임에 적용

```csharp
// 혈당 측정 절차 시작
public void StartGlucoseCheckProcedure()
{
    // 절차 데이터 로드
    GenericProcedureData procedureData = Resources.Load<GenericProcedureData>("Procedures/GlucoseCheckProcedure");
    
    // 각 상호작용 등록
    foreach (var step in procedureData.steps)
    {
        foreach (var interaction in step.interactions)
        {
            interaction.RegisterToInteractionSystem();
        }
    }
    
    // 첫 번째 상호작용 시작
    var firstInteraction = procedureData.steps[0].interactions[0];
    BaseInteractionSystem.Instance.StartInteraction(firstInteraction.interactionId);
}
```

## 자주 묻는 질문

**Q: 기존 상호작용을 다른 절차에서 재사용할 수 있나요?**  
A: 네, 상호작용 데이터는 ScriptableObject로 저장되므로 여러 절차에서 재사용할 수 있습니다.

**Q: 단계의 순서를 변경하려면 어떻게 해야 하나요?**  
A: 간호 절차 에디터에서 단계를 드래그하여 순서를 변경할 수 있습니다.

**Q: 상호작용 중에 동적으로 텍스트나 이미지를 변경할 수 있나요?**  
A: 네, BaseInteractionSystem을 상속받아 확장하고 OnInteractionStarted 및 OnInteractionStepCompleted 이벤트를 사용하여 동적 변경을 구현할 수 있습니다.

**Q: 새로운 상호작용 유형을 추가하려면 어떻게 해야 하나요?**  
A: InteractionType 열거형에 새 유형을 추가하고, BaseInteractionSystem에 해당 유형을 처리하는
메서드를 구현한 다음, GenericInteractionDataEditor에 해당 유형에 대한 UI를 추가하면 됩니다.

---

이 가이드가 간호 시뮬레이션 게임의 에디터 도구 사용에 도움이 되길 바랍니다. 추가 질문이나 문제가 있으면 개발팀에 문의하세요.