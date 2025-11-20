using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("상호작용 설정")]
    [SerializeField] private float pickupDistance = 3f;
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private TextMeshProUGUI pickupPromptText;

    [Header("장착 설정")]
    [SerializeField] private Transform handPoint; // ★ 아이템이 생성될 손 위치 (빈 오브젝트 연결)
    private GameObject currentEquippedObject;     // 현재 들고 있는 아이템

    private Inventory inventory;
    private ItemPickup currentTargetItem;
    private DestroyableObject currentTargetDestroyable; // (기존 이름 유지)
    public Camera playerCamera;
    private Coroutine messageCoroutine;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        inventory = GetComponent<Inventory>();
        if (pickupPromptText != null) pickupPromptText.gameObject.SetActive(false);
    }

    void Update()
    {
        // 메시지가 떠있지 않을 때만 상호작용 확인
        if (messageCoroutine == null)
        {
            CheckForInteraction();
        }

        // E키 상호작용
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentTargetItem != null)
            {
                AttemptPickup();
            }
            else if (currentTargetDestroyable != null)
            {
                AttemptDestroy();
            }
        }
    }

    // ★ 아이템 장착 함수 (InventoryUI에서 호출)
    public void EquipItem(ItemData itemToEquip)
    {
        // 1. 기존에 들고 있는 게 있으면 삭제
        if (currentEquippedObject != null)
        {
            Destroy(currentEquippedObject);
        }

        // 2. 프리팹이 있다면 손 위치에 생성
        if (itemToEquip.prefab != null && handPoint != null)
        {
            currentEquippedObject = Instantiate(itemToEquip.prefab, handPoint);

            // 손 위치에 딱 맞게 초기화
            currentEquippedObject.transform.localPosition = Vector3.zero;
            currentEquippedObject.transform.localRotation = Quaternion.identity;

            Debug.Log($"{itemToEquip.itemName} 장착 완료");
        }
        else
        {
            Debug.LogWarning($"장착 실패: {itemToEquip.itemName}의 프리팹이 없거나 HandPoint가 설정되지 않았습니다.");
        }
    }

    // --- 아래는 기존 상호작용 로직들 ---

    private void CheckForInteraction()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        currentTargetItem = null;
        currentTargetDestroyable = null;

        if (Physics.Raycast(ray, out hit, pickupDistance, itemLayerMask))
        {
            ItemPickup item = hit.collider.GetComponent<ItemPickup>();
            if (item != null && item.itemToPickup != null)
            {
                currentTargetItem = item;
                ShowInteractionPrompt(true, $"Press [E] to pick up {item.itemToPickup.itemName}");
                return;
            }

            DestroyableObject destroyable = hit.collider.GetComponent<DestroyableObject>();
            if (destroyable != null)
            {
                currentTargetDestroyable = destroyable;
                ShowInteractionPrompt(true, $"Press [E] to {destroyable.ActionName} {destroyable.ObjectName}");
                return;
            }
        }
        ShowInteractionPrompt(false);
    }

    private void AttemptPickup()
    {
        if (currentTargetItem == null) return;
        bool success = inventory.AddItem(currentTargetItem.itemToPickup);
        if (success)
        {
            Destroy(currentTargetItem.gameObject);
            ShowInteractionPrompt(false);
        }
    }

    private void AttemptDestroy()
    {
        if (currentTargetDestroyable == null) return;
        string message = currentTargetDestroyable.AttemptDestroy(inventory);
        if (message != null)
        {
            ShowTemporaryMessage(message, 2.0f);
        }
    }

    private void ShowInteractionPrompt(bool show, string message = "")
    {
        if (messageCoroutine != null)
        {
            if (!show) pickupPromptText.gameObject.SetActive(false);
            return;
        }

        if (pickupPromptText == null) return;
        if (show)
        {
            pickupPromptText.text = message;
            pickupPromptText.gameObject.SetActive(true);
        }
        else
        {
            pickupPromptText.gameObject.SetActive(false);
        }
    }

    private void ShowTemporaryMessage(string message, float duration)
    {
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    private IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        pickupPromptText.text = message;
        pickupPromptText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        pickupPromptText.gameObject.SetActive(false);
        messageCoroutine = null;
    }
}