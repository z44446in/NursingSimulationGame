using UnityEngine;

namespace Nursing.Procedure
{
    [CreateAssetMenu(fileName = "New Procedure Type", menuName = "Nursing/Procedure Type", order = 3)]
    public class ProcedureType : ScriptableObject
    {
        [Header("프로시저 정보")]
        public string id;
        public string displayName;
        [TextArea(3, 5)] public string description;
        
        [Header("프로시저 버전")]
        public ProcedureVersionType versionType;
        
        [Header("프로시저 데이터")]
        public ProcedureData procedureData;
    }

    public enum ProcedureVersionType
    {
        Guideline,   // 가이드라인 버전
        Clinical     // 임상 버전
    }
}