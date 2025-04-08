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
}