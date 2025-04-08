using UnityEngine;
using UnityEditor;
using Nursing.Penalty;

namespace Nursing.Editor
{
    [CustomPropertyDrawer(typeof(PenaltyData))]
    public class PenaltyDataDrawer : PropertyDrawer
    {
        private bool isExpanded = false;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!isExpanded)
                return EditorGUIUtility.singleLineHeight;

            // 기본 높이 + 필드 수 * 한 줄 높이 + 패딩
            return EditorGUIUtility.singleLineHeight * 13 + EditorGUIUtility.standardVerticalSpacing * 2;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // 폴드아웃 가져오기 (캐싱 X - 여러 항목에 대해 사용될 수 있음)
            isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), 
                isExpanded, label, true);
            
            if (isExpanded)
            {
                // 패널티 타입 속성
                SerializedProperty penaltyTypeProp = property.FindPropertyRelative("penaltyType");
                SerializedProperty penaltyScoreProp = property.FindPropertyRelative("penaltyScore");
                SerializedProperty speakerProp = property.FindPropertyRelative("speaker");
                SerializedProperty penaltyMessageProp = property.FindPropertyRelative("penaltyMessage");
                SerializedProperty databaseMessageProp = property.FindPropertyRelative("databaseMessage");
                SerializedProperty flashRedScreenProp = property.FindPropertyRelative("flashRedScreen");
                SerializedProperty flashCountProp = property.FindPropertyRelative("flashCount");
                
                EditorGUI.indentLevel++;
                
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float spacing = EditorGUIUtility.standardVerticalSpacing;
                float yPos = position.y + lineHeight + spacing * 0;
                
                // 패널티 타입 필드
                EditorGUI.PropertyField(
                    new Rect(position.x, yPos, position.width, lineHeight),
                    penaltyTypeProp, new GUIContent("패널티 타입", "패널티의 심각도 유형"));
                yPos += lineHeight + spacing ;
                
                // 패널티 점수 필드
                EditorGUI.PropertyField(
                    new Rect(position.x, yPos, position.width, lineHeight),
                    penaltyScoreProp, new GUIContent("패널티 점수", "이 패널티로 감점될 점수"));
                yPos += lineHeight + spacing ;

                // 화자 필드
                EditorGUI.PropertyField(
                    new Rect(position.x, yPos, position.width, lineHeight),
                    speakerProp, new GUIContent("화자", "패널티 메시지를 말하는 화자"));
                yPos += lineHeight + spacing ;
                
                // 패널티 메시지 필드
                EditorGUI.PropertyField(
                    new Rect(position.x, yPos, position.width, lineHeight),
                    penaltyMessageProp, new GUIContent("패널티 메시지", "사용자에게 표시할 패널티 메시지"));
                yPos += lineHeight + spacing ;
                
                // 데이터베이스 메시지 필드
                EditorGUI.PropertyField(
                    new Rect(position.x, yPos, position.width, lineHeight),
                    databaseMessageProp, new GUIContent("데이터베이스 메시지", "데이터베이스에 기록할 패널티 메시지 (비워두면 기록하지 않음)"));
                yPos += lineHeight + spacing + 12f;
                
                // 화면 깜빡임 설정
                EditorGUI.PropertyField(
                    new Rect(position.x, yPos, position.width, lineHeight),
                    flashRedScreenProp, new GUIContent("화면 깜빡임", "패널티 발생 시 화면 가장자리를 빨간색으로 깜빡일지 여부"));
                
                if (flashRedScreenProp.boolValue)
                {
                    yPos += lineHeight + spacing ;
                    
                    // 깜빡임 횟수 필드
                    EditorGUI.PropertyField(
                        new Rect(position.x, yPos, position.width, lineHeight),
                        flashCountProp, new GUIContent("깜빡임 횟수", "화면을 깜빡일 횟수"));
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.EndProperty();
        }
    }
}