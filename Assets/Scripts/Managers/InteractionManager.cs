using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// InteractionManager: 상호작용과 아이템 카트를 관리하는 클래스.
/// </summary>
public class InteractionManager : MonoBehaviour
{
    private static InteractionManager instance;

    /// <summary>
    /// InteractionManager 싱글톤 인스턴스에 접근.
    /// </summary>
    public static InteractionManager Instance => instance;

    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = Color.white; // 하이라이트 색상

    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>(); // 원본 재질 저장

    // 카트 관련
    private List<Item> cartItems = new List<Item>();
    public event Action<Item> OnItemAddedToCart;
    public event Action<Item> OnItemRemovedFromCart;

    // 현재 들고 있는 아이템
    private Item currentHeldItem;
    public Item CurrentHeldItem => currentHeldItem;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 오브젝트 하이라이트를 처리.
    /// </summary>
    public void HighlightObject(GameObject obj, bool highlight)
    {
        if (obj == null)
        {
            Debug.LogWarning("Attempted to highlight a null object.");
            return;
        }

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (highlight)
            {
                ApplyHighlight(renderer);
            }
            else
            {
                RestoreOriginalMaterials(renderer);
            }
        }
    }

    /// <summary>
    /// 하이라이트 효과를 적용.
    /// </summary>
    private void ApplyHighlight(Renderer renderer)
    {
        if (!originalMaterials.ContainsKey(renderer))
        {
            originalMaterials[renderer] = renderer.materials;

            // 모든 재질에 하이라이트 효과 적용
            foreach (var material in renderer.materials)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", highlightColor);
            }
        }
    }

    /// <summary>
    /// 오브젝트의 원래 재질로 복원.
    /// </summary>
    private void RestoreOriginalMaterials(Renderer renderer)
    {
        if (originalMaterials.ContainsKey(renderer))
        {
            renderer.materials = originalMaterials[renderer];
            foreach (var material in renderer.materials)
            {
                material.DisableKeyword("_EMISSION");
            }
            originalMaterials.Remove(renderer);
        }
    }

    /// <summary>
    /// 아이템을 카트에 추가.
    /// </summary>
    public bool AddItemToCart(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("Attempted to add a null item to the cart.");
            return false;
        }

        cartItems.Add(item);
        OnItemAddedToCart?.Invoke(item);
        
        return true;
    }

    /// <summary>
    /// 아이템을 카트에서 제거.
    /// </summary>
    public bool RemoveItemFromCart(Item item)
    {
        if (item == null || !cartItems.Contains(item))
        {
            Debug.LogWarning($"Attempted to remove a non-existent item: {item?.itemName}");
            return false;
        }

        cartItems.Remove(item);
        OnItemRemovedFromCart?.Invoke(item);
        
        return true;
    }

    /// <summary>
    /// 현재 카트에 있는 아이템 리스트를 반환.
    /// </summary>
    public List<Item> GetCartItems()
    {
        return new List<Item>(cartItems);
    }

    /// <summary>
    /// 카트를 초기화.
    /// </summary>
    public void ClearCart()
    {
        cartItems.Clear();
        Debug.Log("Cart cleared.");
    }

    /// <summary>
    /// 아이템을 들기.
    /// </summary>
    public void PickupItem(Item item)
    {
        if (currentHeldItem != null)
        {
            Debug.LogWarning("Cannot pick up item while holding another.");
            return;
        }

        currentHeldItem = item;
        UpdateCursorWithItem(item);
    }

    /// <summary>
    /// 아이템을 내려놓기.
    /// </summary>
    public void DropItem()
    {
        currentHeldItem = null;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Debug.Log("Dropped current held item.");
    }

    /// <summary>
    /// 아이템의 커서를 업데이트.
    /// </summary>
    private void UpdateCursorWithItem(Item item)
    {
        if (item?.itemSprite != null)
        {
            Cursor.SetCursor(TextureFromSprite(item.itemSprite), Vector2.zero, CursorMode.Auto);
        }
    }

    /// <summary>
    /// Sprite를 Texture로 변환.
    /// </summary>
    private Texture2D TextureFromSprite(Sprite sprite)
    {
        if (sprite.texture.isReadable)
        {
            return sprite.texture;
        }

        Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.RGBA32, false);
        texture.SetPixels(sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height));
        texture.Apply();
        return texture;
    }

    /// <summary>
    /// 파괴 시 자원 해제.
    /// </summary>
    private void OnDestroy()
    {
        // 하이라이트 복원
        foreach (var renderer in originalMaterials.Keys)
        {
            if (renderer != null)
            {
                RestoreOriginalMaterials(renderer);
            }
        }
        originalMaterials.Clear();

        // 커서 복원
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}