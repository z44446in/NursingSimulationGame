using UnityEngine;
using UnityEngine.UI;

public class CustomRaycastImage : Image
{
    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        // 스프라이트의 알파 채널 확인
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 localPoint);
        Rect rect = rectTransform.rect;
        if (!rect.Contains(localPoint))
            return false;

        Vector2 normalizedPoint = new Vector2(
            (localPoint.x - rect.x) / rect.width,
            (localPoint.y - rect.y) / rect.height
        );

        Sprite sprite = this.sprite;
        if (sprite == null) return true;

        // Texture의 알파값 확인
        Texture2D texture = sprite.texture;
        Rect spriteRect = sprite.rect;
        int x = Mathf.FloorToInt(spriteRect.x + spriteRect.width * normalizedPoint.x);
        int y = Mathf.FloorToInt(spriteRect.y + spriteRect.height * normalizedPoint.y);
        if (x < 0 || x >= texture.width || y < 0 || y >= texture.height)
            return false;

        Color pixelColor = texture.GetPixel(x, y);
        return pixelColor.a > 0; // 알파값이 0보다 클 때만 선택 가능
    }
}
