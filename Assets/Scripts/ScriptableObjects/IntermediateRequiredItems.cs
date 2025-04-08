using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nursing.Procedure;

// IntermediateRequiredItems.cs
[CreateAssetMenu(fileName = "IntermediateRequiredItems", menuName = "Nursing/IntermediateRequiredItems")]
public class IntermediateRequiredItems : ScriptableObject
{
    public ProcedureTypeEnum procedureType;
    public List<RequiredItem> requiredItems; // RequiredItem 클래스 재사용
}