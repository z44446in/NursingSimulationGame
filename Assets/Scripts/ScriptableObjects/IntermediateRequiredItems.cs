using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// IntermediateRequiredItems.cs
[CreateAssetMenu(fileName = "IntermediateRequiredItems", menuName = "Nursing/IntermediateRequiredItems")]
public class IntermediateRequiredItems : ScriptableObject
{
    public NursingProcedureType procedureType;
    public List<RequiredItem> requiredItems; // RequiredItem 클래스 재사용
}