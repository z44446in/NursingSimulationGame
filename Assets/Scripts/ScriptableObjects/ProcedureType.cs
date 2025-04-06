using UnityEngine;
using Nursing.Procedure;

/// <summary>
/// 간호 시술 유형 정의 - 현재 활성화된 시술 유형을 식별하고 연결된 ProcedureData를 참조합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Procedure Type", menuName = "Nursing/Procedure Type")]
public class ProcedureType : ScriptableObject
{
    [Header("기본 정보")]
    public string id; // 고유 ID
    public string displayName; // 화면에 표시될 이름
    [TextArea(2, 4)] public string description; // 설명
    
    [Header("시술 정보")]
    public Nursing.Procedure.ProcedureType procedureType; // 시술 유형
    public Nursing.Procedure.ProcedureData guidelineVersion; // 가이드라인 버전
    public Nursing.Procedure.ProcedureData clinicalVersion; // 임상 버전
    
    [Header("UI 설정")]
    public Sprite procedureIcon; // 시술 아이콘
}