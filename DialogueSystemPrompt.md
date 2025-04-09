DialogueManager.cs에 있는 Speaker Data랑 PenaltyManager의 Speaker가 연결이 됐으면 좋겠어. 
1.	DialogueManger.cs에 있는 Speaker enum 에 따라, DialogueManger 인스펙터에서 각 Speaker에 따른 ‘화면에 표시될 이름’과 ‘화자 이미지’를 설정할 수 있다. 
2.	 Showsmalldialogue 메서드에 의해 호출되는 smalldialoguePrefab에 있는 오브젝트들을 DialogueManager 인스펙터에 할당한다. 이를 통해 smalldialoguePrefab이 생성될 때 DialogueManager에서  설정한 ‘화면에 표시될 이름’과 ‘화자 이미지’로 내용이 바뀐다. 
3.	PenaltyType.cs의 ‘speaker’는 DialogueManager와 연동되어, 여기에 있는 enum에서만 고를 수 있다. 
4.	그래서 만약, penalty 상황에 대해 설정할 때 speaker만 설정해도, 알아서 smalldialoguePRefab이 형성될 때는 그 speaker에 일치하는 ‘화면에 표시될 이름’과 ‘화자 이미지’가 생성된다 
5.	만약 아무런 화자가 설정되지 않으면, 지금 처럼 그냥 기본 캐릭터를 따라간다. 
