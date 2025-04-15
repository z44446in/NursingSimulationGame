using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nursing.Procedure;

[CreateAssetMenu(fileName = "ProcedureRequiredItems", menuName = "Nursing/ProcedureRequiredItems")]
public class ProcedureRequiredItems : ScriptableObject
{
    public ProcedureTypeEnum procedureType;
    public List<RequiredItem> requiredItems;
}

[System.Serializable]
public class RequiredItem
{
    public Item item;
    public bool isOptional;
    [TextArea(2, 4)] public string needReason; // 이 아이템이 필요한 이유 (부족할 때 표시)
    [TextArea(2, 4)] public string optionalReason; // 이 아이템이 선택적인 이유 
}