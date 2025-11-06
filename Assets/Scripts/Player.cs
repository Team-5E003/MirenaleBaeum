using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float pickupDistance = 3f;
    [SerializeField] private LayerMask itemLayerMask; //상호 작용가능하게 할 레이어 설정

    //연결할 텍스트 UI
    [SerializeField] private TextMeshProUGUI pickupPromptText;

    private Inventory inventory;

    // 현재 바라보고 있는 아이템을 저장할 변수
    private ItemPickup currentTargetItem;
    public Camera playerCamera;
    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        inventory = GetComponent<Inventory>();
        if (pickupPromptText != null)
        {
            pickupPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        CheckForPickupableItem();

        // 2. 바라보고 있는 아이템이 있고, E키를 눌렀으면 줍기
        if (currentTargetItem != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            AttemptPickup();
        }
    }

    private void CheckForPickupableItem()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupDistance, itemLayerMask))
        {
            // Raycast에 맞은 것이 ItemPickup인지 확인
            ItemPickup item = hit.collider.GetComponent<ItemPickup>();

            if (item != null)
            {
                currentTargetItem = item;
                ShowPickupPrompt(true, item.itemToPickup.itemName); // UI 켜기
                return;
            }
        }

        // Raycast에 아이템이 감지되지 않았다면
        currentTargetItem = null;
        ShowPickupPrompt(false); // UI 끄기
    }

    // 아이템 줍기
    private void AttemptPickup()
    {
        if (currentTargetItem == null) return;

        bool success = inventory.AddItem(currentTargetItem.itemToPickup);

        if (success)
        {
            Destroy(currentTargetItem.gameObject);
            ShowPickupPrompt(false);
        }
    }

    private void ShowPickupPrompt(bool show, string itemName = "")
    {
        if (pickupPromptText == null) return;

        if (show)
        {
            // 텍스트를 설정하고 UI를 켜기
            pickupPromptText.text = $"Press 'E' to equip {itemName}";
            pickupPromptText.gameObject.SetActive(true);
        }
        else
        {
            pickupPromptText.gameObject.SetActive(false);
        }
    }
}