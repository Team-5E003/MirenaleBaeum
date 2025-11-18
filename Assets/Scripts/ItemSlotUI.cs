using UnityEngine;
using UnityEngine.UI;
using System;

public class ItemSlotUI : MonoBehaviour
{
    // 클릭되었을 때 InventoryUI에게 알릴 이벤트
    public event Action<ItemData> OnSlotClicked;

    private ItemData currentItem;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClicked);
        }
    }

    // InventoryUI가 데이터를 넣어줄 때 호출하는 함수
    public void SetItem(ItemData item)
    {
        currentItem = item;
    }

    private void OnClicked()
    {
        if (currentItem != null)
        {
            OnSlotClicked?.Invoke(currentItem);
        }
    }
}