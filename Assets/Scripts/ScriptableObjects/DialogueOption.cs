using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DialogueOption.cs
[System.Serializable]
public class DialogueOption
{
    public string optionText;  // ��ư�� ǥ�õ� �ؽ�Ʈ
    public string responseText;  // ���ý� ǥ�õ� ����
    public DialogueCharacterType responderType;  // �����ϴ� ĳ���� (ȯ��/��ȣ��)
    public Sprite responderSprite;  // �����ϴ� ĳ������ �̹���
    public string responderName;  // �����ϴ� ĳ������ �̸�
}

