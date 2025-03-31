using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ProcedureDialogueStep.cs
[CreateAssetMenu(fileName = "DialogueStep", menuName = "Nursing/Dialogue Step")]
public class ProcedureDialogueStep : ScriptableObject
{
    public int stepIndex;  // ���� ���� �ε���
    public List<DialogueOption> dialogueOptions;  // �ش� �ܰ迡�� ��� ������ ��ȭ �ɼǵ�
}

public enum DialogueCharacterType
{
    Patient,
    Guardian,
    Nurse
}