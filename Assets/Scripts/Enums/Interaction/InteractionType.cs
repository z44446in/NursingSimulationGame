using System;

namespace Interaction
{
    /// <summary>
    /// 상호작용 유형 - 각 상호작용 단계가 어떤 종류의 상호작용인지 정의합니다.
    /// </summary>
    [Serializable]
    public enum InteractionType
    {
        SingleClick,        // 단일 클릭
        DoubleClick,        // 더블 클릭
        LongPress,          // 길게 누르기
        Drag,               // 드래그
        TwoFingerDrag,      // 두 손가락 드래그 (핀치/줌)
        ClickAndDrag,       // 클릭 후 드래그
        ConditionalClick,   // 조건부 클릭 (특정 상태일 때만 유효)
        SustainedClick,     // 지속 클릭 (게이지 채우기)
        ObjectCreation,     // 오브젝트 생성
        ObjectDeletion,     // 오브젝트 제거
        ObjectMovement,     // 오브젝트 이동
        Quiz,               // 퀴즈
        MiniGame,           // 미니 게임
        Dialog,             // 대화
        MultiStep           // 여러 단계의 복합 상호작용
    }
    
    /// <summary>
    /// 상호작용 유형 변환 헬퍼 클래스
    /// </summary>
    public static class InteractionTypeHelper
    {
        /// <summary>
        /// 이전 상호작용 유형을 새 상호작용 유형으로 변환합니다.
        /// </summary>
        public static InteractionType ConvertFromLegacy(global::InteractionType legacyType)
        {
            switch (legacyType)
            {
                case global::InteractionType.SingleClick:
                    return InteractionType.SingleClick;
                case global::InteractionType.DoubleClick:
                    return InteractionType.DoubleClick;
                case global::InteractionType.LongPress:
                    return InteractionType.LongPress;
                case global::InteractionType.Drag:
                    return InteractionType.Drag;
                case global::InteractionType.TwoFingerDrag:
                    return InteractionType.TwoFingerDrag;
                case global::InteractionType.ClickAndDrag:
                    return InteractionType.ClickAndDrag;
                case global::InteractionType.Quiz:
                    return InteractionType.Quiz;
                case global::InteractionType.MiniGame:
                    return InteractionType.MiniGame;
                default:
                    return InteractionType.SingleClick;
            }
        }
        
        /// <summary>
        /// 새 상호작용 유형을 이전 상호작용 유형으로 변환합니다.
        /// </summary>
        public static global::InteractionType ConvertToLegacy(InteractionType newType)
        {
            switch (newType)
            {
                case InteractionType.SingleClick:
                    return global::InteractionType.SingleClick;
                case InteractionType.DoubleClick:
                    return global::InteractionType.DoubleClick;
                case InteractionType.LongPress:
                    return global::InteractionType.LongPress;
                case InteractionType.Drag:
                    return global::InteractionType.Drag;
                case InteractionType.TwoFingerDrag:
                    return global::InteractionType.TwoFingerDrag;
                case InteractionType.ClickAndDrag:
                    return global::InteractionType.ClickAndDrag;
                case InteractionType.Quiz:
                    return global::InteractionType.Quiz;
                case InteractionType.MiniGame:
                    return global::InteractionType.MiniGame;
                case InteractionType.ConditionalClick:
                case InteractionType.SustainedClick:
                case InteractionType.ObjectCreation:
                case InteractionType.ObjectDeletion:
                case InteractionType.ObjectMovement:
                case InteractionType.Dialog:
                case InteractionType.MultiStep:
                    // 새 유형은 SingleClick으로 기본 매핑 (필요시 수정)
                    return global::InteractionType.SingleClick;
                default:
                    return global::InteractionType.SingleClick;
            }
        }
        
        /// <summary>
        /// 상호작용 유형에 대한 설명을 반환합니다.
        /// </summary>
        public static string GetDescription(InteractionType type)
        {
            switch (type)
            {
                case InteractionType.SingleClick:
                    return "단일 클릭 - 대상 오브젝트를 한 번 클릭합니다.";
                case InteractionType.DoubleClick:
                    return "더블 클릭 - 대상 오브젝트를 빠르게 두 번 클릭합니다.";
                case InteractionType.LongPress:
                    return "길게 누르기 - 대상 오브젝트를 지정된 시간 동안 누릅니다.";
                case InteractionType.Drag:
                    return "드래그 - 대상 오브젝트를 드래그하여 이동시킵니다.";
                case InteractionType.TwoFingerDrag:
                    return "두 손가락 드래그 - 두 손가락을 사용하여 확대/축소 또는 회전합니다.";
                case InteractionType.ClickAndDrag:
                    return "클릭 후 드래그 - 대상을 클릭하고 다른 위치로 드래그합니다.";
                case InteractionType.ConditionalClick:
                    return "조건부 클릭 - 특정 조건이 충족될 때만 클릭이 유효합니다.";
                case InteractionType.SustainedClick:
                    return "지속 클릭 - 게이지가 차도록 클릭을 유지합니다.";
                case InteractionType.ObjectCreation:
                    return "오브젝트 생성 - 새 오브젝트를 생성합니다.";
                case InteractionType.ObjectDeletion:
                    return "오브젝트 제거 - 기존 오브젝트를 제거합니다.";
                case InteractionType.ObjectMovement:
                    return "오브젝트 이동 - 오브젝트를 지정된 위치로 이동시킵니다.";
                case InteractionType.Quiz:
                    return "퀴즈 - 질문에 답변합니다.";
                case InteractionType.MiniGame:
                    return "미니 게임 - 특정 미니 게임을 수행합니다.";
                case InteractionType.Dialog:
                    return "대화 - NPC와 대화합니다.";
                case InteractionType.MultiStep:
                    return "복합 상호작용 - 여러 단계로 구성된 복잡한 상호작용입니다.";
                default:
                    return "알 수 없는 상호작용 유형";
            }
        }
    }
}