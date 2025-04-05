# Unity Nursing Simulation Game - Interaction System

이 문서는 간호 시뮬레이션 게임의 상호작용 시스템에 대한 설명입니다.

## 시스템 개요

상호작용 시스템은 플레이어가 게임 내에서 여러 가지 상호작용을 할 수 있도록 지원하는 시스템입니다. 클릭, 드래그, 스와이프, 퀴즈 등 다양한 유형의 상호작용을 지원합니다.

## 주요 컴포넌트

### 1. InteractionData (ScriptableObject)

- 상호작용 데이터를 정의하는 ScriptableObject
- 여러 단계(Steps)로 구성된 상호작용 정의
- 각 단계는 InteractionStepData로 구성

### 2. InteractionStepData (직렬화 클래스)

- 각 상호작용 단계의 세부 정보 정의
- 상호작용 유형, 가이드 텍스트, 성공/실패 조건 등 포함

### 3. BaseInteractionSystem (MonoBehaviour)

- 상호작용 실행을 담당하는 핵심 클래스
- 드래그, 클릭, 퀴즈 등 다양한 상호작용 유형 처리
- 런타임에 InteractionData를 RuntimeInteractionData로 변환하여 사용

### 4. InteractionManager (MonoBehaviour)

- 상호작용 관리 및 아이템 카트 관리
- 상호작용 시작, 완료, 오류 처리
- InteractionDataRegistrar와 연동

### 5. InteractionDataRegistrar (MonoBehaviour)

- 상호작용 데이터를 로드하고 등록하는 유틸리티
- Resources 폴더에서 상호작용 데이터 로드
- InteractionManager에 데이터 등록

## 에디터 확장 기능

### 1. GenericInteractionDataEditor

- InteractionData를 위한 커스텀 에디터
- 상호작용 데이터를 시각적으로 편집 가능
- 단계 추가, 편집, 삭제 및 속성 설정 지원

### 2. FixInteractionDataAssets

- 상호작용 데이터 에셋을 새 형식으로 업데이트하는 유틸리티
- 이전 속성명(interactionId, interactionName)에서 새 속성명(id, displayName)으로 변환

## 상호작용 유형

InteractionType 열거형에 정의된 다양한 상호작용 유형을 지원합니다:

- SingleClick: 단일 클릭
- MultipleClick: 여러 번 클릭
- Drag: 드래그
- SwipeUp/Down/Left/Right: 스와이프
- Quiz: 퀴즈/질문
- 그 외 다양한 상호작용 유형

## 상호작용 데이터 생성 방법

1. 프로젝트 뷰에서 우클릭 -> Create -> Nursing -> Interaction Data
2. 에셋 이름 지정 후 생성
3. 인스펙터에서 상호작용 데이터 편집
4. ID 자동 생성 버튼을 클릭하여 고유 ID 생성
5. 단계 추가 및 설정

## 주의사항

- 모든 InteractionData는 고유한 id를 가져야 합니다
- 이전 버전과의 호환성을 위해 interactionId, interactionName 속성이 id, displayName으로 변경되었습니다
- Tools 메뉴의 "Fix Interaction Data Assets" 기능을 통해 기존 에셋을 업데이트할 수 있습니다

## 예제 코드

```csharp
// InteractionData 등록 예제
var interactionData = Resources.Load<InteractionData>("Interactions/MyInteraction");
InteractionManager.Instance.StartInteraction(interactionData.id);

// 직접 상호작용 시작 예제
InteractionManager.Instance.StartInteraction("interaction_12345678");
```

## 문제 해결

- 에셋 업데이트 후 NullReferenceException이 발생하는 경우:
  1. Tools -> Fix Interaction Data Assets 실행
  2. 모든 InteractionData 에셋 업데이트
  3. 프로젝트 재시작