using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레거시 상호작용 유형을 정의하는 열거형 
/// 기존 시스템과의 호환성을 위해 유지됩니다.
/// 새 시스템에서는 Interaction.InteractionType을 사용하세요.
/// </summary>
public enum InteractionType
{
    None,               // 상호작용 없음
    SingleClick,        // 단일 클릭
    DoubleClick,        // 더블 클릭 (이전 DoubleTap)
    LongPress,          // 길게 누르기
    Drag,               // 드래그
    TwoFingerDrag,      // 두 손가락 드래그
    ClickAndDrag,       // 클릭 후 드래그 (이전 RotateDrag)
    Quiz,               // 퀴즈/질문
    MiniGame,           // 미니 게임 (이전 Custom)
    
    // 이전 값 유지 (호환성)
    MultipleClick,      // 여러 번 클릭
    Draw,               // 그리기
    Rotate,             // 회전
    Pinch,              // 핀치 (확대/축소)
    SwipeUp,            // 위로 스와이프
    SwipeDown,          // 아래로 스와이프
    SwipeLeft,          // 왼쪽으로 스와이프
    SwipeRight,         // 오른쪽으로 스와이프
    OrderSequence,      // 순서 맞추기
    Connect,            // 연결하기
    Type,               // 텍스트 입력
    Timer,              // 시간 유지
    Observation,        // 관찰
    Measurement,        // 측정
    Combination,        // 아이템 조합
    Custom              // 사용자 정의 상호작용
}