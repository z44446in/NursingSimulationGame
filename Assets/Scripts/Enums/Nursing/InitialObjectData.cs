using UnityEngine;

namespace Nursing.Interaction
{
    /// <summary>
    /// 상호작용 초기에 생성되는 오브젝트 데이터
    /// </summary>
    [System.Serializable]
    public class InitialObjectData
    {
        public string objectId;
        public string objectName;
        public string tag;
        public Vector2 position;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;
        public Sprite objectSprite;
        public bool useCustomPrefab;
        public GameObject customPrefab;
    }
}