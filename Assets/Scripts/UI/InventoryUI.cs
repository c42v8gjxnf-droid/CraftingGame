using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public Inventory inventory;
    public GameObject slotPrefab;
    public Transform slotParent;     // <- auf 'Content' zeigen
    public ScrollRect scrollRect;    // <- optional zuweisen (InventoryScrollView)

    private ItemSlotUI[] slots;

    private void Start()
    {
        Build();

        if (inventory != null)
            inventory.OnChanged += RefreshAll;
        else
            Debug.LogError("[InventoryUI] 'inventory' ist nicht zugewiesen.", this);
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnChanged -= RefreshAll;
    }

    public void Build()
    {
        if (inventory == null || slotPrefab == null || slotParent == null)
        {
            Debug.LogError("[InventoryUI] Missing refs.", this);
            return;
        }

        // alte Kinder entfernen
        for (int i = slotParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(slotParent.GetChild(i).gameObject);

        int count = inventory.Slots.Count;
        slots = new ItemSlotUI[count];

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(slotPrefab, slotParent);
            var ui = go.GetComponent<ItemSlotUI>();
            if (!ui)
            {
                Debug.LogError("[InventoryUI] slotPrefab hat kein ItemSlotUI!", slotPrefab);
                Destroy(go);
                continue;
            }

            ui.owner = ItemSlotUI.OwnerType.Inventory;
            ui.index = i;
            ui.inventory = inventory;
            ui.Init();
            slots[i] = ui;
        }

        // Layout sofort durchrechnen, damit ScrollRect die richtige Höhe kennt
        var rt = slotParent as RectTransform;
        if (rt) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        // Optional: nach Build oben anfangen
        ScrollToTop();
    }

    public void RefreshAll()
    {
        if (slots == null) return;
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] != null) slots[i].Refresh();

        // Layout nach Mengenänderungen ggf. neu rechnen
        var rt = slotParent as RectTransform;
        if (rt) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    // -------- Optional helpers --------
    public void ScrollToTop()
    {
        if (scrollRect) scrollRect.verticalNormalizedPosition = 1f;
    }
    public void ScrollToBottom()
    {
        if (scrollRect) scrollRect.verticalNormalizedPosition = 0f;
    }

    /// <summary>Scrollt so, dass der Slot mit Index sichtbar wird (einfach, reihenbasiert).</summary>
    public void ScrollToSlot(int index)
    {
        if (!scrollRect || slots == null || index < 0 || index >= slots.Length) return;

        var grid = (slotParent as RectTransform)?.GetComponent<GridLayoutGroup>();
        if (!grid) return;

        int columns = (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount && grid.constraintCount > 0)
            ? grid.constraintCount : 1;
        int row = index / columns;

        // Sichtbare rows schätzen:
        var viewport = scrollRect.viewport ? scrollRect.viewport.rect.height : ((RectTransform)scrollRect.transform).rect.height;
        float rowHeight = grid.cellSize.y + grid.spacing.y;
        if (rowHeight <= 0) rowHeight = 1;

        // Ziel-Offset im Content von oben aus
        float targetY = row * rowHeight;

        // Maximaler Scrollbereich
        var contentRT = slotParent as RectTransform;
        float contentHeight = contentRT.rect.height;
        float viewHeight = viewport;

        float maxScroll = Mathf.Max(0, contentHeight - viewHeight);
        float clamped = Mathf.Clamp(targetY - 0.5f * (viewHeight - rowHeight), 0, maxScroll);

        // Normalized: 1 = Top, 0 = Bottom
        float normalized = (maxScroll <= 0) ? 1f : 1f - (clamped / maxScroll);
        scrollRect.verticalNormalizedPosition = normalized;
    }

    public ItemSlotUI[] Slots => slots;
}
