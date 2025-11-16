using UnityEngine;

public class DragContext
{
    public static DragContext Current { get; private set; }
    public Item draggedItem;
    public ItemSlotUI sourceSlot;

    public static void Begin(Item item, ItemSlotUI source)
    {
        if (item == null) { Current = null; return; }
        Current = new DragContext { draggedItem = item, sourceSlot = source };
    }

    public static void End() { Current = null; }
    public static bool Active => Current != null && Current.draggedItem != null;
}
