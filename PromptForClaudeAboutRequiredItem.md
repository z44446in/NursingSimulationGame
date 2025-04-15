다음의 기능을 구현해주세요. 
1. PreparationManager.cs와 IntermediateManager.cs에서 각각 따로 관리하고 있는 requriedItem 관리 기능을 ProcedureManager.cs를 통해, ProcedureData로 통합한다.
2. 준비단계에서 특정 Procedure에 따라 보이지 않아야 할 아이템들이 있는데, 이것도 ProcedureData에서 관리한다.
3. 구체적으로는, 준비단계에서 PreparationAreaButton을 클릭했을 때 특정 아이템들이 아예 보이지 않게(비활성화) 한다.
4. 수정된대로 ProcedureDataEditor.cs도 수정한다.

다음의 사항을 지켜주세요.
1. ProcedureRequiredItems.cs 의 주요 술기별 필수 아이템 정의를 ProcedureManager.cs나 Gamemanager.cs를 통해 현재의 ProcedureType을 받아와서 한다.
2. IntermediateRequiredItems.cs 의 중간 단계에 필요한 아이템 정의를 ProcedureManager.cs나 Gamemanager.cs를 통해 현재의 ProcedureType을 받아와서 한다.
3. 기능을 구현하며 필요없어진 같은 기능을 하는 변수는 제거해주고(기존의 PreparationManager.cs와 IntermediateManager.cs의 inspector에서 할당했던 방식의 item관련 변수들), 수정된 방식의 변수로 전부 바꿔서 그대로 작동하게 해주세요. 
4. 기존과 통합성을 유지한다며 필요없는 변수를 냅둘 필요는 없습니다. 전부 삭제해주세요. 
5. 하지만, 부탁한 기능과 관련된 다른 기능은 절대 건들지마세요. 온전히 모든 기능이 지금처럼 그대로 작동되어야 합니다. 단지, requriedItem을 받아오는 방식이 전부 ProcedureManager를 통해 ProcedureData에서 받아오게 된거 뿐이에요. 아이템을 받아온 후 다음 동작들은 동일해야합니다.
6. 불필요하게 전체 코드를 다시 작성하지 마세요. 필요한 부분만 수정과 삭제, 추가 해주세요. 
7. 