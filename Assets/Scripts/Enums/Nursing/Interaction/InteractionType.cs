using System;

namespace Nursing.Interaction
{
    /// <summary>
    /// 상호작용 유형 - 각 상호작용 단계가 어떤 종류의 상호작용인지 정의합니다.
    /// </summary>
    [Serializable]
    public enum InteractionType
    {
        SingleClick,        // 단일 클릭
        DoubleClick,        // 더블 클릭
        Drag,               // 드래그
        ConditionalClick,   // 조건부 클릭 (특정 상태일 때만 유효)
        SustainedClick,     // 지속 클릭 (게이지 채우기)
        ObjectCreation,     // 오브젝트 생성
        ObjectDeletion,     // 오브젝트 제거
        ObjectMovement,     // 오브젝트 이동
        QuizPopup,          // 퀴즈
        MiniGame            // 미니 게임
    }
}