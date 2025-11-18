using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using StarterAssets;

public class InventoryUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject slotPrefab; // ★ 주의: 이 프리팹에 ItemSlotUI 스크립트가 붙어야 함

    private Inventory inventory;
    private StarterAssetsInputs _input;
    private Player player;

    void Start()
    {
        inventory = FindAnyObjectByType<Inventory>();
        _input = FindAnyObjectByType<StarterAssetsInputs>();
        player = FindAnyObjectByType<Player>();

        if (inventory != null)
        {
            inventory.OnInventoryChanged += UpdateUI;
        }

        UpdateUI();
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        if (isActive)
        {
            if (_input != null) { _input.cursorInputForLook = false; _input.look = Vector2.zero; }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (_input != null) { _input.cursorInputForLook = true; }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void UpdateUI()
    {
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }

        // InventorySlot(데이터) 리스트를 순회
        foreach (InventorySlot slotData in inventory.slots)
        {
            if (slotData.item == null) continue;

            GameObject newSlotObj = Instantiate(slotPrefab, slotsParent);
            newSlotObj.transform.localScale = Vector3.one;
            newSlotObj.transform.localPosition = new Vector3(newSlotObj.transform.localPosition.x, newSlotObj.transform.localPosition.y, 0);

            // ★★★ 수정됨: ItemSlotUI 컴포넌트를 가져옵니다 ★★★
            ItemSlotUI uiScript = newSlotObj.GetComponent<ItemSlotUI>();

            // 프리팹에 스크립트가 안 붙어있을 경우를 대비해 자동 추가
            if (uiScript == null) uiScript = newSlotObj.AddComponent<ItemSlotUI>();

            // 데이터 전달
            uiScript.SetItem(slotData.item);

            // 클릭 이벤트 연결
            uiScript.OnSlotClicked += (clickedItem) => {
                if (player != null) player.EquipItem(clickedItem);
            };

            // 아이콘 이미지 설정
            Transform iconTransform = newSlotObj.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null) iconImage.sprite = slotData.item.icon;
            }

            // 텍스트 설정
            Transform countTransform = newSlotObj.transform.Find("Item Count");
            if (countTransform != null)
            {
                TextMeshProUGUI countText = countTransform.GetComponent<TextMeshProUGUI>();
                if (countText != null)
                {
                    if (slotData.count > 1) countText.text = slotData.count.ToString();
                    else countText.text = "";
                }
            }
        }
    }
}