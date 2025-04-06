using System;

namespace Nursing.Interaction
{
    /// <summary>
    /// 상호작용 유형 - 각 상호작용 단계가 어떤 종류의 상호작용인지 정의합니다.
    /// </summary>
    [Serializable]
    public enum InteractionType
    {
        [Tooltip("단일 클릭")]
        SingleClick,        
        [Tooltip("더블 클릭")]
        DoubleClick,        
        [Tooltip("드래그")]
        Drag,               
        [Tooltip("조건부 클릭 (특정 상태일 때만 유효)")]
        ConditionalClick,   
        [Tooltip("지속 클릭 (게이지 채우기)")]
        SustainedClick,     
        [Tooltip("오브젝트 생성")]
        ObjectCreation,     
        [Tooltip("오브젝트 제거")]
        ObjectDeletion,     
        [Tooltip("오브젝트 이동")]
        ObjectMovement,     
        [Tooltip("퀴즈")]
        QuizPopup,          
        [Tooltip("미니 게임")]
        MiniGame            
    }
}