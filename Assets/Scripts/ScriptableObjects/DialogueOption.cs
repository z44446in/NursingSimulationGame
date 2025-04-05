using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DialogueOption.cs
[System.Serializable]
public class DialogueOption
{
    public string optionText;  // ��ư�� ǥ�õ� �ؽ�Ʈ
    public string responseText;  // ���ý� ǥ�õ� ����
    // public DialogueCharacterType responderType;  // 응답하는 캐릭터 - 타입 정의가 필요합니다
    public Sprite responderSprite;  // �����ϴ� ĳ������ �̹���
    public string responderName;  // �����ϴ� ĳ������ �̸�
}

