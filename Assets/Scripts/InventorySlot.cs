using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int count;

    // 생성자 (Inventory.cs에서 new InventorySlot(...) 할 때 사용됨)
    public InventorySlot(ItemData item, int count)
    {
        this.item = item;
        this.count = count;
    }
}