# 시스템 재구성 안내

본 프로젝트는 간호 시뮬레이션 게임의 데이터 모델과 에디터를 완전히 재구성하였습니다. 아래는 주요 변경 사항과 사용 방법에 대한 안내입니다.

## 주요 변경 사항

1. **새로운 데이터 구조**
   - InteractionData, ProcedureData, ProcedureType 클래스가 완전히 재설계되었습니다.
   - 새로운 PenaltySystem 클래스가 추가되어 패널티 처리를 담당합니다.
   - 상호작용 유형(InteractionType)이 확장되어 더 다양한 상호작용을 지원합니다.

2. **새로운 에디터 UI**
   - 인스펙터 기반의 직관적인 에디터가 추가되었습니다.
   - InteractionDataEditor, ProcedureDataEditor, ProcedureTypeEditor 클래스가 추가되었습니다.
   - ReorderableList를 사용한 목록 관리가 가능합니다.

3. **페널티 시스템**
   - 시각적 피드백, 데이터베이스 기록, 대화 통합을 포함한 종합적인 페널티 시스템이 구현되었습니다.
   - 페널티 유형(PenaltyType)으로 중요도를 분류할 수 있습니다.

4. **호환성 어댑터**
   - `InteractionSystemAdapter` 클래스를 통해 기존 시스템과의 호환성을 유지합니다.
   - 기존 매니저 클래스(InteractionManager, ProcedureManager)가 새 시스템과 함께 작동합니다.

## 사용 방법

### 새 상호작용 생성하기

1. 프로젝트 뷰에서 우클릭 > Create > Nursing > Interaction Data 선택
2. 새 인터랙션에 이름과 설명을 입력하고 필요한 스테이지와 상호작용을 추가
3. 인스펙터를 통해 상호작용 유형과 설정을 구성

### 새 시술 유형 생성하기

1. 프로젝트 뷰에서 우클릭 > Create > Nursing > Procedure Type 선택
2. 기본 정보와 UI 설정을 구성
3. 가이드라인 버전과 임상 버전 생성 버튼을 클릭하여 ProcedureData 자산 생성

### 패널티 설정하기

1. ProcedureData나 InteractionData 에서 패널티 설정 섹션 확장
2. 페널티 유형, 메시지, 점수 감점 등을 설정
3. 필요에 따라 시각적 효과와 사운드 설정

## 네임스페이스 및 구조

- `Interaction` 네임스페이스: 새로운 상호작용 시스템 관련 클래스와 열거형
- `Scripts/Utils`: 어댑터와 헬퍼 클래스
- `Scripts/ScriptableObjects`: 데이터 모델 클래스
- `Scripts/Editor`: 커스텀 에디터 클래스
- `Scripts/Enums`: 열거형 정의

## 예시 워크플로우

1. ProcedureType 생성 (예: 유치도뇨)
2. 가이드라인 및 임상 버전 ProcedureData 생성
3. 각 단계에 필요한 InteractionData 생성 및 연결
4. 게임 매니저를 통해 시술 시작 (ProcedureManager.Instance.StartProcedure)

## 참고 사항

- 기존 데이터 구조는 완전히 대체되었으나, 호환성 어댑터를 통해 기존 기능은 그대로 유지됩니다.
- 모든 데이터는 ScriptableObject로 저장되므로 씬 간에 유지됩니다.
- 에디터 툴은 Unity 에디터 내에서만 작동하며, 실행 시에는 사용할 수 없습니다.