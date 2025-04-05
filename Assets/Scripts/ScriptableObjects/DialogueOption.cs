using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DialogueOption.cs
[System.Serializable]
public class DialogueOption
{
    public string optionText;  // 버튼에 표시될 텍스트
    public string responseText;  // 선택시 표시될 응답
    public DialogueCharacterType responderType;  // 응답하는 캐릭터 (환자/보호자)
    public Sprite responderSprite;  // 응답하는 캐릭터의 이미지
    public string responderName;  // 응답하는 캐릭터의 이름
}

