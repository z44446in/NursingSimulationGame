using System;
using UnityEngine;

namespace Nursing.Interaction
{
    /// <summary>
    /// 상호작용 유형 - 각 상호작용 단계가 어떤 종류의 상호작용인지 정의합니다.
    /// </summary>
    [Serializable]
    public enum InteractionType
    {
        [Tooltip("단일 드래그 상호작용 - 오브젝트를 드래그하는 상호작용")]
        SingleDragInteraction,

        [Tooltip("다중 드래그 상호작용 - 오브젝트를 드래그하는 상호작용")]
        MultiDragInteraction,

        [Tooltip("오브젝트 생성 - 새로운 오브젝트를 생성하는 상호작용")]
        ObjectCreation,
        
        [Tooltip("조건부 클릭 - 특정 조건에 따라 다른 결과를 가지는 클릭")]
        ConditionalClick,
        
        [Tooltip("지속 클릭 - 특정 시간 동안 버튼을 계속 누르는 상호작용")]
        SustainedClick,
        
        [Tooltip("오브젝트 삭제 - 특정 오브젝트를 제거하는 상호작용")]
        ObjectDeletion,
        
        [Tooltip("오브젝트 이동 - 오브젝트를 자동으로 이동시키는 상호작용")]
        ObjectMovement,
        
        [Tooltip("퀴즈 팝업 - 퀴즈를 표시하는 상호작용")]
        TextQuizPopup,

        [Tooltip("퀴즈 팝업 - 퀴즈를 표시하는 상호작용")]
        ImageQuizPopup,

        [Tooltip("미니게임 - 미니게임을 실행하는 상호작용")]
        MiniGame
    }
}