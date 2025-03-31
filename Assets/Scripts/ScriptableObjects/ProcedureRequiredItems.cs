using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProcedureRequiredItems", menuName = "Nursing/ProcedureRequiredItems")]
public class ProcedureRequiredItems : ScriptableObject
{
    public NursingProcedureType procedureType;
    public List<RequiredItem> requiredItems;
}

[System.Serializable]
public class RequiredItem
{
    public Item item;
    public bool isOptional;
}