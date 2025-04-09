using UnityEngine;
using UnityEditor;
using Nursing.Interaction;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Nursing.Editor
{
    [CustomEditor(typeof(InteractionData))]
    public class InteractionDataEditor : UnityEditor.Editor
    {
        private SerializedProperty idProperty;
        private SerializedProperty displayNameProperty;
        private SerializedProperty descriptionProperty;
        private SerializedProperty stagesProperty;
        private SerializedProperty guideMessageProperty;
        
        private Dictionary<int, bool> stageFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> settingsFoldouts = new Dictionary<int, bool>();
        
        private GUIStyle headerStyle;
        private GUIStyle subheaderStyle;
        
        private void OnEnable()
        {
            idProperty = serializedObject.FindProperty("id");
            displayNameProperty = serializedObject.FindProperty("displayName");
            descriptionProperty = serializedObject.FindProperty("description");
            stagesProperty = serializedObject.FindProperty("stages");
            
            
            // 스타일 초기화는 OnInspectorGUI에서 수행
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // 스타일 초기화
            InitializeStyles();
            
            // 기본 정보 섹션
            EditorGUILayout.LabelField("기본 정보", headerStyle);
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(idProperty, new GUIContent("ID", "인터랙션의 고유 식별자"));
            EditorGUILayout.PropertyField(displayNameProperty, new GUIContent("표시 이름", "인터랙션의 화면에 표시될 이름"));
            EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("설명", "인터랙션에 대한 설명"));
            
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            
            // 인터랙션 스테이지 섹션
            EditorGUILayout.LabelField("인터랙션 스테이지", headerStyle);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("스테이지 추가", GUILayout.Width(150)))
            {
                AddStage();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 스테이지 표시
            for (int i = 0; i < stagesProperty.arraySize; i++)
            {
                DrawStage(i);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel);
                headerStyle.fontSize = 14;
                headerStyle.margin = new RectOffset(0, 0, 10, 5);
            }
            
            if (subheaderStyle == null)
            {
                subheaderStyle = new GUIStyle(EditorStyles.boldLabel);
                subheaderStyle.fontSize = 12;
                subheaderStyle.margin = new RectOffset(0, 0, 5, 3);
            }
        }
        
        private void AddStage()
        {
            // 새 스테이지 추가
            stagesProperty.arraySize++;
            int newIndex = stagesProperty.arraySize - 1;
            
            // 기본값 설정
            SerializedProperty newStage = stagesProperty.GetArrayElementAtIndex(newIndex);
            
            SerializedProperty idProp = newStage.FindPropertyRelative("id");
            idProp.stringValue = "stage_" + newIndex;
            
            SerializedProperty nameProp = newStage.FindPropertyRelative("name");
            nameProp.stringValue = "스테이지 " + newIndex;
            
            SerializedProperty stageNumProp = newStage.FindPropertyRelative("StageNum");
            stageNumProp.intValue = newIndex;
            
            // 새 스테이지의 폴드아웃 상태를 열림으로 설정
            stageFoldouts[newIndex] = true;
            settingsFoldouts[newIndex] = true;
        }
        
        private void DrawStage(int index)
        {
            SerializedProperty stageProperty = stagesProperty.GetArrayElementAtIndex(index);
            
            SerializedProperty idProp = stageProperty.FindPropertyRelative("id");
            SerializedProperty nameProp = stageProperty.FindPropertyRelative("name");
            SerializedProperty stageNumProp = stageProperty.FindPropertyRelative("StageNum");
            SerializedProperty guideMessageProp = stageProperty.FindPropertyRelative("guideMessage");
            SerializedProperty interactionTypeProp = stageProperty.FindPropertyRelative("interactionType");
            SerializedProperty requireSpecificOrderProp = stageProperty.FindPropertyRelative("requireSpecificOrder");
            SerializedProperty requiredPreviousStageIdsProp = stageProperty.FindPropertyRelative("requiredPreviousStageIds");
            SerializedProperty incorrectOrderPenaltyProp = stageProperty.FindPropertyRelative("incorrectOrderPenalty");
            SerializedProperty incorrectInteractionPenaltyProp = stageProperty.FindPropertyRelative("incorrectInteractionPenalty");
            SerializedProperty settingsProp = stageProperty.FindPropertyRelative("settings");
            
            if (!stageFoldouts.ContainsKey(index))
            {
                stageFoldouts[index] = false;
            }
            
            // 스테이지 헤더 (폴드아웃)
            EditorGUILayout.BeginHorizontal();
            
            stageFoldouts[index] = EditorGUILayout.Foldout(stageFoldouts[index], "", true);
            
            // 스테이지 헤더 표시 (이름 + 인터랙션 타입)
            EditorGUILayout.LabelField("스테이지 " + index + ": " + nameProp.stringValue + " (" + interactionTypeProp.enumDisplayNames[interactionTypeProp.enumValueIndex] + ")", EditorStyles.boldLabel);
            
            // 삭제 버튼
            if (GUILayout.Button("삭제", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("스테이지 삭제", "정말로 이 스테이지를 삭제하시겠습니까?", "삭제", "취소"))
                {
                    DeleteStage(index);
                    return; // 더 이상 처리하지 않고 리턴
                }
            }
            
            // 위로 이동 버튼
            GUI.enabled = index > 0;
            if (GUILayout.Button("↑", GUILayout.Width(25)))
            {
                MoveStage(index, index - 1);
                return; // 더 이상 처리하지 않고 리턴
            }
            
            // 아래로 이동 버튼
            GUI.enabled = index < stagesProperty.arraySize - 1;
            if (GUILayout.Button("↓", GUILayout.Width(25)))
            {
                MoveStage(index, index + 1);
                return; // 더 이상 처리하지 않고 리턴
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            // 폴드아웃이 열려있는 경우 세부 정보 표시
            if (stageFoldouts[index])
            {
                EditorGUI.indentLevel++;
                
                // 기본 정보
                EditorGUILayout.PropertyField(idProp, new GUIContent("ID", "스테이지의 고유 식별자"));
                EditorGUILayout.PropertyField(nameProp, new GUIContent("이름", "스테이지의 이름"));
                EditorGUILayout.PropertyField(stageNumProp, new GUIContent("스테이지 번호", "스테이지의 순서 번호"));
                EditorGUILayout.PropertyField(guideMessageProp, new GUIContent("가이드 메시지", "이 스테이지 실행 시 표시될 가이드 메시지"));
                
                EditorGUILayout.Space();
                
                // 인터랙션 타입
                EditorGUILayout.PropertyField(interactionTypeProp, new GUIContent("인터랙션 타입", "이 스테이지의 인터랙션 유형"));
                
                EditorGUILayout.Space();


                // 인터랙션 설정
                EditorGUILayout.PropertyField(incorrectInteractionPenaltyProp, new GUIContent("잘못된 인터랙션 패널티", "인터랙션이 잘못되었을 때 적용할 패널티"));

                if (!settingsFoldouts.ContainsKey(index))
                {
                    settingsFoldouts[index] = false;
                }

                settingsFoldouts[index] = EditorGUILayout.Foldout(settingsFoldouts[index], "인터랙션 설정", true);

                if (settingsFoldouts[index])
                {
                    EditorGUI.indentLevel++;

                    // 인터랙션 타입에 따른 설정 표시
                    DrawInteractionSettings(settingsProp, (InteractionType)interactionTypeProp.enumValueIndex);

                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space();

                // 순서 요구사항
                EditorGUILayout.PropertyField(requireSpecificOrderProp, new GUIContent("특정 순서 요구", "이 스테이지가 특정 순서로 실행되어야 하는지 여부"));
                
                if (requireSpecificOrderProp.boolValue)
                {
                    EditorGUILayout.PropertyField(requiredPreviousStageIdsProp, new GUIContent("필요한 이전 스테이지 ID", "이 스테이지 전에 완료되어야 하는 스테이지 ID 목록"));
                    
                    EditorGUILayout.PropertyField(incorrectOrderPenaltyProp, new GUIContent("잘못된 순서 패널티", "순서가 잘못되었을 때 적용할 패널티"));
                }
                
                EditorGUILayout.Space();
                
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
        }
        
        private void DrawInteractionSettings(SerializedProperty settingsProp, InteractionType interactionType)
        {
            switch (interactionType)
            {
                case InteractionType.DragInteraction:
                    DrawDragInteractionSettings(settingsProp);
                    break;
                    
                case InteractionType.ObjectCreation:
                    DrawObjectCreationSettings(settingsProp);
                    break;
                    
                case InteractionType.ConditionalClick:
                    DrawConditionalClickSettings(settingsProp);
                    break;
                    
                case InteractionType.SustainedClick:
                    DrawSustainedClickSettings(settingsProp);
                    break;
                    
                case InteractionType.ObjectDeletion:
                    DrawObjectDeletionSettings(settingsProp);
                    break;
                    
                case InteractionType.ObjectMovement:
                    DrawObjectMovementSettings(settingsProp);
                    break;
                    
                case InteractionType.QuizPopup:
                    DrawQuizPopupSettings(settingsProp);
                    break;
                    
                case InteractionType.MiniGame:
                    DrawMiniGameSettings(settingsProp);
                    break;
            }
        }
        
        #region 인터랙션 타입별 설정 드로잉 메서드
        
        private void DrawDragInteractionSettings(SerializedProperty settingsProp)
        {
            SerializedProperty isDragInteractionProp = settingsProp.FindPropertyRelative("isDragInteraction");
            SerializedProperty showDirectionArrowsProp = settingsProp.FindPropertyRelative("showDirectionArrows");
            SerializedProperty arrowStartPositionProp = settingsProp.FindPropertyRelative("arrowStartPosition");
            SerializedProperty arrowDirectionProp = settingsProp.FindPropertyRelative("arrowDirection");
            SerializedProperty requireTwoFingerDragProp = settingsProp.FindPropertyRelative("requireTwoFingerDrag");
            SerializedProperty requiredDragDirectionProp = settingsProp.FindPropertyRelative("requiredDragDirection");
            SerializedProperty dragDirectionToleranceProp = settingsProp.FindPropertyRelative("dragDirectionTolerance");
            SerializedProperty targetObjectTagProp = settingsProp.FindPropertyRelative("targetObjectTag");
            SerializedProperty followDragMovementProp = settingsProp.FindPropertyRelative("followDragMovement");
            SerializedProperty dragDistanceLimitProp = settingsProp.FindPropertyRelative("dragDistanceLimit");
            SerializedProperty boundaryObjectTagProp = settingsProp.FindPropertyRelative("boundaryObjectTag");
            SerializedProperty collisionZoneTagProp = settingsProp.FindPropertyRelative("collisionZoneTag");
            SerializedProperty OverDragProp = settingsProp.FindPropertyRelative("OverDrag");
            
            // 이 인터랙션 타입을 활성화하기 위한 플래그
            isDragInteractionProp.boolValue = true;
            
            // 기본 설정
            EditorGUILayout.LabelField("드래그 기본 설정", subheaderStyle);
            EditorGUILayout.PropertyField(targetObjectTagProp, new GUIContent("대상 오브젝트 태그", "드래그할 오브젝트의 태그"));
            EditorGUILayout.PropertyField(requireTwoFingerDragProp, new GUIContent("두 손가락 드래그 필요", "드래그에 두 손가락이 필요한지 여부"));
            
            EditorGUILayout.Space();
            
            // 방향 화살표 설정
            EditorGUILayout.LabelField("방향 화살표 설정", subheaderStyle);
            EditorGUILayout.PropertyField(showDirectionArrowsProp, new GUIContent("방향 화살표 표시", "드래그 방향을 안내하는 화살표 표시 여부"));
            
            if (showDirectionArrowsProp.boolValue)
            {
                EditorGUILayout.PropertyField(arrowStartPositionProp, new GUIContent("화살표 시작 위치", "화살표가 표시될 시작 위치"));
                
                // 화살표 방향 시각적 컨트롤
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("화살표 방향", "화살표가 가리킬 방향"));
                
                // 현재 방향 값 가져오기
                Vector2 currentDirection = arrowDirectionProp.vector2Value;
                
                // 시각적인 방향 컨트롤 (360도 다이얼)
                Rect directionRect = EditorGUILayout.GetControlRect(false, 100f);
                
                // 다이얼 그리기
                Rect circleRect = new Rect(directionRect.x + directionRect.width / 2 - 40, directionRect.y, 80, 80);
                EditorGUI.DrawRect(new Rect(circleRect.x - 1, circleRect.y - 1, circleRect.width + 2, circleRect.height + 2), new Color(0.3f, 0.3f, 0.3f));
                EditorGUI.DrawRect(circleRect, new Color(0.2f, 0.2f, 0.2f));
                
                // 중심점
                Vector2 center = new Vector2(circleRect.x + circleRect.width / 2, circleRect.y + circleRect.height / 2);
                
                // 현재 방향 라인
                float length = 35f;
                Vector2 normalizedDir = currentDirection.normalized;
                Vector2 lineEnd = center + normalizedDir * length;
                Handles.color = Color.white;
                Handles.DrawLine(center, lineEnd);
                
                // 화살표 끝
                Vector2 arrowSize = new Vector2(8, 8);
                float angle = Mathf.Atan2(normalizedDir.y, normalizedDir.x) * Mathf.Rad2Deg;
                
                // 방향 컨트롤 이벤트 처리
                Event evt = Event.current;
                if (evt.type == EventType.MouseDown || evt.type == EventType.MouseDrag)
                {
                    if (circleRect.Contains(evt.mousePosition))
                    {
                        // 마우스 위치로부터 방향 계산
                        Vector2 newDirection = evt.mousePosition - center;
                        arrowDirectionProp.vector2Value = newDirection.normalized;
                        GUI.changed = true;
                        evt.Use();
                    }
                }
                
                // N, E, S, W 보조선
                Handles.color = new Color(0.6f, 0.6f, 0.6f, 0.3f);
                Handles.DrawLine(center - new Vector2(length, 0), center + new Vector2(length, 0)); // 수평선
                Handles.DrawLine(center - new Vector2(0, length), center + new Vector2(0, length)); // 수직선
                
                // 허용 각도 범위 표시 (requiredDragDirection이 true인 경우에만)
                if (requiredDragDirectionProp.boolValue)
                {
                    float toleranceAngle = dragDirectionToleranceProp.floatValue;
                    if (toleranceAngle > 0)
                    {
                        // 허용 각도 범위 영역 그리기
                        Handles.color = new Color(0.0f, 0.8f, 0.0f, 0.15f);
                        float startAngle = angle - toleranceAngle;
                        float endAngle = angle + toleranceAngle;
                        Handles.DrawSolidArc(center, Vector3.forward, new Vector3(Mathf.Cos(startAngle * Mathf.Deg2Rad), Mathf.Sin(startAngle * Mathf.Deg2Rad), 0), 
                                            toleranceAngle * 2 * Mathf.Deg2Rad, length);
                        
                        // 허용 각도 경계선 그리기
                        Handles.color = new Color(0.0f, 0.7f, 0.0f, 0.5f);
                        Vector2 leftBoundary = new Vector2(Mathf.Cos((angle - toleranceAngle) * Mathf.Deg2Rad), 
                                                          Mathf.Sin((angle - toleranceAngle) * Mathf.Deg2Rad)) * length;
                        Vector2 rightBoundary = new Vector2(Mathf.Cos((angle + toleranceAngle) * Mathf.Deg2Rad), 
                                                           Mathf.Sin((angle + toleranceAngle) * Mathf.Deg2Rad)) * length;
                        Handles.DrawLine(center, center + leftBoundary);
                        Handles.DrawLine(center, center + rightBoundary);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                // 수동 입력 필드 (좌표 직접 설정)
                EditorGUILayout.PropertyField(arrowDirectionProp, new GUIContent("방향 좌표 직접 설정", "화살표 방향 좌표 직접 설정"));
                
                // 필수 드래그 방향 설정
                EditorGUILayout.PropertyField(requiredDragDirectionProp, new GUIContent("필수 드래그 방향", "특정 방향으로 드래그해야 하는지 여부"));
                
                if (requiredDragDirectionProp.boolValue)
                {
                    // 드래그 방향 오차 허용 범위 설정
                    EditorGUILayout.Slider(dragDirectionToleranceProp, 0f, 90f, new GUIContent("방향 오차 허용 범위", "드래그 방향의 허용 오차 범위 (각도)"));
                }


            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(
                settingsProp.FindPropertyRelative("deactivateObjectAfterDrag"),
                new GUIContent("드래그 후 오브젝트 비활성화", "드래그 완료 후 해당 오브젝트를 비활성화할지 여부")
            );

            EditorGUILayout.Space();
            

            // 이동 설정
            EditorGUILayout.LabelField("이동 설정", subheaderStyle);
            EditorGUILayout.PropertyField(followDragMovementProp, new GUIContent("드래그 따라 이동", "드래그 위치를 따라 오브젝트가 이동하는지 여부"));
            
            if (followDragMovementProp.boolValue)
            {
                EditorGUILayout.PropertyField(dragDistanceLimitProp, new GUIContent("드래그 거리 제한", "최대 드래그 거리 (0 = 제한 없음)"));
                EditorGUILayout.PropertyField(boundaryObjectTagProp, new GUIContent("경계 오브젝트 태그", "오브젝트가 넘어가면 안 되는 경계의 태그"));
                EditorGUILayout.PropertyField(collisionZoneTagProp, new GUIContent("충돌 영역 태그", "오브젝트가 충돌하면 안 되는 영역의 태그"));
            }
            
            EditorGUILayout.Space();
            
            // 패널티 설정
            EditorGUILayout.LabelField("패널티 설정", subheaderStyle);
            EditorGUILayout.PropertyField(OverDragProp, new GUIContent("과도한 드래그 패널티", "드래그 제한을 초과하거나 경계를 벗어날 때 적용할 패널티"));
        }
        
        private void DrawObjectCreationSettings(SerializedProperty settingsProp)
        {
            SerializedProperty createObjectProp = settingsProp.FindPropertyRelative("createObject");
            SerializedProperty objectToCreateProp = settingsProp.FindPropertyRelative("objectToCreate");
            
            // 이 인터랙션 타입을 활성화하기 위한 플래그
            createObjectProp.boolValue = true;
            
            // 기본 설정
            EditorGUILayout.PropertyField(objectToCreateProp, new GUIContent("생성할 오브젝트 태그", "활성화할 오브젝트의 태그 배열"));
        }
        
        private void DrawConditionalClickSettings(SerializedProperty settingsProp)
        {
            SerializedProperty isConditionalClickProp = settingsProp.FindPropertyRelative("isConditionalClick");
            SerializedProperty validClickTagsProp = settingsProp.FindPropertyRelative("validClickTags");
            SerializedProperty invalidClickTagsProp = settingsProp.FindPropertyRelative("invalidClickTags");
            SerializedProperty conditionalClickPenaltiesProp = settingsProp.FindPropertyRelative("conditionalClickPenalties");
            
            // 이 인터랙션 타입을 활성화하기 위한 플래그
            isConditionalClickProp.boolValue = true;
            
            // 기본 설정
            EditorGUILayout.PropertyField(validClickTagsProp, new GUIContent("유효한 클릭 태그", "클릭해도 되는 오브젝트의 태그 목록"));
            EditorGUILayout.PropertyField(invalidClickTagsProp, new GUIContent("유효하지 않은 클릭 태그", "클릭하면 안 되는 오브젝트의 태그 목록"));
            
            // 패널티 설정
            if (invalidClickTagsProp.arraySize > 0)
            {
                EditorGUILayout.PropertyField(conditionalClickPenaltiesProp, new GUIContent("클릭 패널티", "잘못된 클릭에 대한 패널티 목록"));
                
                // 패널티 배열 크기가 잘못된 클릭 태그 배열 크기와 맞지 않으면 자동 조정
                if (conditionalClickPenaltiesProp.arraySize != invalidClickTagsProp.arraySize)
                {
                    conditionalClickPenaltiesProp.arraySize = invalidClickTagsProp.arraySize;
                }
            }
        }
        
        private void DrawSustainedClickSettings(SerializedProperty settingsProp)
        {
            SerializedProperty isSustainedClickProp = settingsProp.FindPropertyRelative("isSustainedClick");
            SerializedProperty sustainedClickDurationProp = settingsProp.FindPropertyRelative("sustainedClickDuration");
            SerializedProperty earlyReleasePenaltyProp = settingsProp.FindPropertyRelative("earlyReleasePenalty");
            SerializedProperty lateReleasePenaltyProp = settingsProp.FindPropertyRelative("lateReleasePenalty");
            SerializedProperty sustainedClickTargetTagProp = settingsProp.FindPropertyRelative("sustainedClickTargetTag");
            
            // 이 인터랙션 타입을 활성화하기 위한 플래그
            isSustainedClickProp.boolValue = true;
            
            // 기본 설정
            EditorGUILayout.PropertyField(sustainedClickTargetTagProp, new GUIContent("지속 클릭 대상 태그", "지속적으로 클릭해야 하는 오브젝트의 태그"));
            EditorGUILayout.PropertyField(sustainedClickDurationProp, new GUIContent("지속 클릭 시간", "필요한 클릭 지속 시간 (초)"));
            
            // 패널티 설정
            EditorGUILayout.PropertyField(earlyReleasePenaltyProp, new GUIContent("조기 릴리스 패널티", "너무 일찍 클릭을 놓았을 때 적용할 패널티"));
            EditorGUILayout.PropertyField(lateReleasePenaltyProp, new GUIContent("늦은 릴리스 패널티", "너무 오래 클릭을 유지했을 때 적용할 패널티"));
        }
        
        private void DrawObjectDeletionSettings(SerializedProperty settingsProp)
        {
            SerializedProperty deleteObjectProp = settingsProp.FindPropertyRelative("deleteObject");
            SerializedProperty objectToDeleteTagProp = settingsProp.FindPropertyRelative("objectToDeleteTag");
            
            // 이 인터랙션 타입을 활성화하기 위한 플래그
            deleteObjectProp.boolValue = true;
            
            // 기본 설정
            EditorGUILayout.PropertyField(objectToDeleteTagProp, new GUIContent("삭제할 오브젝트 태그", "비활성화할 오브젝트의 태그 배열"));
        }
        
        private void DrawObjectMovementSettings(SerializedProperty settingsProp)
        {
            SerializedProperty moveObjectProp = settingsProp.FindPropertyRelative("moveObject");
            SerializedProperty objectToMoveTagProp = settingsProp.FindPropertyRelative("objectToMoveTag");
            SerializedProperty moveDirectionProp = settingsProp.FindPropertyRelative("moveDirection");
            SerializedProperty moveSpeedProp = settingsProp.FindPropertyRelative("moveSpeed");
            SerializedProperty movePathProp = settingsProp.FindPropertyRelative("movePath");
            
            // 이 인터랙션 타입을 활성화하기 위한 플래그
            moveObjectProp.boolValue = true;
            
            // 기본 설정
            EditorGUILayout.PropertyField(objectToMoveTagProp, new GUIContent("이동할 오브젝트 태그", "이동할 오브젝트의 태그"));
            
            EditorGUILayout.Space();
            
            // 이동 설정
            EditorGUILayout.LabelField("이동 설정", subheaderStyle);
            EditorGUILayout.PropertyField(moveSpeedProp, new GUIContent("이동 속도", "오브젝트 이동 속도"));
            
            // 이동 방법 선택
            bool usePath = movePathProp.arraySize > 0;
            bool newUsePath = EditorGUILayout.Toggle(new GUIContent("경로 사용", "단일 방향 대신 경로를 따라 이동"), usePath);
            
            if (newUsePath != usePath)
            {
                if (newUsePath)
                {
                    movePathProp.arraySize = 2; // 기본 경로 포인트 2개 생성
                }
                else
                {
                    movePathProp.arraySize = 0;
                }
            }
            
            if (newUsePath)
            {
                EditorGUILayout.PropertyField(movePathProp, new GUIContent("이동 경로", "오브젝트가 따라갈 경로 포인트"));
            }
            else
            {
                EditorGUILayout.PropertyField(moveDirectionProp, new GUIContent("이동 방향", "오브젝트가 이동할 방향"));
            }
        }
        
        private void DrawQuizPopupSettings(SerializedProperty settingsProp)
        {
            SerializedProperty showQuizPopupProp = settingsProp.FindPropertyRelative("showQuizPopup");
            SerializedProperty questionTextProp = settingsProp.FindPropertyRelative("questionText");
            SerializedProperty quizOptionsProp = settingsProp.FindPropertyRelative("quizOptions");
            SerializedProperty correctAnswerIndexProp = settingsProp.FindPropertyRelative("correctAnswerIndex");
            SerializedProperty optionImagesProp = settingsProp.FindPropertyRelative("optionImages");
            SerializedProperty timeLimitProp = settingsProp.FindPropertyRelative("timeLimit");
            SerializedProperty WrongAnswerProp = settingsProp.FindPropertyRelative("WrongAnswer");
            
            // 이 인터랙션 타입을 활성화하기 위한 플래그
            showQuizPopupProp.boolValue = true;
            
            // 기본 설정
            EditorGUILayout.PropertyField(questionTextProp, new GUIContent("질문 텍스트", "퀴즈 질문 내용"));
            EditorGUILayout.PropertyField(quizOptionsProp, new GUIContent("퀴즈 옵션", "선택 가능한 답변 목록"));
            
            // 옵션이 있는 경우에만 정답 인덱스 설정 표시
            if (quizOptionsProp.arraySize > 0)
            {
                // 정답 인덱스 값 검증
                int maxIndex = quizOptionsProp.arraySize - 1;
                correctAnswerIndexProp.intValue = Mathf.Clamp(correctAnswerIndexProp.intValue, 0, maxIndex);
                
                // 드롭다운으로 정답 선택
                string[] options = new string[quizOptionsProp.arraySize];
                for (int i = 0; i < options.Length; i++)
                {
                    SerializedProperty option = quizOptionsProp.GetArrayElementAtIndex(i);
                    options[i] = option.stringValue;
                }
                
                int selectedIndex = EditorGUILayout.Popup("정답", correctAnswerIndexProp.intValue, options);
                correctAnswerIndexProp.intValue = selectedIndex;
            }
            else
            {
                EditorGUILayout.PropertyField(correctAnswerIndexProp, new GUIContent("정답 인덱스", "정답 옵션의 인덱스"));
            }
            
            // 추가 설정
            EditorGUILayout.PropertyField(optionImagesProp, new GUIContent("옵션 이미지", "각 답변 옵션의 이미지"));
            EditorGUILayout.PropertyField(timeLimitProp, new GUIContent("시간 제한", "퀴즈 응답 시간 제한 (초, 0 = 제한 없음)"));
            
            // 패널티 설정
            EditorGUILayout.PropertyField(WrongAnswerProp, new GUIContent("오답 패널티", "잘못된 답변을 선택했을 때 적용할 패널티"));
        }
        
        private void DrawMiniGameSettings(SerializedProperty settingsProp)
        {
            SerializedProperty startMiniGameProp = settingsProp.FindPropertyRelative("startMiniGame");
            SerializedProperty miniGamePrefabProp = settingsProp.FindPropertyRelative("miniGamePrefab");
            
            // 이 인터랙션 타입을 활성화하기 위한 플래그
            startMiniGameProp.boolValue = true;
            
            // 기본 설정
            EditorGUILayout.PropertyField(miniGamePrefabProp, new GUIContent("미니게임 프리팹", "실행할 미니게임 프리팹"));
        }
        
        #endregion
        
        private void DeleteStage(int index)
        {
            stagesProperty.DeleteArrayElementAtIndex(index);
            
            // 폴드아웃 상태 업데이트
            var newFoldouts = new Dictionary<int, bool>();
            var newSettingsFoldouts = new Dictionary<int, bool>();
            
            for (int i = 0; i < stagesProperty.arraySize; i++)
            {
                if (i < index)
                {
                    newFoldouts[i] = stageFoldouts[i];
                    newSettingsFoldouts[i] = settingsFoldouts[i];
                }
                else
                {
                    newFoldouts[i] = stageFoldouts[i + 1];
                    newSettingsFoldouts[i] = settingsFoldouts[i + 1];
                }
            }
            
            stageFoldouts = newFoldouts;
            settingsFoldouts = newSettingsFoldouts;
        }
        
        private void MoveStage(int fromIndex, int toIndex)
        {
            stagesProperty.MoveArrayElement(fromIndex, toIndex);
            
            // 스테이지 번호 업데이트
            SerializedProperty fromStage = stagesProperty.GetArrayElementAtIndex(toIndex);
            SerializedProperty toStage = stagesProperty.GetArrayElementAtIndex(fromIndex);
            
            SerializedProperty fromStageNum = fromStage.FindPropertyRelative("StageNum");
            SerializedProperty toStageNum = toStage.FindPropertyRelative("StageNum");
            
            int temp = fromStageNum.intValue;
            fromStageNum.intValue = toStageNum.intValue;
            toStageNum.intValue = temp;
            
            // 폴드아웃 상태 교환
            bool tempFoldout = stageFoldouts[fromIndex];
            stageFoldouts[fromIndex] = stageFoldouts[toIndex];
            stageFoldouts[toIndex] = tempFoldout;
            
            tempFoldout = settingsFoldouts[fromIndex];
            settingsFoldouts[fromIndex] = settingsFoldouts[toIndex];
            settingsFoldouts[toIndex] = tempFoldout;
        }
    }
}