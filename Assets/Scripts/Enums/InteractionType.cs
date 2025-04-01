using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상호작용 유형을 정의하는 열거형
/// 다양한 간호 절차에 필요한 모든 상호작용 유형을 포함
/// </summary>
public enum InteractionType
{
    None,               // 상호작용 없음
    SingleClick,        // 단일 클릭
    MultipleClick,      // 여러 번 클릭
    Drag,               // 드래그
    TwoFingerDrag,      // 두 손가락 드래그
    LongPress,          // 길게 누르기
    DoubleTap,          // 더블 탭
    Draw,               // 그리기
    Rotate,             // 회전
    RotateDrag,         // 회전 드래그
    Pinch,              // 핀치 (확대/축소)
    SwipeUp,            // 위로 스와이프
    SwipeDown,          // 아래로 스와이프
    SwipeLeft,          // 왼쪽으로 스와이프
    SwipeRight,         // 오른쪽으로 스와이프
    Quiz,               // 퀴즈/질문
    OrderSequence,      // 순서 맞추기
    Connect,            // 연결하기
    Type,               // 텍스트 입력
    Timer,              // 시간 유지
    Observation,        // 관찰
    Measurement,        // 측정
    Combination,        // 아이템 조합
    Custom              // 사용자 정의 상호작용
}