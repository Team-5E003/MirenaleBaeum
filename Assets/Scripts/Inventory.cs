using System.Collections.Generic;
using UnityEngine;
using System; 

public class Inventory : MonoBehaviour
{
    public List<InventorySlot> slots = new List<InventorySlot>();
    public int maxSlots = 8; //최대 크기

    public event Action OnInventoryChanged; //인벤토리 변경 이벤트 처리


    // 아이템 추가 함수
    public bool AddItem(ItemData item)
    {
        //아이템이 지금 인벤토리에 있고, 겹칠 수 있는 거라면 인벤토리에 추가
        foreach (InventorySlot slot in slots)
        {
            if (slot.item == item && slot.count < item.maxStack)
            {
                slot.count++;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        //겹칠 수 없거나 새 아이템이면, 빈 슬롯이 있는지 확인
        if (slots.Count < maxSlots)
        {
            slots.Add(new InventorySlot(item, 1));
            OnInventoryChanged?.Invoke();
            return true;
        }

        //인벤토리가 꽉 찼음
        Debug.Log("인벤토리가 꽉 찼습니다.");
        return false;
    }

    public bool HasItem(ItemData itemToFind)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.item == itemToFind)
            {
                return true;
            }
        }
        return false;
    }
}