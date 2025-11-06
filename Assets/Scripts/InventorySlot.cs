using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int count;

    public InventorySlot(ItemData item, int count)
    {
        this.item = item;
        this.count = count;
    }
}
