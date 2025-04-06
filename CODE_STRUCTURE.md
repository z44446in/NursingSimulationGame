# 간호 시뮬레이션 게임 코드 구조

이 문서는 간호 시뮬레이션 게임의 코드 구조와 주요 컴포넌트를 설명합니다.

## 네임스페이스 구조

코드베이스는 명확히 정의된 네임스페이스로 구성되어 있습니다:

### Nursing.Interaction
모든 상호작용 관련 클래스를 포함합니다:
- `InteractionData`: 아이템 상호작용 단계 관리
- `InteractionType`: 다양한 상호작용 유형 정의 (드래그, 클릭 등)
- `InteractionStage`: 상호작용 시퀀스의 단일 단계 표현

### Nursing.Procedure
모든 절차 관련 클래스를 포함합니다:
- `ProcedureData`: 완전한 간호 절차 정의
- `ProcedureType`: 절차의 가이드라인/임상 버전 연결
- `ProcedureTypeEnum`: 사용 가능한 절차 유형 열거 (유치도뇨 등)
- `ProcedureStep`: 절차의 단일 단계 표현

### Nursing.Penalty
모든 패널티 관련 클래스를 포함합니다:
- `PenaltyType`: 패널티 심각도 레벨 정의 (Minor, Major, Critical)
- `PenaltyData`: 패널티 정보 저장 (메시지, 점수, 시각적 효과)

### Nursing.Scoring
점수 관련 클래스를 포함합니다:
- `ScoringSystem`: 점수 계산 및 추적 관리

### Nursing.Managers
모든 매니저 클래스를 포함합니다:
- `InteractionManager`: 모든 상호작용 및 실행 관리
- `ProcedureManager`: 간호 절차 및 실행 관리
- `PenaltyManager`: 패널티 시각적 효과 및 메시징 처리
- `DialogueManager`: 대화 상호작용 관리

## 주요 시스템

1. **상호작용 시스템**: 사용자와 게임 내 객체 간의 모든 상호작용을 처리합니다.
   - 지원: 드래그, 클릭, 퀴즈 팝업, 미니게임 등
   - 각 상호작용은 조건과 패널티가 있는 여러 단계를 가질 수 있습니다.

2. **절차 시스템**: 전체 간호 절차를 관리합니다.
   - 각 절차는 순서대로 따라야 하는 여러 단계가 있습니다.
   - 상세한 상호작용을 위해 상호작용 시스템과 연결됩니다.

3. **패널티 시스템**: 사용자가 실수할 때 피드백을 제공합니다.
   - 시각적 신호 표시 (화면 플래시)
   - 대화 메시지 표시
   - 검토를 위한 패널티 기록
   - 사용자 점수 조정

4. **점수 시스템**: 사용자 성능을 추적합니다.
   - 완료된 작업과 패널티를 기반으로 점수 계산
   - 카테고리별 점수 지원
   - 등급 평가 제공 (A, B, C, D, F)

## 파일 구조

주요 파일과 그 역할:

```
Assets/
├── Scripts/
│   ├── Enums/
│   │   └── Nursing/
│   │       ├── Interaction/
│   │       │   └── InteractionType.cs        # 상호작용 유형 열거형
│   │       ├── PenaltyType.cs                # 패널티 유형 및 데이터 정의
│   │       └── ProcedureTypeEnum.cs          # 절차 유형 열거형
│   │
│   ├── ScriptableObjects/
│   │   └── Nursing/
│   │       ├── InteractionData.cs            # 상호작용 데이터 ScriptableObject
│   │       └── ProcedureData.cs              # 절차 데이터 ScriptableObject
│   │       └── ProcedureType.cs              # 절차 유형 ScriptableObject
│   │
│   ├── Managers/
│   │   └── Nursing/
│   │       ├── InteractionManager.cs         # 상호작용 관리자
│   │       ├── ProcedureManager.cs           # 절차 관리자
│   │       ├── PenaltyManager.cs             # 패널티 관리자
│   │       ├── ScoringSystem.cs              # 점수 시스템
│   │       └── DialogueManager.cs            # 대화 관리자
```

## 주요 상호작용 흐름

1. `ProcedureManager`는 특정 `ProcedureType`의 절차를 시작합니다.
2. 각 `ProcedureStep`은 순서대로 활성화됩니다.
3. 단계가 특정 아이템과의 상호작용을 필요로 하는 경우 `InteractionManager`는 관련 `InteractionData`를 시작합니다.
4. 사용자가 상호작용을 완료하면 `InteractionManager`는 다음 단계로 진행하거나 패널티를 적용합니다.
5. 사용자가 실수하면 `PenaltyManager`가 시각적 효과와 메시지를 표시합니다.
6. `ScoringSystem`은 사용자의 성능을 기록하고 평가합니다.

## 확장 가이드

새로운 간호 절차를 추가하려면:

1. `ProcedureTypeEnum`에 새 절차 유형 추가
2. 각 상호작용 단계에 대한 `InteractionData` ScriptableObject 생성
3. 전체 절차를 나타내는 `ProcedureData` ScriptableObject 생성
4. 가이드라인/임상 버전을 연결하는 `ProcedureType` ScriptableObject 생성
5. 이 절차에 필요한 UI 요소 및 에셋 생성