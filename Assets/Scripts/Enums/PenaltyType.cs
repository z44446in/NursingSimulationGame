using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Legacy
{
    /// <summary>
    /// 간호 시뮬레이션에서 사용하는 페널티 유형을 정의하는 열거형 (레거시 버전)
    /// </summary>
    public enum LegacyPenaltyType
    {
        Minor,      // 경미한 오류 - 작은 감점
        Major,      // 중요한 오류 - 중간 감점
        Critical    // 치명적 오류 - 큰 감점
    }
}