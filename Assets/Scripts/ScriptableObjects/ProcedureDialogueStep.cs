using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ProcedureDialogueStep.cs
[CreateAssetMenu(fileName = "DialogueStep", menuName = "Nursing/Dialogue Step")]
public class ProcedureDialogueStep : ScriptableObject
{
    public int stepIndex;  // 술기 순서 인덱스
    public List<DialogueOption> dialogueOptions;  // 해당 단계에서 사용 가능한 대화 옵션들
}

public enum DialogueCharacterType
{
    Patient,
    Guardian,
    Nurse
}