using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nursing.Procedure
{
    /// <summary>
    /// 간호 시술 유형 열거형
    /// </summary>
    public enum ProcedureTypeEnum
    {
        [Tooltip("유치도뇨")]
        UrinaryCatheterization,
        [Tooltip("기관절개관 관리")]
        TracheostomyCare,
        // ... 다른 술기들 추가
    }
}